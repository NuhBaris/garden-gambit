using System;
using System.Globalization;

namespace GardenGambit.Domain.Combat
{
    public readonly struct CardRank :
        IEquatable<CardRank>,
        IComparable<CardRank>
    {
        public const int MinimumValue = 2;
        public const int MaximumValue = 14;

        private readonly int _value;

        public CardRank(int value)
        {
            if (value < MinimumValue ||
                value > MaximumValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Card rank must be between " +
                    $"{MinimumValue} and {MaximumValue}.");
            }

            _value = value;
        }

        public int Value => _value;

        public bool IsValid =>
            _value >= MinimumValue &&
            _value <= MaximumValue;

        public int CompareTo(CardRank other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(CardRank other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is CardRank other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override string ToString()
        {
            return _value.ToString(
                CultureInfo.InvariantCulture);
        }

        public static bool operator ==(
            CardRank left,
            CardRank right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CardRank left,
            CardRank right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            CardRank left,
            CardRank right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(
            CardRank left,
            CardRank right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(
            CardRank left,
            CardRank right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(
            CardRank left,
            CardRank right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}