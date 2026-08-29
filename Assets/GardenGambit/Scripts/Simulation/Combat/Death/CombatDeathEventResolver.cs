using System;
using GardenGambit.Domain.Combat;

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

            if (!_eventLog.ContainsEvent(
                    damageEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Damage event must already exist " +
                    "in the combat event log.",
                    nameof(damageEvent));
            }

            var loggedDamageEvent =
                _eventLog.GetEvent(
                    damageEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedDamageEvent,
                    damageEvent))
            {
                throw new ArgumentException(
                    "Damage event must be the exact event " +
                    "stored in the combat event log.",
                    nameof(damageEvent));
            }

            if (!damageEvent.Result
                    .EnteredDeathThreshold)
            {
                return null;
            }

            EnsureDeathNotAlreadyLogged(
                damageEvent);

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

            _eventLog.Append(deathEvent);

            return deathEvent;
        }

        private void EnsureDeathNotAlreadyLogged(
            DamageAppliedCombatEvent damageEvent)
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
                    damageEvent.Metadata.EventId)
                {
                    throw new InvalidOperationException(
                        "A Death event has already been " +
                        "logged for this damage event.");
                }
            }
        }
    }
}