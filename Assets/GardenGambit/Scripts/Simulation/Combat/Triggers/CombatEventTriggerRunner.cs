using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatEventTriggerRunner<TTrigger>
        where TTrigger : class
    {
        private readonly CombatEventQueue
            _eventQueue;

        private readonly CombatEventTriggerCoordinator<
            TTrigger> _coordinator;

        public CombatEventTriggerRunner(
            CombatEventQueue eventQueue)
        {
            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            _eventQueue = eventQueue;

            _coordinator =
                new CombatEventTriggerCoordinator<TTrigger>(
                    eventQueue);
        }

        public bool HasActiveBatch =>
            _coordinator.HasActiveBatch;

        public CombatEventTriggerBatch<TTrigger>
            ActiveBatch =>
                _coordinator.ActiveBatch;

        public int PendingTriggerCount =>
            _coordinator.PendingTriggerCount;

        public int Drain(
            int maximumEventCount,
            int maximumTriggerCountPerEvent,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers,
            Action<TTrigger> processTrigger)
        {
            ValidateRequest(
                maximumEventCount,
                maximumTriggerCountPerEvent,
                discoverTriggers);

            if (processTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(processTrigger));
            }

            return DrainCore(
                maximumEventCount,
                maximumTriggerCountPerEvent,
                discoverTriggers,
                (sourceEvent, trigger) =>
                    processTrigger(trigger));
        }

        public int DrainWithSource(
            int maximumEventCount,
            int maximumTriggerCountPerEvent,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers,
            Action<CombatEvent, TTrigger>
                processTrigger)
        {
            ValidateRequest(
                maximumEventCount,
                maximumTriggerCountPerEvent,
                discoverTriggers);

            if (processTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(processTrigger));
            }

            return DrainCore(
                maximumEventCount,
                maximumTriggerCountPerEvent,
                discoverTriggers,
                processTrigger);
        }

        private int DrainCore(
            int maximumEventCount,
            int maximumTriggerCountPerEvent,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers,
            Action<CombatEvent, TTrigger>
                processTrigger)
        {
            var processedEventCount = 0;

            while (_eventQueue.HasPending)
            {
                if (processedEventCount >=
                    maximumEventCount)
                {
                    throw new InvalidOperationException(
                        "Combat event processing budget was " +
                        "exhausted while events were still " +
                        "pending.");
                }

                _coordinator.ProcessNextEventWithSource(
                    maximumTriggerCountPerEvent,
                    discoverTriggers,
                    processTrigger);

                processedEventCount = checked(
                    processedEventCount + 1);
            }

            return processedEventCount;
        }

        private static void ValidateRequest(
            int maximumEventCount,
            int maximumTriggerCountPerEvent,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers)
        {
            if (maximumEventCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumEventCount),
                    maximumEventCount,
                    "Maximum event count must be " +
                    "greater than zero.");
            }

            if (maximumTriggerCountPerEvent <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTriggerCountPerEvent),
                    maximumTriggerCountPerEvent,
                    "Maximum trigger count per event must " +
                    "be greater than zero.");
            }

            if (discoverTriggers == null)
            {
                throw new ArgumentNullException(
                    nameof(discoverTriggers));
            }
        }
    }
}