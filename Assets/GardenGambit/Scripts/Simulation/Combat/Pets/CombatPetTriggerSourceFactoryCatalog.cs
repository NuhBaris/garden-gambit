using System;
using System.Collections.Generic;
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

        private readonly
            CombatPetLimitedTriggerUsageCommitter
            _limitedUsageCommitter;

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
            : this(
                usageCommitter,
                sourceDamageModifierRegistry,
                targetDamageReductionRegistry,
                finalRankModifierRegistry,
                new CombatPetLimitedTriggerUsageCommitter(
                    new
                        CombatPetLimitedTriggerUsageRegistry()))
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
                finalRankModifierRegistry,
            CombatPetLimitedTriggerUsageCommitter
                limitedUsageCommitter)
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

            if (limitedUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(limitedUsageCommitter));
            }

            _usageCommitter =
                usageCommitter;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;

            _finalRankModifierRegistry =
                finalRankModifierRegistry;

            _limitedUsageCommitter =
                limitedUsageCommitter;
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

        public CombatPetLimitedTriggerUsageCommitter
            LimitedUsageCommitter =>
                _limitedUsageCommitter;

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

        public CombatPetTriggerSourceFactoryRegistry CreateRegistry(
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatPetTriggerUsageCommitter petUsageCommitter)
        {
            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(nameof(armorGainResolver));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(nameof(attackGainResolver));
            }

            if (cardLookup == null)
            {
                throw new ArgumentNullException(nameof(cardLookup));
            }

            if (petUsageCommitter == null)
            {
                throw new ArgumentNullException(nameof(petUsageCommitter));
            }

            var factories = new List<ICombatPetTriggerSourceFactory>(
                CreateRegistry(armorGainResolver).Factories);
            factories.Add(new HarvestMousePetTriggerSourceFactory(
                CombatPetDefinitionIds.HarvestMouse,
                petUsageCommitter,
                attackGainResolver,
                cardLookup));

            factories.Add(new WombatPetTriggerSourceFactory(
                CombatPetDefinitionIds.Wombat,
                petUsageCommitter,
                attackGainResolver));

            factories.Add(new LadybugPetTriggerSourceFactory(
                CombatPetDefinitionIds.Ladybug,
                petUsageCommitter,
                armorGainResolver));

            factories.Add(new PikaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Pika,
                _usageCommitter,
                _finalRankModifierRegistry));

            factories.Add(new JackalPetTriggerSourceFactory(
                CombatPetDefinitionIds.Jackal,
                _usageCommitter,
                _finalRankModifierRegistry));

            factories.Add(new LynxPetTriggerSourceFactory(
                CombatPetDefinitionIds.Lynx,
                _usageCommitter,
                _finalRankModifierRegistry));

            factories.Add(new BeaverPetTriggerSourceFactory(
                CombatPetDefinitionIds.Beaver,
                _usageCommitter,
                _finalRankModifierRegistry));

            factories.Add(new ChinchillaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Chinchilla,
                _usageCommitter,
                _sourceDamageModifierRegistry));

            factories.Add(new CapybaraPetTriggerSourceFactory(
                CombatPetDefinitionIds.Capybara,
                _usageCommitter,
                _sourceDamageModifierRegistry));

            factories.Add(
                new HazelDormousePetTriggerSourceFactory(
                    CombatPetDefinitionIds.HazelDormouse,
                    _usageCommitter,
                    _targetDamageReductionRegistry));

            factories.Add(
                new OtterPetTriggerSourceFactory(
                    CombatPetDefinitionIds.Otter,
                    _usageCommitter,
                    attackGainResolver));

            factories.Add(new HummingbirdPetTriggerSourceFactory(
                CombatPetDefinitionIds.Hummingbird,
                _usageCommitter,
                attackGainResolver));

            factories.Add(new TapirPetTriggerSourceFactory(
                CombatPetDefinitionIds.Tapir,
                _usageCommitter,
                attackGainResolver));

            factories.Add(new HawkPetTriggerSourceFactory(
                CombatPetDefinitionIds.Hawk,
                petUsageCommitter,
                attackGainResolver));

            factories.Add(new MacaquePetTriggerSourceFactory(
                CombatPetDefinitionIds.Macaque,
                petUsageCommitter,
                attackGainResolver));

            factories.Add(new AlpacaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Alpaca,
                petUsageCommitter,
                attackGainResolver));

            factories.Add(new SnailPetTriggerSourceFactory(
                CombatPetDefinitionIds.Snail,
                petUsageCommitter,
                armorGainResolver));

            factories.Add(new KoalaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Koala,
                _usageCommitter,
                _targetDamageReductionRegistry));

            factories.Add(new BadgerPetTriggerSourceFactory(
                CombatPetDefinitionIds.Badger,
                _usageCommitter,
                _targetDamageReductionRegistry));

            factories.Add(new PandaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Panda,
                petUsageCommitter,
                _limitedUsageCommitter,
                _targetDamageReductionRegistry));

            factories.Add(new JackalopePetTriggerSourceFactory(
                CombatPetDefinitionIds.Jackalope,
                _usageCommitter,
                _finalRankModifierRegistry));

            return new CombatPetTriggerSourceFactoryRegistry(factories);
        }

        public CombatPetTriggerSourceFactoryRegistry CreateRegistry(
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatPetTriggerUsageCommitter petUsageCommitter,
            CombatRescueResolver rescueResolver)
        {
            if (rescueResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(rescueResolver));
            }

            var factories =
                new List<ICombatPetTriggerSourceFactory>(
                    CreateRegistry(
                            armorGainResolver,
                            attackGainResolver,
                            cardLookup,
                            petUsageCommitter)
                        .Factories);

            factories.Add(
                new PhoenixPetTriggerSourceFactory(
                    CombatPetDefinitionIds.Phoenix,
                    petUsageCommitter,
                    rescueResolver));

            return new CombatPetTriggerSourceFactoryRegistry(
                factories);
        }

        public CombatPetTriggerSourceFactoryRegistry CreateRegistry(
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatPetTriggerUsageCommitter petUsageCommitter,
            CombatRescueResolver rescueResolver,
            CombatHpGainResolver hpGainResolver)
        {
            if (hpGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(hpGainResolver));
            }

            var factories =
                new List<ICombatPetTriggerSourceFactory>(
                    CreateRegistry(
                            armorGainResolver,
                            attackGainResolver,
                            cardLookup,
                            petUsageCommitter,
                            rescueResolver)
                        .Factories);

            factories.Add(
                new WhitePeacockPetTriggerSourceFactory(
                    CombatPetDefinitionIds.WhitePeacock,
                    petUsageCommitter,
                    hpGainResolver,
                    armorGainResolver,
                    attackGainResolver));

            factories.Add(
                new MarmosetPetTriggerSourceFactory(
                    CombatPetDefinitionIds.Marmoset,
                    petUsageCommitter,
                    hpGainResolver,
                    armorGainResolver));

            factories.Add(
                new ToucanPetTriggerSourceFactory(
                    CombatPetDefinitionIds.Toucan,
                    petUsageCommitter,
                    hpGainResolver));

            return new CombatPetTriggerSourceFactoryRegistry(
                factories);
        }

        public CombatPetTriggerSourceFactoryRegistry CreateRegistry(
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup,
            CombatPetTriggerUsageCommitter petUsageCommitter,
            CombatRescueResolver rescueResolver,
            CombatHpGainResolver hpGainResolver,
            CombatEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            var factories =
                new List<ICombatPetTriggerSourceFactory>(
                    CreateRegistry(
                            armorGainResolver,
                            attackGainResolver,
                            cardLookup,
                            petUsageCommitter,
                            rescueResolver,
                            hpGainResolver)
                        .Factories);

            factories.Add(
                new KiwiBirdPetTriggerSourceFactory(
                    CombatPetDefinitionIds.KiwiBird,
                    _usageCommitter,
                    armorGainResolver,
                    eventLog));

            factories.Add(
                new FruitBatPetTriggerSourceFactory(
                    CombatPetDefinitionIds.FruitBat,
                    _usageCommitter,
                    hpGainResolver,
                    eventLog));

            factories.Add(
                new GerbilPetTriggerSourceFactory(
                    CombatPetDefinitionIds.Gerbil,
                    _usageCommitter,
                    attackGainResolver,
                    eventLog));

            factories.Add(
                new IguanaPetTriggerSourceFactory(
                    CombatPetDefinitionIds.Iguana,
                    _usageCommitter,
                    hpGainResolver,
                    eventLog));

            return new CombatPetTriggerSourceFactoryRegistry(
                factories);
        }

        public CombatPetTriggerSourceFactoryRegistry CreateRegistry(
            CombatArmorGainResolver armorGainResolver)
        {
            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            var factories =
                new List<ICombatPetTriggerSourceFactory>(
                    CreateRegistry().Factories);

            factories.Add(
                new RainSparrowPetTriggerSourceFactory(
                    CombatPetDefinitionIds.RainSparrow,
                    _usageCommitter,
                    armorGainResolver));

            return new CombatPetTriggerSourceFactoryRegistry(
                factories);
        }

    }
}
