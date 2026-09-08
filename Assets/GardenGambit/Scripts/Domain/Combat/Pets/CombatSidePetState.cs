using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatSidePetState
    {
        public const int MaximumPetCount = 2;

        public const int UpperPetSourceOrder = 0;

        public const int LowerPetSourceOrder = 1;

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

            if (pets.Count > MaximumPetCount)
            {
                throw new ArgumentException(
                    "A combat side cannot contain more " +
                    "than two Pets.",
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

        public BoardRow GetAffectedRowAt(
            int sourceOrder)
        {
            GetPetAt(
                sourceOrder);

            if (sourceOrder ==
                UpperPetSourceOrder)
            {
                return BoardRow.Front;
            }

            return BoardRow.Back;
        }

        public BoardRow GetAffectedRow(
            InstanceId petInstanceId)
        {
            var sourceOrder =
                GetSourceOrder(
                    petInstanceId);

            return GetAffectedRowAt(
                sourceOrder);
        }
    }
}