using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct BoardPosition :
        IEquatable<BoardPosition>
    {
        public BoardPosition(
            CombatSide side,
            BoardRow row,
            BoardColumn column)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Board position requires Player or Enemy side.");
            }

            if (row != BoardRow.Front &&
                row != BoardRow.Back)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(row),
                    row,
                    "Board position requires Front or Back row.");
            }

            if (!column.IsValid)
            {
                throw new ArgumentException(
                    "Board position requires a valid column.",
                    nameof(column));
            }

            Side = side;
            Row = row;
            Column = column;
        }

        public CombatSide Side { get; }

        public BoardRow Row { get; }

        public BoardColumn Column { get; }

        public bool IsValid =>
            (Side == CombatSide.Player ||
             Side == CombatSide.Enemy) &&
            (Row == BoardRow.Front ||
             Row == BoardRow.Back) &&
            Column.IsValid;

        public bool Equals(BoardPosition other)
        {
            return Side == other.Side &&
                   Row == other.Row &&
                   Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Side;
                hashCode = (hashCode * 397) ^ (int)Row;
                hashCode = (hashCode * 397) ^
                           Column.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Side}:{Row}:Column{Column}";
        }

        public static bool operator ==(
            BoardPosition left,
            BoardPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BoardPosition left,
            BoardPosition right)
        {
            return !left.Equals(right);
        }
    }
}