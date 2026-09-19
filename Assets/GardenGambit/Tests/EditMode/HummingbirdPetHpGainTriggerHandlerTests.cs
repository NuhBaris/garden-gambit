using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HummingbirdPetHpGainTriggerHandlerTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Constructor_RejectsMissingDependency(bool missingUsage)
        {
            var e = new TestEnvironment();
            Assert.Throws<ArgumentNullException>(() => new HummingbirdPetHpGainTriggerHandler(
                e.Side, e.Pets[0].InstanceId, missingUsage ? null : e.Usage, missingUsage ? e.Resolver : null));
        }

        [TestCase(-1)]
        [TestCase(99)]
        public void Constructor_RejectsInvalidSide(int side)
        {
            var e = new TestEnvironment();
            Assert.Throws<ArgumentOutOfRangeException>(() => new HummingbirdPetHpGainTriggerHandler(
                (CombatSide)side, e.Pets[0].InstanceId, e.Usage, e.Resolver));
        }

        [Test]
        public void Constructor_RejectsInvalidPetIdAndPreservesSharedDependencies()
        {
            var e = new TestEnvironment();
            Assert.Throws<ArgumentException>(() => new HummingbirdPetHpGainTriggerHandler(
                e.Side, default(InstanceId), e.Usage, e.Resolver));
            Assert.That(e.Handlers[0].UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Handlers[0].AttackGainResolver, Is.SameAs(e.Resolver));
        }

        [TestCase(CombatSide.Player, BoardRow.Front, false)]
        [TestCase(CombatSide.Player, BoardRow.Front, true)]
        [TestCase(CombatSide.Player, BoardRow.Back, false)]
        [TestCase(CombatSide.Player, BoardRow.Back, true)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, false)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, true)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, false)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, true)]
        public void FirstExternalHpGain_AddsOneAttackAndLogsChild(
            CombatSide side, BoardRow row, bool statGain)
        {
            var e = new TestEnvironment(side);
            var target = e.Card(side, row, 1);
            var hpGain = e.Gain(side, row, statGain: statGain);
            var handler = e.Handler(row);
            var hp = target.CurrentHp;
            var capacity = target.HpCapacity;
            Assert.That(handler.CanTrigger(e.State, hpGain), Is.True);
            Assert.That(handler.CanTrigger(e.State, hpGain), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            Assert.That(target.Attack, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);

            handler.Resolve(e.State, hpGain);

            Assert.That(target.Attack, Is.EqualTo(3));
            Assert.That(target.CurrentHp, Is.EqualTo(hp));
            Assert.That(target.HpCapacity, Is.EqualTo(capacity));
            Assert.That(target.Armor, Is.Zero);
            Assert.That(target.Rank.Value, Is.EqualTo(4));
            Assert.That(e.Card(side, row, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(e.OtherSide, row, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Usage.HasTriggered(handler.PetInstanceId, target.InstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(3));
            var gain = e.AttackGains()[0];
            Assert.That(gain.TargetInstanceId, Is.EqualTo(target.InstanceId));
            Assert.That(gain.TargetPosition, Is.EqualTo(e.Position(side, row, 1)));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
            Assert.That(gain.PreviousAttack, Is.EqualTo(2));
            Assert.That(gain.CurrentAttack, Is.EqualTo(3));
            Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(hpGain.Metadata.EventId));
            Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
            Assert.That(gain.Metadata.SequenceNo.Value, Is.GreaterThan(hpGain.Metadata.SequenceNo.Value));
            Assert.That(handler.CanTrigger(e.State, hpGain), Is.False);
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Autumn)]
        [TestCase(CombatCardSeason.Winter)]
        [TestCase(CombatCardSeason.Seasonless)]
        public void TargetSeason_DoesNotRestrictBonus(CombatCardSeason season)
        {
            var e = new TestEnvironment(season: season);
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SelfSource_DoesNotConsumeFirstExternalGain(bool statGain)
        {
            var e = new TestEnvironment();
            var self = e.Gain(e.Side, BoardRow.Front, statGain: statGain, selfSource: true);
            AssertIgnored(e, e.Handlers[0], self);
            var external = e.Gain(e.Side, BoardRow.Front);
            e.Handlers[0].Resolve(e.State, external);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void OpposingTarget_DoesNotTrigger(CombatSide side)
        {
            var e = new TestEnvironment(side);
            AssertIgnored(e, e.Handlers[0], e.Gain(e.OtherSide, BoardRow.Front));
        }

        [TestCase(BoardRow.Front)]
        [TestCase(BoardRow.Back)]
        public void OtherRow_DoesNotTrigger(BoardRow petRow)
        {
            var e = new TestEnvironment();
            var targetRow = petRow == BoardRow.Front ? BoardRow.Back : BoardRow.Front;
            AssertIgnored(e, e.Handler(petRow), e.Gain(e.Side, targetRow));
        }

        [Test]
        public void RepeatedResolve_DoesNotDuplicateAttackOrEvent()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            e.Handlers[0].Resolve(e.State, hpGain);
            var lastId = e.Ids.LastIssuedValue;
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(lastId));
        }

        [Test]
        public void HealAndStatGain_ShareOneUsagePerCard()
        {
            var e = new TestEnvironment();
            var heal = e.Gain(e.Side, BoardRow.Front);
            var stat = e.Gain(e.Side, BoardRow.Front, statGain: true);
            Assert.That(e.Handlers[0].CanTrigger(e.State, heal), Is.True);
            Assert.That(e.Handlers[0].CanTrigger(e.State, stat), Is.True);
            e.Handlers[0].Resolve(e.State, heal);
            e.Handlers[0].Resolve(e.State, stat);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void DifferentTargetInstances_HaveIndependentUsage()
        {
            var e = new TestEnvironment();
            var first = e.Gain(e.Side, BoardRow.Front);
            var second = e.Gain(e.Side, BoardRow.Front, column: 2);
            e.Handlers[0].Resolve(e.State, first);
            e.Handlers[0].Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void DifferentExternalSources_DoNotResetTargetUsage()
        {
            var e = new TestEnvironment();
            var first = e.Gain(e.Side, BoardRow.Front);
            var second = e.Gain(e.Side, BoardRow.Front, sourceId: e.Pets[0].InstanceId);
            e.Handlers[0].Resolve(e.State, first);
            e.Handlers[0].Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void TargetMovesWithinRow_BonusFollowsIdentity()
        {
            var e = new TestEnvironment();
            var target = e.Card(e.Side, BoardRow.Front, 1);
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            Assert.That(e.Handlers[0].CanTrigger(e.State, hpGain), Is.True);
            var destination = e.Position(e.Side, BoardRow.Front, 5);
            e.State.GetSide(e.Side).MoveCard(hpGain.TargetPosition, destination);
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Side, BoardRow.Front, 2), hpGain.TargetPosition);
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(target.Attack, Is.EqualTo(3));
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(2));
            Assert.That(e.AttackGains()[0].TargetPosition, Is.EqualTo(destination));
            Assert.That(e.AttackGains()[0].TargetInstanceId, Is.EqualTo(target.InstanceId));
        }

        [Test]
        public void TargetMovesToOtherRow_CurrentRowDeterminesEligiblePet()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            Assert.That(e.Handlers[0].CanTrigger(e.State, hpGain), Is.True);
            var destination = e.Position(e.Side, BoardRow.Back, 5);
            e.State.GetSide(e.Side).MoveCard(hpGain.TargetPosition, destination);
            AssertIgnored(e, e.Handlers[0], hpGain);
            Assert.That(e.Handlers[1].CanTrigger(e.State, hpGain), Is.True);
            e.Handlers[1].Resolve(e.State, hpGain);
            Assert.That(e.Usage.HasTriggered(e.Pets[0].InstanceId, hpGain.TargetInstanceId), Is.False);
            Assert.That(e.Usage.HasTriggered(e.Pets[1].InstanceId, hpGain.TargetInstanceId), Is.True);
            Assert.That(e.AttackGains()[0].TargetPosition, Is.EqualTo(destination));
        }

        [Test]
        public void TwoSameDefinitionPets_KeepIndependentUsageForSameCard()
        {
            var e = new TestEnvironment();
            var first = e.Gain(e.Side, BoardRow.Front);
            e.Handlers[0].Resolve(e.State, first);
            var destination = e.Position(e.Side, BoardRow.Back, 5);
            e.State.GetSide(e.Side).MoveCard(first.TargetPosition, destination);
            var second = e.Gain(e.Side, BoardRow.Back, column: 5);
            e.Handlers[1].Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Back, 5).Attack, Is.EqualTo(4));
            Assert.That(e.Usage.HasTriggered(e.Pets[0].InstanceId, first.TargetInstanceId), Is.True);
            Assert.That(e.Usage.HasTriggered(e.Pets[1].InstanceId, first.TargetInstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.AttackGains().Count, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RemovedTarget_DoesNotBuffReplacementOrConsumeUsage(bool removeFromRegistry)
        {
            var e = new TestEnvironment();
            var target = e.Card(e.Side, BoardRow.Front, 1);
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            var owner = e.State.GetSide(e.Side);
            if (removeFromRegistry) { owner.RemoveCardFromCombat(hpGain.TargetPosition); }
            else { owner.RemoveCard(hpGain.TargetPosition); }
            owner.MoveCard(e.Position(e.Side, BoardRow.Front, 2), hpGain.TargetPosition);
            AssertIgnored(e, e.Handlers[0], hpGain);
            Assert.That(target.Attack, Is.EqualTo(2));
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PositiveHpGainAtDeathThreshold_RemainsEligible(bool rescue)
        {
            var e = new TestEnvironment();
            var target = e.Card(e.Side, BoardRow.Front, 1);
            target.SetCurrentHpToZero();
            if (!rescue) { target.ApplyIncomingDamage(2); }
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            Assert.That(target.CurrentHp, Is.EqualTo(rescue ? 1 : -1));
            Assert.That(e.Handlers[0].CanTrigger(e.State, hpGain), Is.True);
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(target.Attack, Is.EqualTo(3));
            Assert.That(target.CurrentHp, Is.EqualTo(rescue ? 1 : -1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void TargetReachesThresholdAfterHpEvent_BoardOccupantStillReceivesBonus()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            var target = e.Card(e.Side, BoardRow.Front, 1);
            target.SetCurrentHpToZero();
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(target.Attack, Is.EqualTo(3));
            Assert.That(target.CurrentHp, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemovedHpSource_DoesNotCancelIndependentPetBonus()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            e.State.GetSide(e.OtherSide).RemoveCardFromCombat(e.Position(e.OtherSide, BoardRow.Front, 2));
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
        }

        [Test]
        public void AttackOverflow_PreservesStatsLogAndUsage_AndAllowsRetry()
        {
            var e = new TestEnvironment();
            var target = e.Card(e.Side, BoardRow.Front, 1);
            target.ApplyAttackGain(int.MaxValue - target.Attack);
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            Assert.Throws<OverflowException>(() => e.Handlers[0].Resolve(e.State, hpGain));
            Assert.That(target.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            target.ReduceAttack(int.MaxValue - 2);
            Assert.That(e.Handlers[0].CanTrigger(e.State, hpGain), Is.True);
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(target.Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void UnloggedParent_DoesNotConsumeUsage_AndAllowsRetryAfterAppend()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front, append: false);
            Assert.Throws<ArgumentException>(() => e.Handlers[0].Resolve(e.State, hpGain));
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            e.Log.Append(hpGain);
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
        }

        [Test]
        public void DifferentParentObjectWithSameMetadata_IsRejectedWithoutUsage()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            var impostor = new HpGainCombatEvent(hpGain.Metadata, hpGain.SourceInstanceId,
                hpGain.TargetInstanceId, hpGain.TargetPosition, hpGain.PreviousHpCapacity,
                hpGain.CurrentHpCapacity, hpGain.PreviousHp, hpGain.CurrentHp);
            Assert.Throws<ArgumentException>(() => e.Handlers[0].Resolve(e.State, impostor));
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
        }

        [Test]
        public void MetadataFactoryBehindParent_DoesNotMutateOrConsumeUsage()
        {
            var e = new TestEnvironment();
            var hpGain = e.Gain(e.Side, BoardRow.Front);
            var behind = new CombatEventMetadataFactory(new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            var resolver = new CombatAttackGainResolver(behind, e.Log);
            var handler = new HummingbirdPetHpGainTriggerHandler(e.Side, e.Pets[0].InstanceId, e.Usage, resolver);
            Assert.Throws<InvalidOperationException>(() => handler.Resolve(e.State, hpGain));
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            e.Handlers[0].Resolve(e.State, hpGain);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
        }

        [Test]
        public void RebuiltHandler_WithSharedUsageDoesNotRepeatBonus()
        {
            var e = new TestEnvironment();
            var first = e.Gain(e.Side, BoardRow.Front);
            e.Handlers[0].Resolve(e.State, first);
            var rebuilt = new HummingbirdPetHpGainTriggerHandler(e.Side, e.Pets[0].InstanceId, e.Usage, e.Resolver);
            var second = e.Gain(e.Side, BoardRow.Front);
            Assert.That(rebuilt.CanTrigger(e.State, second), Is.False);
            rebuilt.Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.AttackGains().Count, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void FreshBattleUsage_AllowsSamePetAndCardIdsAgain()
        {
            var first = new TestEnvironment();
            first.Handlers[0].Resolve(first.State, first.Gain(first.Side, BoardRow.Front));
            var next = new TestEnvironment();
            var hpGain = next.Gain(next.Side, BoardRow.Front);
            Assert.That(next.Handlers[0].CanTrigger(next.State, hpGain), Is.True);
            next.Handlers[0].Resolve(next.State, hpGain);
            Assert.That(first.Card(first.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(next.Card(next.Side, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(first.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(next.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        private static void AssertIgnored(TestEnvironment e, HummingbirdPetHpGainTriggerHandler handler, HpGainCombatEvent hpGain)
        {
            var logCount = e.Log.Count;
            var lastId = e.Ids.LastIssuedValue;
            Assert.That(handler.CanTrigger(e.State, hpGain), Is.False);
            handler.Resolve(e.State, hpGain);
            Assert.That(e.Log.Count, Is.EqualTo(logCount));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(lastId));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.AttackGains(), Is.Empty);
        }

        private sealed class TestEnvironment
        {
            public readonly CombatSide Side;
            public readonly CombatSide OtherSide;
            public readonly CombatState State;
            public readonly CombatPetState[] Pets;
            public readonly HummingbirdPetHpGainTriggerHandler[] Handlers;
            public readonly CombatEventIdAllocator Ids = new CombatEventIdAllocator();
            public readonly CombatEventMetadataFactory Factory;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatAttackGainResolver Resolver;
            public readonly CombatPetCardTriggerUsageCommitter Usage =
                new CombatPetCardTriggerUsageCommitter(new CombatPetCardTriggerUsageRegistry());

            public TestEnvironment(CombatSide side = CombatSide.Player, CombatCardSeason season = CombatCardSeason.Spring)
            {
                Side = side;
                OtherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Pets = new[]
                {
                    new CombatPetState(new DefinitionId("test.hummingbird"), new InstanceId(1002)),
                    new CombatPetState(new DefinitionId("test.hummingbird"), new InstanceId(1001))
                };
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(Pets));
                var otherPets = new CombatSidePetState(OtherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(MakeSide(CombatSide.Player, season), MakeSide(CombatSide.Enemy, season),
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Factory = new CombatEventMetadataFactory(Ids, new CombatSequenceNumberAllocator());
                Root = new CombatStartedCombatEvent(Factory.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAttackGainResolver(Factory, Log);
                Handlers = new[]
                {
                    new HummingbirdPetHpGainTriggerHandler(side, Pets[0].InstanceId, Usage, Resolver),
                    new HummingbirdPetHpGainTriggerHandler(side, Pets[1].InstanceId, Usage, Resolver)
                };
            }

            public BoardPosition Position(CombatSide side, BoardRow row, int column) =>
                new BoardPosition(side, row, new BoardColumn(column));
            public CombatCardState Card(CombatSide side, BoardRow row, int column) =>
                State.GetSide(side).GetCardAt(Position(side, row, column));
            public HummingbirdPetHpGainTriggerHandler Handler(BoardRow row) => Handlers[row == BoardRow.Front ? 0 : 1];

            public HpGainCombatEvent Gain(CombatSide side, BoardRow row, int column = 1,
                bool statGain = false, bool selfSource = false, bool append = true, InstanceId? sourceId = null)
            {
                var target = Card(side, row, column);
                var previousHp = target.CurrentHp;
                var previousCapacity = target.HpCapacity;
                if (statGain) { target.ApplyHpStatGain(1); }
                else { target.Heal(1); }
                // Handler fixture: apply the stat operation and supply its semantic
                // event with explicit provenance. HP resolver integration is separate.
                var source = selfSource ? target.InstanceId :
                    sourceId ?? Card(OtherSide, BoardRow.Front, 2).InstanceId;
                var gain = new HpGainCombatEvent(Factory.CreateChild(Root.Metadata), source,
                    target.InstanceId, Position(side, row, column), previousCapacity,
                    target.HpCapacity, previousHp, target.CurrentHp);
                if (append) { Log.Append(gain); }
                return gain;
            }

            public List<AttackGainCombatEvent> AttackGains()
            {
                var gains = new List<AttackGainCombatEvent>();
                foreach (var item in Log.Events) { if (item is AttackGainCombatEvent gain) { gains.Add(gain); } }
                return gains;
            }

            private CombatSideState MakeSide(CombatSide side, CombatCardSeason season)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var id = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        CombatCardState card = null;
                        if (column <= 2)
                        {
                            card = new CombatCardState(new DefinitionId("test.hummingbird_card"), new InstanceId(id),
                                new CardRank(4), season, hpCapacity: 10, currentHp: 5, armor: 0, attack: 2);
                            cards.Add(card);
                        }
                        slots.Add(new CombatSlotState(new SlotId(id), Position(side, row, column),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
