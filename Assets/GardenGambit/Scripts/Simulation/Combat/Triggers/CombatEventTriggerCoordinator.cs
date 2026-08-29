using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatEventTriggerCoordinator<TTrigger>
        where TTrigger : class
    {
        private readonly CombatEventQueue
            _eventQueue;

        private readonly CombatTriggerDispatcher<TTrigger>
            _triggerDispatcher;

        private CombatEventTriggerBatch<TTrigger>
            _activeBatch;

        public CombatEventTriggerCoordinator(
            CombatEventQueue eventQueue)
        {
            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            _eventQueue = eventQueue;

            _triggerDispatcher =
                new CombatTriggerDispatcher<TTrigger>();
        }

        public bool HasActiveBatch =>
            _activeBatch != null;

        public CombatEventTriggerBatch<TTrigger>
            ActiveBatch =>
                _activeBatch;

        public int PendingTriggerCount =>
            _triggerDispatcher.Count;


        public CombatEvent ProcessNextEventWithSource(
            int maximumTriggerCount,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers,
            Action<CombatEvent, TTrigger>
                processTrigger)
        {
            if (processTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(processTrigger));
            }

            return ProcessNextEvent(
                maximumTriggerCount,
                discoverTriggers,
                trigger =>
                {
                    var activeBatch =
                        _activeBatch;

                    if (activeBatch == null)
                    {
                        throw new InvalidOperationException(
                            "An active trigger batch is " +
                            "required while processing a " +
                            "combat trigger.");
                    }

                    processTrigger(
                        activeBatch.SourceEvent,
                        trigger);
                });
        }

        public CombatEvent ProcessNextEvent(
            int maximumTriggerCount,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers,
            Action<TTrigger> processTrigger)
        {
            if (maximumTriggerCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTriggerCount),
                    maximumTriggerCount,
                    "Maximum trigger count must be " +
                    "greater than zero.");
            }

            if (discoverTriggers == null)
            {
                throw new ArgumentNullException(
                    nameof(discoverTriggers));
            }

            if (processTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(processTrigger));
            }

            var sourceEvent =
                _eventQueue.PeekNext();

            EnsureActiveBatch(
                sourceEvent,
                discoverTriggers);

            _triggerDispatcher.Drain(
                maximumTriggerCount,
                processTrigger);

            var processedEvent =
                _eventQueue.DequeueNext();

            if (!ReferenceEquals(
                    processedEvent,
                    sourceEvent))
            {
                throw new InvalidOperationException(
                    "Dequeued combat event does not match " +
                    "the event whose triggers were processed.");
            }

            _activeBatch = null;

            return processedEvent;
        }

        private void EnsureActiveBatch(
            CombatEvent sourceEvent,
            Func<
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<TTrigger>>>
                discoverTriggers)
        {
            if (_activeBatch != null)
            {
                if (!ReferenceEquals(
                        _activeBatch.SourceEvent,
                        sourceEvent))
                {
                    throw new InvalidOperationException(
                        "Active trigger batch does not match " +
                        "the next pending combat event.");
                }

                return;
            }

            var candidates =
                discoverTriggers(sourceEvent);

            if (candidates == null)
            {
                throw new InvalidOperationException(
                    "Combat trigger discovery cannot " +
                    "return null.");
            }

            var batch =
                new CombatEventTriggerBatch<TTrigger>(
                    sourceEvent,
                    candidates);

            foreach (var candidate in
                     batch.Candidates)
            {
                _triggerDispatcher.Enqueue(
                    candidate);
            }

            _activeBatch = batch;
        }
    }
}