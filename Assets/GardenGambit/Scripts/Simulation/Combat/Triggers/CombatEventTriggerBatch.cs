using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventTriggerBatch<TTrigger>
        where TTrigger : class
    {
        private readonly List<
            CombatTriggerCandidate<TTrigger>>
            _candidates;

        private readonly ReadOnlyCollection<
            CombatTriggerCandidate<TTrigger>>
            _readOnlyCandidates;

        public CombatEventTriggerBatch(
            CombatEvent sourceEvent,
            IEnumerable<
                CombatTriggerCandidate<TTrigger>>
                candidates)
        {
            if (sourceEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceEvent));
            }

            if (candidates == null)
            {
                throw new ArgumentNullException(
                    nameof(candidates));
            }

            _candidates =
                new List<
                    CombatTriggerCandidate<TTrigger>>();

            foreach (var candidate in candidates)
            {
                if (candidate == null)
                {
                    throw new ArgumentException(
                        "Combat event trigger batch cannot " +
                        "contain a null candidate.",
                        nameof(candidates));
                }

                _candidates.Add(candidate);
            }

            SourceEvent = sourceEvent;

            _readOnlyCandidates =
                _candidates.AsReadOnly();
        }

        public CombatEvent SourceEvent { get; }

        public int Count =>
            _candidates.Count;

        public IReadOnlyList<
            CombatTriggerCandidate<TTrigger>>
            Candidates =>
                _readOnlyCandidates;
    }
}