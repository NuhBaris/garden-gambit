using System;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullFinalRankModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetTriggerRuntime(
                        new
                            CombatPetCardTriggerUsageRegistry(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry(),
                        new
                            CombatNormalAttackTargetDamageReductionRegistry(),
                        null));
        }

        [Test]
        public void
            DefaultConstructor_CreatesEmptyFinalRankRegistry()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            Assert.That(
                runtime.FinalRankModifierRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.FinalRankModifierRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            Constructor_WithDependencies_PreservesExactFinalRankRegistry()
        {
            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var sourceDamageModifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var targetDamageReductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var finalRankModifierRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var runtime =
                new CombatPetTriggerRuntime(
                    usageRegistry,
                    sourceDamageModifierRegistry,
                    targetDamageReductionRegistry,
                    finalRankModifierRegistry);

            Assert.That(
                runtime.UsageRegistry,
                Is.SameAs(
                    usageRegistry));

            Assert.That(
                runtime.SourceDamageModifierRegistry,
                Is.SameAs(
                    sourceDamageModifierRegistry));

            Assert.That(
                runtime.TargetDamageReductionRegistry,
                Is.SameAs(
                    targetDamageReductionRegistry));

            Assert.That(
                runtime.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankModifierRegistry));
        }

        [Test]
        public void
            LegacyConstructor_CreatesIndependentEmptyFinalRankRegistry()
        {
            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var sourceDamageModifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var targetDamageReductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var runtime =
                new CombatPetTriggerRuntime(
                    usageRegistry,
                    sourceDamageModifierRegistry,
                    targetDamageReductionRegistry);

            Assert.That(
                runtime.FinalRankModifierRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.FinalRankModifierRegistry.Count,
                Is.Zero);

            Assert.That(
                runtime.UsageRegistry,
                Is.SameAs(
                    usageRegistry));

            Assert.That(
                runtime.SourceDamageModifierRegistry,
                Is.SameAs(
                    sourceDamageModifierRegistry));

            Assert.That(
                runtime.TargetDamageReductionRegistry,
                Is.SameAs(
                    targetDamageReductionRegistry));
        }
    }
}