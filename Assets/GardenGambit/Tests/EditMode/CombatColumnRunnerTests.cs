using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatColumnRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnRunner(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnRunner(
                        environment.State,
                        null,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnRunner(
                        environment.State,
                        environment.MetadataFactory,
                        null,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullSourceRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        null));
        }

        [Test]
        public void StartAndResolveColumn_WithNullCombatStartedEvent_ThrowsWithoutDraining()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        null,
                        new BoardColumn(1),
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void StartAndResolveColumn_WithInvalidColumn_ThrowsWithoutDraining()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        default(BoardColumn),
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void StartAndResolveColumn_WithInvalidBudgets_ThrowsWithoutStartingColumn()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        0,
                        4,
                        16,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        2,
                        0,
                        16,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        2,
                        4,
                        0,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        2,
                        4,
                        16,
                        0));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void StartAndResolveColumn_WithUnloggedCombatStartedEvent_ThrowsWithoutDraining()
        {
            var environment =
                CreateEnvironment();

            var unloggedCombatStartedEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        unloggedCombatStartedEvent,
                        new BoardColumn(1),
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void StartAndResolveColumn_WithDifferentLoggedReference_ThrowsWithoutDraining()
        {
            var environment =
                CreateEnvironment();

            var differentReference =
                new CombatStartedCombatEvent(
                    environment.CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        differentReference,
                        new BoardColumn(1),
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResumeActiveColumn_WithoutActiveColumn_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveColumn(
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void StartAndResolveColumn_WithEmptyColumn_DrainsLifecycleAndReturnsZero()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false);

            var exchangeCount =
                environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        2,
                        4,
                        16,
                        1);

            Assert.That(
                exchangeCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));

            var columnStartedEvent =
                environment.EventLog.Events[1]
                    as ColumnStartedCombatEvent;

            Assert.That(
                columnStartedEvent,
                Is.Not.Null);

            Assert.That(
                columnStartedEvent.Column,
                Is.EqualTo(
                    new BoardColumn(1)));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveColumn_WithLethalExchange_CompletesColumnAndClearsActiveState()
        {
            var environment =
                CreateEnvironment(
                    playerFrontHp: 10,
                    enemyFrontHp: 5,
                    playerFrontAttack: 5,
                    enemyFrontAttack: 1);

            var exchangeCount =
                environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        1,
                        8,
                        32,
                        4);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveColumn_WithTwoEmptyColumns_PreservesLeftToRightOrder()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false);

            var firstExchangeCount =
                environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        1,
                        4,
                        16,
                        1);

            var secondExchangeCount =
                environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(2),
                        1,
                        4,
                        16,
                        1);

            Assert.That(
                firstExchangeCount,
                Is.Zero);

            Assert.That(
                secondExchangeCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(3));

            var firstColumnEvent =
                environment.EventLog.Events[1]
                    as ColumnStartedCombatEvent;

            var secondColumnEvent =
                environment.EventLog.Events[2]
                    as ColumnStartedCombatEvent;

            Assert.That(
                firstColumnEvent,
                Is.Not.Null);

            Assert.That(
                secondColumnEvent,
                Is.Not.Null);

            Assert.That(
                firstColumnEvent.Column,
                Is.EqualTo(
                    new BoardColumn(1)));

            Assert.That(
                secondColumnEvent.Column,
                Is.EqualTo(
                    new BoardColumn(2)));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveColumn_WhenExchangeDrainBudgetIsExhausted_PreservesActiveColumnAndAllowsResume()
        {
            var environment =
                CreateEnvironment(
                    playerFrontHp: 10,
                    enemyFrontHp: 5,
                    playerFrontAttack: 5,
                    enemyFrontAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        1,
                        1,
                        1,
                        1));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ActiveColumnEvent.Column,
                Is.EqualTo(
                    new BoardColumn(1)));

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.True);

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            var eventCountAfterFailure =
                environment.EventLog.Count;

            var processedCountAfterFailure =
                environment.EventQueue
                    .ProcessedCount;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(2),
                        1,
                        8,
                        32,
                        4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCountAfterFailure));

            Assert.That(
                environment.EventQueue
                    .ProcessedCount,
                Is.EqualTo(
                    processedCountAfterFailure));

            var resumedExchangeCount =
                environment.Runner
                    .ResumeActiveColumn(
                        1,
                        8,
                        32,
                        4);

            Assert.That(
                resumedExchangeCount,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerFrontOccupied = true,
                bool enemyFrontOccupied = true,
                int playerFrontHp = 10,
                int enemyFrontHp = 5,
                int playerFrontAttack = 5,
                int enemyFrontAttack = 1)
        {
            var playerCard =
                playerFrontOccupied
                    ? CreateCard(
                        "card.player",
                        100,
                        playerFrontHp,
                        playerFrontAttack)
                    : null;

            var enemyCard =
                enemyFrontOccupied
                    ? CreateCard(
                        "card.enemy",
                        200,
                        enemyFrontHp,
                        enemyFrontAttack)
                    : null;

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        1,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        11,
                        enemyCard));

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[0]);

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition =
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                EnemyPosition =
                    new BoardPosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        new BoardColumn(1)),
                InitialPlayerHp = playerFrontHp,
                InitialEnemyHp = enemyFrontHp,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry = sourceRegistry,
                CombatStartedEvent =
                    combatStartedEvent,
                Runner =
                    new CombatColumnRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            int firstSlotId,
            CombatCardState firstFrontCard)
        {
            var slots =
                new List<CombatSlotState>();

            var cards =
                new List<CombatCardState>();

            if (firstFrontCard != null)
            {
                cards.Add(firstFrontCard);
            }

            var nextSlotId =
                firstSlotId;

            for (var columnValue = 1;
                 columnValue <= 5;
                 columnValue++)
            {
                var column =
                    new BoardColumn(columnValue);

                var frontPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        column);

                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        column);

                if (columnValue == 1 &&
                    firstFrontCard != null)
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(nextSlotId),
                            frontPosition,
                            firstFrontCard.InstanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(nextSlotId),
                            frontPosition));
                }

                nextSlotId++;

                slots.Add(
                    new CombatSlotState(
                        new SlotId(nextSlotId),
                        backPosition));

                nextSlotId++;
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                0,
                attack);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private static int CountEventsOfKind(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertInitialStateUnchanged(
            TestEnvironment environment)
        {
            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Null);

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            var enemySide =
                environment.State.GetSide(
                    CombatSide.Enemy);

            if (environment.PlayerCard != null)
            {
                Assert.That(
                    environment.PlayerCard.CurrentHp,
                    Is.EqualTo(
                        environment.InitialPlayerHp));

                Assert.That(
                    playerSide.GetCardAt(
                        environment.PlayerPosition),
                    Is.SameAs(
                        environment.PlayerCard));
            }

            if (environment.EnemyCard != null)
            {
                Assert.That(
                    environment.EnemyCard.CurrentHp,
                    Is.EqualTo(
                        environment.InitialEnemyHp));

                Assert.That(
                    enemySide.GetCardAt(
                        environment.EnemyPosition),
                    Is.SameAs(
                        environment.EnemyCard));
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public CombatCardState PlayerCard
            {
                get;
                set;
            }

            public CombatCardState EnemyCard
            {
                get;
                set;
            }

            public BoardPosition PlayerPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyPosition
            {
                get;
                set;
            }

            public int InitialPlayerHp { get; set; }

            public int InitialEnemyHp { get; set; }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatEventQueue EventQueue
            {
                get;
                set;
            }

            public CombatTriggerSourceRegistry
                SourceRegistry
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public CombatColumnRunner Runner
            {
                get;
                set;
            }
        }
    }
}