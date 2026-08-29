using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public abstract class CombatRescueTriggerHandler :
        CombatEventTriggerHandler<DeathCombatEvent>
    {
        private readonly CombatRescueResolver
            _rescueResolver;

        private readonly CombatEventLog
            _eventLog;

        protected CombatRescueTriggerHandler(
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

            _rescueResolver =
                new CombatRescueResolver(
                    metadataFactory,
                    eventLog);

            _eventLog = eventLog;
        }

        protected sealed override bool CanTriggerTyped(
            CombatState state,
            DeathCombatEvent sourceEvent)
        {
            return CanRescue(
                state,
                sourceEvent);
        }

        protected sealed override void ResolveTyped(
            CombatState state,
            DeathCombatEvent sourceEvent)
        {
            if (WasDirectDeleted(sourceEvent))
            {
                return;
            }

            _rescueResolver.ApplyRescue(
                state,
                sourceEvent);
        }

        protected abstract bool CanRescue(
            CombatState state,
            DeathCombatEvent sourceEvent);

        private bool WasDirectDeleted(
            DeathCombatEvent deathEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var directDeleteEvent =
                    _eventLog.Events[index]
                        as DirectDeleteCombatEvent;

                if (directDeleteEvent == null)
                {
                    continue;
                }

                if (directDeleteEvent.InstanceId !=
                    deathEvent.InstanceId)
                {
                    continue;
                }

                if (directDeleteEvent.Metadata.SequenceNo <=
                    deathEvent.Metadata.SequenceNo)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}