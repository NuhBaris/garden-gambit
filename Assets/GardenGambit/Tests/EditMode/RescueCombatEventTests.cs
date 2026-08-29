using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class RescueCombatEventTests
    {
        [Test]
        public void Constructor_WithHpAtZero_SetsSnapshot()
        {
            var metadata =
                CreateMetadata();

            var position =
                CreatePosition();

            var rescueEvent =
                new RescueCombatEvent(
                    metadata,
                    new InstanceId(100),
                    position,
                    0,
                    1);

            Assert.That(
                rescueEvent.Kind,
                Is.EqualTo(CombatEventKind.Rescue));

            Assert.That(
                rescueEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                rescueEvent.InstanceId,
                Is.EqualTo(new InstanceId(100)));

            Assert.That(
                rescueEvent.Position,
                Is.EqualTo(position));

            Assert.That(
                rescueEvent.PreviousHp,
                Is.Zero);

            Assert.That(
                rescueEvent.CurrentHp,
                Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithHpBelowZero_AllowsSnapshot()
        {
            var rescueEvent =
                new RescueCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreatePosition(),
                    -3,
                    1);

            Assert.That(
                rescueEvent.PreviousHp,
                Is.EqualTo(-3));

            Assert.That(
                rescueEvent.CurrentHp,
                Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new RescueCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreatePosition(),
                        0,
                        1));
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new RescueCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreatePosition(),
                        0,
                        1));
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new RescueCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        0,
                        1));
        }

        [Test]
        public void Constructor_WithPositivePreviousHp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new RescueCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(),
                        1,
                        1));
        }

        [Test]
        public void Constructor_WithCurrentHpNotEqualToOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new RescueCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(),
                        0,
                        0));
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