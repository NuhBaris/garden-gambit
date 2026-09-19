using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class LlamaPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetNormalAttackTriggerSource
            _petTriggerSource;

        public LlamaPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageReductionRegistry));
            }

            Handler =
                new LlamaPetNormalAttackTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    targetDamageReductionRegistry);

            _petTriggerSource =
                new CombatPetNormalAttackTriggerSource(
                    Handler);
        }

        public LlamaPetNormalAttackTriggerHandler Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                Handler.UsageCommitter;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                Handler.TargetDamageReductionRegistry;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _petTriggerSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _petTriggerSource.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}
