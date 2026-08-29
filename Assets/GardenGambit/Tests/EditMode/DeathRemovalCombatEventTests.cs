using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        DeathRemovalCombatEventTests
    {
        [Test]
        public void Constructor_WithHpAtZero_SetsSnapshot()
        {
            var metadata =
                CreateMetadata();

            var position =
                CreatePosition();

            var removalEvent =
                new DeathRemovalCombatEvent(
                    metadata,
                    new InstanceId(100),
                    position,
                    0);

            Assert.That(
                removalEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DeathRemoval));

            Assert.That(
                removalEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                removalEvent.InstanceId,
                Is.EqualTo(new InstanceId(100)));

            Assert.That(
                removalEvent.Position,
                Is.EqualTo(position));

            Assert.That(
                removalEvent.HpAtRemoval,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithHpBelowZero_AllowsSnapshot()
        {
            var removalEvent =
                new DeathRemovalCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreatePosition(),
                    -3);

            Assert.That(
                removalEvent.HpAtRemoval,
                Is.EqualTo(-3));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DeathRemovalCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreatePosition(),
                        0));
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DeathRemovalCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreatePosition(),
                        0));
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DeathRemovalCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        0));
        }

        [Test]
        public void Constructor_WithPositiveHpAtRemoval_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new DeathRemovalCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(),
                        1));
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private static BoardPosition
            CreatePosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}