using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSlotStateTests
    {
        [Test]
        public void Constructor_WithoutOccupant_CreatesEmptySlot()
        {
            var slotId = new SlotId(1);
            var position = CreateValidPosition();

            var slot = new CombatSlotState(
                slotId,
                position);

            Assert.That(slot.SlotId, Is.EqualTo(slotId));
            Assert.That(slot.Position, Is.EqualTo(position));
            Assert.That(slot.IsOccupied, Is.False);
            Assert.That(
                slot.OccupantInstanceId.HasValue,
                Is.False);
        }

        [Test]
        public void Constructor_WithOccupant_CreatesOccupiedSlot()
        {
            var occupantInstanceId = new InstanceId(10);

            var slot = new CombatSlotState(
                new SlotId(1),
                CreateValidPosition(),
                occupantInstanceId);

            Assert.That(slot.IsOccupied, Is.True);
            Assert.That(
                slot.OccupantInstanceId.HasValue,
                Is.True);
            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(occupantInstanceId));
        }

        [Test]
        public void Constructor_WithInvalidSlotId_Throws()
        {
            var invalidSlotId = default(SlotId);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatSlotState(
                    invalidSlotId,
                    CreateValidPosition());
            });
        }

        [Test]
        public void Constructor_WithInvalidPosition_Throws()
        {
            var invalidPosition = default(BoardPosition);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatSlotState(
                    new SlotId(1),
                    invalidPosition);
            });
        }

        [Test]
        public void Constructor_WithInvalidOccupant_Throws()
        {
            var invalidOccupant = default(InstanceId);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatSlotState(
                    new SlotId(1),
                    CreateValidPosition(),
                    invalidOccupant);
            });
        }

        [Test]
        public void DifferentSlots_CanReferenceSamePositionIndependently()
        {
            var position = CreateValidPosition();

            var firstSlot = new CombatSlotState(
                new SlotId(1),
                position);

            var secondSlot = new CombatSlotState(
                new SlotId(2),
                position);

            Assert.That(
                firstSlot.Position,
                Is.EqualTo(secondSlot.Position));

            Assert.That(
                firstSlot.SlotId,
                Is.Not.EqualTo(secondSlot.SlotId));
        }

        private static BoardPosition CreateValidPosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}