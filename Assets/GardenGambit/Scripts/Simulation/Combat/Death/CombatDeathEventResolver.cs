using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatDeathEventResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatDeathEventResolver(
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

        public DeathCombatEvent AppendFromDamage(
            DamageAppliedCombatEvent damageEvent)
        {
            if (damageEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(damageEvent));
            }

            ValidateLoggedEvent(
                damageEvent,
                nameof(damageEvent),
                "Damage");

            if (!damageEvent.Result
                    .EnteredDeathThreshold)
            {
                return null;
            }

            EnsureDeathNotAlreadyLogged(
                damageEvent,
                "damage event");

            var metadata =
                _metadataFactory.CreateChild(
                    damageEvent.Metadata);

            var deathEvent =
                new DeathCombatEvent(
                    metadata,
                    damageEvent.TargetInstanceId,
                    damageEvent.TargetPosition,
                    damageEvent.Result.PreviousHp,
                    damageEvent.Result.CurrentHp);

            _eventLog.Append(
                deathEvent);

            return deathEvent;
        }

        public DeathCombatEvent AppendFromAltar(
            CombatEvent altarEvent)
        {
            if (altarEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(altarEvent));
            }

            InstanceId donorInstanceId;
            BoardPosition donorPosition;
            int donorPreviousHp;

            var sacrificialEvent =
                altarEvent as
                    SacrificialAltarActivatedCombatEvent;

            if (sacrificialEvent != null)
            {
                donorInstanceId =
                    sacrificialEvent.DonorInstanceId;

                donorPosition =
                    sacrificialEvent.DonorPosition;

                donorPreviousHp =
                    sacrificialEvent.DonorPreviousHp;
            }
            else
            {
                var warEvent =
                    altarEvent as
                        WarAltarActivatedCombatEvent;

                if (warEvent == null)
                {
                    throw new ArgumentException(
                        "An Altar activation event is " +
                        "required.",
                        nameof(altarEvent));
                }

                donorInstanceId =
                    warEvent.DonorInstanceId;

                donorPosition =
                    warEvent.DonorPosition;

                donorPreviousHp =
                    warEvent.DonorPreviousHp;
            }

            ValidateLoggedEvent(
                altarEvent,
                nameof(altarEvent),
                "Altar activation");

            EnsureDeathNotAlreadyLogged(
                altarEvent,
                "Altar activation event");

            var metadata =
                _metadataFactory.CreateChild(
                    altarEvent.Metadata);

            var deathEvent =
                new DeathCombatEvent(
                    metadata,
                    donorInstanceId,
                    donorPosition,
                    donorPreviousHp,
                    currentHp: 0);

            _eventLog.Append(
                deathEvent);

            return deathEvent;
        }

        private void ValidateLoggedEvent(
            CombatEvent combatEvent,
            string parameterName,
            string eventDescription)
        {
            if (!_eventLog.ContainsEvent(
                    combatEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    $"{eventDescription} event must already " +
                    "exist in the combat event log.",
                    parameterName);
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatEvent))
            {
                throw new ArgumentException(
                    $"{eventDescription} event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    parameterName);
            }
        }

        private void EnsureDeathNotAlreadyLogged(
            CombatEvent parentEvent,
            string parentDescription)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingEvent =
                    _eventLog.Events[index];

                if (existingEvent.Kind !=
                    CombatEventKind.Death)
                {
                    continue;
                }

                if (!existingEvent.Metadata.HasParent)
                {
                    continue;
                }

                if (existingEvent.Metadata
                        .ParentEventId.Value ==
                    parentEvent.Metadata.EventId)
                {
                    throw new InvalidOperationException(
                        "A Death event has already been " +
                        $"logged for this {parentDescription}.");
                }
            }
        }
    }
}