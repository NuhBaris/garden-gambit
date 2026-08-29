using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatCardTombstoneRegistryTests
    {
        [Test]
        public void DefaultConstructor_CreatesEmptyRegistry()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.Tombstones,
                Is.Empty);
        }

        [Test]
        public void Constructor_WithNullCollection_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardTombstoneRegistry(
                        null));
        }

        [Test]
        public void Constructor_WithNullTombstone_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatCardTombstoneRegistry(
                        new CombatCardTombstone[]
                        {
                            null
                        }));
        }

        [Test]
        public void Constructor_WithValidTombstones_PreservesOrder()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var first =
                CreateTombstone(
                    metadataFactory,
                    100);

            var second =
                CreateTombstone(
                    metadataFactory,
                    200);

            var registry =
                new CombatCardTombstoneRegistry(
                    new[]
                    {
                        first,
                        second
                    });

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Tombstones[0],
                Is.SameAs(first));

            Assert.That(
                registry.Tombstones[1],
                Is.SameAs(second));
        }

        [Test]
        public void Append_AddsTombstoneAndSupportsBothLookups()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var tombstone =
                CreateTombstone(
                    metadataFactory,
                    100);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(tombstone);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Contains(
                    tombstone.InstanceId),
                Is.True);

            Assert.That(
                registry.Get(
                    tombstone.InstanceId),
                Is.SameAs(tombstone));

            Assert.That(
                registry.GetByRemovalEvent(
                    tombstone.RemovalMetadata.EventId),
                Is.SameAs(tombstone));
        }

        [Test]
        public void Append_WithNullTombstone_Throws()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<ArgumentNullException>(
                () => registry.Append(null));

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void Append_WithDuplicateInstanceId_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var first =
                CreateTombstone(
                    metadataFactory,
                    100);

            var duplicate =
                CreateTombstone(
                    metadataFactory,
                    100);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(first);

            Assert.Throws<ArgumentException>(
                () => registry.Append(
                    duplicate));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Get(
                    first.InstanceId),
                Is.SameAs(first));
        }

        [Test]
        public void Append_WithDuplicateRemovalEventId_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            var removalMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            var first =
                CreateTombstone(
                    100,
                    removalMetadata);

            var duplicateEvent =
                CreateTombstone(
                    200,
                    removalMetadata);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(first);

            Assert.Throws<ArgumentException>(
                () => registry.Append(
                    duplicateEvent));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Append_WithNonIncreasingSequence_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var earlier =
                CreateTombstone(
                    metadataFactory,
                    100);

            var later =
                CreateTombstone(
                    metadataFactory,
                    200);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(later);

            Assert.Throws<ArgumentException>(
                () => registry.Append(
                    earlier));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Tombstones[0],
                Is.SameAs(later));
        }

        [Test]
        public void Contains_WithInvalidInstanceId_Throws()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.Contains(
                    default(InstanceId)));
        }

        [Test]
        public void Get_WithInvalidInstanceId_Throws()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.Get(
                    default(InstanceId)));
        }

        [Test]
        public void Get_WithMissingInstanceId_Throws()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<KeyNotFoundException>(
                () => registry.Get(
                    new InstanceId(999)));
        }

        [Test]
        public void GetByRemovalEvent_WithInvalidEventId_Throws()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.GetByRemovalEvent(
                    default(CombatEventId)));
        }

        [Test]
        public void GetByRemovalEvent_WithMissingEventId_Throws()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<KeyNotFoundException>(
                () => registry.GetByRemovalEvent(
                    new CombatEventId(999)));
        }

        [Test]
        public void Tombstones_CannotBeChangedThroughCollectionView()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var first =
                CreateTombstone(
                    metadataFactory,
                    100);

            var second =
                CreateTombstone(
                    metadataFactory,
                    200);

            var registry =
                new CombatCardTombstoneRegistry(
                    new[]
                    {
                        first
                    });

            var collection =
                registry.Tombstones
                    as ICollection<
                        CombatCardTombstone>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.Throws<NotSupportedException>(
                () => collection.Add(
                    second));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Tombstones[0],
                Is.SameAs(first));
        }

        [Test]
        public void EnsureCanAppend_WithValidTombstone_DoesNotChangeRegistry()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var tombstone =
                CreateTombstone(
                    metadataFactory,
                    100);

            var registry =
                new CombatCardTombstoneRegistry();

            Assert.DoesNotThrow(
                () => registry.EnsureCanAppend(
                    tombstone));

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.Contains(
                    tombstone.InstanceId),
                Is.False);
        }

        [Test]
        public void EnsureCanAppend_WithNullTombstone_ThrowsWithoutChangingRegistry()
        {
            var registry =
                new CombatCardTombstoneRegistry();

            Assert.Throws<ArgumentNullException>(
                () => registry.EnsureCanAppend(
                    null));

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void EnsureCanAppend_WithDuplicateInstanceId_ThrowsWithoutChangingRegistry()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var first =
                CreateTombstone(
                    metadataFactory,
                    100);

            var duplicate =
                CreateTombstone(
                    metadataFactory,
                    100);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(first);

            Assert.Throws<ArgumentException>(
                () => registry.EnsureCanAppend(
                    duplicate));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Tombstones[0],
                Is.SameAs(first));
        }

        [Test]
        public void EnsureCanAppend_WithDuplicateRemovalEventId_ThrowsWithoutChangingRegistry()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            var removalMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            var first =
                CreateTombstone(
                    100,
                    removalMetadata);

            var duplicateEvent =
                CreateTombstone(
                    200,
                    removalMetadata);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(first);

            Assert.Throws<ArgumentException>(
                () => registry.EnsureCanAppend(
                    duplicateEvent));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Tombstones[0],
                Is.SameAs(first));
        }

        [Test]
        public void EnsureCanAppend_WithNonIncreasingSequence_ThrowsWithoutChangingRegistry()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var earlier =
                CreateTombstone(
                    metadataFactory,
                    100);

            var later =
                CreateTombstone(
                    metadataFactory,
                    200);

            var registry =
                new CombatCardTombstoneRegistry();

            registry.Append(later);

            Assert.Throws<ArgumentException>(
                () => registry.EnsureCanAppend(
                    earlier));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.Tombstones[0],
                Is.SameAs(later));
        }

        private static CombatCardTombstone
            CreateTombstone(
                CombatEventMetadataFactory
                    metadataFactory,
                long instanceId)
        {
            var rootMetadata =
                metadataFactory.CreateRoot();

            var removalMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            return CreateTombstone(
                instanceId,
                removalMetadata);
        }

        private static CombatCardTombstone
            CreateTombstone(
                long instanceId,
                CombatEventMetadata removalMetadata)
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        $"card-{instanceId}"),
                    new InstanceId(instanceId),
                    new CardRank(2),
                    7,
                    5,
                    0,
                    3);

            return new CombatCardTombstone(
                card,
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardRemovalReason.DirectDelete,
                removalMetadata);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }
    }
}