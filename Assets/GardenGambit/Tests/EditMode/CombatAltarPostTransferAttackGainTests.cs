using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatAltarPostTransferAttackGainTests
    {
        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void SacrificialAltar_RecipientAttackGainAfterHpTransfer_DoesNotBlockDonorDeath(CombatSide side)
        {
            var e = new Environment(side);
            var execution = e.Start();
            var hpGain = (HpGainCombatEvent)e.Log.Events[2];
            Assert.That(hpGain.SourceInstanceId, Is.EqualTo(e.Donor.InstanceId));
            new CombatAttackGainResolver(e.Metadata, e.Log).TryApplyAttackGain(
                e.State, hpGain, e.RecipientPosition, 1);
            execution.MarkTransferTriggersResolved();

            var death = e.Resolver.StartDonorDeath(execution);

            Assert.That(death.InstanceId, Is.EqualTo(e.Donor.InstanceId));
            Assert.That(death.Metadata.ParentEventId.Value, Is.EqualTo(execution.AltarEvent.Metadata.EventId));
            Assert.That(death.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.TriggerRootId));
            Assert.That(e.Donor.CurrentHp, Is.Zero);
            Assert.That(e.Recipient.Attack, Is.EqualTo(5));
            Assert.That(e.Recipient.CurrentHp, Is.EqualTo(8));
            Assert.That(e.Recipient.HpCapacity, Is.EqualTo(13));
            Assert.That(e.Log.Count, Is.EqualTo(5));
            Assert.That(execution.Stage, Is.EqualTo(CombatAltarActivationExecutionStage.DonorDeathStarted));
        }

        [Test]
        public void SacrificialAltar_RecipientAttackReductionAfterTransfer_IsPreserved()
        {
            var e = new Environment();
            var execution = e.Start();
            e.Recipient.ReduceAttack(1);
            execution.MarkTransferTriggersResolved();
            e.Resolver.StartDonorDeath(execution);
            Assert.That(e.Donor.CurrentHp, Is.Zero);
            Assert.That(e.Recipient.Attack, Is.EqualTo(3));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void StartDonorDeath_BeforeTransferTriggersResolve_RemainsRejected(bool changeAttack)
        {
            var e = new Environment();
            var execution = e.Start();
            if (changeAttack) { e.Recipient.ApplyAttackGain(1); }
            var count = e.Log.Count;
            Assert.Throws<InvalidOperationException>(() => e.Resolver.StartDonorDeath(execution));
            Assert.That(e.Donor.CurrentHp, Is.EqualTo(3));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(execution.Stage, Is.EqualTo(CombatAltarActivationExecutionStage.TransferApplied));
        }

        [Test]
        public void DirectApplier_AfterRecipientAttackChange_StillRequiresExactTransfer()
        {
            var e = new Environment();
            var execution = e.Start();
            e.Recipient.ApplyAttackGain(1);
            Assert.Throws<InvalidOperationException>(() => new CombatAltarTransferApplier()
                .ApplyDonorDeathThreshold(execution.TransferPreview));
            Assert.That(e.Donor.CurrentHp, Is.EqualTo(3));
            Assert.That(e.Recipient.Attack, Is.EqualTo(5));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void PostTriggerPath_RetainsOtherTransferGuards(int changedValue)
        {
            var e = new Environment(kind: changedValue == 3 ? CombatSlotEnhanceKind.WarAltar : CombatSlotEnhanceKind.SacrificialAltar);
            var execution = e.Start();
            if (changedValue == 0) { e.Recipient.Heal(1); }
            else if (changedValue == 1) { e.Recipient.ApplyHpStatGain(1); }
            else if (changedValue == 2) { e.Donor.ApplyIncomingDamage(1); }
            else { e.Recipient.ApplyAttackGain(1); }
            execution.MarkTransferTriggersResolved();
            var donorHp = e.Donor.CurrentHp;
            var count = e.Log.Count;
            Assert.Throws<InvalidOperationException>(() => e.Resolver.StartDonorDeath(execution));
            Assert.That(e.Donor.CurrentHp, Is.EqualTo(donorHp));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(execution.Stage, Is.EqualTo(CombatAltarActivationExecutionStage.TransferTriggersResolved));
        }

        [Test]
        public void PostTriggerApplier_RejectsNullExecutionState()
        {
            Assert.Throws<ArgumentNullException>(() => new CombatAltarTransferApplier()
                .ApplyDonorDeathThresholdAfterTransferTriggers(null));
        }

        [Test]
        public void PostTriggerApplier_RejectsUnresolvedExecutionState()
        {
            var e = new Environment();
            var execution = e.Start();
            Assert.Throws<InvalidOperationException>(() => new CombatAltarTransferApplier()
                .ApplyDonorDeathThresholdAfterTransferTriggers(execution));
            Assert.That(e.Donor.CurrentHp, Is.EqualTo(3));
        }

        [Test]
        public void RepeatedStartDonorDeath_DoesNotAppendSecondDeath()
        {
            var e = new Environment();
            var execution = e.Start();
            e.Recipient.ApplyAttackGain(1);
            execution.MarkTransferTriggersResolved();
            e.Resolver.StartDonorDeath(execution);
            var count = e.Log.Count;
            Assert.Throws<InvalidOperationException>(() => e.Resolver.StartDonorDeath(execution));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Recipient.Attack, Is.EqualTo(5));
        }

        private sealed class Environment
        {
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatAltarActivationResolver Resolver;
            public readonly CombatState State;
            public readonly CombatCardState Donor;
            public readonly CombatCardState Recipient;
            public readonly BoardPosition DonorPosition;
            public readonly BoardPosition RecipientPosition;

            public Environment(CombatSide side = CombatSide.Player,
                CombatSlotEnhanceKind kind = CombatSlotEnhanceKind.SacrificialAltar)
            {
                Donor = new CombatCardState(new DefinitionId("test.altar_donor"), new InstanceId(1),
                    new CardRank(2), hpCapacity: 3, currentHp: 3, armor: 0, attack: 2);
                Recipient = new CombatCardState(new DefinitionId("test.altar_recipient"), new InstanceId(2),
                    new CardRank(2), hpCapacity: 10, currentHp: 5, armor: 0, attack: 4);
                DonorPosition = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
                RecipientPosition = new BoardPosition(side, BoardRow.Back, new BoardColumn(1));
                var own = new CombatSideState(new CombatBoardState(side, new[]
                {
                    new CombatSlotState(new SlotId(1), DonorPosition, Donor.InstanceId, kind),
                    new CombatSlotState(new SlotId(2), RecipientPosition, Recipient.InstanceId)
                }), new CombatCardRegistry(new[] { Donor, Recipient }), new BattleHealth(20), new AttackMultiplier(1));
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var other = new CombatSideState(new CombatBoardState(otherSide, Array.Empty<CombatSlotState>()),
                    new CombatCardRegistry(Array.Empty<CombatCardState>()), new BattleHealth(20), new AttackMultiplier(1));
                State = new CombatState(side == CombatSide.Player ? own : other, side == CombatSide.Enemy ? own : other);
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAltarActivationResolver(Metadata, Log);
            }

            public CombatAltarActivationExecutionState Start() => Resolver.TryStartActivation(State, Root, DonorPosition);
        }
    }
}
