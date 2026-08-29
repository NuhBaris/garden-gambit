using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatBoardStateTests
    {
        [Test]
        public void Constructor_WithTenUniqueSlots_CreatesBoard()
        {
            var slots = CreateFullBoardSlots(CombatSide.Player);

            var board = new CombatBoardState(
                CombatSide.Player,
                slots);

            Assert.That(board.Side, Is.EqualTo(CombatSide.Player));
            Assert.That(board.SlotCount, Is.EqualTo(10));
            Assert.That(board.Slots.Count, Is.EqualTo(10));

            var collection =
                (ICollection<CombatSlotState>)board.Slots;

            Assert.That(collection.IsReadOnly, Is.True);
        }

        [Test]
        public void Constructor_WithEmptySlots_AllowsEmptyBoard()
        {
            var board = new CombatBoardState(
                CombatSide.Player,
                new CombatSlotState[0]);

            Assert.That(board.SlotCount, Is.Zero);
            Assert.That(board.Slots, Is.Empty);
        }

        [TestCase(CombatSide.Unspecified)]
        [TestCase((CombatSide)999)]
        public void Constructor_WithInvalidSide_Throws(
            CombatSide side)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new CombatBoardState(
                    side,
                    new CombatSlotState[0]);
            });
        }

        [Test]
        public void Constructor_WithNullCollection_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    null);
            });
        }

        [Test]
        public void Constructor_WithNullSlot_Throws()
        {
            var slots = new CombatSlotState[]
            {
                null
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    slots);
            });
        }

        [Test]
        public void Constructor_WithMoreThanTenSlots_Throws()
        {
            var slots = CreateFullBoardSlots(CombatSide.Player);

            slots.Add(CreateSlot(
                11,
                CombatSide.Player,
                BoardRow.Front,
                1));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    slots);
            });
        }

        [Test]
        public void Constructor_WithOppositeSideSlot_Throws()
        {
            var slots = new[]
            {
                CreateSlot(
                    1,
                    CombatSide.Enemy,
                    BoardRow.Front,
                    1)
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    slots);
            });
        }

        [Test]
        public void Constructor_WithDuplicateSlotId_Throws()
        {
            var slots = new[]
            {
                CreateSlot(
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    1),

                CreateSlot(
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    2)
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    slots);
            });
        }

        [Test]
        public void Constructor_WithDuplicatePosition_Throws()
        {
            var slots = new[]
            {
                CreateSlot(
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    1),

                CreateSlot(
                    2,
                    CombatSide.Player,
                    BoardRow.Front,
                    1)
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    slots);
            });
        }

        [Test]
        public void GetSlot_WithSlotId_ReturnsCorrectSlot()
        {
            var firstSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var secondSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Back,
                5);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { firstSlot, secondSlot });

            var result = board.GetSlot(new SlotId(2));

            Assert.That(result, Is.SameAs(secondSlot));
        }

        [Test]
        public void GetSlot_WithPosition_ReturnsCorrectSlot()
        {
            var firstSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var secondSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Back,
                5);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { firstSlot, secondSlot });

            var result = board.GetSlot(secondSlot.Position);

            Assert.That(result, Is.SameAs(secondSlot));
        }

        [Test]
        public void GetSlot_WithMissingSlotId_Throws()
        {
            var board = new CombatBoardState(
                CombatSide.Player,
                new[]
                {
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1)
                });

            Assert.Throws<KeyNotFoundException>(() =>
            {
                _ = board.GetSlot(new SlotId(99));
            });
        }

        [Test]
        public void GetSlot_WithMissingPosition_Throws()
        {
            var board = new CombatBoardState(
                CombatSide.Player,
                new[]
                {
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1)
                });

            var missingPosition = new BoardPosition(
                CombatSide.Player,
                BoardRow.Back,
                new BoardColumn(5));

            Assert.Throws<KeyNotFoundException>(() =>
            {
                _ = board.GetSlot(missingPosition);
            });
        }

        [Test]
        public void GetSlot_WithInvalidIdentifier_Throws()
        {
            var board = new CombatBoardState(
                CombatSide.Player,
                new CombatSlotState[0]);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = board.GetSlot(default(SlotId));
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = board.GetSlot(default(BoardPosition));
            });
        }

        [Test]
        public void PlaceOccupant_IntoEmptySlot_OccupiesSlot()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(slot.Position, occupant);

            Assert.That(slot.IsOccupied, Is.True);
            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(occupant));
        }

        [Test]
        public void Constructor_WithDuplicateOccupantInstanceId_Throws()
        {
            var occupant = new InstanceId(100);

            var slots = new[]
            {
                new CombatSlotState(
                    new SlotId(1),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    occupant),

                new CombatSlotState(
                    new SlotId(2),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(2)),
                    occupant)
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new CombatBoardState(
                    CombatSide.Player,
                    slots);
            });
        }

        [Test]
        public void Constructor_WithDistinctOccupants_CreatesBoard()
        {
            var slots = new[]
            {
                new CombatSlotState(
                    new SlotId(1),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    new InstanceId(100)),

                new CombatSlotState(
                    new SlotId(2),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(2)),
                    new InstanceId(101))
            };

            var board = new CombatBoardState(
                CombatSide.Player,
                slots);

            Assert.That(board.SlotCount, Is.EqualTo(2));
            Assert.That(board.Slots[0].IsOccupied, Is.True);
            Assert.That(board.Slots[1].IsOccupied, Is.True);
        }

        [Test]
        public void PlaceOccupant_IntoOccupiedSlot_ThrowsWithoutReplacingOccupant()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var firstOccupant = new InstanceId(100);
            var secondOccupant = new InstanceId(101);

            board.PlaceOccupant(
                slot.Position,
                firstOccupant);

            Assert.Throws<InvalidOperationException>(
                () => board.PlaceOccupant(
                    slot.Position,
                    secondOccupant));

            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(firstOccupant));
        }

        [Test]
        public void PlaceOccupant_WhenInstanceAlreadyExists_ThrowsWithoutChangingTarget()
        {
            var firstSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var secondSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Front,
                2);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { firstSlot, secondSlot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(
                firstSlot.Position,
                occupant);

            Assert.Throws<InvalidOperationException>(
                () => board.PlaceOccupant(
                    secondSlot.Position,
                    occupant));

            Assert.That(
                firstSlot.OccupantInstanceId.Value,
                Is.EqualTo(occupant));

            Assert.That(secondSlot.IsOccupied, Is.False);
        }

        [Test]
        public void PlaceOccupant_WithInvalidInstanceId_ThrowsWithoutChangingSlot()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            Assert.Throws<ArgumentException>(
                () => board.PlaceOccupant(
                    slot.Position,
                    default(InstanceId)));

            Assert.That(slot.IsOccupied, Is.False);
        }

        [Test]
        public void PlaceOccupant_AtMissingPosition_ThrowsWithoutChangingBoard()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var missingPosition = new BoardPosition(
                CombatSide.Player,
                BoardRow.Back,
                new BoardColumn(5));

            Assert.Throws<KeyNotFoundException>(
                () => board.PlaceOccupant(
                    missingPosition,
                    new InstanceId(100)));

            Assert.That(slot.IsOccupied, Is.False);
        }

        [Test]
        public void RemoveOccupant_FromOccupiedSlot_ReturnsIdAndEmptiesSlot()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(
                slot.Position,
                occupant);

            var removedOccupant =
                board.RemoveOccupant(slot.Position);

            Assert.That(
                removedOccupant,
                Is.EqualTo(occupant));

            Assert.That(slot.IsOccupied, Is.False);
            Assert.That(
                slot.OccupantInstanceId.HasValue,
                Is.False);
        }

        [Test]
        public void RemoveOccupant_FromEmptySlot_Throws()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            Assert.Throws<InvalidOperationException>(
                () => board.RemoveOccupant(
                    slot.Position));

            Assert.That(slot.IsOccupied, Is.False);
        }

        [Test]
        public void MoveOccupant_ToEmptySlot_MovesOccupant()
        {
            var sourceSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var destinationSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Front,
                2);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { sourceSlot, destinationSlot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(
                sourceSlot.Position,
                occupant);

            board.MoveOccupant(
                sourceSlot.Position,
                destinationSlot.Position);

            Assert.That(sourceSlot.IsOccupied, Is.False);
            Assert.That(destinationSlot.IsOccupied, Is.True);
            Assert.That(
                destinationSlot.OccupantInstanceId.Value,
                Is.EqualTo(occupant));
        }

        [Test]
        public void MoveOccupant_FromEmptySlot_ThrowsWithoutChangingBoard()
        {
            var sourceSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var destinationSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Front,
                2);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { sourceSlot, destinationSlot });

            Assert.Throws<InvalidOperationException>(
                () => board.MoveOccupant(
                    sourceSlot.Position,
                    destinationSlot.Position));

            Assert.That(sourceSlot.IsOccupied, Is.False);
            Assert.That(destinationSlot.IsOccupied, Is.False);
        }

        [Test]
        public void MoveOccupant_ToOccupiedSlot_ThrowsWithoutChangingBoard()
        {
            var sourceSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var destinationSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Front,
                2);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { sourceSlot, destinationSlot });

            var sourceOccupant = new InstanceId(100);
            var destinationOccupant = new InstanceId(101);

            board.PlaceOccupant(
                sourceSlot.Position,
                sourceOccupant);

            board.PlaceOccupant(
                destinationSlot.Position,
                destinationOccupant);

            Assert.Throws<InvalidOperationException>(
                () => board.MoveOccupant(
                    sourceSlot.Position,
                    destinationSlot.Position));

            Assert.That(
                sourceSlot.OccupantInstanceId.Value,
                Is.EqualTo(sourceOccupant));

            Assert.That(
                destinationSlot.OccupantInstanceId.Value,
                Is.EqualTo(destinationOccupant));
        }

        [Test]
        public void MoveOccupant_ToSameSlot_ThrowsWithoutRemovingOccupant()
        {
            var slot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { slot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(slot.Position, occupant);

            Assert.Throws<InvalidOperationException>(
                () => board.MoveOccupant(
                    slot.Position,
                    slot.Position));

            Assert.That(slot.IsOccupied, Is.True);
            Assert.That(
                slot.OccupantInstanceId.Value,
                Is.EqualTo(occupant));
        }

        [Test]
        public void MoveOccupant_FromMissingPosition_ThrowsWithoutChangingBoard()
        {
            var destinationSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                2);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { destinationSlot });

            var missingSource = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));

            Assert.Throws<KeyNotFoundException>(
                () => board.MoveOccupant(
                    missingSource,
                    destinationSlot.Position));

            Assert.That(destinationSlot.IsOccupied, Is.False);
        }

        [Test]
        public void MoveOccupant_ToMissingPosition_ThrowsWithoutRemovingOccupant()
        {
            var sourceSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { sourceSlot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(
                sourceSlot.Position,
                occupant);

            var missingDestination = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(2));

            Assert.Throws<KeyNotFoundException>(
                () => board.MoveOccupant(
                    sourceSlot.Position,
                    missingDestination));

            Assert.That(sourceSlot.IsOccupied, Is.True);
            Assert.That(
                sourceSlot.OccupantInstanceId.Value,
                Is.EqualTo(occupant));
        }

        [Test]
        public void GetSlotContaining_WithExistingOccupant_ReturnsSlot()
        {
            var expectedSlot = CreateSlot(
                1,
                CombatSide.Player,
                BoardRow.Front,
                1);

            var otherSlot = CreateSlot(
                2,
                CombatSide.Player,
                BoardRow.Front,
                2);

            var board = new CombatBoardState(
                CombatSide.Player,
                new[] { expectedSlot, otherSlot });

            var occupant = new InstanceId(100);

            board.PlaceOccupant(
                expectedSlot.Position,
                occupant);

            var result =
                board.GetSlotContaining(occupant);

            Assert.That(result, Is.SameAs(expectedSlot));
        }

        [Test]
        public void GetSlotContaining_WithMissingOccupant_Throws()
        {
            var board = new CombatBoardState(
                CombatSide.Player,
                new[]
                {
                    CreateSlot(
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1)
                });

            Assert.Throws<KeyNotFoundException>(
                () => board.GetSlotContaining(
                    new InstanceId(999)));
        }

        [Test]
        public void GetSlotContaining_WithInvalidInstanceId_Throws()
        {
            var board = new CombatBoardState(
                CombatSide.Player,
                new CombatSlotState[0]);

            Assert.Throws<ArgumentException>(
                () => board.GetSlotContaining(
                    default(InstanceId)));
        }

        private static List<CombatSlotState> CreateFullBoardSlots(
            CombatSide side)
        {
            var slots = new List<CombatSlotState>();
            var nextSlotId = 1L;

            var rows = new[]
            {
                BoardRow.Front,
                BoardRow.Back
            };

            foreach (var row in rows)
            {
                for (
                    var column = BoardColumn.MinimumValue;
                    column <= BoardColumn.MaximumValue;
                    column++)
                {
                    slots.Add(CreateSlot(
                        nextSlotId,
                        side,
                        row,
                        column));

                    nextSlotId++;
                }
            }

            return slots;
        }

        private static CombatSlotState CreateSlot(
            long slotId,
            CombatSide side,
            BoardRow row,
            int column)
        {
            var position = new BoardPosition(
                side,
                row,
                new BoardColumn(column));

            return new CombatSlotState(
                new SlotId(slotId),
                position);
        }
    }
}