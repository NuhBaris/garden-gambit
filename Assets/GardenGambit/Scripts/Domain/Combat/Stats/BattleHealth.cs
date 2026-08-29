using System;
using System.Globalization;

namespace GardenGambit.Domain.Combat
{
    public readonly struct BattleHealth :
        IEquatable<BattleHealth>,
        IComparable<BattleHealth>
    {
        public const int NormalBaselineValue = 20;

        private readonly int _value;

        public BattleHealth(int value)
        {
            _value = value;
        }

        public int Value => _value;

        public BattleHealth ApplyDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damage),
                    damage,
                    "Battle Health damage cannot be negative.");
            }

            var value = checked(
                _value - damage);

            return new BattleHealth(value);
        }

        public BattleHealth ApplyGain(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Battle Health gain cannot be negative.");
            }

            var value = checked(
                _value + amount);

            return new BattleHealth(value);
        }

        public int CompareTo(BattleHealth other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(BattleHealth other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleHealth other &&
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
            BattleHealth left,
            BattleHealth right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleHealth left,
            BattleHealth right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            BattleHealth left,
            BattleHealth right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(
            BattleHealth left,
            BattleHealth right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(
            BattleHealth left,
            BattleHealth right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(
            BattleHealth left,
            BattleHealth right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}