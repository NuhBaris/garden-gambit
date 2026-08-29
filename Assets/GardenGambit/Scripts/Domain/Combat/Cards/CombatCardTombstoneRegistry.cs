using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatCardTombstoneRegistry
    {
        private readonly List<CombatCardTombstone>
            _tombstones;

        private readonly ReadOnlyCollection<
            CombatCardTombstone> _readOnlyTombstones;

        private readonly Dictionary<
            InstanceId,
            CombatCardTombstone> _byInstanceId;

        private readonly Dictionary<
            CombatEventId,
            CombatCardTombstone> _byRemovalEventId;

        public CombatCardTombstoneRegistry()
            : this(
                new CombatCardTombstone[0])
        {
        }

        public CombatCardTombstoneRegistry(
            IEnumerable<CombatCardTombstone>
                tombstones)
        {
            if (tombstones == null)
            {
                throw new ArgumentNullException(
                    nameof(tombstones));
            }

            _tombstones =
                new List<CombatCardTombstone>();

            _readOnlyTombstones =
                _tombstones.AsReadOnly();

            _byInstanceId =
                new Dictionary<
                    InstanceId,
                    CombatCardTombstone>();

            _byRemovalEventId =
                new Dictionary<
                    CombatEventId,
                    CombatCardTombstone>();

            foreach (var tombstone in tombstones)
            {
                Append(tombstone);
            }
        }

        public int Count =>
            _tombstones.Count;

        public IReadOnlyList<CombatCardTombstone>
            Tombstones =>
                _readOnlyTombstones;

        public void EnsureCanAppend(
            CombatCardTombstone tombstone)
        {
            if (tombstone == null)
            {
                throw new ArgumentNullException(
                    nameof(tombstone));
            }

            if (_byInstanceId.ContainsKey(
                    tombstone.InstanceId))
            {
                throw new ArgumentException(
                    $"A tombstone already exists for " +
                    $"card {tombstone.InstanceId}.",
                    nameof(tombstone));
            }

            var removalEventId =
                tombstone.RemovalMetadata.EventId;

            if (_byRemovalEventId.ContainsKey(
                    removalEventId))
            {
                throw new ArgumentException(
                    $"A tombstone already exists for " +
                    $"removal event {removalEventId}.",
                    nameof(tombstone));
            }

            if (_tombstones.Count == 0)
            {
                return;
            }

            var previousSequence =
                _tombstones[
                    _tombstones.Count - 1]
                    .RemovalMetadata.SequenceNo;

            if (tombstone.RemovalMetadata.SequenceNo <=
                previousSequence)
            {
                throw new ArgumentException(
                    "Tombstone removal sequences must " +
                    "be strictly increasing.",
                    nameof(tombstone));
            }
        }

        public void Append(
            CombatCardTombstone tombstone)
        {
            EnsureCanAppend(tombstone);

            var removalEventId =
                tombstone.RemovalMetadata.EventId;

            _tombstones.Add(tombstone);

            _byInstanceId.Add(
                tombstone.InstanceId,
                tombstone);

            _byRemovalEventId.Add(
                removalEventId,
                tombstone);
        }

        public bool Contains(
            InstanceId instanceId)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(instanceId));
            }

            return _byInstanceId.ContainsKey(
                instanceId);
        }

        public CombatCardTombstone Get(
            InstanceId instanceId)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(instanceId));
            }

            CombatCardTombstone tombstone;

            if (_byInstanceId.TryGetValue(
                    instanceId,
                    out tombstone))
            {
                return tombstone;
            }

            throw new KeyNotFoundException(
                $"Card tombstone was not found: " +
                $"{instanceId}.");
        }

        public CombatCardTombstone GetByRemovalEvent(
            CombatEventId removalEventId)
        {
            if (!removalEventId.IsValid)
            {
                throw new ArgumentException(
                    "A valid removal CombatEventId " +
                    "is required.",
                    nameof(removalEventId));
            }

            CombatCardTombstone tombstone;

            if (_byRemovalEventId.TryGetValue(
                    removalEventId,
                    out tombstone))
            {
                return tombstone;
            }

            throw new KeyNotFoundException(
                $"Card tombstone for removal event " +
                $"{removalEventId} was not found.");
        }
    }
}