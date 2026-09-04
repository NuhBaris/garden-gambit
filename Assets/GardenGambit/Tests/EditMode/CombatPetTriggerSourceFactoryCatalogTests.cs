using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceFactoryCatalogTests
    {
        [Test]
        public void Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryCatalog(
                            null,
                            new
                                CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [Test]
        public void Constructor_WithNullModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryCatalog(
                            CreateUsageCommitter(),
                            null));
        }

        [Test]
        public void
            Constructor_WithNullTargetReductionRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryCatalog(
                            CreateUsageCommitter(),
                            new
                                CombatNormalAttackSourceDamageModifierRegistry(),
                            null));
        }

        [Test]
        public void
            Constructor_ExposesExactDependencies()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        usageCommitter,
                        modifierRegistry,
                        reductionRegistry);

            Assert.That(
                catalog.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                catalog.SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));

            Assert.That(
                catalog.TargetDamageReductionRegistry,
                Is.SameAs(
                    reductionRegistry));
        }

        [Test]
        public void
            Constructor_LegacyOverloadCreatesTargetReductionRegistry()
        {
            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry());

            Assert.That(
                catalog.TargetDamageReductionRegistry,
                Is.Not.Null);

            Assert.That(
                catalog.TargetDamageReductionRegistry
                    .Count,
                Is.Zero);
        }

        [Test]
        public void CreateRegistry_RegistersSunBirdFactory()
        {
            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry());

            var registry =
                catalog.CreateRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds
                        .SunBird),
                Is.True);

            Assert.That(
                registry.Factories[0],
                Is.TypeOf<
                    SunBirdPetTriggerSourceFactory>());
        }

        [Test]
        public void
            CreateRegistry_SunBirdFactoryUsesCatalogDependencies()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        usageCommitter,
                        modifierRegistry);

            var registry =
                catalog.CreateRegistry();

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds
                            .SunBird)
                    as
                        SunBirdPetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    CombatPetDefinitionIds
                        .SunBird));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                factory.SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));
        }

        [Test]
        public void
            CreateRegistry_CalledTwice_ReturnsIndependentRegistriesWithSharedRuntimeDependencies()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        usageCommitter,
                        modifierRegistry);

            var firstRegistry =
                catalog.CreateRegistry();

            var secondRegistry =
                catalog.CreateRegistry();

            Assert.That(
                firstRegistry,
                Is.Not.SameAs(
                    secondRegistry));

            Assert.That(
                firstRegistry.Count,
                Is.EqualTo(2));

            Assert.That(
                secondRegistry.Count,
                Is.EqualTo(2));

            var firstFactory =
                firstRegistry.Factories[0]
                    as
                        SunBirdPetTriggerSourceFactory;

            var secondFactory =
                secondRegistry.Factories[0]
                    as
                        SunBirdPetTriggerSourceFactory;

            Assert.That(
                firstFactory,
                Is.Not.SameAs(
                    secondFactory));

            Assert.That(
                firstFactory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                secondFactory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                firstFactory
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));

            Assert.That(
                secondFactory
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));
        }

        private static
            CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new
                CombatPetCardTriggerUsageCommitter(
                    new
                        CombatPetCardTriggerUsageRegistry());
        }
    }
}