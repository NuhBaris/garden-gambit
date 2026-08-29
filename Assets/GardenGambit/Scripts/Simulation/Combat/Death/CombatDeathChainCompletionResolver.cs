using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatDeathChainCompletionResolver
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly CombatDeathRemovalResolver
            _deathRemovalResolver;

        private readonly CombatPostChainAdvancementResolver
            _advancementResolver;

        public CombatDeathChainCompletionResolver(
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

            _eventLog = eventLog;

            _deathRemovalResolver =
                new CombatDeathRemovalResolver(
                    metadataFactory,
                    eventLog);

            _advancementResolver =
                new CombatPostChainAdvancementResolver(
                    metadataFactory,
                    eventLog);
        }

        public DeathRemovalCombatEvent
            CompleteDeathChain(
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

            var removalEvent =
                _deathRemovalResolver.TryApplyRemoval(
                    state,
                    deathEvent);

            if (removalEvent != null)
            {
                _advancementResolver.TryAdvanceAfterChain(
                    state,
                    removalEvent);

                return removalEvent;
            }

            var directDeleteEvent =
                GetDirectDeleteFor(
                    deathEvent);

            if (directDeleteEvent != null)
            {
                _advancementResolver.TryAdvanceAfterChain(
                    state,
                    directDeleteEvent);
            }

            return null;
        }

        private DirectDeleteCombatEvent
            GetDirectDeleteFor(
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

                return deleteEvent;
            }

            return null;
        }
    }
}