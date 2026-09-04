using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAltarActivationResolver
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly
            CombatAltarActivationContextResolver
            _contextResolver;

        private readonly CombatAltarActivatedEventFactory
            _eventFactory;

        private readonly CombatAltarTransferApplier
            _transferApplier;

        private readonly CombatDeathEventResolver
            _deathEventResolver;

        public CombatAltarActivationResolver(
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

            _eventLog =
                eventLog;

            _contextResolver =
                new CombatAltarActivationContextResolver();

            _eventFactory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            _transferApplier =
                new CombatAltarTransferApplier();

            _deathEventResolver =
                new CombatDeathEventResolver(
                    metadataFactory,
                    eventLog);
        }

        public CombatEvent TryActivate(
            CombatState state,
            CombatStartedCombatEvent
                combatStartedEvent,
            BoardPosition donorPosition)
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

            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "A valid Altar donor position " +
                    "is required.",
                    nameof(donorPosition));
            }

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            var sideState =
                state.GetSide(
                    donorPosition.Side);

            var context =
                _contextResolver.TryResolve(
                    sideState,
                    donorPosition);

            if (context == null)
            {
                return null;
            }

            EnsureActivationNotAlreadyLogged(
                combatStartedEvent,
                context.DonorInstanceId);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            var preview =
                new CombatAltarTransferApplicationPreview(
                    snapshot);

            var altarEvent =
                _eventFactory.Create(
                    combatStartedEvent,
                    snapshot);

            EnsureMetadataCanBeAppended(
                altarEvent.Metadata);

            _transferApplier.Apply(
                preview);

            _eventLog.Append(
                altarEvent);

            _deathEventResolver.AppendFromAltar(
                altarEvent);

            return altarEvent;
        }

        private void ValidateLoggedCombatStartedEvent(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            if (!combatStartedEvent.Metadata
                    .IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            if (!_eventLog.ContainsEvent(
                    combatStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent.Metadata.EventId);

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

        private void EnsureActivationNotAlreadyLogged(
            CombatStartedCombatEvent
                combatStartedEvent,
            InstanceId donorInstanceId)
        {
            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingEvent =
                    _eventLog.Events[index];

                if (existingEvent.Metadata.TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                var sacrificialEvent =
                    existingEvent as
                        SacrificialAltarActivatedCombatEvent;

                if (sacrificialEvent != null &&
                    sacrificialEvent.DonorInstanceId ==
                    donorInstanceId)
                {
                    throw new InvalidOperationException(
                        "This donor card has already " +
                        "activated an Altar during this " +
                        "combat.");
                }

                var warEvent =
                    existingEvent as
                        WarAltarActivatedCombatEvent;

                if (warEvent != null &&
                    warEvent.DonorInstanceId ==
                    donorInstanceId)
                {
                    throw new InvalidOperationException(
                        "This donor card has already " +
                        "activated an Altar during this " +
                        "combat.");
                }
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