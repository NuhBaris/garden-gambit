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
            : this(
                usageRegistry,
                sourceDamageModifierRegistry,
                targetDamageReductionRegistry,
                finalRankModifierRegistry,
                new CombatPetTriggerUsageRegistry())
        {
        }

        public CombatPetTriggerRuntime(
            CombatPetCardTriggerUsageRegistry usageRegistry,
            CombatNormalAttackSourceDamageModifierRegistry sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry targetDamageReductionRegistry,
            CombatFinalRankModifierRegistry finalRankModifierRegistry,
            CombatPetTriggerUsageRegistry petUsageRegistry)
            : this(
                usageRegistry,
                sourceDamageModifierRegistry,
                targetDamageReductionRegistry,
                finalRankModifierRegistry,
                petUsageRegistry,
                new
                    CombatPetLimitedTriggerUsageRegistry())
        {
        }

        public CombatPetTriggerRuntime(
            CombatPetCardTriggerUsageRegistry usageRegistry,
            CombatNormalAttackSourceDamageModifierRegistry sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionRegistry targetDamageReductionRegistry,
            CombatFinalRankModifierRegistry finalRankModifierRegistry,
            CombatPetTriggerUsageRegistry petUsageRegistry,
            CombatPetLimitedTriggerUsageRegistry
                limitedUsageRegistry)
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

            if (petUsageRegistry == null)
            {
                throw new ArgumentNullException(nameof(petUsageRegistry));
            }

            if (limitedUsageRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(limitedUsageRegistry));
            }

            PetUsageRegistry = petUsageRegistry;
            PetUsageCommitter = new CombatPetTriggerUsageCommitter(petUsageRegistry);

            LimitedUsageRegistry = limitedUsageRegistry;
            LimitedUsageCommitter =
                new CombatPetLimitedTriggerUsageCommitter(
                    limitedUsageRegistry);

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
                        UsageCommitter,
                        LimitedUsageCommitter);

            FactoryCatalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        UsageCommitter,
                        sourceDamageModifierRegistry,
                        targetDamageReductionRegistry,
                        finalRankModifierRegistry,
                        LimitedUsageCommitter);

            FactoryRegistry =
                FactoryCatalog.CreateRegistry();

            SourceBuilder =
                new CombatPetTriggerSourceBuilder(
                    FactoryRegistry);
        }

        public CombatPetTriggerUsageRegistry PetUsageRegistry { get; }

        public CombatPetTriggerUsageCommitter PetUsageCommitter { get; }

        public CombatPetLimitedTriggerUsageRegistry
            LimitedUsageRegistry
        {
            get;
        }

        public CombatPetLimitedTriggerUsageCommitter
            LimitedUsageCommitter
        {
            get;
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

        public CombatTriggerSourceRegistry BuildSourceRegistry(
            CombatState state,
            CombatArmorGainResolver armorGainResolver)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            var factoryRegistry = FactoryCatalog.CreateRegistry(
                armorGainResolver);

            var sourceBuilder = new CombatPetTriggerSourceBuilder(
                factoryRegistry);

            return sourceBuilder.BuildRegistry(state);
        }

        public CombatTriggerSourceRegistry BuildSourceRegistry(
            CombatState state,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var factoryRegistry = FactoryCatalog.CreateRegistry(
                armorGainResolver, attackGainResolver, cardLookup, PetUsageCommitter);
            return new CombatPetTriggerSourceBuilder(factoryRegistry).BuildRegistry(state);
        }

        public CombatTriggerSourceRegistry BuildSourceRegistry(
            CombatState state,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatRescueResolver rescueResolver)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var factoryRegistry = FactoryCatalog.CreateRegistry(
                armorGainResolver,
                attackGainResolver,
                cardLookup,
                PetUsageCommitter,
                rescueResolver);

            return new CombatPetTriggerSourceBuilder(
                    factoryRegistry)
                .BuildRegistry(state);
        }

        public CombatTriggerSourceRegistry BuildSourceRegistry(
            CombatState state,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatRescueResolver rescueResolver,
            CombatHpGainResolver hpGainResolver,
            CombatEventLog eventLog)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            var factoryRegistry =
                FactoryCatalog.CreateRegistry(
                    armorGainResolver,
                    attackGainResolver,
                    cardLookup,
                    PetUsageCommitter,
                    rescueResolver,
                    hpGainResolver,
                    eventLog);

            return new CombatPetTriggerSourceBuilder(
                    factoryRegistry)
                .BuildRegistry(state);
        }

        public CombatTriggerSourceRegistry BuildSourceRegistry(
            CombatState state,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatRescueResolver rescueResolver,
            CombatHpGainResolver hpGainResolver)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var factoryRegistry = FactoryCatalog.CreateRegistry(
                armorGainResolver,
                attackGainResolver,
                cardLookup,
                PetUsageCommitter,
                rescueResolver,
                hpGainResolver);

            return new CombatPetTriggerSourceBuilder(
                    factoryRegistry)
                .BuildRegistry(state);
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

            var armorGainResolver = new CombatArmorGainResolver(
                metadataFactory,
                eventLog);

            var sourceRegistry = BuildSourceRegistry(
                state,
                armorGainResolver,
                new CombatAttackGainResolver(metadataFactory, eventLog),
                new CombatCardLookup(eventLog),
                new CombatRescueResolver(metadataFactory, eventLog),
                new CombatHpGainResolver(metadataFactory, eventLog),
                eventLog);

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
