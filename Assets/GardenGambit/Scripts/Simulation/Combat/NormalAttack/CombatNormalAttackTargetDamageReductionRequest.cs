using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackTargetDamageReductionRequest
    {
        public CombatNormalAttackTargetDamageReductionRequest(
            CombatEventId normalAttackEventId,
            InstanceId petInstanceId,
            InstanceId targetCardInstanceId,
            int reductionAmount)
        {
            if (!normalAttackEventId.IsValid)
            {
                throw new ArgumentException(
                    "Target damage reduction request " +
                    "requires a valid Normal Attack " +
                    "EventId.",
                    nameof(normalAttackEventId));
            }

            if (!petInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Target damage reduction request " +
                    "requires a valid Pet InstanceId.",
                    nameof(petInstanceId));
            }

            if (!targetCardInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Target damage reduction request " +
                    "requires a valid target card " +
                    "InstanceId.",
                    nameof(targetCardInstanceId));
            }

            if (petInstanceId ==
                targetCardInstanceId)
            {
                throw new ArgumentException(
                    "Pet and target card InstanceIds " +
                    "must be different.",
                    nameof(targetCardInstanceId));
            }

            if (reductionAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reductionAmount),
                    reductionAmount,
                    "Target damage reduction amount " +
                    "must be greater than zero.");
            }

            NormalAttackEventId =
                normalAttackEventId;

            PetInstanceId =
                petInstanceId;

            TargetCardInstanceId =
                targetCardInstanceId;

            ReductionAmount =
                reductionAmount;

            UsageKey =
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    targetCardInstanceId);
        }

        public CombatEventId NormalAttackEventId
        {
            get;
        }

        public InstanceId PetInstanceId
        {
            get;
        }

        public InstanceId TargetCardInstanceId
        {
            get;
        }

        public int ReductionAmount
        {
            get;
        }

        public CombatPetCardTriggerKey UsageKey
        {
            get;
        }
    }
}