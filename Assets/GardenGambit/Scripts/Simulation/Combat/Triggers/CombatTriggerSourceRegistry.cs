using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerSourceRegistry :
        ICombatTriggerSource
    {
        private readonly List<ICombatTriggerSource>
            _sources;

        private readonly ReadOnlyCollection<
            ICombatTriggerSource> _readOnlySources;

        public CombatTriggerSourceRegistry(
            IEnumerable<ICombatTriggerSource> sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(
                    nameof(sources));
            }

            _sources =
                new List<ICombatTriggerSource>();

            foreach (var source in sources)
            {
                if (source == null)
                {
                    throw new ArgumentException(
                        "Combat trigger source registry " +
                        "cannot contain a null source.",
                        nameof(sources));
                }

                _sources.Add(source);
            }

            _readOnlySources =
                _sources.AsReadOnly();
        }

        public int Count =>
            _sources.Count;

        public IReadOnlyList<ICombatTriggerSource>
            Sources =>
                _readOnlySources;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (sourceEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceEvent));
            }

            var candidates =
                new List<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>();

            foreach (var source in _sources)
            {
                var discoveredCandidates =
                    source.DiscoverTriggers(
                        state,
                        sourceEvent);

                if (discoveredCandidates == null)
                {
                    throw new InvalidOperationException(
                        "Combat trigger source discovery " +
                        "cannot return null.");
                }

                foreach (var candidate in
                         discoveredCandidates)
                {
                    if (candidate == null)
                    {
                        throw new InvalidOperationException(
                            "Combat trigger source discovery " +
                            "cannot contain a null candidate.");
                    }

                    candidates.Add(candidate);
                }
            }

            return candidates.AsReadOnly();
        }
    }
}