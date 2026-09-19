using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatAttackGainResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatAttackGainResolver(null, new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatAttackGainResolver(CreateMetadataFactory(), null));
        }

        [Test]
        public void TryApplyAttackGain_WithNullStateOrParent_ThrowsWithoutMutation()
        {
            var environment = CreateEnvironment();

            Assert.Throws<ArgumentNullException>(() => environment.Resolver.TryApplyAttackGain(
                null, environment.ParentEvent, environment.Position, 1));
            Assert.Throws<ArgumentNullException>(() => environment.Resolver.TryApplyAttackGain(
                environment.State, null, environment.Position, 1));

            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void TryApplyAttackGain_WithPositiveAmount_MutatesAndAppendsEvent(CombatSide side)
        {
            var environment = CreateEnvironment(side: side);
            var gain = Apply(environment, 3);

            Assert.That(gain, Is.Not.Null);
            Assert.That(environment.Card.Attack, Is.EqualTo(5));
            Assert.That(environment.Card.Armor, Is.EqualTo(4));
            Assert.That(environment.Card.CurrentHp, Is.EqualTo(7));
            Assert.That(environment.Card.HpCapacity, Is.EqualTo(10));
            Assert.That(gain.Kind, Is.EqualTo(CombatEventKind.AttackGain));
            Assert.That(gain.TargetInstanceId, Is.EqualTo(environment.Card.InstanceId));
            Assert.That(gain.TargetPosition, Is.EqualTo(environment.Position));
            Assert.That(gain.TargetSide, Is.EqualTo(side));
            Assert.That(gain.PreviousAttack, Is.EqualTo(2));
            Assert.That(gain.CurrentAttack, Is.EqualTo(5));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(3));
            Assert.That(gain.Metadata.ParentEventId.Value,
                Is.EqualTo(environment.ParentEvent.Metadata.EventId));
            Assert.That(gain.Metadata.TriggerRootId,
                Is.EqualTo(environment.ParentEvent.Metadata.TriggerRootId));
            Assert.That(gain.Metadata.EventId.Value, Is.EqualTo(2));
            Assert.That(gain.Metadata.SequenceNo.Value, Is.EqualTo(2));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));
            Assert.That(environment.EventLog.Events[1], Is.SameAs(gain));
        }

        [Test]
        public void TryApplyAttackGain_WithZeroAmount_DoesNotMutateEmitOrAllocate()
        {
            var environment = CreateEnvironment();

            Assert.That(Apply(environment, 0), Is.Null);
            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [Test]
        public void TryApplyAttackGain_WithNegativeAmount_ThrowsWithoutMutation()
        {
            var environment = CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(() => Apply(environment, -1));
            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [Test]
        public void TryApplyAttackGain_WithInvalidPosition_ThrowsWithoutMutation()
        {
            var environment = CreateEnvironment();

            Assert.Throws<ArgumentException>(() => environment.Resolver.TryApplyAttackGain(
                environment.State, environment.ParentEvent, default(BoardPosition), 1));
            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [Test]
        public void TryApplyAttackGain_WithUnloggedParent_ThrowsWithoutMutation()
        {
            var environment = CreateEnvironment();
            var rootId = new CombatEventId(100);
            var unloggedParent = new CombatStartedCombatEvent(new CombatEventMetadata(
                rootId, new CombatSequenceNumber(100), null, rootId));

            Assert.Throws<ArgumentException>(() => environment.Resolver.TryApplyAttackGain(
                environment.State, unloggedParent, environment.Position, 1));
            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [Test]
        public void TryApplyAttackGain_WithDifferentParentReference_ThrowsWithoutMutation()
        {
            var environment = CreateEnvironment();
            var differentReference = new CombatStartedCombatEvent(environment.ParentEvent.Metadata);

            Assert.Throws<ArgumentException>(() => environment.Resolver.TryApplyAttackGain(
                environment.State, differentReference, environment.Position, 1));
            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [Test]
        public void TryApplyAttackGain_WhenAttackWouldOverflow_ThrowsWithoutMutationOrAllocation()
        {
            var environment = CreateEnvironment(attack: int.MaxValue);

            Assert.Throws<OverflowException>(() => Apply(environment, 1));
            AssertRejectedRequestDidNotChangeState(environment, int.MaxValue);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void TryApplyAttackGain_WithEmptySlot_ThrowsWithoutMutation(int amount)
        {
            var environment = CreateEnvironment();
            environment.State.GetSide(environment.Position.Side).RemoveCard(environment.Position);

            Assert.Throws<InvalidOperationException>(() => Apply(environment, amount));
            AssertRejectedRequestDidNotChangeState(environment, 2);
        }

        [Test]
        public void TryApplyAttackGain_WithChildParent_PreservesParentAndRoot()
        {
            var environment = CreateEnvironment();
            var hpGain = new CombatHpGainResolver(environment.MetadataFactory, environment.EventLog)
                .TryApplyHpStatGain(environment.State, environment.ParentEvent, environment.Position, 1);

            var gain = environment.Resolver.TryApplyAttackGain(
                environment.State, hpGain, environment.Position, 1);

            Assert.That(environment.Card.Attack, Is.EqualTo(3));
            Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(hpGain.Metadata.EventId));
            Assert.That(gain.Metadata.TriggerRootId,
                Is.EqualTo(environment.ParentEvent.Metadata.TriggerRootId));
            Assert.That(gain.Metadata.EventId.Value, Is.EqualTo(3));
            Assert.That(gain.Metadata.SequenceNo.Value, Is.EqualTo(3));
            Assert.That(environment.EventLog.Count, Is.EqualTo(3));
            Assert.That(environment.EventLog.Events[2], Is.SameAs(gain));
        }

        [Test]
        public void TryApplyAttackGain_WhenLogRejectsDuplicateEvent_DoesNotMutateCard()
        {
            var environment = CreateEnvironment();
            var staleFactory = CreateMetadataFactory();
            staleFactory.CreateRoot();
            var staleResolver = new CombatAttackGainResolver(staleFactory, environment.EventLog);
            var firstGain = Apply(environment, 1);

            Assert.Throws<ArgumentException>(() => staleResolver.TryApplyAttackGain(
                environment.State, environment.ParentEvent, environment.Position, 5));

            Assert.That(environment.Card.Attack, Is.EqualTo(3));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));
            Assert.That(environment.EventLog.Events[1], Is.SameAs(firstGain));

            var nextGain = Apply(environment, 1);
            Assert.That(environment.Card.Attack, Is.EqualTo(4));
            Assert.That(nextGain.Metadata.EventId.Value, Is.EqualTo(3));
            Assert.That(environment.EventLog.Count, Is.EqualTo(3));
        }

        [Test]
        public void TryApplyAttackGain_UpToIntMaxValue_Succeeds()
        {
            var environment = CreateEnvironment(attack: int.MaxValue - 1);
            var gain = Apply(environment, 1);

            Assert.That(environment.Card.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(gain.CurrentAttack, Is.EqualTo(int.MaxValue));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
            Assert.That(environment.EventLog.Events[1], Is.SameAs(gain));
        }

        [Test]
        public void TryApplyAttackGain_WithCardAtDeathThreshold_DoesNotApplyLivingFilter()
        {
            var environment = CreateEnvironment();
            environment.Card.SetCurrentHpToZero();
            var gain = Apply(environment, 1);

            Assert.That(environment.Card.Attack, Is.EqualTo(3));
            Assert.That(environment.Card.CurrentHp, Is.Zero);
            Assert.That(environment.Card.IsAtDeathThreshold, Is.True);
            Assert.That(gain.TargetInstanceId, Is.EqualTo(environment.Card.InstanceId));
            Assert.That(environment.EventLog.Events[1], Is.SameAs(gain));
        }

        [Test]
        public void TryApplyAttackGain_CalledTwice_RecordsSeparateSequentialGains()
        {
            var environment = CreateEnvironment();
            var first = Apply(environment, 1);
            var second = Apply(environment, 2);

            Assert.That(environment.Card.Attack, Is.EqualTo(5));
            Assert.That(first.PreviousAttack, Is.EqualTo(2));
            Assert.That(first.CurrentAttack, Is.EqualTo(3));
            Assert.That(second.PreviousAttack, Is.EqualTo(3));
            Assert.That(second.CurrentAttack, Is.EqualTo(5));
            Assert.That(first.Metadata.EventId.Value, Is.EqualTo(2));
            Assert.That(second.Metadata.EventId.Value, Is.EqualTo(3));
            Assert.That(first.Metadata.SequenceNo.Value, Is.EqualTo(2));
            Assert.That(second.Metadata.SequenceNo.Value, Is.EqualTo(3));
            Assert.That(environment.EventLog.Count, Is.EqualTo(3));
            Assert.That(environment.EventLog.Events[1], Is.SameAs(first));
            Assert.That(environment.EventLog.Events[2], Is.SameAs(second));
        }

        private static AttackGainCombatEvent Apply(TestEnvironment environment, int amount)
        {
            return environment.Resolver.TryApplyAttackGain(
                environment.State, environment.ParentEvent, environment.Position, amount);
        }

        private static void AssertRejectedRequestDidNotChangeState(
            TestEnvironment environment, int expectedAttack)
        {
            Assert.That(environment.Card.Attack, Is.EqualTo(expectedAttack));
            Assert.That(environment.Card.Armor, Is.EqualTo(4));
            Assert.That(environment.Card.CurrentHp, Is.EqualTo(7));
            Assert.That(environment.Card.HpCapacity, Is.EqualTo(10));
            Assert.That(environment.EventLog.Count, Is.EqualTo(1));
            Assert.That(environment.EventLog.Events[0], Is.SameAs(environment.ParentEvent));

            var nextMetadata = environment.MetadataFactory.CreateChild(environment.ParentEvent.Metadata);
            Assert.That(nextMetadata.EventId.Value, Is.EqualTo(2));
            Assert.That(nextMetadata.SequenceNo.Value, Is.EqualTo(2));
        }

        private static CombatEventMetadataFactory CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
        }

        private static TestEnvironment CreateEnvironment(
            int attack = 2, CombatSide side = CombatSide.Player)
        {
            var position = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
            var card = new CombatCardState(
                new DefinitionId("test.attack_gain_card"), new InstanceId(1), new CardRank(5),
                CombatCardSeason.Autumn, hpCapacity: 10, currentHp: 7, armor: 4, attack: attack);

            var owner = new CombatSideState(
                new CombatBoardState(side, new[] { new CombatSlotState(new SlotId(1), position, card.InstanceId) }),
                new CombatCardRegistry(new[] { card }),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));

            var opposingSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
            var other = new CombatSideState(
                new CombatBoardState(opposingSide, Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(Array.Empty<CombatCardState>()),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));

            var state = new CombatState(
                side == CombatSide.Player ? owner : other,
                side == CombatSide.Enemy ? owner : other);
            var metadataFactory = CreateMetadataFactory();
            var eventLog = new CombatEventLog();
            var parent = new CombatStartedCombatEvent(metadataFactory.CreateRoot());
            eventLog.Append(parent);

            return new TestEnvironment
            {
                State = state,
                Card = card,
                Position = position,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                ParentEvent = parent,
                Resolver = new CombatAttackGainResolver(metadataFactory, eventLog)
            };
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }
            public CombatCardState Card { get; set; }
            public BoardPosition Position { get; set; }
            public CombatEventMetadataFactory MetadataFactory { get; set; }
            public CombatEventLog EventLog { get; set; }
            public CombatStartedCombatEvent ParentEvent { get; set; }
            public CombatAttackGainResolver Resolver { get; set; }
        }
    }
}
