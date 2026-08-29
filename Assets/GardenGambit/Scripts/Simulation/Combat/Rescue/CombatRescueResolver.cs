using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatRescueResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatRescueResolver(
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
        }

        public RescueCombatEvent ApplyRescue(
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

            EnsureRescueNotAlreadyLogged(
                deathEvent);

            EnsureCardWasNotDirectDeleted(
                deathEvent);

            var card =
                state.GetSide(
                        deathEvent.Position.Side)
                    .GetCardAt(
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
                throw new InvalidOperationException(
                    "Only a card still at the death " +
                    "threshold can be rescued.");
            }

            var metadata =
                _metadataFactory.CreateChild(
                    deathEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            var previousHp =
                card.CurrentHp;

            var rescueEvent =
                new RescueCombatEvent(
                    metadata,
                    card.InstanceId,
                    deathEvent.Position,
                    previousHp,
                    1);

            card.RescueToOneHp();

            _eventLog.Append(
                rescueEvent);

            return rescueEvent;
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

        private void EnsureRescueNotAlreadyLogged(
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
                    throw new InvalidOperationException(
                        "A Rescue event has already been " +
                        "logged for this Death event.");
                }
            }
        }

        private void EnsureCardWasNotDirectDeleted(
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

                throw new InvalidOperationException(
                    "A Direct Deleted card cannot be rescued.");
            }
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