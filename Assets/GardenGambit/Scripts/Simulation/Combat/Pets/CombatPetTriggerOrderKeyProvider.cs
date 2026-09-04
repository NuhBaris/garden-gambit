using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetTriggerOrderKeyProvider :
        ICombatTriggerOrderKeyProvider
    {
        public CombatPetTriggerOrderKeyProvider(
            CombatSide side,
            InstanceId petInstanceId)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Pet trigger order requires Player " +
                    "or Enemy side.");
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

        public CombatTriggerOrderKey GetOrderKey(
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

            var petSide =
                state.GetPets(
                    Side);

            var sourceOrder =
                petSide.GetSourceOrder(
                    PetInstanceId);

            return new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Pet,
                Side,
                sourceOrder,
                0);
        }
    }
}