using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatDeathChainFinalizer
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly CombatEventQueue
            _eventQueue;

        private readonly CombatDeathChainCompletionResolver
            _completionResolver;

        private readonly
            CombatPostChainAdvancementResolver
            _advancementResolver;

        private int _scannedEventCount;

        public CombatDeathChainFinalizer(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue)
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

            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            _eventLog =
                eventLog;

            _eventQueue =
                eventQueue;

            _completionResolver =
                new CombatDeathChainCompletionResolver(
                    metadataFactory,
                    eventLog);

            _advancementResolver =
                new CombatPostChainAdvancementResolver(
                    metadataFactory,
                    eventLog);
        }

        public int ScannedEventCount =>
            _scannedEventCount;

        public int UnscannedEventCount =>
            _eventLog.Count -
            _scannedEventCount;

        public int CompletePendingDeathChains(
            CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (_eventQueue.HasPending)
            {
                throw new InvalidOperationException(
                    "Death chains cannot be finalized while " +
                    "combat events are still pending.");
            }

            var scanLimit =
                _eventLog.Count;

            var completedDeathChainCount = 0;

            while (_scannedEventCount < scanLimit)
            {
                var combatEvent =
                    _eventLog.Events[
                        _scannedEventCount];

                var deathEvent =
                    combatEvent
                        as DeathCombatEvent;

                if (deathEvent != null)
                {
                    _completionResolver
                        .CompleteDeathChain(
                            state,
                            deathEvent);

                    completedDeathChainCount =
                        checked(
                            completedDeathChainCount +
                            1);
                }
                else
                {
                    var directDeleteEvent =
                        combatEvent
                            as DirectDeleteCombatEvent;

                    if (directDeleteEvent != null)
                    {
                        CompleteDirectDeleteChain(
                            state,
                            directDeleteEvent);
                    }
                }

                _scannedEventCount =
                    checked(
                        _scannedEventCount +
                        1);
            }

            return completedDeathChainCount;
        }

        private void CompleteDirectDeleteChain(
            CombatState state,
            DirectDeleteCombatEvent
                directDeleteEvent)
        {
            if (HasAdvancementFor(
                    directDeleteEvent))
            {
                return;
            }

            _advancementResolver
                .TryAdvanceAfterChain(
                    state,
                    directDeleteEvent);
        }

        private bool HasAdvancementFor(
            DirectDeleteCombatEvent
                directDeleteEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var advancementEvent =
                    _eventLog.Events[index]
                        as CardAdvancedCombatEvent;

                if (advancementEvent == null)
                {
                    continue;
                }

                if (!advancementEvent.Metadata
                        .HasParent)
                {
                    continue;
                }

                if (advancementEvent.Metadata
                        .ParentEventId.Value !=
                    directDeleteEvent.Metadata
                        .EventId)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}