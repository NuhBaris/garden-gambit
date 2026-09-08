using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetTriggerSourceFactoryCatalog
    {
        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackSourceDamageModifierRegistry
            _sourceDamageModifierRegistry;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        private readonly CombatFinalRankModifierRegistry
            _finalRankModifierRegistry;

        public CombatPetTriggerSourceFactoryCatalog(
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
            : this(
                usageCommitter,
                sourceDamageModifierRegistry,
                new
                    CombatNormalAttackTargetDamageReductionRegistry())
        {
        }

        public CombatPetTriggerSourceFactoryCatalog(
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
            : this(
                usageCommitter,
                sourceDamageModifierRegistry,
                targetDamageReductionRegistry,
                new
                    CombatFinalRankModifierRegistry())
        {
        }

        public CombatPetTriggerSourceFactoryCatalog(
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry,
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (sourceDamageModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        sourceDamageModifierRegistry));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        targetDamageReductionRegistry));
            }

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        finalRankModifierRegistry));
            }

            _usageCommitter =
                usageCommitter;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;

            _finalRankModifierRegistry =
                finalRankModifierRegistry;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _sourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                _targetDamageReductionRegistry;

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _finalRankModifierRegistry;

        public CombatPetTriggerSourceFactoryRegistry
            CreateRegistry()
        {
            return new
                CombatPetTriggerSourceFactoryRegistry(
                    new
                        ICombatPetTriggerSourceFactory[]
                    {
                        new
                            SunBirdPetTriggerSourceFactory(
                                CombatPetDefinitionIds
                                    .SunBird,
                                _usageCommitter,
                                _sourceDamageModifierRegistry),

                        new
                            PolarFerretPetTriggerSourceFactory(
                                CombatPetDefinitionIds
                                    .PolarFerret,
                                _usageCommitter,
                                _targetDamageReductionRegistry),

                        new
                            MuskCatPetTriggerSourceFactory(
                                CombatPetDefinitionIds
                                    .MuskCat,
                                _usageCommitter,
                                _finalRankModifierRegistry)
                    });
        }
    }
}