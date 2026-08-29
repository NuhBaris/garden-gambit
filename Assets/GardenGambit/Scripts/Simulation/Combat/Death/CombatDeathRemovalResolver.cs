using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatDeathRemovalResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatCardRemovalCommitter
            _removalCommitter;

        public CombatDeathRemovalResolver(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog)
        {
            if (metadataFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(metadataFactory));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _metadataFactory = metadataFactory;
            _eventLog = eventLog;

            _removalCommitter =
                new CombatCardRemovalCommitter(
                    eventLog);
        }

        public DeathRemovalCombatEvent TryApplyRemoval(
            CombatState state,
            DeathCombatEvent deathEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (deathEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(deathEvent));
            }

            ValidateLoggedDeathEvent(
                deathEvent);

            EnsureRemovalNotAlreadyLogged(
                deathEvent);

            if (HasDirectDeleteFor(
                    deathEvent))
            {
                return null;
            }

            if (HasRescueFor(
                    deathEvent))
            {
                return null;
            }

            var side =
                state.GetSide(
                    deathEvent.Position.Side);

            var card =
                side.GetCardAt(
                    deathEvent.Position);

            if (card.InstanceId !=
                deathEvent.InstanceId)
            {
                throw new InvalidOperationException(
                    "Death event card does not match " +
                    "the card currently occupying its position.");
            }

            if (!card.IsAtDeathThreshold)
            {
                return null;
            }

            var metadata =
                _metadataFactory.CreateChild(
                    deathEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            var removalEvent =
                new DeathRemovalCombatEvent(
                    metadata,
                    card.InstanceId,
                    deathEvent.Position,
                    card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    card,
                    deathEvent.Position,
                    CombatCardRemovalReason.DeathRemoval,
                    metadata);

            _removalCommitter.EnsureCanCommit(
                removalEvent,
                tombstone);

            var removedCard =
                side.RemoveCardFromCombat(
                    deathEvent.Position);

            if (!ReferenceEquals(
                    removedCard,
                    card))
            {
                throw new InvalidOperationException(
                    "Removed card does not match the " +
                    "validated Death event card.");
            }

            _removalCommitter.Commit(
                removalEvent,
                tombstone);

            return removalEvent;
        }

        private void ValidateLoggedDeathEvent(
            DeathCombatEvent deathEvent)
        {
            if (!_eventLog.ContainsEvent(
                    deathEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Death event must already exist " +
                    "in the combat event log.",
                    nameof(deathEvent));
            }

            var loggedDeathEvent =
                _eventLog.GetEvent(
                    deathEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedDeathEvent,
                    deathEvent))
            {
                throw new ArgumentException(
                    "Death event must be the exact event " +
                    "stored in the combat event log.",
                    nameof(deathEvent));
            }
        }

        private void EnsureRemovalNotAlreadyLogged(
            DeathCombatEvent deathEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingEvent =
                    _eventLog.Events[index];

                if (existingEvent.Kind !=
                    CombatEventKind.DeathRemoval)
                {
                    continue;
                }

                if (!existingEvent.Metadata.HasParent)
                {
                    continue;
                }

                if (existingEvent.Metadata
                        .ParentEventId.Value ==
                    deathEvent.Metadata.EventId)
                {
                    throw new InvalidOperationException(
                        "A Death Removal event has already " +
                        "been logged for this Death event.");
                }
            }
        }

        private bool HasDirectDeleteFor(
            DeathCombatEvent deathEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var deleteEvent =
                    _eventLog.Events[index]
                        as DirectDeleteCombatEvent;

                if (deleteEvent == null)
                {
                    continue;
                }

                if (deleteEvent.InstanceId !=
                    deathEvent.InstanceId)
                {
                    continue;
                }

                if (deleteEvent.Metadata.SequenceNo <=
                    deathEvent.Metadata.SequenceNo)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool HasRescueFor(
            DeathCombatEvent deathEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingEvent =
                    _eventLog.Events[index];

                if (existingEvent.Kind !=
                    CombatEventKind.Rescue)
                {
                    continue;
                }

                if (!existingEvent.Metadata.HasParent)
                {
                    continue;
                }

                if (existingEvent.Metadata
                        .ParentEventId.Value ==
                    deathEvent.Metadata.EventId)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureMetadataCanBeAppended(
            CombatEventMetadata metadata)
        {
            if (_eventLog.ContainsEvent(
                    metadata.EventId))
            {
                throw new InvalidOperationException(
                    $"Allocated EventId already exists " +
                    $"in the log: {metadata.EventId}.");
            }

            if (_eventLog.Count == 0)
            {
                return;
            }

            var previousSequence =
                _eventLog.Events[
                    _eventLog.Count - 1]
                    .Metadata.SequenceNo;

            if (metadata.SequenceNo <=
                previousSequence)
            {
                throw new InvalidOperationException(
                    "Allocated SequenceNo is not greater " +
                    "than the latest logged sequence.");
            }
        }
    }
}