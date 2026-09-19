using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatStartResolverPokerTests
    {
        [Test]
        public void ExplicitStart_TransfersAllLockedPokerHands()
        {
            var environment = new Environment();

            var started = environment.Resolver.Start(
                environment.State,
                CombatPokerHand.HighCard,
                CombatPokerHand.Pair,
                CombatPokerHand.StraightFlush,
                CombatPokerHand.FlushFive);

            Assert.That(started.HasBattleStartSnapshot, Is.True);
            Assert.That(
                started.BattleStartSnapshot.Player
                    .FrontPokerHand,
                Is.EqualTo(CombatPokerHand.HighCard));
            Assert.That(
                started.BattleStartSnapshot.Player
                    .BackPokerHand,
                Is.EqualTo(CombatPokerHand.Pair));
            Assert.That(
                started.BattleStartSnapshot.Enemy
                    .FrontPokerHand,
                Is.EqualTo(CombatPokerHand.StraightFlush));
            Assert.That(
                started.BattleStartSnapshot.Enemy
                    .BackPokerHand,
                Is.EqualTo(CombatPokerHand.FlushFive));
            Assert.That(started.Metadata.IsTriggerRoot, Is.True);
            Assert.That(environment.Log.Count, Is.EqualTo(1));
            Assert.That(
                environment.Log.Events[0],
                Is.SameAs(started));
            Assert.That(
                environment.EventIds.LastIssuedValue,
                Is.EqualTo(1));
        }

        [Test]
        public void LegacyStart_UsesUnspecifiedPokerHands()
        {
            var environment = new Environment();

            var started =
                environment.Resolver.Start(
                    environment.State);

            Assert.That(
                started.BattleStartSnapshot.Player
                    .FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                started.BattleStartSnapshot.Player
                    .BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                started.BattleStartSnapshot.Enemy
                    .FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                started.BattleStartSnapshot.Enemy
                    .BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(environment.Log.Count, Is.EqualTo(1));
        }

        [TestCase(0, -1)]
        [TestCase(0, 13)]
        [TestCase(1, -1)]
        [TestCase(1, 13)]
        [TestCase(2, -1)]
        [TestCase(2, 13)]
        [TestCase(3, -1)]
        [TestCase(3, 13)]
        public void ExplicitStart_RejectsInvalidPokerHandAtomically(
            int parameterIndex,
            int rawPokerHand)
        {
            var environment = new Environment();
            var hands = new[]
            {
                CombatPokerHand.HighCard,
                CombatPokerHand.Pair,
                CombatPokerHand.TwoPair,
                CombatPokerHand.Flush
            };

            hands[parameterIndex] =
                (CombatPokerHand)rawPokerHand;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                environment.Resolver.Start(
                    environment.State,
                    hands[0],
                    hands[1],
                    hands[2],
                    hands[3]));

            Assert.That(environment.Log.Count, Is.Zero);
            Assert.That(
                environment.Log.CardTombstones.Count,
                Is.Zero);
            Assert.That(
                environment.EventIds.LastIssuedValue,
                Is.Zero);
            Assert.That(
                environment.SequenceNumbers.LastIssuedValue,
                Is.Zero);
        }

        [Test]
        public void ExplicitStart_WithNullState_ThrowsWithoutMutation()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                environment.Resolver.Start(
                    null,
                    CombatPokerHand.HighCard,
                    CombatPokerHand.Pair,
                    CombatPokerHand.TwoPair,
                    CombatPokerHand.Flush));

            Assert.That(environment.Log.Count, Is.Zero);
            Assert.That(
                environment.EventIds.LastIssuedValue,
                Is.Zero);
            Assert.That(
                environment.SequenceNumbers.LastIssuedValue,
                Is.Zero);
        }

        [Test]
        public void ExplicitStart_WithExistingHistoryThrowsWithoutAppending()
        {
            var environment = new Environment();
            var existing =
                new TestCombatEvent(
                    environment.Metadata.CreateRoot());

            environment.Log.Append(existing);

            Assert.Throws<InvalidOperationException>(() =>
                environment.Resolver.Start(
                    environment.State,
                    CombatPokerHand.HighCard,
                    CombatPokerHand.Pair,
                    CombatPokerHand.TwoPair,
                    CombatPokerHand.Flush));

            Assert.That(environment.Log.Count, Is.EqualTo(1));
            Assert.That(
                environment.Log.Events[0],
                Is.SameAs(existing));
            Assert.That(
                environment.EventIds.LastIssuedValue,
                Is.EqualTo(1));
            Assert.That(
                environment.SequenceNumbers.LastIssuedValue,
                Is.EqualTo(1));
        }

        private sealed class Environment
        {
            public readonly CombatEventIdAllocator EventIds =
                new CombatEventIdAllocator();
            public readonly CombatSequenceNumberAllocator
                SequenceNumbers =
                    new CombatSequenceNumberAllocator();
            public readonly CombatEventMetadataFactory Metadata;
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatState State;
            public readonly CombatStartResolver Resolver;

            public Environment()
            {
                Metadata = new CombatEventMetadataFactory(
                    EventIds,
                    SequenceNumbers);
                State = CreateState();
                Resolver = new CombatStartResolver(
                    Metadata,
                    Log);
            }
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSide(CombatSide.Player, 1),
                CreateSide(CombatSide.Enemy, 101));
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            long instanceId)
        {
            var position = new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
            var card = new CombatCardState(
                new DefinitionId(
                    "test.combat_start_poker.card"),
                new InstanceId(instanceId),
                new CardRank(2),
                5,
                5,
                0,
                2);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(instanceId),
                            position,
                            card.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private sealed class TestCombatEvent : CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }
    }
}
