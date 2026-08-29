using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerHandlerSource :
        ICombatTriggerSource
    {
        private readonly CombatTriggerCandidateFactory
            _candidateFactory;

        public CombatTriggerHandlerSource(
            ICombatTriggerOrderKeyProvider
                orderKeyProvider,
            ICombatTriggerHandler handler)
        {
            if (orderKeyProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(orderKeyProvider));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(
                    nameof(handler));
            }

            OrderKeyProvider = orderKeyProvider;
            Handler = handler;

            _candidateFactory =
                new CombatTriggerCandidateFactory();
        }

        public ICombatTriggerOrderKeyProvider
            OrderKeyProvider
        {
            get;
        }

        public ICombatTriggerHandler Handler
        {
            get;
        }

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            var wasCreated =
                _candidateFactory.TryCreate(
                    state,
                    sourceEvent,
                    OrderKeyProvider,
                    Handler,
                    out candidate);

            if (!wasCreated)
            {
                return new CombatTriggerCandidate<
                    ICombatTriggerHandler>[0];
            }

            return new[]
            {
                candidate
            };
        }
    }
}