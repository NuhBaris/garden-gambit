using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleEndResolverTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleEndResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void
            Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleEndResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void
            StartBattleEnd_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .StartBattleEnd(
                        null,
                        environment
                            .CombatStartedEvent));
        }

        [Test]
        public void
            StartBattleEnd_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        null));
        }

        [Test]
        public void
            StartBattleEnd_BeforeFiveColumns_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 4);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.Zero);
        }

        [Test]
        public void
            StartBattleEnd_WithSnapshot_AppendsDirectChildAndCopiesSnapshot()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: true);

            AppendColumns(
                environment,
                columnCount: 5);

            var battleEndEvent =
                environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            Assert.That(
                battleEndEvent,
                Is.Not.Null);

            Assert.That(
                battleEndEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .BattleEndStarted));

            Assert.That(
                battleEndEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                battleEndEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment
                        .CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.SameAs(
                    environment.Snapshot));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Events[6],
                Is.SameAs(
                    battleEndEvent));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartBattleEnd_WithoutSnapshot_AppendsSnapshotlessEvent()
        {
            var environment =
                CreateEnvironment(
                    includeSnapshot: false);

            AppendColumns(
                environment,
                columnCount: 5);

            var battleEndEvent =
                environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            Assert.That(
                battleEndEvent
                    .HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                battleEndEvent
                    .BattleStartSnapshot,
                Is.Null);

            Assert.That(
                environment.EventLog.Events[6],
                Is.SameAs(
                    battleEndEvent));
        }

        [Test]
        public void
            StartBattleEnd_CalledTwice_ThrowsWithoutAppendingDuplicate()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 5);

            var firstEvent =
                environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent);

            var eventCount =
                environment.EventLog.Count;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCount));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[
                    eventCount - 1],
                Is.SameAs(
                    firstEvent));
        }

        [Test]
        public void
            StartBattleEnd_AfterResultWasCalculated_ThrowsWithoutAppending()
        {
            var environment =
                CreateEnvironment();

            AppendColumns(
                environment,
                columnCount: 5);

            var resultResolver =
                new CombatResultCalculationResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var resultEvent =
                resultResolver.Resolve(
                    environment.State,
                    environment
                        .CombatStartedEvent);

            var eventCount =
                environment.EventLog.Count;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .StartBattleEnd(
                        environment.State,
                        environment
                            .CombatStartedEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCount));

            Assert.That(
                environment.EventLog.Events[
                    eventCount - 1],
                Is.SameAs(
                    resultEvent));

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
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeSnapshot = true)
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var state =
                CreateEmptyState();

            CombatBattleStartSnapshot
                snapshot = null;

            CombatStartedCombatEvent
                combatStartedEvent;

            if (includeSnapshot)
            {
                snapshot =
                    CreateEmptySnapshot();

                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadataFactory.CreateRoot(),
                        snapshot);
            }
            else
            {
                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadataFactory.CreateRoot());
            }

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
                Snapshot =
                    snapshot,
                Resolver =
                    new CombatBattleEndResolver(
                        metadataFactory,
                        eventLog)
            };
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new
                    CombatSequenceNumberAllocator());
        }

        private static CombatState
            CreateEmptyState()
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy));
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

        private static CombatBattleStartSnapshot
            CreateEmptySnapshot()
        {
            return new CombatBattleStartSnapshot(
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>()),
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>()));
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

            public CombatBattleStartSnapshot Snapshot
            {
                get;
                set;
            }

            public CombatBattleEndResolver Resolver
            {
                get;
                set;
            }
        }
    }
}