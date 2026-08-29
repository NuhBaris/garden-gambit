using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatBoardState
    {
        public const int MaximumSlotCount = 10;

        private readonly List<CombatSlotState> _slots;
        private readonly ReadOnlyCollection<CombatSlotState> _readOnlySlots;

        public CombatBoardState(
            CombatSide side,
            IEnumerable<CombatSlotState> slots)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Combat board requires Player or Enemy side.");
            }

            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            var slotIds = new HashSet<SlotId>();
            var positions = new HashSet<BoardPosition>();
            var occupantIds = new HashSet<InstanceId>();

            _slots = new List<CombatSlotState>();

            foreach (var slot in slots)
            {
                if (slot == null)
                {
                    throw new ArgumentException(
                        "Combat board cannot contain a null slot.",
                        nameof(slots));
                }

                if (_slots.Count >= MaximumSlotCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(slots),
                        $"Combat board cannot contain more than " +
                        $"{MaximumSlotCount} slots.");
                }

                if (slot.Position.Side != side)
                {
                    throw new ArgumentException(
                        "Every combat slot must belong to the " +
                        "same side as its board.",
                        nameof(slots));
                }

                if (!slotIds.Add(slot.SlotId))
                {
                    throw new ArgumentException(
                        $"Duplicate SlotId detected: {slot.SlotId}.",
                        nameof(slots));
                }

                if (!positions.Add(slot.Position))
                {
                    throw new ArgumentException(
                        $"Duplicate board position detected: " +
                        $"{slot.Position}.",
                        nameof(slots));
                }
                if (slot.OccupantInstanceId.HasValue &&
                    !occupantIds.Add(
                        slot.OccupantInstanceId.Value))
                {
                    throw new ArgumentException(
                        $"Duplicate occupant InstanceId detected: " +
                        $"{slot.OccupantInstanceId.Value}.",
                        nameof(slots));
                }

                _slots.Add(slot);
            }

            Side = side;
            _readOnlySlots = _slots.AsReadOnly();
        }

        public CombatSide Side { get; }

        public int SlotCount => _slots.Count;

        public IReadOnlyList<CombatSlotState> Slots =>
            _readOnlySlots;

        public CombatSlotState GetSlot(SlotId slotId)
        {
            if (!slotId.IsValid)
            {
                throw new ArgumentException(
                    "A valid SlotId is required.",
                    nameof(slotId));
            }

            foreach (var slot in _slots)
            {
                if (slot.SlotId == slotId)
                {
                    return slot;
                }
            }

            throw new KeyNotFoundException(
                $"Combat slot was not found: {slotId}.");
        }

        public CombatSlotState GetSlot(BoardPosition position)
        {
            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "A valid board position is required.",
                    nameof(position));
            }

            foreach (var slot in _slots)
            {
                if (slot.Position == position)
                {
                    return slot;
                }
            }

            throw new KeyNotFoundException(
                $"Combat slot was not found at: {position}.");
        }

        public CombatSlotState GetSlotContaining(
             InstanceId occupantInstanceId)
        {
            if (!occupantInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid occupant InstanceId is required.",
                    nameof(occupantInstanceId));
            }

            foreach (var slot in _slots)
            {
                if (slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value ==
                    occupantInstanceId)
                {
                    return slot;
                }
            }

            throw new KeyNotFoundException(
                $"Combat slot containing occupant " +
                $"{occupantInstanceId} was not found.");
        }

        public void PlaceOccupant(
            BoardPosition position,
            InstanceId occupantInstanceId)
        {
            if (!occupantInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid occupant InstanceId is required.",
                    nameof(occupantInstanceId));
            }

            var targetSlot = GetSlot(position);

            if (targetSlot.IsOccupied)
            {
                throw new InvalidOperationException(
                    $"Cannot place occupant {occupantInstanceId} " +
                    $"into occupied slot {targetSlot.SlotId}.");
            }

            foreach (var slot in _slots)
            {
                if (slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value ==
                    occupantInstanceId)
                {
                    throw new InvalidOperationException(
                        $"Occupant {occupantInstanceId} is already " +
                        $"placed in slot {slot.SlotId}.");
                }
            }

            targetSlot.SetOccupant(occupantInstanceId);
        }

        public void MoveOccupant(
            BoardPosition sourcePosition,
            BoardPosition destinationPosition)
        {
            var sourceSlot = GetSlot(sourcePosition);
            var destinationSlot = GetSlot(destinationPosition);

            if (sourceSlot.SlotId == destinationSlot.SlotId)
            {
                throw new InvalidOperationException(
                    "Source and destination slots must be different.");
            }

            if (!sourceSlot.IsOccupied)
            {
                throw new InvalidOperationException(
                    $"Cannot move an occupant from empty slot " +
                    $"{sourceSlot.SlotId}.");
            }

            if (destinationSlot.IsOccupied)
            {
                throw new InvalidOperationException(
                    $"Cannot move an occupant into occupied slot " +
                    $"{destinationSlot.SlotId}.");
            }

            var occupantInstanceId =
                sourceSlot.OccupantInstanceId.Value;

            sourceSlot.RemoveOccupant();
            destinationSlot.SetOccupant(occupantInstanceId);
        }

        public InstanceId RemoveOccupant(
            BoardPosition position)
        {
            var slot = GetSlot(position);

            return slot.RemoveOccupant();
        }
    }
}