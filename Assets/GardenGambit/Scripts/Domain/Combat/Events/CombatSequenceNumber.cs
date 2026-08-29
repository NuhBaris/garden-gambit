using System;
using System.Globalization;

namespace GardenGambit.Domain.Combat
{
    public readonly struct CombatSequenceNumber :
        IEquatable<CombatSequenceNumber>,
        IComparable<CombatSequenceNumber>
    {
        private readonly long _value;

        public CombatSequenceNumber(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Combat sequence number must be positive.");
            }

            _value = value;
        }

        public long Value => _value;

        public bool IsValid => _value > 0;

        public int CompareTo(
            CombatSequenceNumber other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(
            CombatSequenceNumber other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is CombatSequenceNumber other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString(
                CultureInfo.InvariantCulture);
        }

        public static bool operator ==(
            CombatSequenceNumber left,
            CombatSequenceNumber right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CombatSequenceNumber left,
            CombatSequenceNumber right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            CombatSequenceNumber left,
            CombatSequenceNumber right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(
            CombatSequenceNumber left,
            CombatSequenceNumber right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(
            CombatSequenceNumber left,
            CombatSequenceNumber right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(
            CombatSequenceNumber left,
            CombatSequenceNumber right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}