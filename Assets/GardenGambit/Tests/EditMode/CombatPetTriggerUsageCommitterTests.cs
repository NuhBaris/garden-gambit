using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerUsageCommitterTests
    {
        [Test]
        public void Constructor_WithNullRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatPetTriggerUsageCommitter(null));
        }

        [Test]
        public void SharedRegistry_PreventsSecondCommitterFromRepeatingEffect()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var first = new CombatPetTriggerUsageCommitter(registry);
            var second = new CombatPetTriggerUsageCommitter(registry);
            var petId = new InstanceId(1001);
            var calls = 0;

            Assert.That(first.UsageRegistry, Is.SameAs(registry));
            Assert.That(second.UsageRegistry, Is.SameAs(registry));
            Assert.That(first.TryCommit(petId, () => calls++), Is.True);
            Assert.That(second.HasTriggered(petId), Is.True);
            Assert.That(second.TryCommit(petId, () => calls++), Is.False);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryCommit_RegistersOnlyAfterCallbackCompletes()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(registry);
            var petId = new InstanceId(1001);
            var calls = 0;

            Assert.That(committer.HasTriggered(petId), Is.False);
            var committed = committer.TryCommit(petId, () =>
            {
                Assert.That(committer.HasTriggered(petId), Is.False);
                Assert.That(registry.Count, Is.Zero);
                calls++;
            });

            Assert.That(committed, Is.True);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(committer.HasTriggered(petId), Is.True);
            Assert.That(registry.PetInstanceIds, Is.EqualTo(new[] { petId }));
        }

        [Test]
        public void TryCommit_WhenCallbackThrows_LeavesUsageAvailableForRetry()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(registry);
            var petId = new InstanceId(1001);
            var failure = new TestResolutionException();
            var calls = 0;

            var thrown = Assert.Throws<TestResolutionException>(() => committer.TryCommit(petId, () =>
            {
                calls++;
                throw failure;
            }));

            Assert.That(thrown, Is.SameAs(failure));
            Assert.That(committer.HasTriggered(petId), Is.False);
            Assert.That(registry.Count, Is.Zero);
            Assert.That(committer.TryCommit(petId, () => calls++), Is.True);
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryCommit_WithInvalidInstanceId_ThrowsBeforeCallback()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(registry);
            var calls = 0;

            Assert.Throws<ArgumentException>(() => committer.TryCommit(
                default(InstanceId), () => calls++));
            Assert.Throws<ArgumentException>(
                () => committer.HasTriggered(default(InstanceId)));
            Assert.That(calls, Is.Zero);
            Assert.That(registry.Count, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TryCommit_WithNullCallback_ThrowsForUnusedAndUsedPet(bool alreadyUsed)
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(registry);
            var petId = new InstanceId(1001);
            if (alreadyUsed)
            {
                registry.TryRegister(petId);
            }

            Assert.Throws<ArgumentNullException>(() => committer.TryCommit(petId, null));
            Assert.That(registry.Count, Is.EqualTo(alreadyUsed ? 1 : 0));
            Assert.That(committer.HasTriggered(petId), Is.EqualTo(alreadyUsed));
        }

        [Test]
        public void SameDefinitionDifferentInstances_HaveIndependentUsage()
        {
            var definitionId = new DefinitionId("test.once_pet");
            var firstPet = new CombatPetState(definitionId, new InstanceId(1001));
            var secondPet = new CombatPetState(definitionId, new InstanceId(1002));
            var registry = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(registry);
            var calls = 0;

            Assert.That(committer.TryCommit(firstPet.InstanceId, () => calls++), Is.True);
            Assert.That(committer.HasTriggered(secondPet.InstanceId), Is.False);
            Assert.That(committer.TryCommit(secondPet.InstanceId, () => calls++), Is.True);
            Assert.That(committer.TryCommit(firstPet.InstanceId, () => calls++), Is.False);
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(registry.Count, Is.EqualTo(2));
        }

        [Test]
        public void TryCommit_WhenCallbackRegistersSamePet_ReportsRegistrationConflict()
        {
            var registry = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(registry);
            var petId = new InstanceId(1001);

            Assert.Throws<InvalidOperationException>(() => committer.TryCommit(
                petId, () => registry.TryRegister(petId)));
            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(registry.PetInstanceIds, Is.EqualTo(new[] { petId }));
        }

        [Test]
        public void AttackGainCallback_UsedPetSkipsRepeatButAnotherPetCanApplyGain()
        {
            var environment = CreateAttackEnvironment(attack: 2);
            var usage = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(usage);
            var firstPet = new InstanceId(1001);
            var secondPet = new InstanceId(1002);

            Assert.That(committer.TryCommit(firstPet, () => ApplyAttack(environment)), Is.True);
            var firstGain = (AttackGainCombatEvent)environment.EventLog.Events[1];
            Assert.That(committer.TryCommit(firstPet, () => ApplyAttack(environment)), Is.False);
            Assert.That(environment.Card.Attack, Is.EqualTo(3));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));

            Assert.That(committer.TryCommit(secondPet, () => ApplyAttack(environment)), Is.True);
            var secondGain = (AttackGainCombatEvent)environment.EventLog.Events[2];
            Assert.That(environment.Card.Attack, Is.EqualTo(4));
            Assert.That(environment.EventLog.Count, Is.EqualTo(3));
            Assert.That(firstGain.Metadata.EventId.Value, Is.EqualTo(2));
            Assert.That(secondGain.Metadata.EventId.Value, Is.EqualTo(3));
            Assert.That(firstGain.TargetInstanceId, Is.EqualTo(environment.Card.InstanceId));
            Assert.That(secondGain.TargetInstanceId, Is.EqualTo(environment.Card.InstanceId));
            Assert.That(usage.PetInstanceIds, Is.EqualTo(new[] { firstPet, secondPet }));
        }

        [Test]
        public void AttackGainOverflow_DoesNotConsumeUsageAndRetryUsesNextEventId()
        {
            var environment = CreateAttackEnvironment(attack: int.MaxValue);
            var usage = new CombatPetTriggerUsageRegistry();
            var committer = new CombatPetTriggerUsageCommitter(usage);
            var petId = new InstanceId(1001);

            Assert.Throws<OverflowException>(() => committer.TryCommit(
                petId, () => ApplyAttack(environment)));
            Assert.That(committer.HasTriggered(petId), Is.False);
            Assert.That(usage.Count, Is.Zero);
            Assert.That(environment.Card.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(environment.EventLog.Count, Is.EqualTo(1));

            environment.Card.ReduceAttack(1);
            Assert.That(committer.TryCommit(petId, () => ApplyAttack(environment)), Is.True);
            Assert.That(committer.HasTriggered(petId), Is.True);
            Assert.That(environment.Card.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));
            Assert.That(environment.EventLog.Events[1].Metadata.EventId.Value, Is.EqualTo(2));
            Assert.That(environment.EventLog.Events[1].Metadata.SequenceNo.Value, Is.EqualTo(2));
        }

        private static AttackGainCombatEvent ApplyAttack(AttackEnvironment environment)
        {
            return environment.Resolver.TryApplyAttackGain(
                environment.State, environment.ParentEvent, environment.Position, 1);
        }

        private static AttackEnvironment CreateAttackEnvironment(int attack)
        {
            var position = new BoardPosition(CombatSide.Player, BoardRow.Front, new BoardColumn(1));
            var card = new CombatCardState(
                new DefinitionId("test.once_target"), new InstanceId(1), new CardRank(2),
                CombatCardSeason.Autumn, hpCapacity: 10, currentHp: 10, armor: 0, attack: attack);
            var player = new CombatSideState(
                new CombatBoardState(CombatSide.Player,
                    new[] { new CombatSlotState(new SlotId(1), position, card.InstanceId) }),
                new CombatCardRegistry(new[] { card }),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));
            var enemy = new CombatSideState(
                new CombatBoardState(CombatSide.Enemy, Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(Array.Empty<CombatCardState>()),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));
            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            var eventLog = new CombatEventLog();
            var parent = new CombatStartedCombatEvent(metadataFactory.CreateRoot());
            eventLog.Append(parent);

            return new AttackEnvironment
            {
                State = new CombatState(player, enemy),
                Card = card,
                Position = position,
                ParentEvent = parent,
                EventLog = eventLog,
                Resolver = new CombatAttackGainResolver(metadataFactory, eventLog)
            };
        }

        private sealed class AttackEnvironment
        {
            public CombatState State { get; set; }
            public CombatCardState Card { get; set; }
            public BoardPosition Position { get; set; }
            public CombatStartedCombatEvent ParentEvent { get; set; }
            public CombatEventLog EventLog { get; set; }
            public CombatAttackGainResolver Resolver { get; set; }
        }

        private sealed class TestResolutionException : Exception
        {
        }
    }
}
