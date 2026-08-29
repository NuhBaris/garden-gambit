using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventMetadataFactory
    {
        private readonly CombatEventIdAllocator
            _eventIdAllocator;

        private readonly CombatSequenceNumberAllocator
            _sequenceNumberAllocator;

        public CombatEventMetadataFactory(
            CombatEventIdAllocator eventIdAllocator,
            CombatSequenceNumberAllocator
                sequenceNumberAllocator)
        {
            if (eventIdAllocator == null)
            {
                throw new ArgumentNullException(
                    nameof(eventIdAllocator));
            }

            if (sequenceNumberAllocator == null)
            {
                throw new ArgumentNullException(
                    nameof(sequenceNumberAllocator));
            }

            _eventIdAllocator = eventIdAllocator;
            _sequenceNumberAllocator =
                sequenceNumberAllocator;
        }

        public CombatEventMetadata CreateRoot()
        {
            EnsureAllocationIsAvailable();

            var eventId =
                _eventIdAllocator.Allocate();

            var sequenceNo =
                _sequenceNumberAllocator.Allocate();

            return new CombatEventMetadata(
                eventId,
                sequenceNo,
                null,
                eventId);
        }

        public CombatEventMetadata CreateChild(
            CombatEventMetadata parent)
        {
            if (!parent.IsValid)
            {
                throw new ArgumentException(
                    "A valid parent event metadata is required.",
                    nameof(parent));
            }

            if (_eventIdAllocator.LastIssuedValue <
                parent.EventId.Value)
            {
                throw new InvalidOperationException(
                    "Event ID allocator is behind the parent event.");
            }

            if (_sequenceNumberAllocator.LastIssuedValue <
                parent.SequenceNo.Value)
            {
                throw new InvalidOperationException(
                    "Sequence allocator is behind the parent event.");
            }

            EnsureAllocationIsAvailable();

            var eventId =
                _eventIdAllocator.Allocate();

            var sequenceNo =
                _sequenceNumberAllocator.Allocate();

            return new CombatEventMetadata(
                eventId,
                sequenceNo,
                parent.EventId,
                parent.TriggerRootId);
        }

        private void EnsureAllocationIsAvailable()
        {
            if (!_eventIdAllocator.CanAllocate ||
                !_sequenceNumberAllocator.CanAllocate)
            {
                throw new InvalidOperationException(
                    "Combat event metadata allocation " +
                    "is exhausted.");
            }
        }
    }
}