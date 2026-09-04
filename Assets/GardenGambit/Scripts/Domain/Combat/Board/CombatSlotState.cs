using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatSlotState
    {
        public CombatSlotState(
            SlotId slotId,
            BoardPosition position,
            InstanceId? occupantInstanceId = null,
            CombatSlotEnhanceKind enhanceKind =
                CombatSlotEnhanceKind.None)
        {
            if (!slotId.IsValid)
            {
                throw new ArgumentException(
                    "Combat slot requires a valid SlotId.",
                    nameof(slotId));
            }

            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "Combat slot requires a valid " +
                    "board position.",
                    nameof(position));
            }

            if (occupantInstanceId.HasValue &&
                !occupantInstanceId.Value.IsValid)
            {
                throw new ArgumentException(
                    "Combat slot occupant requires a valid " +
                    "InstanceId.",
                    nameof(occupantInstanceId));
            }

            if (enhanceKind !=
                    CombatSlotEnhanceKind.None &&
                enhanceKind !=
                    CombatSlotEnhanceKind
                        .ProtectiveSeal &&
                enhanceKind !=
                    CombatSlotEnhanceKind.WarBanner &&
                enhanceKind !=
                    CombatSlotEnhanceKind
                        .SacrificialAltar &&
                enhanceKind !=
                    CombatSlotEnhanceKind.WarAltar)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(enhanceKind),
                    enhanceKind,
                    "Combat slot requires a supported " +
                    "Slot Enhance kind.");
            }

            SlotId = slotId;
            Position = position;
            OccupantInstanceId = occupantInstanceId;
            EnhanceKind = enhanceKind;
        }

        public SlotId SlotId { get; }

        public BoardPosition Position { get; }

        public InstanceId? OccupantInstanceId
        {
            get;
            private set;
        }

        public CombatSlotEnhanceKind EnhanceKind
        {
            get;
        }

        public bool IsOccupied =>
            OccupantInstanceId.HasValue;

        public bool HasEnhance =>
            EnhanceKind !=
            CombatSlotEnhanceKind.None;

        public bool HasProtectiveSeal =>
            EnhanceKind ==
            CombatSlotEnhanceKind.ProtectiveSeal;

        public bool HasWarBanner =>
            EnhanceKind ==
            CombatSlotEnhanceKind.WarBanner;

        public bool HasSacrificialAltar =>
            EnhanceKind ==
            CombatSlotEnhanceKind
                .SacrificialAltar;

        public bool HasWarAltar =>
            EnhanceKind ==
            CombatSlotEnhanceKind.WarAltar;

        internal void SetOccupant(
            InstanceId occupantInstanceId)
        {
            if (!occupantInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Combat slot occupant requires a valid " +
                    "InstanceId.",
                    nameof(occupantInstanceId));
            }

            if (IsOccupied)
            {
                throw new InvalidOperationException(
                    "Cannot place an occupant into an " +
                    "occupied combat slot.");
            }

            OccupantInstanceId =
                occupantInstanceId;
        }

        internal InstanceId RemoveOccupant()
        {
            if (!OccupantInstanceId.HasValue)
            {
                throw new InvalidOperationException(
                    "Cannot remove an occupant from an " +
                    "empty combat slot.");
            }

            var removedInstanceId =
                OccupantInstanceId.Value;

            OccupantInstanceId = null;

            return removedInstanceId;
        }
    }
}