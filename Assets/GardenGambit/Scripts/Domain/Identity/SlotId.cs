using System;
using System.Globalization;

namespace GardenGambit.Domain.Identity
{
    public readonly struct SlotId : IEquatable<SlotId>
    {
        public SlotId(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "SlotId must be greater than zero.");
            }

            Value = value;
        }

        public long Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(SlotId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SlotId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public static bool operator ==(
            SlotId left,
            SlotId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            SlotId left,
            SlotId right)
        {
            return !left.Equals(right);
        }
    }
}