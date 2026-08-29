using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatSequenceNumberAllocator
    {
        private long _lastIssuedValue;

        public CombatSequenceNumberAllocator(
            long lastIssuedValue = 0)
        {
            if (lastIssuedValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lastIssuedValue),
                    lastIssuedValue,
                    "Last issued sequence number " +
                    "cannot be negative.");
            }

            _lastIssuedValue = lastIssuedValue;
        }

        public long LastIssuedValue =>
            _lastIssuedValue;

        public bool CanAllocate =>
            _lastIssuedValue < long.MaxValue;

        public CombatSequenceNumber Allocate()
        {
            if (!CanAllocate)
            {
                throw new InvalidOperationException(
                    "Combat sequence number space " +
                    "is exhausted.");
            }

            _lastIssuedValue = checked(
                _lastIssuedValue + 1);

            return new CombatSequenceNumber(
                _lastIssuedValue);
        }
    }
}