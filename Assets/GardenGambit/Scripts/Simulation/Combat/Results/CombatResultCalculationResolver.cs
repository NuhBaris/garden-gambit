using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatResultCalculationResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatResultDamageResolver
            _damageResolver;

        private readonly
            CombatResultDamageResolutionResolver
            _resolutionResolver;

        public CombatResultCalculationResolver(
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

            _metadataFactory =
                metadataFactory;

            _eventLog =
                eventLog;

            _damageResolver =
                new CombatResultDamageResolver();

            _resolutionResolver =
                new CombatResultDamageResolutionResolver();

        }

        public CombatResultCalculatedCombatEvent
            Resolve(
                CombatState state,
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            EnsureResultNotAlreadyLogged(
                combatStartedEvent);

            var calculation =
    _damageResolver.Resolve(
        state);

            var resolution =
                _resolutionResolver.Resolve(
                    state,
                    calculation);

            var metadata =
                _metadataFactory.CreateChild(
                    combatStartedEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    metadata,
                    resolution);

            _eventLog.Append(
                resultEvent);

            return resultEvent;
        }

        private void
            ValidateLoggedCombatStartedEvent(
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            if (!combatStartedEvent
                    .Metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            if (!_eventLog.ContainsEvent(
                    combatStartedEvent
                        .Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent
                        .Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatStartedEvent))
            {
                throw new ArgumentException(
                    "Combat Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(combatStartedEvent));
            }
        }

        private void EnsureResultNotAlreadyLogged(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var resultEvent =
                    _eventLog.Events[index]
                        as
                        CombatResultCalculatedCombatEvent;

                if (resultEvent == null)
                {
                    continue;
                }

                if (resultEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "A Combat Result Calculated event " +
                    "has already been logged for this " +
                    "combat.");
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