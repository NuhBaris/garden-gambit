using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatEventQueueProcessorTests
    {
        [Test]
        public void Constructor_WithNullQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventQueueProcessor(
                        null));
        }

        [Test]
        public void ProcessNext_WithNullCallback_ThrowsWithoutConsumingEvent()
        {
            var environment =
                CreateEnvironment(
                    appendRootEvent: true);

            Assert.Throws<ArgumentNullException>(
                () => environment.Processor
                    .ProcessNext(null));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.EqualTo(1));
        }

        [Test]
        public void ProcessNext_WithSuccessfulCallback_ProcessesAndConsumesExactEvent()
        {
            var environment =
                CreateEnvironment(
                    appendRootEvent: true);

            CombatEvent receivedEvent = null;

            var processedEvent =
                environment.Processor.ProcessNext(
                    combatEvent =>
                    {
                        receivedEvent =
                            combatEvent;
                    });

            Assert.That(
                receivedEvent,
                Is.SameAs(
                    environment.RootEvent));

            Assert.That(
                processedEvent,
                Is.SameAs(
                    environment.RootEvent));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ProcessNext_WhenCallbackThrows_DoesNotConsumeEvent()
        {
            var environment =
                CreateEnvironment(
                    appendRootEvent: true);

            Assert.Throws<InvalidOperationException>(
                () => environment.Processor
                    .ProcessNext(
                        _ => throw
                            new InvalidOperationException(
                                "Test processing failure.")));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(
                    environment.RootEvent));
        }

        [Test]
        public void ProcessNext_WhenCallbackAppendsChild_LeavesChildPendingAfterParent()
        {
            var environment =
                CreateEnvironment(
                    appendRootEvent: true);

            TestCombatEvent childEvent = null;

            var processedParent =
                environment.Processor.ProcessNext(
                    combatEvent =>
                    {
                        childEvent =
                            new TestCombatEvent(
                                environment.MetadataFactory
                                    .CreateChild(
                                        combatEvent.Metadata));

                        environment.EventLog.Append(
                            childEvent);
                    });

            Assert.That(
                processedParent,
                Is.SameAs(
                    environment.RootEvent));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.PeekNext(),
                Is.SameAs(childEvent));

            var processedChild =
                environment.Processor.ProcessNext(
                    _ =>
                    {
                    });

            Assert.That(
                processedChild,
                Is.SameAs(childEvent));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.Zero);
        }

        [Test]
        public void ProcessNext_WithEmptyQueue_ThrowsBeforeInvokingCallback()
        {
            var environment =
                CreateEnvironment(
                    appendRootEvent: false);

            var callbackWasInvoked =
                false;

            Assert.Throws<InvalidOperationException>(
                () => environment.Processor
                    .ProcessNext(
                        _ =>
                        {
                            callbackWasInvoked =
                                true;
                        }));

            Assert.That(
                callbackWasInvoked,
                Is.False);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.Zero);

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.Zero);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool appendRootEvent)
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            TestCombatEvent rootEvent = null;

            if (appendRootEvent)
            {
                rootEvent =
                    new TestCombatEvent(
                        metadataFactory.CreateRoot());

                eventLog.Append(
                    rootEvent);
            }

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            return new TestEnvironment
            {
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                RootEvent = rootEvent,
                Processor =
                    new CombatEventQueueProcessor(
                        eventQueue)
            };
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

            public CombatEventLog EventLog { get; set; }

            public CombatEventQueue EventQueue { get; set; }

            public TestCombatEvent RootEvent { get; set; }

            public CombatEventQueueProcessor
                Processor
            {
                get;
                set;
            }
        }
    }
}