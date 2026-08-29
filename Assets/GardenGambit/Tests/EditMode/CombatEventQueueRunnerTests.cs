using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventQueueRunnerTests
    {
        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventQueueRunner(null));
        }

        [Test]
        public void Drain_WithInvalidMaximumEventCount_ThrowsWithoutConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner.Drain(
                    0,
                    combatEvent => { }));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner.Drain(
                    -1,
                    combatEvent => { }));

            Assert.That(
                environment.Queue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.Queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Queue.PeekNext(),
                Is.SameAs(rootEvent));
        }

        [Test]
        public void Drain_WithNullProcessEvent_ThrowsWithoutConsumingEvent()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner.Drain(
                    1,
                    null));

            Assert.That(
                environment.Queue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.Queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Queue.PeekNext(),
                Is.SameAs(rootEvent));
        }

        [Test]
        public void Drain_WithEmptyQueue_ReturnsZero()
        {
            var environment =
                CreateEnvironment();

            var callbackCount = 0;

            var processedCount =
                environment.Runner.Drain(
                    1,
                    combatEvent =>
                        callbackCount++);

            Assert.That(
                processedCount,
                Is.Zero);

            Assert.That(
                callbackCount,
                Is.Zero);

            Assert.That(
                environment.Queue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WithExistingEvents_ProcessesAllInLogOrder()
        {
            var environment =
                CreateEnvironment();

            var firstEvent =
                AppendRootEvent(environment);

            var secondEvent =
                AppendRootEvent(environment);

            var thirdEvent =
                AppendRootEvent(environment);

            var processedEvents =
                new List<CombatEvent>();

            var processedCount =
                environment.Runner.Drain(
                    3,
                    processedEvents.Add);

            Assert.That(
                processedCount,
                Is.EqualTo(3));

            Assert.That(
                processedEvents.Count,
                Is.EqualTo(3));

            Assert.That(
                processedEvents[0],
                Is.SameAs(firstEvent));

            Assert.That(
                processedEvents[1],
                Is.SameAs(secondEvent));

            Assert.That(
                processedEvents[2],
                Is.SameAs(thirdEvent));

            Assert.That(
                environment.Queue.ProcessedCount,
                Is.EqualTo(3));

            Assert.That(
                environment.Queue.PendingCount,
                Is.Zero);
        }

        [Test]
        public void Drain_WhenCallbackAppendsChild_ProcessesChildInSameDrain()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            TestCombatEvent childEvent = null;

            var processedEvents =
                new List<CombatEvent>();

            var processedCount =
                environment.Runner.Drain(
                    2,
                    combatEvent =>
                    {
                        processedEvents.Add(
                            combatEvent);

                        if (!ReferenceEquals(
                                combatEvent,
                                rootEvent))
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
                processedCount,
                Is.EqualTo(2));

            Assert.That(
                childEvent,
                Is.Not.Null);

            Assert.That(
                processedEvents.Count,
                Is.EqualTo(2));

            Assert.That(
                processedEvents[0],
                Is.SameAs(rootEvent));

            Assert.That(
                processedEvents[1],
                Is.SameAs(childEvent));

            Assert.That(
                environment.Queue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenEventCountEqualsBudget_CompletesSuccessfully()
        {
            var environment =
                CreateEnvironment();

            AppendRootEvent(environment);
            AppendRootEvent(environment);

            var callbackCount = 0;

            var processedCount =
                environment.Runner.Drain(
                    2,
                    combatEvent =>
                        callbackCount++);

            Assert.That(
                processedCount,
                Is.EqualTo(2));

            Assert.That(
                callbackCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Queue.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenBudgetIsExhausted_ThrowsAndLeavesRemainingEventPending()
        {
            var environment =
                CreateEnvironment();

            var firstEvent =
                AppendRootEvent(environment);

            var secondEvent =
                AppendRootEvent(environment);

            var thirdEvent =
                AppendRootEvent(environment);

            var processedEvents =
                new List<CombatEvent>();

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner.Drain(
                    2,
                    processedEvents.Add));

            Assert.That(
                processedEvents.Count,
                Is.EqualTo(2));

            Assert.That(
                processedEvents[0],
                Is.SameAs(firstEvent));

            Assert.That(
                processedEvents[1],
                Is.SameAs(secondEvent));

            Assert.That(
                environment.Queue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Queue.PeekNext(),
                Is.SameAs(thirdEvent));
        }

        [Test]
        public void Drain_WhenCallbackThrows_LeavesCurrentEventPending()
        {
            var environment =
                CreateEnvironment();

            var rootEvent =
                AppendRootEvent(environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner.Drain(
                    1,
                    combatEvent =>
                        throw new InvalidOperationException(
                            "Test callback failure.")));

            Assert.That(
                environment.Queue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.Queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Queue.PeekNext(),
                Is.SameAs(rootEvent));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var eventIdAllocator =
                new CombatEventIdAllocator();

            var sequenceNumberAllocator =
                new CombatSequenceNumberAllocator();

            var metadataFactory =
                new CombatEventMetadataFactory(
                    eventIdAllocator,
                    sequenceNumberAllocator);

            var eventLog =
                new CombatEventLog();

            var queue =
                new CombatEventQueue(eventLog);

            return new TestEnvironment
            {
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                Queue = queue,
                Runner =
                    new CombatEventQueueRunner(queue)
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

            public CombatEventQueue Queue
            {
                get;
                set;
            }

            public CombatEventQueueRunner Runner
            {
                get;
                set;
            }
        }
    }
}