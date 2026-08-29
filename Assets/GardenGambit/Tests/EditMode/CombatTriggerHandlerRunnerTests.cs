using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatTriggerHandlerRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerHandlerRunner(
                        null,
                        new CombatEventQueue(
                            new CombatEventLog())));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerHandlerRunner(
                        CreateState(),
                        null));
        }

        [Test]
        public void Drain_WithNullDiscovery_ThrowsWithoutChangingQueue()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner.Drain(
                    1,
                    1,
                    null));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Runner.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void Drain_WithInvalidBudgets_ThrowsWithoutDiscovering()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var discoveryCount = 0;

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner.Drain(
                    0,
                    1,
                    (state, combatEvent) =>
                    {
                        discoveryCount++;

                        return EmptyCandidates();
                    }));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner.Drain(
                    1,
                    0,
                    (state, combatEvent) =>
                    {
                        discoveryCount++;

                        return EmptyCandidates();
                    }));

            Assert.That(
                discoveryCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));
        }

        [Test]
        public void Drain_WithEmptyEventQueue_ReturnsZero()
        {
            var environment =
                CreateEnvironment();

            var discoveryCount = 0;

            var processedEventCount =
                environment.Runner.Drain(
                    1,
                    1,
                    (state, combatEvent) =>
                    {
                        discoveryCount++;

                        return EmptyCandidates();
                    });

            Assert.That(
                processedEventCount,
                Is.Zero);

            Assert.That(
                discoveryCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WithValidHandlers_PassesExactStateAndEventInPriorityOrder()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var processOrder =
                new List<string>();

            var petHandler =
                new TestTriggerHandler("Pet");

            var slotHandler =
                new TestTriggerHandler("Slot");

            petHandler.ResolveAction =
                (state, combatEvent) =>
                    processOrder.Add(
                        petHandler.Name);

            slotHandler.ResolveAction =
                (state, combatEvent) =>
                    processOrder.Add(
                        slotHandler.Name);

            var discoveryCount = 0;

            var processedEventCount =
                environment.Runner.Drain(
                    1,
                    2,
                    (state, combatEvent) =>
                    {
                        discoveryCount++;

                        Assert.That(
                            state,
                            Is.SameAs(
                                environment.State));

                        Assert.That(
                            combatEvent,
                            Is.SameAs(sourceEvent));

                        return new[]
                        {
                            CreateCandidate(
                                petHandler,
                                CombatTriggerSourceKind.Pet,
                                0),
                            CreateCandidate(
                                slotHandler,
                                CombatTriggerSourceKind.Slot,
                                4)
                        };
                    });

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Slot",
                        "Pet"
                    }));

            AssertHandlerReceived(
                slotHandler,
                environment.State,
                sourceEvent);

            AssertHandlerReceived(
                petHandler,
                environment.State,
                sourceEvent);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenHandlerAppendsChildEvent_ProcessesChildInSameDrain()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            var handler =
                new TestTriggerHandler("Root");

            TestCombatEvent childEvent = null;

            handler.ResolveAction =
                (state, combatEvent) =>
                {
                    childEvent =
                        new TestCombatEvent(
                            environment.MetadataFactory
                                .CreateChild(
                                    combatEvent.Metadata));

                    environment.EventLog.Append(
                        childEvent);
                };

            var processedEventCount =
                environment.Runner.Drain(
                    2,
                    1,
                    (state, combatEvent) =>
                    {
                        if (ReferenceEquals(
                                combatEvent,
                                rootEvent))
                        {
                            return new[]
                            {
                                CreateCandidate(
                                    handler,
                                    CombatTriggerSourceKind.Card,
                                    0)
                            };
                        }

                        Assert.That(
                            combatEvent,
                            Is.SameAs(childEvent));

                        return EmptyCandidates();
                    });

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            AssertHandlerReceived(
                handler,
                environment.State,
                rootEvent);

            Assert.That(
                childEvent,
                Is.Not.Null);

            Assert.That(
                childEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    rootEvent.Metadata.EventId));

            Assert.That(
                childEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    rootEvent.Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenHandlerThrows_RetryUsesSameStateEventAndBatch()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var handler =
                new TestTriggerHandler("Handler")
                {
                    ThrowOnNextResolve = true
                };

            var candidate =
                CreateCandidate(
                    handler,
                    CombatTriggerSourceKind.Card,
                    0);

            var discoveryCount = 0;

            Func<
                CombatState,
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>>
                discoverTriggers =
                    (state, combatEvent) =>
                    {
                        discoveryCount++;

                        return new[]
                        {
                            candidate
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner.Drain(
                    1,
                    1,
                    discoverTriggers));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Runner.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.Runner.PendingTriggerCount,
                Is.EqualTo(1));

            var processedEventCount =
                environment.Runner.Drain(
                    1,
                    1,
                    discoverTriggers);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(2));

            Assert.That(
                handler.ReceivedStates.Count,
                Is.EqualTo(2));

            Assert.That(
                handler.ReceivedStates[0],
                Is.SameAs(environment.State));

            Assert.That(
                handler.ReceivedStates[1],
                Is.SameAs(environment.State));

            Assert.That(
                handler.ReceivedEvents[0],
                Is.SameAs(sourceEvent));

            Assert.That(
                handler.ReceivedEvents[1],
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Runner.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        private static void AssertHandlerReceived(
            TestTriggerHandler handler,
            CombatState expectedState,
            CombatEvent expectedEvent)
        {
            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedStates[0],
                Is.SameAs(expectedState));

            Assert.That(
                handler.ReceivedEvents[0],
                Is.SameAs(expectedEvent));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var state =
                CreateState();

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(eventLog);

            return new TestEnvironment
            {
                State = state,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                Runner =
                    new CombatTriggerHandlerRunner(
                        state,
                        eventQueue)
            };
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player),
                CreateSideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState CreateSideState(
            CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static TestCombatEvent AppendRootEvent(
            TestEnvironment environment)
        {
            var combatEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                combatEvent);

            return combatEvent;
        }

        private static CombatTriggerCandidate<
            ICombatTriggerHandler> CreateCandidate(
                ICombatTriggerHandler handler,
                CombatTriggerSourceKind sourceKind,
                int horizontalOrder)
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>(
                    new CombatTriggerOrderKey(
                        sourceKind,
                        CombatSide.Player,
                        horizontalOrder,
                        0),
                    handler);
        }

        private static IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            EmptyCandidates()
        {
            return new CombatTriggerCandidate<
                ICombatTriggerHandler>[0];
        }

        private sealed class TestTriggerHandler :
            ICombatTriggerHandler
        {
            public TestTriggerHandler(string name)
            {
                Name = name;

                ReceivedStates =
                    new List<CombatState>();

                ReceivedEvents =
                    new List<CombatEvent>();
            }

            public string Name { get; }

            public bool ThrowOnNextResolve
            {
                get;
                set;
            }

            public Action<CombatState, CombatEvent>
                ResolveAction
            {
                get;
                set;
            }

            public int ResolveCallCount { get; private set; }

            public List<CombatState> ReceivedStates
            {
                get;
            }

            public List<CombatEvent> ReceivedEvents
            {
                get;
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

                ReceivedStates.Add(state);
                ReceivedEvents.Add(sourceEvent);

                if (ThrowOnNextResolve)
                {
                    ThrowOnNextResolve = false;

                    throw new InvalidOperationException(
                        "Test handler failure.");
                }

                if (ResolveAction != null)
                {
                    ResolveAction(
                        state,
                        sourceEvent);
                }
            }
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
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

            public CombatEventQueue EventQueue
            {
                get;
                set;
            }

            public CombatTriggerHandlerRunner Runner
            {
                get;
                set;
            }
        }
    }
}