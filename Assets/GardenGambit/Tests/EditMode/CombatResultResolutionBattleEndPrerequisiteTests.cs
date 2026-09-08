using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultResolutionBattleEndPrerequisiteTests
    {



        [Test]
        public void
            ResolveAfterNormalColumns_WithoutBattleEnd_ThrowsWithoutResult()
        {
            var environment =
                CreateEnvironment();

            AppendAllColumns(
                environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAfterNormalColumns(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(6));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.Zero);

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
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue)));

            Assert.That(
                environment.State.Enemy
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue)));
        }
        [Test]
        public void
            ResolveAfterBattleEnd_WithoutBattleEnd_ThrowsWithoutChangingResultState()
        {
            var environment =
                CreateEnvironment();

            AppendAllColumns(
                environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAfterBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(6));

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
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue)));

            Assert.That(
                environment.State.Enemy
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue)));
        }

        [Test]
        public void
            ResolveAfterBattleEnd_WithCompletedPrerequisites_CompletesDraw()
        {
            var environment =
                CreateEnvironment();

            AppendAllColumns(
                environment);

            var battleEndEvent =
                AppendBattleEnd(
                    environment);

            var completedEvent =
                environment.Resolver
                    .ResolveAfterBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

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
                resultEvent.Metadata.SequenceNo,
                Is.LessThan(
                    completedEvent.Metadata.SequenceNo));

            Assert.That(
                environment.EventLog.Events[8],
                Is.SameAs(
                    completedEvent));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new
                        CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

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
                        eventLog)
            };
        }

        private static void AppendAllColumns(
            TestEnvironment environment)
        {
            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                     BoardColumn.MaximumValue;
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
        }
    }
}