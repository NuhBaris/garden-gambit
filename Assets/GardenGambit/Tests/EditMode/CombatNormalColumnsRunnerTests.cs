using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalColumnsRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment(0);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
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
                CreateEnvironment(0);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
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
                CreateEnvironment(0);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
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
                CreateEnvironment(0);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
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
                CreateEnvironment(0);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        null));
        }

        [Test]
        public void StartAndResolveAllColumns_WithInvalidBudget_ThrowsWithoutStartingCombat()
        {
            var environment =
                CreateEnvironment(0);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        0,
                        1,
                        1,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        1,
                        0,
                        1,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        1,
                        1,
                        0,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        1,
                        1,
                        1,
                        0));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResumeActiveCombat_WithoutActiveCombat_Throws()
        {
            var environment =
                CreateEnvironment(0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCombat(
                        1,
                        1,
                        1,
                        1));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllColumns_WithEmptyBoards_ProcessesFiveColumnsInOrder()
        {
            var environment =
                CreateEnvironment(0);

            var resolvedExchangeCount =
                environment.Runner
                    .StartAndResolveAllColumns(
                        8,
                        16,
                        64,
                        16);

            Assert.That(
                resolvedExchangeCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(6));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.NormalAttackExchange),
                Is.Zero);

            AssertColumnOrder(
                environment.EventLog);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);

            Assert.That(
                environment.Runner.ActiveCombatStartedEvent,
                Is.Null);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Null);

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.Zero);

            Assert.That(
                environment.Runner.ResolvedExchangeCount,
                Is.Zero);
        }

        [Test]
        public void StartAndResolveAllColumns_WithOneOccupiedColumn_ResolvesOneExchange()
        {
            var environment =
                CreateEnvironment(1);

            var resolvedExchangeCount =
                environment.Runner
                    .StartAndResolveAllColumns(
                        8,
                        16,
                        64,
                        16);

            Assert.That(
                resolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(11));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EnemySide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Cards
                    .Cards[0].CurrentHp,
                Is.EqualTo(4));

            AssertColumnOrder(
                environment.EventLog);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllColumns_WithFiveOccupiedColumns_ResolvesEachColumnOnce()
        {
            var environment =
                CreateEnvironment(5);

            var resolvedExchangeCount =
                environment.Runner
                    .StartAndResolveAllColumns(
                        8,
                        16,
                        64,
                        16);

            Assert.That(
                resolvedExchangeCount,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(31));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.NormalAttackExchange),
                Is.EqualTo(5));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(10));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(5));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EnemySide.Cards.Count,
                Is.Zero);

            for (var index = 0;
                 index < environment.PlayerSide
                     .Cards.Count;
                 index++)
            {
                Assert.That(
                    environment.PlayerSide.Cards
                        .Cards[index].CurrentHp,
                    Is.EqualTo(4));
            }

            AssertColumnOrder(
                environment.EventLog);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllColumns_WhenCombatIsActive_ThrowsWithoutChangingProgress()
        {
            var environment =
                CreateEnvironment(1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        1,
                        1,
                        1,
                        1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            var previousEventCount =
                environment.EventLog.Count;

            var previousExchangeCount =
                environment.Runner
                    .ResolvedExchangeCount;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        8,
                        16,
                        64,
                        16));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(previousEventCount));

            Assert.That(
                environment.Runner.ResolvedExchangeCount,
                Is.EqualTo(previousExchangeCount));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);
        }

        [Test]
        public void ResumeActiveCombat_AfterBudgetFailure_ReturnsPreviouslyStartedExchangeInTotal()
        {
            var environment =
                CreateEnvironment(1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        1,
                        1,
                        1,
                        1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveCombatStartedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner.ActiveColumnEvent.Column,
                Is.EqualTo(
                    new BoardColumn(1)));

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.NormalAttackExchange),
                Is.EqualTo(1));

            var resolvedExchangeCount =
                environment.Runner
                    .ResumeActiveCombat(
                        1,
                        16,
                        64,
                        16);

            Assert.That(
                resolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(11));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EnemySide.Cards.Count,
                Is.Zero);

            AssertColumnOrder(
                environment.EventLog);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.HasPendingResolution,
                Is.False);

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.Zero);

            Assert.That(
                environment.Runner.ResolvedExchangeCount,
                Is.Zero);
        }

        private static TestEnvironment CreateEnvironment(
            int occupiedColumnCount)
        {
            if (occupiedColumnCount < 0 ||
                occupiedColumnCount >
                BoardColumn.MaximumValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(occupiedColumnCount));
            }

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    occupiedColumnCount,
                    1,
                    100,
                    5);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    occupiedColumnCount,
                    11,
                    200,
                    1);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[0]);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                EnemySide = enemySide,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry = sourceRegistry,
                Runner =
                    new CombatNormalColumnsRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            int occupiedColumnCount,
            int firstSlotId,
            long firstInstanceId,
            int attack)
        {
            var slots =
                new List<CombatSlotState>();

            var cards =
                new List<CombatCardState>();

            var definitionPrefix =
                side == CombatSide.Player
                    ? "player-card-"
                    : "enemy-card-";

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                 BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

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

                var slotOffset =
                    (columnValue -
                     BoardColumn.MinimumValue) * 2;

                var frontSlotId =
                    new SlotId(
                        firstSlotId +
                        slotOffset);

                var backSlotId =
                    new SlotId(
                        firstSlotId +
                        slotOffset +
                        1);

                if (columnValue <=
                    occupiedColumnCount)
                {
                    var instanceId =
                        new InstanceId(
                            firstInstanceId +
                            columnValue);

                    var card =
                        new CombatCardState(
                            new DefinitionId(
                                definitionPrefix +
                                columnValue),
                            instanceId,
                            new CardRank(2),
                            5,
                            5,
                            0,
                            attack);

                    cards.Add(card);

                    slots.Add(
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition,
                            instanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition));
                }

                slots.Add(
                    new CombatSlotState(
                        backSlotId,
                        backPosition));
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

        private static void AssertColumnOrder(
            CombatEventLog eventLog)
        {
            var expectedColumnValue =
                BoardColumn.MinimumValue;

            var columnEventCount = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var columnEvent =
                    eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (columnEvent == null)
                {
                    continue;
                }

                Assert.That(
                    columnEvent.Column,
                    Is.EqualTo(
                        new BoardColumn(
                            expectedColumnValue)));

                expectedColumnValue++;
                columnEventCount++;
            }

            Assert.That(
                columnEventCount,
                Is.EqualTo(5));

            Assert.That(
                expectedColumnValue,
                Is.EqualTo(
                    BoardColumn.MaximumValue + 1));
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatSideState EnemySide
            {
                get;
                set;
            }

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

            public CombatNormalColumnsRunner Runner
            {
                get;
                set;
            }
        }
    }
}