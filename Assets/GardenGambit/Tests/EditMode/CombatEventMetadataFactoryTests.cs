using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventMetadataFactoryTests
    {
        [Test]
        public void Constructor_WithNullEventIdAllocator_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventMetadataFactory(
                        null,
                        new CombatSequenceNumberAllocator()));
        }

        [Test]
        public void Constructor_WithNullSequenceAllocator_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        null));
        }

        [Test]
        public void CreateRoot_CreatesRootMetadata()
        {
            var factory = CreateFactory();

            var metadata =
                factory.CreateRoot();

            Assert.That(
                metadata.EventId,
                Is.EqualTo(new CombatEventId(1)));

            Assert.That(
                metadata.SequenceNo,
                Is.EqualTo(
                    new CombatSequenceNumber(1)));

            Assert.That(metadata.HasParent, Is.False);
            Assert.That(metadata.IsTriggerRoot, Is.True);

            Assert.That(
                metadata.TriggerRootId,
                Is.EqualTo(metadata.EventId));
        }

        [Test]
        public void CreateChild_CreatesDirectChildOfParent()
        {
            var factory = CreateFactory();

            var root =
                factory.CreateRoot();

            var child =
                factory.CreateChild(root);

            Assert.That(
                child.EventId,
                Is.EqualTo(new CombatEventId(2)));

            Assert.That(
                child.SequenceNo,
                Is.EqualTo(
                    new CombatSequenceNumber(2)));

            Assert.That(
                child.ParentEventId.Value,
                Is.EqualTo(root.EventId));

            Assert.That(
                child.TriggerRootId,
                Is.EqualTo(root.EventId));

            Assert.That(child.IsTriggerRoot, Is.False);
        }

        [Test]
        public void CreateChild_ForNestedChild_PreservesRootAndUsesImmediateParent()
        {
            var factory = CreateFactory();

            var root =
                factory.CreateRoot();

            var child =
                factory.CreateChild(root);

            var grandchild =
                factory.CreateChild(child);

            Assert.That(
                grandchild.EventId,
                Is.EqualTo(new CombatEventId(3)));

            Assert.That(
                grandchild.SequenceNo,
                Is.EqualTo(
                    new CombatSequenceNumber(3)));

            Assert.That(
                grandchild.ParentEventId.Value,
                Is.EqualTo(child.EventId));

            Assert.That(
                grandchild.TriggerRootId,
                Is.EqualTo(root.EventId));
        }

        [Test]
        public void CreateChild_WithInvalidParent_ThrowsWithoutAllocating()
        {
            var eventIdAllocator =
                new CombatEventIdAllocator();

            var sequenceAllocator =
                new CombatSequenceNumberAllocator();

            var factory =
                new CombatEventMetadataFactory(
                    eventIdAllocator,
                    sequenceAllocator);

            Assert.Throws<ArgumentException>(
                () => factory.CreateChild(
                    default(CombatEventMetadata)));

            Assert.That(
                eventIdAllocator.LastIssuedValue,
                Is.Zero);

            Assert.That(
                sequenceAllocator.LastIssuedValue,
                Is.Zero);
        }

        [Test]
        public void CreateChild_WhenEventIdAllocatorIsBehindParent_ThrowsWithoutAllocating()
        {
            var parent = CreateExistingRoot(
                eventId: 5,
                sequenceNo: 5);

            var eventIdAllocator =
                new CombatEventIdAllocator(4);

            var sequenceAllocator =
                new CombatSequenceNumberAllocator(5);

            var factory =
                new CombatEventMetadataFactory(
                    eventIdAllocator,
                    sequenceAllocator);

            Assert.Throws<InvalidOperationException>(
                () => factory.CreateChild(parent));

            Assert.That(
                eventIdAllocator.LastIssuedValue,
                Is.EqualTo(4));

            Assert.That(
                sequenceAllocator.LastIssuedValue,
                Is.EqualTo(5));
        }

        [Test]
        public void CreateChild_WhenSequenceAllocatorIsBehindParent_ThrowsWithoutAllocating()
        {
            var parent = CreateExistingRoot(
                eventId: 5,
                sequenceNo: 5);

            var eventIdAllocator =
                new CombatEventIdAllocator(5);

            var sequenceAllocator =
                new CombatSequenceNumberAllocator(4);

            var factory =
                new CombatEventMetadataFactory(
                    eventIdAllocator,
                    sequenceAllocator);

            Assert.Throws<InvalidOperationException>(
                () => factory.CreateChild(parent));

            Assert.That(
                eventIdAllocator.LastIssuedValue,
                Is.EqualTo(5));

            Assert.That(
                sequenceAllocator.LastIssuedValue,
                Is.EqualTo(4));
        }

        [Test]
        public void CreateRoot_WhenEventIdIsExhausted_DoesNotAdvanceSequence()
        {
            var eventIdAllocator =
                new CombatEventIdAllocator(
                    long.MaxValue);

            var sequenceAllocator =
                new CombatSequenceNumberAllocator(10);

            var factory =
                new CombatEventMetadataFactory(
                    eventIdAllocator,
                    sequenceAllocator);

            Assert.Throws<InvalidOperationException>(
                () => factory.CreateRoot());

            Assert.That(
                eventIdAllocator.LastIssuedValue,
                Is.EqualTo(long.MaxValue));

            Assert.That(
                sequenceAllocator.LastIssuedValue,
                Is.EqualTo(10));
        }

        [Test]
        public void CreateRoot_WhenSequenceIsExhausted_DoesNotAdvanceEventId()
        {
            var eventIdAllocator =
                new CombatEventIdAllocator(10);

            var sequenceAllocator =
                new CombatSequenceNumberAllocator(
                    long.MaxValue);

            var factory =
                new CombatEventMetadataFactory(
                    eventIdAllocator,
                    sequenceAllocator);

            Assert.Throws<InvalidOperationException>(
                () => factory.CreateRoot());

            Assert.That(
                eventIdAllocator.LastIssuedValue,
                Is.EqualTo(10));

            Assert.That(
                sequenceAllocator.LastIssuedValue,
                Is.EqualTo(long.MaxValue));
        }

        private static CombatEventMetadataFactory
            CreateFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private static CombatEventMetadata
            CreateExistingRoot(
                long eventId,
                long sequenceNo)
        {
            var rootEventId =
                new CombatEventId(eventId);

            return new CombatEventMetadata(
                rootEventId,
                new CombatSequenceNumber(sequenceNo),
                null,
                rootEventId);
        }
    }
}