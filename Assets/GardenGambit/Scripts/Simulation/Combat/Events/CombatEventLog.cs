using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventLog
    {
        private readonly List<CombatEvent>
            _events;

        private readonly ReadOnlyCollection<CombatEvent>
            _readOnlyEvents;

        private readonly Dictionary<
            CombatEventId,
            CombatEventMetadata> _metadataByEventId;

        private readonly CombatCardTombstoneRegistry
            _cardTombstones;

        public CombatEventLog()
        {
            _events =
                new List<CombatEvent>();

            _readOnlyEvents =
                _events.AsReadOnly();

            _metadataByEventId =
                new Dictionary<
                    CombatEventId,
                    CombatEventMetadata>();

            _cardTombstones =
                new CombatCardTombstoneRegistry();
        }

        public int Count =>
            _events.Count;

        public IReadOnlyList<CombatEvent> Events =>
            _readOnlyEvents;

        public CombatCardTombstoneRegistry
            CardTombstones =>
                _cardTombstones;

        public bool ContainsEvent(
            CombatEventId eventId)
        {
            if (!eventId.IsValid)
            {
                throw new ArgumentException(
                    "A valid CombatEventId is required.",
                    nameof(eventId));
            }

            return _metadataByEventId.ContainsKey(
                eventId);
        }

        public CombatEvent GetEvent(
            CombatEventId eventId)
        {
            if (!eventId.IsValid)
            {
                throw new ArgumentException(
                    "A valid CombatEventId is required.",
                    nameof(eventId));
            }

            foreach (var combatEvent in _events)
            {
                if (combatEvent.Metadata.EventId ==
                    eventId)
                {
                    return combatEvent;
                }
            }

            throw new KeyNotFoundException(
                $"Combat event was not found: " +
                $"{eventId}.");
        }

        public void EnsureCanAppend(
            CombatEvent combatEvent)
        {
            if (combatEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatEvent));
            }

            var metadata =
                combatEvent.Metadata;

            if (_metadataByEventId.ContainsKey(
                    metadata.EventId))
            {
                throw new ArgumentException(
                    $"Duplicate combat EventId detected: " +
                    $"{metadata.EventId}.",
                    nameof(combatEvent));
            }

            if (_events.Count > 0)
            {
                var previousSequence =
                    _events[
                        _events.Count - 1]
                        .Metadata.SequenceNo;

                if (metadata.SequenceNo <=
                    previousSequence)
                {
                    throw new ArgumentException(
                        "Combat event SequenceNo must be " +
                        "strictly increasing.",
                        nameof(combatEvent));
                }
            }

            if (!metadata.HasParent)
            {
                return;
            }

            var parentEventId =
                metadata.ParentEventId.Value;

            CombatEventMetadata parentMetadata;

            if (!_metadataByEventId.TryGetValue(
                    parentEventId,
                    out parentMetadata))
            {
                throw new ArgumentException(
                    $"Parent event was not found: " +
                    $"{parentEventId}.",
                    nameof(combatEvent));
            }

            if (parentMetadata.TriggerRootId !=
                metadata.TriggerRootId)
            {
                throw new ArgumentException(
                    "Parent and child events must share " +
                    "the same TriggerRootId.",
                    nameof(combatEvent));
            }

            CombatEventMetadata rootMetadata;

            if (!_metadataByEventId.TryGetValue(
                    metadata.TriggerRootId,
                    out rootMetadata))
            {
                throw new ArgumentException(
                    $"Trigger-root event was not found: " +
                    $"{metadata.TriggerRootId}.",
                    nameof(combatEvent));
            }

            if (!rootMetadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "TriggerRootId must reference an " +
                    "actual root event.",
                    nameof(combatEvent));
            }
        }

        public void Append(
            CombatEvent combatEvent)
        {
            EnsureCanAppend(combatEvent);

            var metadata =
                combatEvent.Metadata;

            _events.Add(combatEvent);

            _metadataByEventId.Add(
                metadata.EventId,
                metadata);
        }
    }
}