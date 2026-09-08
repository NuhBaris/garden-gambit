using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerBattleEndIntegrationTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void
            StartAndResolveCombat_LogsBattleEndAfterAllColumnsAndBeforeResult(
                bool useStagedPipeline)
        {
            var environment =
                CreateEnvironment();

            var completedEvent =
                StartCombat(
                    environment.Runner,
                    useStagedPipeline,
                    maximumTriggerCountPerEvent: 100);

            var combatStartedEvent =
                environment.EventLog.Events[0]
                    as CombatStartedCombatEvent;

            var lastColumnEvent =
                GetLastColumnEvent(
                    environment.EventLog);

            var battleEndEvent =
                GetSingleBattleEndEvent(
                    environment.EventLog);

            var resultEvent =
                GetSingleResultEvent(
                    environment.EventLog);

            Assert.That(
                combatStartedEvent,
                Is.Not.Null);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

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
                    CombatEventKind.CombatCompleted),
                Is.EqualTo(1));

            Assert.That(
                lastColumnEvent.Column.Value,
                Is.EqualTo(5));

            Assert.That(
                lastColumnEvent.Metadata.SequenceNo,
                Is.LessThan(
                    battleEndEvent.Metadata.SequenceNo));

            Assert.That(
                battleEndEvent.Metadata.SequenceNo,
                Is.LessThan(
                    resultEvent.Metadata.SequenceNo));

            Assert.That(
                resultEvent.Metadata.SequenceNo,
                Is.LessThan(
                    completedEvent.Metadata.SequenceNo));

            Assert.That(
                battleEndEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                battleEndEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent.Metadata.EventId));

            Assert.That(
                battleEndEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent.Metadata.EventId));

            Assert.That(
                environment.Runner
                    .HasActiveBattleEndResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveBattleEndEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingBattleEndResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.EventLog.Events[
                    environment.EventLog.Count - 1],
                Is.SameAs(
                    completedEvent));
        }

        [Test]
        public void
            ResumeActiveCombat_AfterBattleEndTriggerBudgetExhaustion_DoesNotRepeatBattleEndOrResult()
        {
            var handler =
                new TestTriggerHandler();

            var source =
                new TestTriggerSource
                {
                    DiscoverAction =
                        (state, sourceEvent) =>
                        {
                            if (sourceEvent.Kind !=
                                CombatEventKind
                                    .BattleEndStarted)
                            {
                                return EmptyCandidates();
                            }

                            return new[]
                            {
                                CreateCandidate(
                                    handler,
                                    sourceLocalOrder: 0),

                                CreateCandidate(
                                    handler,
                                    sourceLocalOrder: 1)
                            };
                        }
                };

            var environment =
                CreateEnvironment(
                    source);

            Assert.Throws<InvalidOperationException>(
                () => StartCombat(
                    environment.Runner,
                    useStagedPipeline: false,
                    maximumTriggerCountPerEvent: 1));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveBattleEndResolution,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveBattleEndEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .HasPendingBattleEndResolution,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCompletedEvent,
                Is.Null);

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
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatCompleted),
                Is.Zero);

            var preparedBattleEndEvent =
                environment.Runner
                    .ActiveBattleEndEvent;

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(2));

            Assert.That(
                handler.LastSourceEvent,
                Is.SameAs(
                    preparedBattleEndEvent));

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
                    CombatEventKind.CombatCompleted),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasActiveBattleEndResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveBattleEndEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingBattleEndResolution,
                Is.False);

            Assert.That(
                environment.EventLog.Events[
                    environment.EventLog.Count - 1],
                Is.SameAs(
                    completedEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                params ICombatTriggerSource[] sources)
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
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            return new TestEnvironment
            {
                EventLog =
                    eventLog,

                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatCompletedCombatEvent
            StartCombat(
                CombatResolutionRunner runner,
                bool useStagedPipeline,
                int maximumTriggerCountPerEvent)
        {
            if (useStagedPipeline)
            {
                return runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent:
                            maximumTriggerCountPerEvent);
            }

            return runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent:
                    maximumTriggerCountPerEvent);
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatTriggerCandidate<
            ICombatTriggerHandler>
            CreateCandidate(
                ICombatTriggerHandler handler,
                int sourceLocalOrder)
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        horizontalOrder: 0,
                        verticalOrder:
                            sourceLocalOrder),
                    handler);
        }

        private static IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            EmptyCandidates()
        {
            return Array.Empty<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>();
        }

        private static ColumnStartedCombatEvent
            GetLastColumnEvent(
                CombatEventLog eventLog)
        {
            ColumnStartedCombatEvent
                lastColumnEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var columnEvent =
                    eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (columnEvent != null)
                {
                    lastColumnEvent =
                        columnEvent;
                }
            }

            if (lastColumnEvent == null)
            {
                throw new InvalidOperationException(
                    "Column Started event was not found.");
            }

            return lastColumnEvent;
        }

        private static BattleEndStartedCombatEvent
            GetSingleBattleEndEvent(
                CombatEventLog eventLog)
        {
            BattleEndStartedCombatEvent
                battleEndEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as BattleEndStartedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (battleEndEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Battle End Started " +
                        "events were found.");
                }

                battleEndEvent =
                    candidate;
            }

            if (battleEndEvent == null)
            {
                throw new InvalidOperationException(
                    "Battle End Started event was not found.");
            }

            return battleEndEvent;
        }

        private static
            CombatResultCalculatedCombatEvent
            GetSingleResultEvent(
                CombatEventLog eventLog)
        {
            CombatResultCalculatedCombatEvent
                resultEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as
                        CombatResultCalculatedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (resultEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Combat Result events " +
                        "were found.");
                }

                resultEvent =
                    candidate;
            }

            if (resultEvent == null)
            {
                throw new InvalidOperationException(
                    "Combat Result event was not found.");
            }

            return resultEvent;
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

        private sealed class TestTriggerSource :
            ICombatTriggerSource
        {
            public Func<
                CombatState,
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>>
                DiscoverAction
            {
                get;
                set;
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                DiscoverTriggers(
                    CombatState state,
                    CombatEvent sourceEvent)
            {
                if (DiscoverAction == null)
                {
                    return EmptyCandidates();
                }

                return DiscoverAction(
                    state,
                    sourceEvent);
            }
        }

        private sealed class TestTriggerHandler :
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
                return true;
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
            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatResolutionRunner Runner
            {
                get;
                set;
            }
        }
    }
}