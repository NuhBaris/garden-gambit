using System;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceFactoryCatalogFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullFinalRankModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatPetTriggerSourceFactoryCatalog(
                            CreateUsageCommitter(),
                            new
                                CombatNormalAttackSourceDamageModifierRegistry(),
                            new
                                CombatNormalAttackTargetDamageReductionRegistry(),
                            null));
        }

        [Test]
        public void
            Constructor_WithExplicitFinalRankRegistry_PreservesExactInstance()
        {
            var finalRankModifierRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry(),
                        new
                            CombatNormalAttackTargetDamageReductionRegistry(),
                        finalRankModifierRegistry);

            Assert.That(
                catalog.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));
        }

        [Test]
        public void
            LegacyConstructor_CreatesEmptyFinalRankRegistry()
        {
            var catalog =
                new
                    CombatPetTriggerSourceFactoryCatalog(
                        CreateUsageCommitter(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry(),
                        new
                            CombatNormalAttackTargetDamageReductionRegistry());

            Assert.That(
                catalog.FinalRankModifierRegistry,
                Is.Not.Null);

            Assert.That(
                catalog.FinalRankModifierRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            Runtime_ProvidesItsFinalRankRegistryToFactoryCatalog()
        {
            var finalRankModifierRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var runtime =
                new CombatPetTriggerRuntime(
                    new
                        CombatPetCardTriggerUsageRegistry(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry(),
                    new
                        CombatNormalAttackTargetDamageReductionRegistry(),
                    finalRankModifierRegistry);

            Assert.That(
                runtime.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));

            Assert.That(
                runtime.FactoryCatalog
                    .FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));

            Assert.That(
                runtime.FactoryCatalog
                    .FinalRankModifierRegistry,
                Is.SameAs(
                    runtime.FinalRankModifierRegistry));
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