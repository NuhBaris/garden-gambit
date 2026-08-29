using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventQueueTests
    {
        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventQueue(null));
        }

        [Test]
        public void EmptyQueue_HasNoPendingEventsAndOperationsThrow()
        {
            var queue =
                new CombatEventQueue(
                    new CombatEventLog());

            Assert.That(
                queue.ProcessedCount,
                Is.Zero);

            Assert.That(
                queue.PendingCount,
                Is.Zero);

            Assert.That(
                queue.HasPending,
                Is.False);

            Assert.Throws<InvalidOperationException>(
                () => queue.PeekNext());

            Assert.Throws<InvalidOperationException>(
                () => queue.DequeueNext());

            Assert.That(
                queue.ProcessedCount,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithExistingEvents_ProcessesInLogOrder()
        {
            var environment =
                CreateEnvironment();

            var firstEvent =
                AppendRootEvent(
                    environment);

            var secondEvent =
                AppendRootEvent(
                    environment);

            var thirdEvent =
                AppendRootEvent(
                    environment);

            var queue =
                new CombatEventQueue(
                    environment.EventLog);

            Assert.That(
                queue.PendingCount,
                Is.EqualTo(3));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(firstEvent));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(secondEvent));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(thirdEvent));

            Assert.That(
                queue.ProcessedCount,
                Is.EqualTo(3));

            Assert.That(
                queue.PendingCount,
                Is.Zero);

            Assert.That(
                queue.HasPending,
                Is.False);
        }

        [Test]
        public void PeekNext_DoesNotConsumeEvent()
        {
            var environment =
                CreateEnvironment();

            var combatEvent =
                AppendRootEvent(
                    environment);

            var queue =
                new CombatEventQueue(
                    environment.EventLog);

            Assert.That(
                queue.PeekNext(),
                Is.SameAs(combatEvent));

            Assert.That(
                queue.PeekNext(),
                Is.SameAs(combatEvent));

            Assert.That(
                queue.ProcessedCount,
                Is.Zero);

            Assert.That(
                queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(combatEvent));
        }

        [Test]
        public void EventAppendedAfterQueueCreation_BecomesPending()
        {
            var environment =
                CreateEnvironment();

            var queue =
                new CombatEventQueue(
                    environment.EventLog);

            Assert.That(
                queue.HasPending,
                Is.False);

            var combatEvent =
                AppendRootEvent(
                    environment);

            Assert.That(
                queue.HasPending,
                Is.True);

            Assert.That(
                queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(combatEvent));
        }

        [Test]
        public void ChildAppendedWhileProcessing_IsHandledAfterParent()
        {
            var environment =
                CreateEnvironment();

            var parentEvent =
                AppendRootEvent(
                    environment);

            var queue =
                new CombatEventQueue(
                    environment.EventLog);

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(parentEvent));

            var childEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            parentEvent.Metadata));

            environment.EventLog.Append(
                childEvent);

            Assert.That(
                queue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                queue.PendingCount,
                Is.EqualTo(1));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(childEvent));

            Assert.That(
                queue.HasPending,
                Is.False);
        }

        [Test]
        public void DequeueNext_DoesNotRemoveEventsFromLog()
        {
            var environment =
                CreateEnvironment();

            var firstEvent =
                AppendRootEvent(
                    environment);

            var secondEvent =
                AppendRootEvent(
                    environment);

            var queue =
                new CombatEventQueue(
                    environment.EventLog);

            queue.DequeueNext();
            queue.DequeueNext();

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(firstEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(secondEvent));

            Assert.That(
                queue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                queue.PendingCount,
                Is.Zero);
        }

        private static TestCombatEvent
            AppendRootEvent(
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

        private static TestEnvironment
            CreateEnvironment()
        {
            return new TestEnvironment
            {
                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator()),
                EventLog =
                    new CombatEventLog()
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
        }
    }
}