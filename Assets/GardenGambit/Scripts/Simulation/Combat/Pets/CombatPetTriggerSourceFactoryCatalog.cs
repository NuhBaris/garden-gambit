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

            _usageCommitter =
                usageCommitter;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;
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
                                _targetDamageReductionRegistry)
                    });
        }
    }
}