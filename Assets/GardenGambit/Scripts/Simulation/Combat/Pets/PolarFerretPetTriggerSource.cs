using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        PolarFerretPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly
            CombatPetNormalAttackTriggerSource
            _normalAttackTriggerSource;

        public PolarFerretPetTriggerSource(
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
                    nameof(
                        targetDamageReductionRegistry));
            }

            Handler =
                new
                    PolarFerretPetNormalAttackTriggerHandler(
                        side,
                        petInstanceId,
                        usageCommitter,
                        targetDamageReductionRegistry);

            _normalAttackTriggerSource =
                new
                    CombatPetNormalAttackTriggerSource(
                        Handler);
        }

        public
            PolarFerretPetNormalAttackTriggerHandler
            Handler
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
                Handler
                    .TargetDamageReductionRegistry;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _normalAttackTriggerSource
                    .OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _normalAttackTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}