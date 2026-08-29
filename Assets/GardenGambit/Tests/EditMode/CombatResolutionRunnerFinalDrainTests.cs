using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerFinalDrainTests
    {
        [Test]
        public void ResumeActiveCombat_AfterFinalTriggerBudgetExhaustion_DoesNotRepeatResultOrCompletion()
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
                                    .CombatResultCalculated)
                            {
                                return EmptyCandidates();
                            }

                            return new[]
                            {
                                CreateCandidate(
                                    handler,
                                    0),
                                CreateCandidate(
                                    handler,
                                    1)
                            };
                        }
                };

            var environment =
                CreateEnvironment(
                    source);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        10,
                        100,
                        100,
                        1));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ActiveCompletedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .HasPendingColumnResolution,
                Is.True);

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            var preparedCompletedEvent =
                environment.Runner
                    .ActiveCompletedEvent;

            var resumedCompletedEvent =
                environment.Runner
                    .ResumeActiveCombat(
                        10,
                        100,
                        100,
                        10);

            Assert.That(
                resumedCompletedEvent,
                Is.SameAs(
                    preparedCompletedEvent));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .ActiveCompletedEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingColumnResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.Zero);

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.Zero);

            Assert.That(
                resumedCompletedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                resumedCompletedEvent
                    .PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                resumedCompletedEvent
                    .EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));
        }

        private static TestEnvironment
            CreateEnvironment(
                params ICombatTriggerSource[] sources)
        {
            var state =
                new CombatState(
                    CreateEmptySideState(
                        CombatSide.Player),
                    CreateEmptySideState(
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
                State = state,
                EventLog = eventLog,
                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatSideState
            CreateEmptySideState(
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
            ICombatTriggerHandler> CreateCandidate(
                ICombatTriggerHandler handler,
                int sourceLocalOrder)
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        0,
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

            public CombatResolutionRunner Runner
            {
                get;
                set;
            }
        }
    }
}