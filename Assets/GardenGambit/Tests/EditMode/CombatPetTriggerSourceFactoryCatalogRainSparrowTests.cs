using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceFactoryCatalogRainSparrowTests
    {
        [Test]
        public void CreateRegistry_WithNullArmorGainResolver_Throws()
        {
            var catalog = CreateCatalog();

            Assert.Throws<ArgumentNullException>(
                () => catalog.CreateRegistry(null));
        }

        [Test]
        public void
            CreateRegistry_WithArmorGainResolver_PreservesExistingRegistrationsAndDependencies()
        {
            var catalog = CreateCatalog();
            var originalRegistry = catalog.CreateRegistry();

            var registry = catalog.CreateRegistry(
                CreateArmorGainResolver());

            Assert.That(originalRegistry.Count, Is.EqualTo(3));
            Assert.That(registry.Count, Is.EqualTo(4));

            for (var index = 0;
                 index < originalRegistry.Count;
                 index++)
            {
                var definitionId =
                    originalRegistry.Factories[index].PetDefinitionId;

                Assert.That(
                    registry.Factories[index].PetDefinitionId,
                    Is.EqualTo(definitionId));

                Assert.That(
                    registry.GetFactory(definitionId),
                    Is.SameAs(registry.Factories[index]));
            }

            Assert.That(
                registry.Factories[3].PetDefinitionId,
                Is.EqualTo(CombatPetDefinitionIds.RainSparrow));

            var sunBird = registry.GetFactory(
                CombatPetDefinitionIds.SunBird)
                as SunBirdPetTriggerSourceFactory;

            var polarFerret = registry.GetFactory(
                CombatPetDefinitionIds.PolarFerret)
                as PolarFerretPetTriggerSourceFactory;

            var muskCat = registry.GetFactory(
                CombatPetDefinitionIds.MuskCat)
                as MuskCatPetTriggerSourceFactory;

            Assert.That(sunBird, Is.Not.Null);
            Assert.That(polarFerret, Is.Not.Null);
            Assert.That(muskCat, Is.Not.Null);

            Assert.That(
                sunBird.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
            Assert.That(
                polarFerret.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
            Assert.That(
                muskCat.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));

            Assert.That(
                sunBird.SourceDamageModifierRegistry,
                Is.SameAs(catalog.SourceDamageModifierRegistry));
            Assert.That(
                polarFerret.TargetDamageReductionRegistry,
                Is.SameAs(catalog.TargetDamageReductionRegistry));
            Assert.That(
                muskCat.FinalRankModifierRegistry,
                Is.SameAs(catalog.FinalRankModifierRegistry));

            Assert.That(
                originalRegistry.Contains(
                    CombatPetDefinitionIds.RainSparrow),
                Is.False);

            var laterDefaultRegistry = catalog.CreateRegistry();

            Assert.That(laterDefaultRegistry.Count, Is.EqualTo(3));
            Assert.That(
                laterDefaultRegistry.Contains(
                    CombatPetDefinitionIds.RainSparrow),
                Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void
            CreateRegistry_RainSparrowRegistration_CreatesSourceWithExactDependencies(
                CombatSide side)
        {
            var catalog = CreateCatalog();
            var armorGainResolver = CreateArmorGainResolver();

            var registry = catalog.CreateRegistry(
                armorGainResolver);

            var factory = GetRainSparrowFactory(registry);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(CombatPetDefinitionIds.RainSparrow));
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
            Assert.That(
                factory.ArmorGainResolver,
                Is.SameAs(armorGainResolver));

            var pet = new CombatPetState(
                CombatPetDefinitionIds.RainSparrow,
                new InstanceId(1001));

            var sources = new List<ICombatTriggerSource>(
                factory.CreateSources(side, pet));

            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(
                sources[0],
                Is.TypeOf<RainSparrowPetTriggerSource>());

            var source = (RainSparrowPetTriggerSource)sources[0];

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(pet.InstanceId));
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
            Assert.That(
                source.ArmorGainResolver,
                Is.SameAs(armorGainResolver));

            Assert.That(
                source.Handler.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
            Assert.That(
                source.Handler.ArmorGainResolver,
                Is.SameAs(armorGainResolver));
        }

        [Test]
        public void
            CreateRegistry_WithDifferentResolvers_KeepsEachFactoryBoundToItsResolver()
        {
            var catalog = CreateCatalog();
            var firstResolver = CreateArmorGainResolver();
            var secondResolver = CreateArmorGainResolver();

            var firstRegistry = catalog.CreateRegistry(
                firstResolver);

            var secondRegistry = catalog.CreateRegistry(
                secondResolver);

            var firstFactory = GetRainSparrowFactory(
                firstRegistry);

            var secondFactory = GetRainSparrowFactory(
                secondRegistry);

            Assert.That(
                secondRegistry,
                Is.Not.SameAs(firstRegistry));
            Assert.That(
                secondFactory,
                Is.Not.SameAs(firstFactory));

            Assert.That(firstRegistry.Count, Is.EqualTo(4));
            Assert.That(secondRegistry.Count, Is.EqualTo(4));

            Assert.That(
                firstFactory.ArmorGainResolver,
                Is.SameAs(firstResolver));
            Assert.That(
                secondFactory.ArmorGainResolver,
                Is.SameAs(secondResolver));

            Assert.That(
                firstFactory.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
            Assert.That(
                secondFactory.UsageCommitter,
                Is.SameAs(catalog.UsageCommitter));
        }

        private static RainSparrowPetTriggerSourceFactory
            GetRainSparrowFactory(
                CombatPetTriggerSourceFactoryRegistry registry)
        {
            var factory = registry.GetFactory(
                CombatPetDefinitionIds.RainSparrow);

            Assert.That(
                factory,
                Is.TypeOf<RainSparrowPetTriggerSourceFactory>());

            return (RainSparrowPetTriggerSourceFactory)factory;
        }

        private static CombatPetTriggerSourceFactoryCatalog
            CreateCatalog()
        {
            return new CombatPetTriggerSourceFactoryCatalog(
                new CombatPetCardTriggerUsageCommitter(
                    new CombatPetCardTriggerUsageRegistry()),
                new CombatNormalAttackSourceDamageModifierRegistry(),
                new CombatNormalAttackTargetDamageReductionRegistry(),
                new CombatFinalRankModifierRegistry());
        }

        private static CombatArmorGainResolver CreateArmorGainResolver()
        {
            return new CombatArmorGainResolver(
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator()),
                new CombatEventLog());
        }
    }
}