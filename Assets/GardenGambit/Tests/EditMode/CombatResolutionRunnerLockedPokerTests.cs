using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatResolutionRunnerLockedPokerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front, 3)]
        [TestCase(CombatSide.Player, BoardRow.Back, 2)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 3)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 2)]
        public void FullCombat_TransfersLockedPokerAndAppliesJackalope(
            CombatSide side,
            BoardRow row,
            int expectedBonus)
        {
            var environment = new Environment(side, row);

            var completed = environment.StartWithLockedPoker();

            Assert.That(completed, Is.Not.Null);
            Assert.That(
                completed.Outcome,
                Is.EqualTo(
                    side == CombatSide.Player
                        ? CombatOutcome.PlayerVictory
                        : CombatOutcome.EnemyVictory));
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(
                        environment.Card(row).InstanceId),
                Is.EqualTo(expectedBonus));
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(
                        environment.Card(OtherRow(row))
                            .InstanceId),
                Is.Zero);
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));

            var started = environment.StartedEvent();
            var ownSnapshot =
                started.BattleStartSnapshot.GetSide(side);

            Assert.That(
                ownSnapshot.GetPokerHand(row),
                Is.EqualTo(
                    row == BoardRow.Front
                        ? CombatPokerHand.HighCard
                        : CombatPokerHand.Pair));
            Assert.That(
                ownSnapshot.GetPokerHand(OtherRow(row)),
                Is.EqualTo(CombatPokerHand.FlushFive));
            Assert.That(environment.Runner.HasActiveCombat, Is.False);
            Assert.That(environment.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void LegacyStart_UsesUnspecifiedAndDoesNotTriggerJackalope()
        {
            var environment = new Environment();

            var completed = environment.Runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(completed, Is.Not.Null);

            var snapshot =
                environment.StartedEvent().BattleStartSnapshot;

            Assert.That(
                snapshot.Player.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.Player.BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.Enemy.FrontPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                snapshot.Enemy.BackPokerHand,
                Is.EqualTo(CombatPokerHand.Unspecified));
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .Count,
                Is.Zero);
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void ExplicitStagedStart_TransfersLockedPoker()
        {
            var environment = new Environment();
            var hands = environment.LockedPokerHands();

            var completed =
                environment.Runner.StartAndResolveCombatStaged(
                    maximumExchangeCountPerColumn: 20,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100,
                    playerFrontPokerHand: hands[0],
                    playerBackPokerHand: hands[1],
                    enemyFrontPokerHand: hands[2],
                    enemyBackPokerHand: hands[3]);

            Assert.That(completed, Is.Not.Null);
            Assert.That(
                environment.StartedEvent()
                    .BattleStartSnapshot
                    .GetSide(environment.Side)
                    .GetPokerHand(environment.QualifiedRow),
                Is.EqualTo(CombatPokerHand.HighCard));
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(
                        environment.Card(
                            environment.QualifiedRow)
                            .InstanceId),
                Is.EqualTo(3));
        }

        [TestCase(0, -1)]
        [TestCase(0, 13)]
        [TestCase(1, -1)]
        [TestCase(1, 13)]
        [TestCase(2, -1)]
        [TestCase(2, 13)]
        [TestCase(3, -1)]
        [TestCase(3, 13)]
        public void InvalidLockedPoker_LeavesCombatUnstarted(
            int parameterIndex,
            int rawPokerHand)
        {
            var environment = new Environment();
            var hands = environment.LockedPokerHands();

            hands[parameterIndex] =
                (CombatPokerHand)rawPokerHand;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                environment.Runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 20,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100,
                    playerFrontPokerHand: hands[0],
                    playerBackPokerHand: hands[1],
                    enemyFrontPokerHand: hands[2],
                    enemyBackPokerHand: hands[3]));

            Assert.That(environment.Runner.HasActiveCombat, Is.False);
            Assert.That(environment.Log.Count, Is.Zero);
            Assert.That(environment.Queue.PendingCount, Is.Zero);
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .Count,
                Is.Zero);
        }

        [Test]
        public void BattleEndOverflowResume_PreservesLockedSnapshot()
        {
            var environment = new Environment();
            var target =
                environment.Card(
                    environment.QualifiedRow);

            environment.Runtime.FinalRankModifierRegistry
                .AddModifier(
                    target.InstanceId,
                    int.MaxValue);

            Assert.Throws<OverflowException>(() =>
                environment.StartWithLockedPoker());

            Assert.That(environment.Runner.HasActiveCombat, Is.True);
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
            Assert.That(
                environment.Find<CombatStartedCombatEvent>()
                    .Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Find<BattleEndStartedCombatEvent>()
                    .Count,
                Is.EqualTo(1));
            Assert.That(
                environment.StartedEvent()
                    .BattleStartSnapshot
                    .GetSide(environment.Side)
                    .GetPokerHand(environment.QualifiedRow),
                Is.EqualTo(CombatPokerHand.HighCard));

            environment.Runtime.FinalRankModifierRegistry
                .AddModifier(
                    target.InstanceId,
                    -int.MaxValue);

            var completed =
                environment.Runner.ResumeActiveCombat(
                    maximumExchangeCountPerColumn: 20,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(completed, Is.Not.Null);
            Assert.That(environment.Runner.HasActiveCombat, Is.False);
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(target.InstanceId),
                Is.EqualTo(3));
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Find<CombatStartedCombatEvent>()
                    .Count,
                Is.EqualTo(1));
            Assert.That(
                environment.Find<BattleEndStartedCombatEvent>()
                    .Count,
                Is.EqualTo(1));
            Assert.That(environment.Queue.PendingCount, Is.Zero);
        }

        private static BoardRow OtherRow(
            BoardRow row)
        {
            return row == BoardRow.Front
                ? BoardRow.Back
                : BoardRow.Front;
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow QualifiedRow;
            public readonly CombatPetTriggerRuntime Runtime =
                new CombatPetTriggerRuntime();
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatEventQueue Queue;
            public readonly CombatState State;
            public readonly CombatResolutionRunner Runner;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow qualifiedRow = BoardRow.Front)
            {
                Side = side;
                QualifiedRow = qualifiedRow;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;
                var own = CreateSide(
                    side,
                    frontCardCount: 1,
                    backCardCount: 1,
                    isOpponent: false);
                var opposing = CreateSide(
                    opposingSide,
                    frontCardCount: 1,
                    backCardCount: 0,
                    isOpponent: true);
                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[]
                        {
                            new CombatPetState(
                                CombatPetDefinitionIds.Jackalope,
                                new InstanceId(1001)),
                            new CombatPetState(
                                CombatPetDefinitionIds.Jackalope,
                                new InstanceId(1002))
                        }));
                var opposingPets =
                    new CombatSidePetState(
                        opposingSide,
                        new CombatPetRegistry(
                            Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? own
                        : opposing,
                    side == CombatSide.Enemy
                        ? own
                        : opposing,
                    side == CombatSide.Player
                        ? ownPets
                        : opposingPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : opposingPets);

                Queue = new CombatEventQueue(Log);
                Runner = Runtime.CreateResolutionRunner(
                    State,
                    Metadata,
                    Log,
                    Queue);
            }

            public CombatCompletedCombatEvent StartWithLockedPoker()
            {
                var hands = LockedPokerHands();

                return Runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 20,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100,
                    playerFrontPokerHand: hands[0],
                    playerBackPokerHand: hands[1],
                    enemyFrontPokerHand: hands[2],
                    enemyBackPokerHand: hands[3]);
            }

            public CombatPokerHand[] LockedPokerHands()
            {
                var ownFront =
                    QualifiedRow == BoardRow.Front
                        ? CombatPokerHand.HighCard
                        : CombatPokerHand.FlushFive;
                var ownBack =
                    QualifiedRow == BoardRow.Back
                        ? CombatPokerHand.Pair
                        : CombatPokerHand.FlushFive;

                return new[]
                {
                    Side == CombatSide.Player
                        ? ownFront
                        : CombatPokerHand.FlushFive,
                    Side == CombatSide.Player
                        ? ownBack
                        : CombatPokerHand.FlushFive,
                    Side == CombatSide.Enemy
                        ? ownFront
                        : CombatPokerHand.FlushFive,
                    Side == CombatSide.Enemy
                        ? ownBack
                        : CombatPokerHand.FlushFive
                };
            }

            public CombatCardState Card(
                BoardRow row)
            {
                return State.GetSide(Side).GetCardAt(
                    new BoardPosition(
                        Side,
                        row,
                        new BoardColumn(1)));
            }

            public CombatStartedCombatEvent StartedEvent()
            {
                return Find<CombatStartedCombatEvent>()[0];
            }

            public List<TEvent> Find<TEvent>()
                where TEvent : CombatEvent
            {
                var result = new List<TEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    var typed = combatEvent as TEvent;

                    if (typed != null)
                    {
                        result.Add(typed);
                    }
                }

                return result;
            }

            private static CombatSideState CreateSide(
                CombatSide side,
                int frontCardCount,
                int backCardCount,
                bool isOpponent)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();

                for (var column = 1;
                     column <= 5;
                     column++)
                {
                    foreach (var row in
                             new[]
                             {
                                 BoardRow.Front,
                                 BoardRow.Back
                             })
                    {
                        var instanceId =
                            (side == CombatSide.Player
                                ? 0
                                : 100) +
                            (row == BoardRow.Front
                                ? 0
                                : 5) +
                            column;
                        var requiredCount =
                            row == BoardRow.Front
                                ? frontCardCount
                                : backCardCount;
                        CombatCardState card = null;

                        if (column <= requiredCount)
                        {
                            card = new CombatCardState(
                                new DefinitionId(
                                    "test.runner_locked_poker.card"),
                                new InstanceId(instanceId),
                                new CardRank(2),
                                CombatCardSeason.Winter,
                                isOpponent
                                    ? 1
                                    : 10,
                                isOpponent
                                    ? 1
                                    : 10,
                                0,
                                isOpponent
                                    ? 0
                                    : 2);

                            cards.Add(card);
                        }

                        slots.Add(
                            new CombatSlotState(
                                new SlotId(instanceId),
                                new BoardPosition(
                                    side,
                                    row,
                                    new BoardColumn(column)),
                                card == null
                                    ? (InstanceId?)null
                                    : card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(100),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));
            }
        }
    }
}
