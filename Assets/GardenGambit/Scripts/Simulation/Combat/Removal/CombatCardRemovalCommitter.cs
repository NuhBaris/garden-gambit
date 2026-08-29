using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatCardRemovalCommitter
    {
        private readonly CombatEventLog
            _eventLog;

        public CombatCardRemovalCommitter(
            CombatEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _eventLog = eventLog;
        }

        public void EnsureCanCommit(
            CombatEvent removalEvent,
            CombatCardTombstone tombstone)
        {
            if (removalEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(removalEvent));
            }

            if (tombstone == null)
            {
                throw new ArgumentNullException(
                    nameof(tombstone));
            }

            EnsureMetadataMatches(
                removalEvent.Metadata,
                tombstone.RemovalMetadata);

            var deathRemovalEvent =
                removalEvent
                    as DeathRemovalCombatEvent;

            if (deathRemovalEvent != null)
            {
                EnsureDeathRemovalMatches(
                    deathRemovalEvent,
                    tombstone);
            }
            else
            {
                var directDeleteEvent =
                    removalEvent
                        as DirectDeleteCombatEvent;

                if (directDeleteEvent == null)
                {
                    throw new ArgumentException(
                        "Card removal commit requires a " +
                        "Death Removal or Direct Delete event.",
                        nameof(removalEvent));
                }

                EnsureDirectDeleteMatches(
                    directDeleteEvent,
                    tombstone);
            }

            _eventLog.EnsureCanAppend(
                removalEvent);

            _eventLog.CardTombstones
                .EnsureCanAppend(
                    tombstone);
        }

        public void Commit(
            CombatEvent removalEvent,
            CombatCardTombstone tombstone)
        {
            EnsureCanCommit(
                removalEvent,
                tombstone);

            _eventLog.Append(
                removalEvent);

            _eventLog.CardTombstones.Append(
                tombstone);
        }

        private static void EnsureMetadataMatches(
            CombatEventMetadata eventMetadata,
            CombatEventMetadata tombstoneMetadata)
        {
            if (eventMetadata.EventId !=
                    tombstoneMetadata.EventId ||
                eventMetadata.SequenceNo !=
                    tombstoneMetadata.SequenceNo ||
                eventMetadata.ParentEventId !=
                    tombstoneMetadata.ParentEventId ||
                eventMetadata.TriggerRootId !=
                    tombstoneMetadata.TriggerRootId)
            {
                throw new ArgumentException(
                    "Removal event metadata must exactly " +
                    "match tombstone removal metadata.",
                    nameof(tombstoneMetadata));
            }
        }

        private static void EnsureDeathRemovalMatches(
            DeathRemovalCombatEvent removalEvent,
            CombatCardTombstone tombstone)
        {
            if (tombstone.RemovalReason !=
                CombatCardRemovalReason.DeathRemoval)
            {
                throw new ArgumentException(
                    "Death Removal event requires a " +
                    "Death Removal tombstone.",
                    nameof(tombstone));
            }

            if (removalEvent.InstanceId !=
                tombstone.InstanceId)
            {
                throw new ArgumentException(
                    "Death Removal event and tombstone " +
                    "must reference the same card.",
                    nameof(tombstone));
            }

            if (removalEvent.Position !=
                tombstone.LastPosition)
            {
                throw new ArgumentException(
                    "Death Removal event and tombstone " +
                    "must reference the same position.",
                    nameof(tombstone));
            }

            if (removalEvent.HpAtRemoval !=
                tombstone.CurrentHp)
            {
                throw new ArgumentException(
                    "Death Removal event HP must match " +
                    "the tombstone HP snapshot.",
                    nameof(tombstone));
            }
        }

        private static void EnsureDirectDeleteMatches(
            DirectDeleteCombatEvent deleteEvent,
            CombatCardTombstone tombstone)
        {
            if (tombstone.RemovalReason !=
                CombatCardRemovalReason.DirectDelete)
            {
                throw new ArgumentException(
                    "Direct Delete event requires a " +
                    "Direct Delete tombstone.",
                    nameof(tombstone));
            }

            if (deleteEvent.InstanceId !=
                tombstone.InstanceId)
            {
                throw new ArgumentException(
                    "Direct Delete event and tombstone " +
                    "must reference the same card.",
                    nameof(tombstone));
            }

            if (deleteEvent.Position !=
                tombstone.LastPosition)
            {
                throw new ArgumentException(
                    "Direct Delete event and tombstone " +
                    "must reference the same position.",
                    nameof(tombstone));
            }

            if (deleteEvent.HpAtDeletion !=
                tombstone.CurrentHp)
            {
                throw new ArgumentException(
                    "Direct Delete event HP must match " +
                    "the tombstone HP snapshot.",
                    nameof(tombstone));
            }
        }
    }
}