using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatTriggerHandlerSource
            _handlerSource;

        public CombatPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            ICombatTriggerHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(
                    nameof(handler));
            }

            OrderKeyProvider =
                new CombatPetTriggerOrderKeyProvider(
                    side,
                    petInstanceId);

            Handler = handler;

            _handlerSource =
                new CombatTriggerHandlerSource(
                    OrderKeyProvider,
                    handler);
        }

        public CombatSide Side =>
            OrderKeyProvider.Side;

        public InstanceId PetInstanceId =>
            OrderKeyProvider.PetInstanceId;

        public CombatPetTriggerOrderKeyProvider
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
            return _handlerSource.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}