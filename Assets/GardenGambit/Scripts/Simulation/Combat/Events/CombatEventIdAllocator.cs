using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventIdAllocator
    {
        private long _lastIssuedValue;

        public CombatEventIdAllocator(
            long lastIssuedValue = 0)
        {
            if (lastIssuedValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lastIssuedValue),
                    lastIssuedValue,
                    "Last issued event ID cannot be negative.");
            }

            _lastIssuedValue = lastIssuedValue;
        }

        public long LastIssuedValue =>
            _lastIssuedValue;

        public bool CanAllocate =>
            _lastIssuedValue < long.MaxValue;

        public CombatEventId Allocate()
        {
            if (!CanAllocate)
            {
                throw new InvalidOperationException(
                    "Combat event ID space is exhausted.");
            }

            _lastIssuedValue = checked(
                _lastIssuedValue + 1);

            return new CombatEventId(
                _lastIssuedValue);
        }
    }
}