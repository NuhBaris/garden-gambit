using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatTriggerEngineTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerEngine(
                        null,
                        new CombatEventQueue(
                            new CombatEventLog()),
                        CreateEmptyRegistry()));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerEngine(
                        CreateState(),
                        null,
                        CreateEmptyRegistry()));
        }

        [Test]
        public void Constructor_WithNullSourceRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerEngine(
                        CreateState(),
                        new CombatEventQueue(
                            new CombatEventLog()),
                        null));
        }

        [Test]
        public void Drain_WithInvalidBudgets_ThrowsWithoutDiscoveringOrConsumingEvent()
        {
            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var sourceEvent =
                AppendRootEvent(environment);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Engine.Drain(
                    0,
                    1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Engine.Drain(
                    1,
                    0));

            Assert.That(
                source.DiscoveryCallCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Engine.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void Drain_WithEmptyEventQueue_ReturnsZeroWithoutDiscovering()
        {
            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    1);

            Assert.That(
                processedEventCount,
                Is.Zero);

            Assert.That(
                source.DiscoveryCallCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WithNoSources_ConsumesEventWithoutTriggers()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);

            Assert.That(
                environment.Engine.HasActiveBatch,
                Is.False);

            Assert.That(
                sourceEvent,
                Is.Not.Null);
        }

        [Test]
        public void Drain_WithMultipleSources_ResolvesHandlersInPriorityOrder()
        {
            var processOrder =
                new List<string>();

            var petHandler =
                new TestTriggerHandler("Pet");

            var slotHandler =
                new TestTriggerHandler("Slot");

            petHandler.ResolveAction =
                (state, sourceEvent) =>
                    processOrder.Add(
                        petHandler.Name);

            slotHandler.ResolveAction =
                (state, sourceEvent) =>
                    processOrder.Add(
                        slotHandler.Name);

            var petSource =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            CreateCandidate(
                                petHandler,
                                CombatTriggerSourceKind.Pet,
                                0)
                        }
                };

            var slotSource =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            CreateCandidate(
                                slotHandler,
                                CombatTriggerSourceKind.Slot,
                                4)
                        }
                };

            var environment =
                CreateEnvironment(
                    petSource,
                    slotSource);

            var sourceEvent =
                AppendRootEvent(environment);

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    2);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                processOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Slot",
                        "Pet"
                    }));

            AssertSourceReceived(
                petSource,
                environment.State,
                sourceEvent);

            AssertSourceReceived(
                slotSource,
                environment.State,
                sourceEvent);

            AssertHandlerReceived(
                petHandler,
                environment.State,
                sourceEvent);

            AssertHandlerReceived(
                slotHandler,
                environment.State,
                sourceEvent);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenHandlerAppendsChildEvent_ProcessesChildInSameDrain()
        {
            var handler =
                new TestTriggerHandler("Root");

            var source =
                new TestTriggerSource();

            var environment =
                CreateEnvironment(source);

            var rootEvent =
                AppendRootEvent(environment);

            TestCombatEvent childEvent = null;

            source.DiscoverAction =
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
                };

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
                environment.Engine.Drain(
                    2,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(2));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

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
        public void Drain_WhenHandlerThrows_RetryUsesSameBatchWithoutRediscovery()
        {
            var handler =
                new TestTriggerHandler("Handler")
                {
                    ThrowOnNextResolve = true
                };

            var source =
                new TestTriggerSource
                {
                    Candidates =
                        new[]
                        {
                            CreateCandidate(
                                handler,
                                CombatTriggerSourceKind.Card,
                                0)
                        }
                };

            var environment =
                CreateEnvironment(source);

            var sourceEvent =
                AppendRootEvent(environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Engine.Drain(
                    1,
                    1));

            Assert.That(
                source.DiscoveryCallCount,
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
                environment.Engine.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.Engine.PendingTriggerCount,
                Is.EqualTo(1));

            var processedEventCount =
                environment.Engine.Drain(
                    1,
                    1);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
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
                environment.Engine.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        private static TestEnvironment CreateEnvironment(
            params ICombatTriggerSource[] sources)
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

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            return new TestEnvironment
            {
                State = state,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry = sourceRegistry,
                Engine =
                    new CombatTriggerEngine(
                        state,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatTriggerSourceRegistry
            CreateEmptyRegistry()
        {
            return new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[0]);
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

        private static void AssertSourceReceived(
            TestTriggerSource source,
            CombatState expectedState,
            CombatEvent expectedEvent)
        {
            Assert.That(
                source.DiscoveryCallCount,
                Is.EqualTo(1));

            Assert.That(
                source.ReceivedState,
                Is.SameAs(expectedState));

            Assert.That(
                source.ReceivedEvent,
                Is.SameAs(expectedEvent));
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

        private sealed class TestTriggerSource :
            ICombatTriggerSource
        {
            public TestTriggerSource()
            {
                Candidates =
                    EmptyCandidates();
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                Candidates
            {
                get;
                set;
            }

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

            public int DiscoveryCallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public CombatEvent ReceivedEvent
            {
                get;
                private set;
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                DiscoverTriggers(
                    CombatState state,
                    CombatEvent sourceEvent)
            {
                DiscoveryCallCount++;

                ReceivedState = state;
                ReceivedEvent = sourceEvent;

                if (DiscoverAction != null)
                {
                    return DiscoverAction(
                        state,
                        sourceEvent);
                }

                return Candidates;
            }
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

            public int ResolveCallCount
            {
                get;
                private set;
            }

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

            public CombatTriggerSourceRegistry
                SourceRegistry
            {
                get;
                set;
            }

            public CombatTriggerEngine Engine
            {
                get;
                set;
            }
        }
    }
}