using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class DeathCombatEventTests
    {
        [Test]
        public void Constructor_WithHpReachingZero_SetsSnapshot()
        {
            var metadata =
                CreateMetadata();

            var position =
                CreatePosition();

            var deathEvent =
                new DeathCombatEvent(
                    metadata,
                    new InstanceId(100),
                    position,
                    3,
                    0);

            Assert.That(
                deathEvent.Kind,
                Is.EqualTo(CombatEventKind.Death));

            Assert.That(
                deathEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                deathEvent.InstanceId,
                Is.EqualTo(new InstanceId(100)));

            Assert.That(
                deathEvent.Position,
                Is.EqualTo(position));

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(3));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithHpBelowZero_AllowsSnapshot()
        {
            var deathEvent =
                new DeathCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreatePosition(),
                    3,
                    -2);

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(3));

            Assert.That(
                deathEvent.CurrentHp,
                Is.EqualTo(-2));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DeathCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreatePosition(),
                        3,
                        0));
        }

        [Test]
        public void Constructor_WithInvalidInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DeathCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreatePosition(),
                        3,
                        0));
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new DeathCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        3,
                        0));
        }

        [Test]
        public void Constructor_WithPreviousHpAtZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new DeathCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(),
                        0,
                        -1));
        }

        [Test]
        public void Constructor_WithPositiveCurrentHp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new DeathCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePosition(),
                        3,
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