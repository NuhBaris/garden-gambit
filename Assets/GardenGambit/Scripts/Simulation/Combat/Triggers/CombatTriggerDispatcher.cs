using System;
using System.Collections.Generic;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerDispatcher<TTrigger>
        where TTrigger : class
    {
        private readonly StableCombatTriggerQueue<TTrigger>
            _triggerQueue;

        private readonly List<
            CombatTriggerCandidate<TTrigger>>
            _deferredCandidates;

        private bool _isProcessing;

        public CombatTriggerDispatcher()
        {
            _triggerQueue =
                new StableCombatTriggerQueue<TTrigger>();

            _deferredCandidates =
                new List<
                    CombatTriggerCandidate<TTrigger>>();
        }

        public int Count =>
            _triggerQueue.Count;

        public bool HasPending =>
            _triggerQueue.HasPending;

        public void Enqueue(
            CombatTriggerCandidate<TTrigger> candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(
                    nameof(candidate));
            }

            if (_isProcessing)
            {
                _deferredCandidates.Add(
                    candidate);

                return;
            }

            EnqueueImmediately(candidate);
        }

        public TTrigger PeekNext()
        {
            return _triggerQueue.PeekNext();
        }

        public TTrigger ProcessNext(
            Action<TTrigger> processTrigger)
        {
            if (processTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(processTrigger));
            }

            if (_isProcessing)
            {
                throw new InvalidOperationException(
                    "Nested combat trigger processing " +
                    "is not allowed.");
            }

            var trigger =
                _triggerQueue.PeekNext();

            TTrigger dequeuedTrigger;

            _isProcessing = true;

            try
            {
                processTrigger(trigger);

                dequeuedTrigger =
                    _triggerQueue.DequeueNext();

                if (!ReferenceEquals(
                        dequeuedTrigger,
                        trigger))
                {
                    throw new InvalidOperationException(
                        "Dequeued combat trigger does not " +
                        "match the trigger that was processed.");
                }
            }
            catch
            {
                _deferredCandidates.Clear();

                throw;
            }
            finally
            {
                _isProcessing = false;
            }

            FlushDeferredCandidates();

            return dequeuedTrigger;
        }

        public int Drain(
            int maximumTriggerCount,
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

            if (processTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(processTrigger));
            }

            var processedCount = 0;

            while (HasPending)
            {
                if (processedCount >=
                    maximumTriggerCount)
                {
                    throw new InvalidOperationException(
                        "Combat trigger processing budget " +
                        "was exhausted while triggers were " +
                        "still pending.");
                }

                ProcessNext(processTrigger);

                processedCount = checked(
                    processedCount + 1);
            }

            return processedCount;
        }

        private void EnqueueImmediately(
            CombatTriggerCandidate<TTrigger> candidate)
        {
            _triggerQueue.Enqueue(
                candidate.Trigger,
                candidate.OrderKey);
        }

        private void FlushDeferredCandidates()
        {
            foreach (var candidate in
                     _deferredCandidates)
            {
                EnqueueImmediately(candidate);
            }

            _deferredCandidates.Clear();
        }
    }
}