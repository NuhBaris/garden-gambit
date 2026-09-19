using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        KoalaPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly
            CombatPetNormalAttackTriggerSource
            _normalAttackTriggerSource;

        public KoalaPetTriggerSource(
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
                    KoalaPetNormalAttackTriggerHandler(
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
            KoalaPetNormalAttackTriggerHandler
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