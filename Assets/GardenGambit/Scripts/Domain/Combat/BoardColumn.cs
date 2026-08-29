using System;
using System.Globalization;

namespace GardenGambit.Domain.Combat
{
    public readonly struct BoardColumn :
        IEquatable<BoardColumn>,
        IComparable<BoardColumn>
    {
        public const int MinimumValue = 1;
        public const int MaximumValue = 5;

        public BoardColumn(int value)
        {
            if (value < MinimumValue || value > MaximumValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Board column must be between 1 and 5.");
            }

            Value = value;
        }

        public int Value { get; }

        public bool IsValid =>
            Value >= MinimumValue &&
            Value <= MaximumValue;

        public int CompareTo(BoardColumn other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(BoardColumn other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardColumn other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public static bool operator ==(
            BoardColumn left,
            BoardColumn right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BoardColumn left,
            BoardColumn right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            BoardColumn left,
            BoardColumn right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(
            BoardColumn left,
            BoardColumn right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(
            BoardColumn left,
            BoardColumn right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(
            BoardColumn left,
            BoardColumn right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}