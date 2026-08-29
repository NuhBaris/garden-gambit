using System;
using System.Globalization;

namespace GardenGambit.Domain.Identity
{
    public readonly struct InstanceId :
        IEquatable<InstanceId>,
        IComparable<InstanceId>
    {
        public InstanceId(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "InstanceId must be greater than zero.");
            }

            Value = value;
        }

        public long Value { get; }

        public bool IsValid => Value > 0;

        public int CompareTo(InstanceId other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(InstanceId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is InstanceId other && Equals(other);
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
            InstanceId left,
            InstanceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            InstanceId left,
            InstanceId right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            InstanceId left,
            InstanceId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(
            InstanceId left,
            InstanceId right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(
            InstanceId left,
            InstanceId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(
            InstanceId left,
            InstanceId right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}