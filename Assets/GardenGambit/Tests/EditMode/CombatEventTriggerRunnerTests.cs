using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventTriggerRunnerTests
    {
        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventTriggerRunner<
                        TestTrigger>(null));
        }

        [Test]
        public void Drain_WithInvalidBudgets_ThrowsWithoutChangingQueue()
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
                    combatEvent =>
                    {
                        discoveryCount++;

                        return EmptyCandidates();
                    },
                    trigger => { }));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner.Drain(
                    1,
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
                environment.Runner.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void Drain_WithNullCallbacks_ThrowsWithoutChangingQueue()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner.Drain(
                    1,
                    1,
                    null,
                    trigger => { }));

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner.Drain(
                    1,
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
                environment.Runner.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void Drain_WithEmptyEventQueue_ReturnsZero()
        {
            var environment =
                CreateEnvironment();

            var discoveryCount = 0;
            var processCount = 0;

            var processedEventCount =
                environment.Runner.Drain(
                    1,
                    1,
                    combatEvent =>
                    {
                        discoveryCount++;

                        return EmptyCandidates();
                    },
                    trigger =>
                        processCount++);

            Assert.That(
                processedEventCount,
                Is.Zero);

            Assert.That(
                discoveryCount,
                Is.Zero);

            Assert.That(
                processCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WithSuccessiveEvents_ProcessesAllInEventOrder()
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

            var processedTriggers =
                new List<TestTrigger>();

            var discoveryCount = 0;

            var processedEventCount =
                environment.Runner.Drain(
                    2,
                    1,
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
                                    0)
                            };
                        }

                        Assert.That(
                            combatEvent,
                            Is.SameAs(secondEvent));

                        return new[]
                        {
                            CreateCandidate(
                                secondTrigger,
                                0)
                        };
                    },
                    processedTriggers.Add);

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

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
        public void Drain_WhenTriggerAppendsEvent_ProcessesNewEventInSameDrain()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            var rootTrigger =
                new TestTrigger("Root");

            var childTrigger =
                new TestTrigger("Child");

            TestCombatEvent childEvent = null;

            var processedTriggers =
                new List<TestTrigger>();

            var processedEventCount =
                environment.Runner.Drain(
                    2,
                    1,
                    combatEvent =>
                    {
                        if (ReferenceEquals(
                                combatEvent,
                                rootEvent))
                        {
                            return new[]
                            {
                                CreateCandidate(
                                    rootTrigger,
                                    0)
                            };
                        }

                        Assert.That(
                            combatEvent,
                            Is.SameAs(childEvent));

                        return new[]
                        {
                            CreateCandidate(
                                childTrigger,
                                0)
                        };
                    },
                    trigger =>
                    {
                        processedTriggers.Add(
                            trigger);

                        if (!ReferenceEquals(
                                trigger,
                                rootTrigger))
                        {
                            return;
                        }

                        childEvent =
                            new TestCombatEvent(
                                environment.MetadataFactory
                                    .CreateChild(
                                        rootEvent.Metadata));

                        environment.EventLog.Append(
                            childEvent);
                    });

            Assert.That(
                childEvent,
                Is.Not.Null);

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers.Count,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(rootTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(childTrigger));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenEventBudgetIsExhausted_LeavesNextEventPending()
        {
            var environment =
                CreateEnvironment();

            var firstEvent =
                AppendRootEvent(environment);

            var secondEvent =
                AppendRootEvent(environment);

            var discoveryCount = 0;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner.Drain(
                    1,
                    1,
                    combatEvent =>
                    {
                        discoveryCount++;

                        return EmptyCandidates();
                    },
                    trigger => { }));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(secondEvent));

            Assert.That(
                environment.Runner.HasActiveBatch,
                Is.False);

            Assert.That(
                firstEvent,
                Is.Not.SameAs(secondEvent));
        }

        [Test]
        public void Drain_WhenTriggerBudgetIsExhausted_RetryContinuesActiveBatch()
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
                                0),
                            CreateCandidate(
                                secondTrigger,
                                1)
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner.Drain(
                    1,
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
                    discoverTriggers,
                    processedTriggers.Add);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

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
                environment.Runner.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenTriggerCallbackThrows_RetryUsesSameBatch()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var trigger =
                new TestTrigger("Trigger");

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
                            CreateCandidate(
                                trigger,
                                0)
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner.Drain(
                    1,
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
                environment.Runner.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.Runner.PendingTriggerCount,
                Is.EqualTo(1));

            var processedEventCount =
                environment.Runner.Drain(
                    1,
                    1,
                    discoverTriggers,
                    currentTrigger =>
                        processAttemptCount++);

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

            Assert.That(
                discoveryCount,
                Is.EqualTo(1));

            Assert.That(
                processAttemptCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Runner.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void DrainWithSource_PassesExactSourceEventForSuccessiveEvents()
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

            var receivedEvents =
                new List<CombatEvent>();

            var processedTriggers =
                new List<TestTrigger>();

            var processedEventCount =
                environment.Runner.DrainWithSource(
                    2,
                    1,
                    combatEvent =>
                    {
                        if (ReferenceEquals(
                                combatEvent,
                                firstEvent))
                        {
                            return new[]
                            {
                                CreateCandidate(
                                    firstTrigger,
                                    0)
                            };
                        }

                        Assert.That(
                            combatEvent,
                            Is.SameAs(secondEvent));

                        return new[]
                        {
                            CreateCandidate(
                                secondTrigger,
                                0)
                        };
                    },
                    (combatEvent, trigger) =>
                    {
                        receivedEvents.Add(
                            combatEvent);

                        processedTriggers.Add(
                            trigger);
                    });

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

            Assert.That(
                receivedEvents.Count,
                Is.EqualTo(2));

            Assert.That(
                receivedEvents[0],
                Is.SameAs(firstEvent));

            Assert.That(
                receivedEvents[1],
                Is.SameAs(secondEvent));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(firstTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(secondTrigger));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void DrainWithSource_WithNullCallback_ThrowsWithoutDiscoveringOrConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var discoveryCount = 0;

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .DrainWithSource(
                        1,
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
                environment.Runner.HasActiveBatch,
                Is.False);
        }

        [Test]
        public void DrainWithSource_WhenCallbackThrows_RetryKeepsExactSourceWithoutRediscovery()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                AppendRootEvent(environment);

            var trigger =
                new TestTrigger("Trigger");

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
                            CreateCandidate(
                                trigger,
                                0)
                        };
                    };

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .DrainWithSource(
                        1,
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
                environment.Runner.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(sourceEvent));

            var processedEventCount =
                environment.Runner
                    .DrainWithSource(
                        1,
                        1,
                        discoverTriggers,
                        (combatEvent, currentTrigger) =>
                        {
                            receivedEvents.Add(
                                combatEvent);

                            processAttemptCount++;
                        });

            Assert.That(
                processedEventCount,
                Is.EqualTo(1));

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
                environment.Runner.HasActiveBatch,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void DrainWithSource_WhenTriggerAppendsChildEvent_UsesReceivedEventAsParent()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            var rootTrigger =
                new TestTrigger("Root");

            TestCombatEvent childEvent = null;

            var processedEventCount =
                environment.Runner.DrainWithSource(
                    2,
                    1,
                    combatEvent =>
                    {
                        if (ReferenceEquals(
                                combatEvent,
                                rootEvent))
                        {
                            return new[]
                            {
                                CreateCandidate(
                                    rootTrigger,
                                    0)
                            };
                        }

                        Assert.That(
                            combatEvent,
                            Is.SameAs(childEvent));

                        return EmptyCandidates();
                    },
                    (combatEvent, trigger) =>
                    {
                        Assert.That(
                            combatEvent,
                            Is.SameAs(rootEvent));

                        Assert.That(
                            trigger,
                            Is.SameAs(rootTrigger));

                        childEvent =
                            new TestCombatEvent(
                                environment.MetadataFactory
                                    .CreateChild(
                                        combatEvent.Metadata));

                        environment.EventLog.Append(
                            childEvent);
                    });

            Assert.That(
                processedEventCount,
                Is.EqualTo(2));

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
                Runner =
                    new CombatEventTriggerRunner<
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
                int horizontalOrder)
        {
            return new CombatTriggerCandidate<TestTrigger>(
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Slot,
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

            public CombatEventTriggerRunner<
                TestTrigger> Runner
            {
                get;
                set;
            }
        }
    }
}