using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatStartedCombatEventTests
    {
        [Test]
        public void Constructor_WithRootMetadata_SetsEvent()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var metadata =
                metadataFactory.CreateRoot();

            var combatEvent =
                new CombatStartedCombatEvent(
                    metadata);

            Assert.That(
                combatEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatStarted));

            Assert.That(
                combatEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                combatEvent.Metadata.SequenceNo,
                Is.EqualTo(
                    metadata.SequenceNo));

            Assert.That(
                combatEvent.Metadata.HasParent,
                Is.False);

            Assert.That(
                combatEvent.Metadata.IsTriggerRoot,
                Is.True);

            Assert.That(
                combatEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    metadata.EventId));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatStartedCombatEvent(
                        default(CombatEventMetadata)));
        }

        [Test]
        public void Constructor_WithChildMetadata_Throws()
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
                    new CombatStartedCombatEvent(
                        childMetadata));
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