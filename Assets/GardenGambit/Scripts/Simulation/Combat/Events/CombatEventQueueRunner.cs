using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventQueueRunner
    {
        private readonly CombatEventQueue
            _eventQueue;

        private readonly CombatEventQueueProcessor
            _eventProcessor;

        public CombatEventQueueRunner(
            CombatEventQueue eventQueue)
        {
            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            _eventQueue = eventQueue;
            _eventProcessor =
                new CombatEventQueueProcessor(
                    eventQueue);
        }

        public int Drain(
            int maximumEventCount,
            Action<CombatEvent> processEvent)
        {
            if (maximumEventCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumEventCount),
                    maximumEventCount,
                    "Maximum event count must be " +
                    "greater than zero.");
            }

            if (processEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(processEvent));
            }

            var processedCount = 0;

            while (_eventQueue.HasPending)
            {
                if (processedCount >=
                    maximumEventCount)
                {
                    throw new InvalidOperationException(
                        "Combat event processing budget " +
                        "was exhausted while events " +
                        "were still pending.");
                }

                _eventProcessor.ProcessNext(
                    processEvent);

                processedCount =
                    checked(processedCount + 1);
            }

            return processedCount;
        }
    }
}