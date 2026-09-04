using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatSidePetState
    {
        public CombatSidePetState(
            CombatSide side,
            CombatPetRegistry pets)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Combat Pet side requires Player " +
                    "or Enemy.");
            }

            if (pets == null)
            {
                throw new ArgumentNullException(
                    nameof(pets));
            }

            Side =
                side;

            Pets =
                pets;
        }

        public CombatSide Side
        {
            get;
        }

        public CombatPetRegistry Pets
        {
            get;
        }

        public int Count =>
            Pets.Count;

        public CombatPetState GetPetAt(
            int sourceOrder)
        {
            if (sourceOrder < 0 ||
                sourceOrder >= Pets.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sourceOrder),
                    sourceOrder,
                    "Pet source order must identify " +
                    "an existing Pet.");
            }

            return Pets.Pets[
                sourceOrder];
        }

        public int GetSourceOrder(
            InstanceId instanceId)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid Pet InstanceId is required.",
                    nameof(instanceId));
            }

            for (var index = 0;
                 index < Pets.Count;
                 index++)
            {
                if (Pets.Pets[index].InstanceId ==
                    instanceId)
                {
                    return index;
                }
            }

            throw new ArgumentException(
                $"Pet {instanceId} does not belong " +
                $"to the {Side} combat Pet side.",
                nameof(instanceId));
        }
    }
}