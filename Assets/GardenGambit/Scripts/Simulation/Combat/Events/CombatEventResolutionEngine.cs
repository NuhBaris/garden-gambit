using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventResolutionEngine
    {
        private readonly CombatState
            _state;

        private readonly CombatEventQueue
            _eventQueue;

        private readonly CombatTriggerEngine
            _triggerEngine;

        private readonly CombatDeathChainFinalizer
            _deathChainFinalizer;

        public CombatEventResolutionEngine(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

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

            if (sourceRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceRegistry));
            }

            _state = state;
            _eventQueue = eventQueue;

            _triggerEngine =
                new CombatTriggerEngine(
                    state,
                    eventQueue,
                    sourceRegistry);

            _deathChainFinalizer =
                new CombatDeathChainFinalizer(
                    metadataFactory,
                    eventLog,
                    eventQueue);
        }

        public bool HasPendingWork =>
            _eventQueue.HasPending ||
            _deathChainFinalizer
                .UnscannedEventCount > 0;

        public int PendingEventCount =>
            _eventQueue.PendingCount;

        public int UnscannedEventCount =>
            _deathChainFinalizer
                .UnscannedEventCount;

        public int Drain(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumPassCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCount),
                    maximumPassCount,
                    "Maximum pass count must be " +
                    "greater than zero.");
            }

            if (maximumEventCountPerPass <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumEventCountPerPass),
                    maximumEventCountPerPass,
                    "Maximum event count per pass must " +
                    "be greater than zero.");
            }

            if (maximumTriggerCountPerEvent <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTriggerCountPerEvent),
                    maximumTriggerCountPerEvent,
                    "Maximum trigger count per event must " +
                    "be greater than zero.");
            }

            var completedPassCount = 0;
            var processedEventCount = 0;

            while (HasPendingWork)
            {
                if (completedPassCount >=
                    maximumPassCount)
                {
                    throw new InvalidOperationException(
                        "Combat event resolution pass budget " +
                        "was exhausted while work was still " +
                        "pending.");
                }

                if (_eventQueue.HasPending)
                {
                    var processedInPass =
                        _triggerEngine.Drain(
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);

                    processedEventCount = checked(
                        processedEventCount +
                        processedInPass);
                }

                _deathChainFinalizer
                    .CompletePendingDeathChains(
                        _state);

                completedPassCount = checked(
                    completedPassCount + 1);
            }

            return processedEventCount;
        }
    }
}