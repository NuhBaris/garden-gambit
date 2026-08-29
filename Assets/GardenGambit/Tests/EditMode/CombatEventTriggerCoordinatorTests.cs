using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatEventTriggerCoordinatorTests
    {
        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventTriggerCoordinator<
                        TestTrigger>(null));
        }

        [Test]
        public void ProcessNextEvent_WithInvalidBudget_ThrowsWithoutDiscoveringOrConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var discoveryCount = 0;

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        0,
                        combatEvent =>
                        {
                            discoveryCount++;

                            return EmptyCandidates();
                        },
                        trigger => { }));

            Assert.That(
                discoveryCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void ProcessNextEvent_WithNullCallbacks_ThrowsWithoutConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            Assert.Throws<ArgumentNullException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        null,
                        trigger => { }));

            Assert.Throws<ArgumentNullException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        combatEvent =>
                            EmptyCandidates(),
                        null));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void ProcessNextEvent_WithEmptyEventQueue_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        combatEvent =>
                            EmptyCandidates(),
                        trigger => { }));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.Coordinator
                    .PendingTriggerCount,
                Is.Zero);
        }

        [Test]
        public void ProcessNextEvent_WithNoTriggers_ConsumesEvent()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var discoveryCount = 0;
            var processCount = 0;

            var processedEvent =
                environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        combatEvent =>
                        {
                            discoveryCount++;

                            Assert.That(
                                combatEvent,
                                Is.SameAs(sourceEvent));

                            return EmptyCandidates();
                        },
                        trigger =>
                            processCount++);

            Assert.That(
                processedEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void ProcessNextEvent_WithTriggers_ProcessesPriorityOrderBeforeConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var petTrigger =
                new TestTrigger("Pet");

            var slotTrigger =
                new TestTrigger("Slot");

            var processedTriggers =
                new List<TestTrigger>();

            var processedEvent =
                environment.Coordinator
                    .ProcessNextEvent(
                        2,
                        combatEvent =>
                            new[]
                            {
                                CreateCandidate(
                                    petTrigger,
                                    CombatTriggerSourceKind.Pet,
                                    0),
                                CreateCandidate(
                                    slotTrigger,
                                    CombatTriggerSourceKind.Slot,
                                    4)
                            },
                        processedTriggers.Add);

            Assert.That(
                processedEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                processedTriggers.Count,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(slotTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(petTrigger));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);

            Assert.That(
                environment.Coordinator
                    .PendingTriggerCount,
                Is.Zero);
        }

        [Test]
        public void ProcessNextEvent_WhenCallbackThrows_RetryUsesSameBatchWithoutRediscovery()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var trigger =
                new TestTrigger("Trigger");

            var candidate =
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Card,
                    0);

            var discoveryCount = 0;
            var processAttemptCount = 0;

            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TestTrigger>>>
                discoverTriggers =
                    combatEvent =>
                    {
                        discoveryCount++;

                        return new[]
                        {
                            candidate
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        discoverTriggers,
                        currentTrigger =>
                        {
                            processAttemptCount++;

                            throw new InvalidOperationException(
                                "Test trigger failure.");
                        }));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processAttemptCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.Coordinator
                    .ActiveBatch.SourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator
                    .PendingTriggerCount,
                Is.EqualTo(1));

            var processedEvent =
                environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        discoverTriggers,
                        currentTrigger =>
                            processAttemptCount++);

            Assert.That(
                processedEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processAttemptCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void ProcessNextEvent_WhenBudgetIsExhausted_RetryContinuesSameBatch()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var firstTrigger =
                new TestTrigger("First");

            var secondTrigger =
                new TestTrigger("Second");

            var discoveryCount = 0;

            var processedTriggers =
                new List<TestTrigger>();

            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TestTrigger>>>
                discoverTriggers =
                    combatEvent =>
                    {
                        discoveryCount++;

                        return new[]
                        {
                            CreateCandidate(
                                firstTrigger,
                                CombatTriggerSourceKind.Slot,
                                0),
                            CreateCandidate(
                                secondTrigger,
                                CombatTriggerSourceKind.Slot,
                                1)
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        discoverTriggers,
                        processedTriggers.Add));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processedTriggers.Count,
                Is.EqualTo(1));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(firstTrigger));

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.Coordinator
                    .PendingTriggerCount,
                Is.EqualTo(1));

            var processedEvent =
                environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        discoverTriggers,
                        processedTriggers.Add);

            Assert.That(
                processedEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processedTriggers.Count,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(secondTrigger));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void ProcessNextEvent_WhenDiscoveryReturnsNull_LeavesEventPending()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        combatEvent => null,
                        trigger => { }));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.Coordinator
                    .PendingTriggerCount,
                Is.Zero);
        }

        [Test]
        public void ProcessNextEvent_WithSuccessiveEvents_CreatesSeparateBatches()
        {
            var environment =
                CreateEnvironment();

            var firstEvent =
                AppendRootEvent(environment);

            var secondEvent =
                AppendRootEvent(environment);

            var firstTrigger =
                new TestTrigger("First");

            var secondTrigger =
                new TestTrigger("Second");

            var discoveryCount = 0;

            var processedTriggers =
                new List<TestTrigger>();

            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TestTrigger>>>
                discoverTriggers =
                    combatEvent =>
                    {
                        discoveryCount++;

                        if (ReferenceEquals(
                                combatEvent,
                                firstEvent))
                        {
                            return new[]
                            {
                                CreateCandidate(
                                    firstTrigger,
                                    CombatTriggerSourceKind.Slot,
                                    0)
                            };
                        }

                        return new[]
                        {
                            CreateCandidate(
                                secondTrigger,
                                CombatTriggerSourceKind.Pet,
                                0)
                        };
                    };

            Assert.That(
                environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        discoverTriggers,
                        processedTriggers.Add),
                Is.SameAs(firstEvent));

            Assert.That(
                environment.Coordinator
                    .ProcessNextEvent(
                        1,
                        discoverTriggers,
                        processedTriggers.Add),
                Is.SameAs(secondEvent));

            Assert.That(
                discoveryCount,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers.Count,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(firstTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(secondTrigger));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void ProcessNextEventWithSource_PassesExactSourceEventForEveryTrigger()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var petTrigger =
                new TestTrigger("Pet");

            var slotTrigger =
                new TestTrigger("Slot");

            var receivedEvents =
                new List<CombatEvent>();

            var processedTriggers =
                new List<TestTrigger>();

            var processedEvent =
                environment.Coordinator
                    .ProcessNextEventWithSource(
                        2,
                        combatEvent =>
                            new[]
                            {
                                CreateCandidate(
                                    petTrigger,
                                    CombatTriggerSourceKind.Pet,
                                    0),
                                CreateCandidate(
                                    slotTrigger,
                                    CombatTriggerSourceKind.Slot,
                                    0)
                            },
                        (combatEvent, trigger) =>
                        {
                            receivedEvents.Add(
                                combatEvent);

                            processedTriggers.Add(
                                trigger);
                        });

            Assert.That(
                processedEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                receivedEvents.Count,
                Is.EqualTo(2));

            Assert.That(
                receivedEvents[0],
                Is.SameAs(sourceEvent));

            Assert.That(
                receivedEvents[1],
                Is.SameAs(sourceEvent));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(slotTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(petTrigger));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void ProcessNextEventWithSource_WithNullCallback_ThrowsWithoutDiscoveringOrConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var discoveryCount = 0;

            Assert.Throws<ArgumentNullException>(
                () => environment.Coordinator
                    .ProcessNextEventWithSource(
                        1,
                        combatEvent =>
                        {
                            discoveryCount++;

                            return EmptyCandidates();
                        },
                        null));

            Assert.That(
                discoveryCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void ProcessNextEventWithSource_WhenCallbackThrows_RetryKeepsExactSourceWithoutRediscovery()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var trigger =
                new TestTrigger("Trigger");

            var candidate =
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Card,
                    0);

            var discoveryCount = 0;
            var processAttemptCount = 0;

            var receivedEvents =
                new List<CombatEvent>();

            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TestTrigger>>>
                discoverTriggers =
                    combatEvent =>
                    {
                        discoveryCount++;

                        return new[]
                        {
                            candidate
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Coordinator
                    .ProcessNextEventWithSource(
                        1,
                        discoverTriggers,
                        (combatEvent, currentTrigger) =>
                        {
                            receivedEvents.Add(
                                combatEvent);

                            processAttemptCount++;

                            throw new InvalidOperationException(
                                "Test trigger failure.");
                        }));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            var processedEvent =
                environment.Coordinator
                    .ProcessNextEventWithSource(
                        1,
                        discoverTriggers,
                        (combatEvent, currentTrigger) =>
                        {
                            receivedEvents.Add(
                                combatEvent);

                            processAttemptCount++;
                        });

            Assert.That(
                processedEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processAttemptCount,
                Is.EqualTo(2));

            Assert.That(
                receivedEvents.Count,
                Is.EqualTo(2));

            Assert.That(
                receivedEvents[0],
                Is.SameAs(sourceEvent));

            Assert.That(
                receivedEvents[1],
                Is.SameAs(sourceEvent));

            Assert.That(
                environment.Coordinator.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment()
        {
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
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                Coordinator =
                    new CombatEventTriggerCoordinator<
                        TestTrigger>(eventQueue)
            };
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

        private static CombatTriggerCandidate<TestTrigger>
            CreateCandidate(
                TestTrigger trigger,
                CombatTriggerSourceKind sourceKind,
                int horizontalOrder)
        {
            return new CombatTriggerCandidate<TestTrigger>(
                new CombatTriggerOrderKey(
                    sourceKind,
                    CombatSide.Player,
                    horizontalOrder,
                    0),
                trigger);
        }

        private static IEnumerable<
            CombatTriggerCandidate<TestTrigger>>
            EmptyCandidates()
        {
            return new CombatTriggerCandidate<
                TestTrigger>[0];
        }

        private sealed class TestTrigger
        {
            public TestTrigger(string name)
            {
                Name = name;
            }

            public string Name { get; }
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

            public CombatEventTriggerCoordinator<
                TestTrigger> Coordinator
            {
                get;
                set;
            }
        }
    }
}