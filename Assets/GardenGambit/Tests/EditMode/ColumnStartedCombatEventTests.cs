using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        ColumnStartedCombatEventTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsEvent()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            var childMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            var column =
                new BoardColumn(1);

            var columnEvent =
                new ColumnStartedCombatEvent(
                    childMetadata,
                    column);

            Assert.That(
                columnEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.ColumnStarted));

            Assert.That(
                columnEvent.Column,
                Is.EqualTo(column));

            Assert.That(
                columnEvent.Metadata.EventId,
                Is.EqualTo(
                    childMetadata.EventId));

            Assert.That(
                columnEvent.Metadata.SequenceNo,
                Is.EqualTo(
                    childMetadata.SequenceNo));

            Assert.That(
                columnEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                columnEvent.Metadata.ParentEventId,
                Is.EqualTo(
                    childMetadata.ParentEventId));

            Assert.That(
                columnEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    rootMetadata.TriggerRootId));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new ColumnStartedCombatEvent(
                        default(CombatEventMetadata),
                        new BoardColumn(1)));
        }

        [Test]
        public void Constructor_WithRootMetadata_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new ColumnStartedCombatEvent(
                        rootMetadata,
                        new BoardColumn(1)));
        }

        [Test]
        public void Constructor_WithInvalidColumn_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var rootMetadata =
                metadataFactory.CreateRoot();

            var childMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new ColumnStartedCombatEvent(
                        childMetadata,
                        default(BoardColumn)));
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