using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultResolutionNormalColumnsValidationTests
    {
        [Test]
        public void
            ResolveAfterNormalColumns_WithNoColumns_ThrowsWithoutChangingResultState()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAfterNormalColumns(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            AssertUnchangedBeforeResult(
                environment,
                expectedEventCount: 1);
        }

        [Test]
        public void
            ResolveAfterNormalColumns_WithOnlyFourColumns_ThrowsWithoutChangingResultState()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 4);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAfterNormalColumns(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            AssertUnchangedBeforeResult(
                environment,
                expectedEventCount: 5);
        }

        [Test]
        public void
            ResolveAfterNormalColumns_WithUnresolvedOpposingFronts_ThrowsWithoutChangingResultState()
        {
            var environment =
                CreateEnvironment(
                    includeOpposingFrontCards: true);

            AppendColumns(
                environment,
                columnCount: 5);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAfterNormalColumns(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            AssertUnchangedBeforeResult(
                environment,
                expectedEventCount: 6);
        }

        [Test]
        public void
            ResolveAfterNormalColumns_WithFiveResolvedColumnsAndBattleEnd_CompletesCombat()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 5);

            var battleEndEvent =
                AppendBattleEnd(
                    environment);

            var completedEvent =
                environment.Resolver
                    .ResolveAfterNormalColumns(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .CombatResultCalculated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .CombatCompleted),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(9));

            var resultEvent =
                environment.EventLog.Events[7]
                    as
                        CombatResultCalculatedCombatEvent;

            Assert.That(
                resultEvent,
                Is.Not.Null);

            Assert.That(
                battleEndEvent.Metadata.SequenceNo,
                Is.LessThan(
                    resultEvent.Metadata.SequenceNo));

            Assert.That(
                resultEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Events[8],
                Is.SameAs(
                    completedEvent));

            Assert.That(
                completedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    resultEvent.Metadata.EventId));

            Assert.That(
                environment.State.Player
                    .BattleHealth.Value,
                Is.EqualTo(
                    BattleHealth
                        .NormalBaselineValue));

            Assert.That(
                environment.State.Enemy
                    .BattleHealth.Value,
                Is.EqualTo(
                    BattleHealth
                        .NormalBaselineValue));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeOpposingFrontCards = false)
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new
                        CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            CombatState state;

            if (includeOpposingFrontCards)
            {
                state =
                    CreateStateWithOpposingFrontCards();
            }
            else
            {
                state =
                    new CombatState(
                        CreateEmptySide(
                            CombatSide.Player),
                        CreateEmptySide(
                            CombatSide.Enemy));
            }

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            return new TestEnvironment
            {
                State =
                    state,
                MetadataFactory =
                    metadataFactory,
                EventLog =
                    eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Resolver =
                    new CombatResultResolutionResolver(
                        metadataFactory,
                        eventLog),
                InitialPlayerBattleHealth =
                    state.Player
                        .BattleHealth.Value,
                InitialEnemyBattleHealth =
                    state.Enemy
                        .BattleHealth.Value
            };
        }

        private static BattleEndStartedCombatEvent
            AppendBattleEnd(
                TestEnvironment environment)
        {
            var battleEndEvent =
                new BattleEndStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment
                                .CombatStartedEvent
                                .Metadata));

            environment.EventLog.Append(
                battleEndEvent);

            return battleEndEvent;
        }

        private static void AppendColumns(
            TestEnvironment environment,
            int columnCount)
        {
            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <
                     BoardColumn.MinimumValue +
                     columnCount;
                 columnValue++)
            {
                var columnEvent =
                    new ColumnStartedCombatEvent(
                        environment.MetadataFactory
                            .CreateChild(
                                environment
                                    .CombatStartedEvent
                                    .Metadata),
                        new BoardColumn(
                            columnValue));

                environment.EventLog.Append(
                    columnEvent);
            }
        }

        private static void
            AssertUnchangedBeforeResult(
                TestEnvironment environment,
                int expectedEventCount)
        {
            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    expectedEventCount));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .CombatResultCalculated),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .CombatCompleted),
                Is.Zero);

            Assert.That(
                environment.State.Player
                    .BattleHealth.Value,
                Is.EqualTo(
                    environment
                        .InitialPlayerBattleHealth));

            Assert.That(
                environment.State.Enemy
                    .BattleHealth.Value,
                Is.EqualTo(
                    environment
                        .InitialEnemyBattleHealth));
        }

        private static int CountEvents(
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

        private static CombatState
            CreateStateWithOpposingFrontCards()
        {
            var column =
                new BoardColumn(1);

            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column);

            var playerCard =
                CreateCard(
                    "player-card",
                    1001);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    2001);

            return new CombatState(
                CreateOccupiedSide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition,
                    playerCard),
                CreateOccupiedSide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition,
                    enemyCard));
        }

        private static CombatSideState
            CreateOccupiedSide(
                CombatSide side,
                SlotId slotId,
                BoardPosition position,
                CombatCardState card)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
                            position,
                            card.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<
                        CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<
                        CombatCardState>()),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 2);
        }

        private sealed class TestEnvironment
        {
            public CombatState State
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

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public CombatResultResolutionResolver
                Resolver
            {
                get;
                set;
            }

            public int InitialPlayerBattleHealth
            {
                get;
                set;
            }

            public int InitialEnemyBattleHealth
            {
                get;
                set;
            }
        }
    }
}