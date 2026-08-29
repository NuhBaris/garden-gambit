using System;

namespace GardenGambit.Domain.Identity
{
    public sealed class InstanceIdAllocator
    {
        private long _lastIssuedValue;

        public InstanceIdAllocator()
            : this(0)
        {
        }

        public InstanceIdAllocator(long lastIssuedValue)
        {
            if (lastIssuedValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lastIssuedValue),
                    lastIssuedValue,
                    "Last issued InstanceId value cannot be negative.");
            }

            _lastIssuedValue = lastIssuedValue;
        }

        public long LastIssuedValue => _lastIssuedValue;

        public bool IsExhausted => _lastIssuedValue == long.MaxValue;

        public InstanceId Allocate()
        {
            if (IsExhausted)
            {
                throw new InvalidOperationException(
                    "The InstanceId allocator is exhausted.");
            }

            _lastIssuedValue++;

            return new InstanceId(_lastIssuedValue);
        }
    }
}