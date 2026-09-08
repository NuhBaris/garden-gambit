using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetBattleEndTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetTriggerSource
            _petTriggerSource;

        public CombatPetBattleEndTriggerSource(
            CombatPetBattleEndTriggerHandler
                handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(
                    nameof(handler));
            }

            Handler =
                handler;

            _petTriggerSource =
                new CombatPetTriggerSource(
                    handler.Side,
                    handler.PetInstanceId,
                    handler);
        }

        public CombatPetBattleEndTriggerHandler
            Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _petTriggerSource
                    .OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _petTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}