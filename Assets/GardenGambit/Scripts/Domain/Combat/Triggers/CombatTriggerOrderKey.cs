using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct CombatTriggerOrderKey :
        IComparable<CombatTriggerOrderKey>,
        IEquatable<CombatTriggerOrderKey>
    {
        public CombatTriggerOrderKey(
            CombatTriggerSourceKind sourceKind,
            CombatSide side,
            int horizontalOrder,
            int verticalOrder)
        {
            if (sourceKind <
                    CombatTriggerSourceKind.Slot ||
                sourceKind >
                    CombatTriggerSourceKind
                        .NormalEnemySpecial)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sourceKind),
                    sourceKind,
                    "A valid trigger source kind is required.");
            }

            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Trigger order requires Player or Enemy side.");
            }

            if (horizontalOrder < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(horizontalOrder),
                    horizontalOrder,
                    "Horizontal order cannot be negative.");
            }

            if (verticalOrder < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(verticalOrder),
                    verticalOrder,
                    "Vertical order cannot be negative.");
            }

            SourceKind = sourceKind;
            Side = side;
            HorizontalOrder = horizontalOrder;
            VerticalOrder = verticalOrder;
        }

        public CombatTriggerSourceKind SourceKind { get; }

        public CombatSide Side { get; }

        public int HorizontalOrder { get; }

        public int VerticalOrder { get; }

        public bool IsValid =>
            SourceKind >=
                CombatTriggerSourceKind.Slot &&
            SourceKind <=
                CombatTriggerSourceKind
                    .NormalEnemySpecial &&
            (Side == CombatSide.Player ||
             Side == CombatSide.Enemy) &&
            HorizontalOrder >= 0 &&
            VerticalOrder >= 0;

        public int CompareTo(
            CombatTriggerOrderKey other)
        {
            var sourceKindComparison =
                ((int)SourceKind).CompareTo(
                    (int)other.SourceKind);

            if (sourceKindComparison != 0)
            {
                return sourceKindComparison;
            }

            var horizontalComparison =
                HorizontalOrder.CompareTo(
                    other.HorizontalOrder);

            if (horizontalComparison != 0)
            {
                return horizontalComparison;
            }

            var verticalComparison =
                VerticalOrder.CompareTo(
                    other.VerticalOrder);

            if (verticalComparison != 0)
            {
                return verticalComparison;
            }

            return GetSidePriority(Side).CompareTo(
                GetSidePriority(other.Side));
        }

        public bool Equals(
            CombatTriggerOrderKey other)
        {
            return SourceKind == other.SourceKind &&
                   Side == other.Side &&
                   HorizontalOrder ==
                   other.HorizontalOrder &&
                   VerticalOrder ==
                   other.VerticalOrder;
        }

        public override bool Equals(object obj)
        {
            return obj is CombatTriggerOrderKey other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode =
                    (int)SourceKind;

                hashCode =
                    (hashCode * 397) ^
                    (int)Side;

                hashCode =
                    (hashCode * 397) ^
                    HorizontalOrder;

                hashCode =
                    (hashCode * 397) ^
                    VerticalOrder;

                return hashCode;
            }
        }

        public override string ToString()
        {
            return
                $"{SourceKind}:{Side}:" +
                $"H{HorizontalOrder}:" +
                $"V{VerticalOrder}";
        }

        public static bool operator ==(
            CombatTriggerOrderKey left,
            CombatTriggerOrderKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CombatTriggerOrderKey left,
            CombatTriggerOrderKey right)
        {
            return !left.Equals(right);
        }

        private static int GetSidePriority(
            CombatSide side)
        {
            return side == CombatSide.Player
                ? 0
                : 1;
        }
    }
}