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
                    CombatNormalAttackTargetDamageReductionRegistry(),
                new
                    CombatFinalRankModifierRegistry())
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
                    CombatNormalAttackTargetDamageReductionRegistry(),
                new
                    CombatFinalRankModifierRegistry())
        {
        }

        public CombatPetTriggerRuntime(
            CombatPetCardTriggerUsageRegistry
                usageRegistry,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
            : this(
                usageRegistry,
                sourceDamageModifierRegistry,
                targetDamageReductionRegistry,
                new
                    CombatFinalRankModifierRegistry())
        {
        }

        public CombatPetTriggerRuntime(
            CombatPetCardTriggerUsageRegistry
                usageRegistry,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry,
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
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

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        finalRankModifierRegistry));
            }

            UsageRegistry =
                usageRegistry;

            SourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            TargetDamageReductionRegistry =
                targetDamageReductionRegistry;

            FinalRankModifierRegistry =
                finalRankModifierRegistry;

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
                        targetDamageReductionRegistry,
                        finalRankModifierRegistry);

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

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry
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

        public CombatResolutionRunner
            CreateResolutionRunner(
                CombatState state,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog,
                CombatEventQueue eventQueue)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (metadataFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(metadataFactory));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            var sourceRegistry =
                BuildSourceRegistry(
                    state);

            return new CombatResolutionRunner(
                state,
                metadataFactory,
                eventLog,
                eventQueue,
                sourceRegistry,
                SourceDamageModifierRegistry,
                TargetDamageReductionResolver,
                FinalRankModifierRegistry,
                useStagedNormalAttackByDefault: true);
        }
    }
}