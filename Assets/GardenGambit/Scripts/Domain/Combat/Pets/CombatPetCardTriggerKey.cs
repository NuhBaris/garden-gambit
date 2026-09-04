using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public readonly struct
        CombatPetCardTriggerKey :
        IEquatable<CombatPetCardTriggerKey>
    {
        public CombatPetCardTriggerKey(
            InstanceId petInstanceId,
            InstanceId cardInstanceId)
        {
            if (!petInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Pet trigger key requires a valid " +
                    "Pet InstanceId.",
                    nameof(petInstanceId));
            }

            if (!cardInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Pet trigger key requires a valid " +
                    "card InstanceId.",
                    nameof(cardInstanceId));
            }

            PetInstanceId =
                petInstanceId;

            CardInstanceId =
                cardInstanceId;
        }

        public InstanceId PetInstanceId
        {
            get;
        }

        public InstanceId CardInstanceId
        {
            get;
        }

        public bool IsValid =>
            PetInstanceId.IsValid &&
            CardInstanceId.IsValid;

        public bool Equals(
            CombatPetCardTriggerKey other)
        {
            return PetInstanceId ==
                       other.PetInstanceId &&
                   CardInstanceId ==
                       other.CardInstanceId;
        }

        public override bool Equals(
            object obj)
        {
            return obj is
                       CombatPetCardTriggerKey other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode =
                    PetInstanceId.GetHashCode();

                hashCode =
                    (hashCode * 397) ^
                    CardInstanceId.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString()
        {
            return
                $"Pet:{PetInstanceId}:" +
                $"Card:{CardInstanceId}";
        }

        public static bool operator ==(
            CombatPetCardTriggerKey left,
            CombatPetCardTriggerKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CombatPetCardTriggerKey left,
            CombatPetCardTriggerKey right)
        {
            return !left.Equals(right);
        }
    }
}