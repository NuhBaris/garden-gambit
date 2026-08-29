using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatEventLogPreflightTests
    {
        [Test]
        public void EnsureCanAppend_WithValidRoot_DoesNotChangeLog()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var rootEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            Assert.DoesNotThrow(
                () => eventLog.EnsureCanAppend(
                    rootEvent));

            Assert.That(
                eventLog.Count,
                Is.Zero);

            Assert.That(
                eventLog.ContainsEvent(
                    rootEvent.Metadata.EventId),
                Is.False);
        }

        [Test]
        public void EnsureCanAppend_WithValidChild_DoesNotChangeLog()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var rootEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(rootEvent);

            var childEvent =
                new TestCombatEvent(
                    metadataFactory.CreateChild(
                        rootEvent.Metadata));

            Assert.DoesNotThrow(
                () => eventLog.EnsureCanAppend(
                    childEvent));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(rootEvent));

            Assert.That(
                eventLog.ContainsEvent(
                    childEvent.Metadata.EventId),
                Is.False);
        }

        [Test]
        public void EnsureCanAppend_WithNullEvent_ThrowsWithoutChangingLog()
        {
            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => eventLog.EnsureCanAppend(
                    null));

            Assert.That(
                eventLog.Count,
                Is.Zero);
        }

        [Test]
        public void EnsureCanAppend_WithDuplicateEventId_ThrowsWithoutChangingLog()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var firstEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(firstEvent);

            var duplicateEvent =
                new TestCombatEvent(
                    firstEvent.Metadata);

            Assert.Throws<ArgumentException>(
                () => eventLog.EnsureCanAppend(
                    duplicateEvent));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(firstEvent));
        }

        [Test]
        public void EnsureCanAppend_WithNonIncreasingSequence_ThrowsWithoutChangingLog()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var firstEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(firstEvent);

            var invalidMetadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(1),
                    null,
                    new CombatEventId(2));

            var invalidEvent =
                new TestCombatEvent(
                    invalidMetadata);

            Assert.Throws<ArgumentException>(
                () => eventLog.EnsureCanAppend(
                    invalidEvent));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(firstEvent));
        }

        [Test]
        public void EnsureCanAppend_WithMissingParent_ThrowsWithoutChangingLog()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var rootEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(rootEvent);

            var invalidMetadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    new CombatEventId(99),
                    rootEvent.Metadata.EventId);

            var invalidEvent =
                new TestCombatEvent(
                    invalidMetadata);

            Assert.Throws<ArgumentException>(
                () => eventLog.EnsureCanAppend(
                    invalidEvent));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(rootEvent));
        }

        [Test]
        public void EnsureCanAppend_WithDifferentTriggerRoot_ThrowsWithoutChangingLog()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var firstRoot =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            var secondRoot =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(firstRoot);
            eventLog.Append(secondRoot);

            var invalidMetadata =
                new CombatEventMetadata(
                    new CombatEventId(3),
                    new CombatSequenceNumber(3),
                    firstRoot.Metadata.EventId,
                    secondRoot.Metadata.EventId);

            var invalidEvent =
                new TestCombatEvent(
                    invalidMetadata);

            Assert.Throws<ArgumentException>(
                () => eventLog.EnsureCanAppend(
                    invalidEvent));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                eventLog.Events[0],
                Is.SameAs(firstRoot));

            Assert.That(
                eventLog.Events[1],
                Is.SameAs(secondRoot));
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
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
    }
}