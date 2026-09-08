using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceFactoryCatalogMuskCatTests
    {
        [Test]
        public void
            CreateRegistry_RegistersMuskCatFactory()
        {
            var catalog =
                CreateCatalog();

            var registry =
                catalog.CreateRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(3));

            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds
                        .MuskCat),
                Is.True);

            Assert.That(
                registry.Factories[0],
                Is.TypeOf<
                    SunBirdPetTriggerSourceFactory>());

            Assert.That(
                registry.Factories[1],
                Is.TypeOf<
                    PolarFerretPetTriggerSourceFactory>());

            Assert.That(
                registry.Factories[2],
                Is.TypeOf<
                    MuskCatPetTriggerSourceFactory>());
        }

        [Test]
        public void
            CreateRegistry_MuskCatFactoryUsesCatalogDependencies()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var sourceDamageModifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var targetDamageReductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var finalRankModifierRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        usageCommitter,
                        sourceDamageModifierRegistry,
                        targetDamageReductionRegistry,
                        finalRankModifierRegistry);

            var registry =
                catalog.CreateRegistry();

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds
                            .MuskCat)
                    as
                        MuskCatPetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    CombatPetDefinitionIds
                        .MuskCat));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                factory.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));

            Assert.That(
                factory.FinalRankModifierRegistry,
                Is.SameAs(
                    catalog
                        .FinalRankModifierRegistry));
        }

        [Test]
        public void
            CreateRegistry_CalledTwice_CreatesIndependentMuskCatFactoriesWithSharedDependencies()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var finalRankModifierRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        usageCommitter,
                        new
                            CombatNormalAttackSourceDamageModifierRegistry(),
                        new
                            CombatNormalAttackTargetDamageReductionRegistry(),
                        finalRankModifierRegistry);

            var firstRegistry =
                catalog.CreateRegistry();

            var secondRegistry =
                catalog.CreateRegistry();

            var firstFactory =
                firstRegistry.GetFactory(
                        CombatPetDefinitionIds
                            .MuskCat)
                    as
                        MuskCatPetTriggerSourceFactory;

            var secondFactory =
                secondRegistry.GetFactory(
                        CombatPetDefinitionIds
                            .MuskCat)
                    as
                        MuskCatPetTriggerSourceFactory;

            Assert.That(
                firstRegistry,
                Is.Not.SameAs(
                    secondRegistry));

            Assert.That(
                firstFactory,
                Is.Not.Null);

            Assert.That(
                secondFactory,
                Is.Not.Null);

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
                firstFactory.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));

            Assert.That(
                secondFactory.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));
        }

        private static
            CombatPetTriggerSourceFactoryCatalog
            CreateCatalog()
        {
            return new
                CombatPetTriggerSourceFactoryCatalog(
                    CreateUsageCommitter(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry(),
                    new
                        CombatNormalAttackTargetDamageReductionRegistry(),
                    new
                        CombatFinalRankModifierRegistry());
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