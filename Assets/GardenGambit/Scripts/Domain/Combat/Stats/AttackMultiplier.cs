using System;
using System.Globalization;

namespace GardenGambit.Domain.Combat
{
    public readonly struct AttackMultiplier :
        IEquatable<AttackMultiplier>,
        IComparable<AttackMultiplier>
    {
        public const int BaseValue = 1;
        public const int MinimumValue = 1;

        private readonly int _value;

        public AttackMultiplier(int value)
        {
            if (value < MinimumValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Attack multiplier must be at least " +
                    $"{MinimumValue}.");
            }

            _value = value;
        }

        public int Value => _value;

        public bool IsValid =>
            _value >= MinimumValue;

        public int CompareTo(
            AttackMultiplier other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(
            AttackMultiplier other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is AttackMultiplier other &&
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
            AttackMultiplier left,
            AttackMultiplier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            AttackMultiplier left,
            AttackMultiplier right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            AttackMultiplier left,
            AttackMultiplier right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(
            AttackMultiplier left,
            AttackMultiplier right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(
            AttackMultiplier left,
            AttackMultiplier right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(
            AttackMultiplier left,
            AttackMultiplier right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}