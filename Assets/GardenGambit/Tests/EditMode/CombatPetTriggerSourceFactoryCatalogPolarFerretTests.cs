using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceFactoryCatalogPolarFerretTests
    {
        [Test]
        public void
            CreateRegistry_RegistersPolarFerretFactory()
        {
            var catalog =
                CreateCatalog();

            var registry =
                catalog.CreateRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.Contains(
                    CombatPetDefinitionIds
                        .PolarFerret),
                Is.True);

            Assert.That(
                registry.Factories[0],
                Is.TypeOf<
                    SunBirdPetTriggerSourceFactory>());

            Assert.That(
                registry.Factories[1],
                Is.TypeOf<
                    PolarFerretPetTriggerSourceFactory>());
        }

        [Test]
        public void
            CreateRegistry_PolarFerretFactoryUsesCatalogDependencies()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var sourceModifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var targetReductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        usageCommitter,
                        sourceModifierRegistry,
                        targetReductionRegistry);

            var registry =
                catalog.CreateRegistry();

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds
                            .PolarFerret)
                    as
                        PolarFerretPetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    CombatPetDefinitionIds
                        .PolarFerret));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                factory.TargetDamageReductionRegistry,
                Is.SameAs(
                    targetReductionRegistry));
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
                        CombatNormalAttackTargetDamageReductionRegistry());
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