using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventMetadataTests
    {
        [Test]
        public void Constructor_WithRootEvent_CreatesValidMetadata()
        {
            var eventId = new CombatEventId(1);
            var sequenceNo =
                new CombatSequenceNumber(1);

            var metadata = new CombatEventMetadata(
                eventId,
                sequenceNo,
                null,
                eventId);

            Assert.That(
                metadata.EventId,
                Is.EqualTo(eventId));

            Assert.That(
                metadata.SequenceNo,
                Is.EqualTo(sequenceNo));

            Assert.That(
                metadata.ParentEventId.HasValue,
                Is.False);

            Assert.That(
                metadata.TriggerRootId,
                Is.EqualTo(eventId));

            Assert.That(metadata.HasParent, Is.False);
            Assert.That(metadata.IsTriggerRoot, Is.True);
            Assert.That(metadata.IsValid, Is.True);
        }

        [Test]
        public void Constructor_WithChildEvent_CreatesValidMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            var parentEventId =
                new CombatEventId(2);

            var eventId =
                new CombatEventId(3);

            var metadata = new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(3),
                parentEventId,
                rootEventId);

            Assert.That(
                metadata.ParentEventId.Value,
                Is.EqualTo(parentEventId));

            Assert.That(
                metadata.TriggerRootId,
                Is.EqualTo(rootEventId));

            Assert.That(metadata.HasParent, Is.True);
            Assert.That(metadata.IsTriggerRoot, Is.False);
            Assert.That(metadata.IsValid, Is.True);
        }

        [Test]
        public void Constructor_WithInvalidEventId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    default(CombatEventId),
                    new CombatSequenceNumber(1),
                    null,
                    new CombatEventId(1)));
        }

        [Test]
        public void Constructor_WithInvalidSequenceNumber_Throws()
        {
            var eventId = new CombatEventId(1);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    eventId,
                    default(CombatSequenceNumber),
                    null,
                    eventId));
        }

        [Test]
        public void Constructor_WithInvalidTriggerRootId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    new CombatEventId(1),
                    new CombatSequenceNumber(1),
                    new CombatEventId(2),
                    default(CombatEventId)));
        }

        [Test]
        public void Constructor_WithInvalidParentEventId_Throws()
        {
            CombatEventId? invalidParentEventId =
                default(CombatEventId);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    invalidParentEventId,
                    new CombatEventId(1)));
        }

        [Test]
        public void Constructor_WithSelfParent_Throws()
        {
            var eventId = new CombatEventId(2);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    eventId,
                    new CombatSequenceNumber(2),
                    eventId,
                    new CombatEventId(1)));
        }

        [Test]
        public void Constructor_WithParentOnRootEvent_Throws()
        {
            var eventId = new CombatEventId(1);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    eventId,
                    new CombatSequenceNumber(1),
                    new CombatEventId(2),
                    eventId));
        }

        [Test]
        public void Constructor_WithNonRootEventWithoutParent_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ = new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    null,
                    new CombatEventId(1)));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var metadata =
                default(CombatEventMetadata);

            Assert.That(metadata.IsValid, Is.False);
            Assert.That(metadata.HasParent, Is.False);
            Assert.That(metadata.IsTriggerRoot, Is.False);
        }
    }
}