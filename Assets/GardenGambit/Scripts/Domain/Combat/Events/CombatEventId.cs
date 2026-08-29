using System;
using System.Globalization;

namespace GardenGambit.Domain.Combat
{
    public readonly struct CombatEventId :
        IEquatable<CombatEventId>
    {
        private readonly long _value;

        public CombatEventId(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Combat event ID must be positive.");
            }

            _value = value;
        }

        public long Value => _value;

        public bool IsValid => _value > 0;

        public bool Equals(CombatEventId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is CombatEventId other &&
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
            CombatEventId left,
            CombatEventId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CombatEventId left,
            CombatEventId right)
        {
            return !left.Equals(right);
        }
    }
}