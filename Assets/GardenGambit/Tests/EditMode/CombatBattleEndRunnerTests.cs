using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatBattleEndRunnerTests
    {
        [Test]
        public void
            Constructor_WithNullState_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var state =
                CreateEmptyState();

            var engine =
                CreateEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    CreateEmptySourceRegistry());

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleEndRunner(
                        null,
                        metadataFactory,
                        eventLog,
                        engine));
        }

        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var state =
                CreateEmptyState();

            var engine =
                CreateEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    CreateEmptySourceRegistry());

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleEndRunner(
                        state,
                        null,
                        eventLog,
                        engine));
        }

        [Test]
        public void
            Constructor_WithNullEventLog_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var state =
                CreateEmptyState();

            var engine =
                CreateEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    CreateEmptySourceRegistry());

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleEndRunner(
                        state,
                        metadataFactory,
                        null,
                        engine));
        }

        [Test]
        public void
            Constructor_WithNullEngine_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleEndRunner(
                        CreateEmptyState(),
                        CreateMetadataFactory(),
                        new CombatEventLog(),
                        null));
        }

        [Test]
        public void
            StartAndResolveBattleEnd_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    columnCount: 5,
                    handlerCount: 0);

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveBattleEnd(
                        null,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveBattleEnd_BeforeFiveColumns_ThrowsWithoutStarting()
        {
            var environment =
                CreateEnvironment(
                    columnCount: 4,
                    handlerCount: 0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveBattleEnd(
                        environment
                            .CombatStartedEvent,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.False);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.Zero);
        }

        [Test]
        public void
            StartAndResolveBattleEnd_WithFiveColumns_DrainsTriggerAndCompletes()
        {
            var environment =
                CreateEnvironment(
                    columnCount: 5,
                    handlerCount: 1);

            var battleEndEvent =
                environment.Runner
                    .StartAndResolveBattleEnd(
                        environment
                            .CombatStartedEvent,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                battleEndEvent,
                Is.Not.Null);

            Assert.That(
                battleEndEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .BattleEndStarted));

            Assert.That(
                environment.Handlers[0]
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Handlers[0]
                    .LastSourceEvent,
                Is.SameAs(
                    battleEndEvent));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveBattleEndEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Events[6],
                Is.SameAs(
                    battleEndEvent));
        }

        [Test]
        public void
            ResumeActiveBattleEnd_WithoutActiveResolution_Throws()
        {
            var environment =
                CreateEnvironment(
                    columnCount: 5,
                    handlerCount: 0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveBattleEnd(
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));
        }

        [Test]
        public void
            ResumeActiveBattleEnd_AfterTriggerBudgetExhaustion_DoesNotRepeatEventOrFirstTrigger()
        {
            var environment =
                CreateEnvironment(
                    columnCount: 5,
                    handlerCount: 2);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveBattleEnd(
                        environment
                            .CombatStartedEvent,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 1));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.True);

            var activeEvent =
                environment.Runner
                    .ActiveBattleEndEvent;

            Assert.That(
                activeEvent,
                Is.Not.Null);

            Assert.That(
                environment.Handlers[0]
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Handlers[1]
                    .ResolveCallCount,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            var eventCountBeforeResume =
                environment.EventLog.Count;

            var resumedEvent =
                environment.Runner
                    .ResumeActiveBattleEnd(
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resumedEvent,
                Is.SameAs(
                    activeEvent));

            Assert.That(
                environment.Handlers[0]
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Handlers[1]
                    .ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Handlers[1]
                    .LastSourceEvent,
                Is.SameAs(
                    activeEvent));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCountBeforeResume));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleEndStarted),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveBattleEndEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                int columnCount,
                int handlerCount)
        {
            var state =
                CreateEmptyState();

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            AppendColumns(
                metadataFactory,
                eventLog,
                combatStartedEvent,
                columnCount);

            var handlers =
                new List<
                    CountingBattleEndHandler>();

            var sources =
                new List<
                    ICombatTriggerSource>();

            for (var index = 0;
                 index < handlerCount;
                 index++)
            {
                var handler =
                    new CountingBattleEndHandler();

                handlers.Add(
                    handler);

                sources.Add(
                    new CombatTriggerHandlerSource(
                        new
                            FixedCombatTriggerOrderKeyProvider(
                                new CombatTriggerOrderKey(
                                    CombatTriggerSourceKind
                                        .Card,
                                    CombatSide.Player,
                                    horizontalOrder:
                                        index,
                                    verticalOrder: 0)),
                        handler));
            }

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            var engine =
                CreateEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    sourceRegistry);

            return new TestEnvironment
            {
                State =
                    state,
                EventLog =
                    eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Handlers =
                    handlers,
                Runner =
                    new CombatBattleEndRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        engine)
            };
        }

        private static CombatEventResolutionEngine
            CreateEngine(
                CombatState state,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog,
                CombatTriggerSourceRegistry
                    sourceRegistry)
        {
            return new CombatEventResolutionEngine(
                state,
                metadataFactory,
                eventLog,
                new CombatEventQueue(
                    eventLog),
                sourceRegistry);
        }

        private static void AppendColumns(
            CombatEventMetadataFactory
                metadataFactory,
            CombatEventLog eventLog,
            CombatStartedCombatEvent
                combatStartedEvent,
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
                        metadataFactory.CreateChild(
                            combatStartedEvent.Metadata),
                        new BoardColumn(
                            columnValue));

                eventLog.Append(
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

        private static CombatTriggerSourceRegistry
            CreateEmptySourceRegistry()
        {
            return new CombatTriggerSourceRegistry(
                Array.Empty<
                    ICombatTriggerSource>());
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

        private sealed class
            CountingBattleEndHandler :
            ICombatTriggerHandler
        {
            public int ResolveCallCount
            {
                get;
                private set;
            }

            public CombatEvent LastSourceEvent
            {
                get;
                private set;
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                return sourceEvent is
                    BattleEndStartedCombatEvent;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;

                LastSourceEvent =
                    sourceEvent;
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State
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

            public List<CountingBattleEndHandler>
                Handlers
            {
                get;
                set;
            }

            public CombatBattleEndRunner Runner
            {
                get;
                set;
            }
        }
    }
}