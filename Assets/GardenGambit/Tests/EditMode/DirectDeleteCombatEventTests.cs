using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        DirectDeleteCombatEventTests
    {
        [Test]
        public void Constructor_WithPositiveHp_SetsSnapshot()
        {
            var metadata =
                CreateMetadata();

            var position =
                CreatePosition();

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    new InstanceId(100),
                    position,
                    5);

            Assert.That(
                deleteEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DirectDelete));

            Assert.That(
                deleteEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                deleteEvent.InstanceId,
                Is.EqualTo(new InstanceId(100)));

            Assert.That(
                deleteEvent.Position,
                Is.EqualTo(position));

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WithHpAtZero_AllowsSnapshot()
        {
            var deleteEvent =
                new DirectDeleteCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreatePosition(),
                    0);

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithHpBelowZero_AllowsSnapshot()
        {
            var deleteEvent =
                new DirectDeleteCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreatePosition(),
                    -3);

            Assert.That(
                deleteEvent.HpAtDeletion,
                Is.EqualTo(-3));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DirectDeleteCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreatePosition(),
                        5));
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DirectDeleteCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreatePosition(),
                        5));
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DirectDeleteCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        5));
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