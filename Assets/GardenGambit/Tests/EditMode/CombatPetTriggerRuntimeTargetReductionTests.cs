using System;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeTargetReductionTests
    {
        [Test]
        public void
            Constructor_WithNullTargetReductionRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetTriggerRuntime(
                        new
                            CombatPetCardTriggerUsageRegistry(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry(),
                        null));
        }

        [Test]
        public void
            DefaultConstructor_CreatesTargetReductionDependencies()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            Assert.That(
                runtime.TargetDamageReductionRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.TargetDamageReductionResolver,
                Is.Not.Null);

            Assert.That(
                runtime.TargetDamageReductionResolver
                    .ReductionRegistry,
                Is.SameAs(
                    runtime
                        .TargetDamageReductionRegistry));

            Assert.That(
                runtime.TargetDamageReductionResolver
                    .UsageCommitter,
                Is.SameAs(
                    runtime.UsageCommitter));

            Assert.That(
                runtime.FactoryCatalog
                    .TargetDamageReductionRegistry,
                Is.SameAs(
                    runtime
                        .TargetDamageReductionRegistry));
        }

        [Test]
        public void
            Constructor_WithDependencies_PreservesTargetRegistry()
        {
            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var sourceModifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var targetReductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var runtime =
                new CombatPetTriggerRuntime(
                    usageRegistry,
                    sourceModifierRegistry,
                    targetReductionRegistry);

            Assert.That(
                runtime.UsageRegistry,
                Is.SameAs(
                    usageRegistry));

            Assert.That(
                runtime.SourceDamageModifierRegistry,
                Is.SameAs(
                    sourceModifierRegistry));

            Assert.That(
                runtime.TargetDamageReductionRegistry,
                Is.SameAs(
                    targetReductionRegistry));

            Assert.That(
                runtime.TargetDamageReductionResolver
                    .ReductionRegistry,
                Is.SameAs(
                    targetReductionRegistry));

            Assert.That(
                runtime.FactoryCatalog
                    .TargetDamageReductionRegistry,
                Is.SameAs(
                    targetReductionRegistry));
        }

        [Test]
        public void
            LegacyConstructor_CreatesSharedTargetRegistry()
        {
            var runtime =
                new CombatPetTriggerRuntime(
                    new
                        CombatPetCardTriggerUsageRegistry(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry());

            Assert.That(
                runtime.TargetDamageReductionRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.TargetDamageReductionResolver
                    .ReductionRegistry,
                Is.SameAs(
                    runtime
                        .TargetDamageReductionRegistry));

            Assert.That(
                runtime.FactoryCatalog
                    .TargetDamageReductionRegistry,
                Is.SameAs(
                    runtime
                        .TargetDamageReductionRegistry));
        }
    }
}