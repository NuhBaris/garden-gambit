using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatPetTriggerRuntime
    {
        public CombatPetTriggerRuntime()
            : this(
                new
                    CombatPetCardTriggerUsageRegistry(),
                new
                    CombatNormalAttackSourceDamageModifierRegistry(),
                new
                    CombatNormalAttackTargetDamageReductionRegistry())
        {
        }

        public CombatPetTriggerRuntime(
            CombatPetCardTriggerUsageRegistry
                usageRegistry,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
            : this(
                usageRegistry,
                sourceDamageModifierRegistry,
                new
                    CombatNormalAttackTargetDamageReductionRegistry())
        {
        }

        public CombatPetTriggerRuntime(
            CombatPetCardTriggerUsageRegistry
                usageRegistry,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
        {
            if (usageRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(usageRegistry));
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

            UsageRegistry =
                usageRegistry;

            SourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            TargetDamageReductionRegistry =
                targetDamageReductionRegistry;

            UsageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        usageRegistry);

            TargetDamageReductionResolver =
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        targetDamageReductionRegistry,
                        UsageCommitter);

            FactoryCatalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        UsageCommitter,
                        sourceDamageModifierRegistry,
                        targetDamageReductionRegistry);

            FactoryRegistry =
                FactoryCatalog.CreateRegistry();

            SourceBuilder =
                new CombatPetTriggerSourceBuilder(
                    FactoryRegistry);
        }

        public CombatPetCardTriggerUsageRegistry
            UsageRegistry
        {
            get;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter
        {
            get;
        }

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry
        {
            get;
        }

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry
        {
            get;
        }

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver
        {
            get;
        }

        public CombatPetTriggerSourceFactoryCatalog
            FactoryCatalog
        {
            get;
        }

        public CombatPetTriggerSourceFactoryRegistry
            FactoryRegistry
        {
            get;
        }

        public CombatPetTriggerSourceBuilder
            SourceBuilder
        {
            get;
        }

        public CombatTriggerSourceRegistry
            BuildSourceRegistry(
                CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            return SourceBuilder.BuildRegistry(
                state);
        }
    }
}