using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventQueueProcessor
    {
        private readonly CombatEventQueue
            _eventQueue;

        public CombatEventQueueProcessor(
            CombatEventQueue eventQueue)
        {
            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            _eventQueue = eventQueue;
        }

        public CombatEvent ProcessNext(
            Action<CombatEvent> processEvent)
        {
            if (processEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(processEvent));
            }

            var nextEvent =
                _eventQueue.PeekNext();

            processEvent(
                nextEvent);

            var processedEvent =
                _eventQueue.DequeueNext();

            if (!ReferenceEquals(
                    processedEvent,
                    nextEvent))
            {
                throw new InvalidOperationException(
                    "Processed event does not match " +
                    "the next queued combat event.");
            }

            return processedEvent;
        }
    }
}