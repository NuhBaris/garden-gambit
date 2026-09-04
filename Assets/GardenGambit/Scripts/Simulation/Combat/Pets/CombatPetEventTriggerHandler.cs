using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetEventTriggerHandler<TEvent> :
        CombatEventTriggerHandler<TEvent>
        where TEvent : CombatEvent
    {
        protected CombatPetEventTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Pet trigger handler requires " +
                    "Player or Enemy side.");
            }

            if (!petInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid Pet InstanceId is required.",
                    nameof(petInstanceId));
            }

            Side = side;
            PetInstanceId = petInstanceId;
        }

        public CombatSide Side
        {
            get;
        }

        public InstanceId PetInstanceId
        {
            get;
        }

        protected sealed override bool
            CanTriggerTyped(
                CombatState state,
                TEvent sourceEvent)
        {
            var pet =
                GetPet(
                    state);

            return CanPetTrigger(
                state,
                sourceEvent,
                pet);
        }

        protected sealed override void ResolveTyped(
            CombatState state,
            TEvent sourceEvent)
        {
            var pet =
                GetPet(
                    state);

            ResolvePetTrigger(
                state,
                sourceEvent,
                pet);
        }

        protected abstract bool CanPetTrigger(
            CombatState state,
            TEvent sourceEvent,
            CombatPetState pet);

        protected abstract void ResolvePetTrigger(
            CombatState state,
            TEvent sourceEvent,
            CombatPetState pet);

        private CombatPetState GetPet(
            CombatState state)
        {
            var petSide =
                state.GetPets(
                    Side);

            return petSide.Pets.GetPet(
                PetInstanceId);
        }
    }
}