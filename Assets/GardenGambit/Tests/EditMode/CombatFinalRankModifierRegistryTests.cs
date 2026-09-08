using System;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatFinalRankModifierRegistryTests
    {
        [Test]
        public void Constructor_StartsWithoutModifiers()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.HasModifier(
                    cardInstanceId),
                Is.False);

            Assert.That(
                registry.GetTotalModifier(
                    cardInstanceId),
                Is.Zero);
        }

        [Test]
        public void
            AddModifier_WithInvalidInstanceId_Throws()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.AddModifier(
                    default(InstanceId),
                    1));

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void AddModifier_WithZeroDelta_Throws()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => registry.AddModifier(
                    new InstanceId(100),
                    0));

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void
            HasModifier_WithInvalidInstanceId_Throws()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.HasModifier(
                    default(InstanceId)));
        }

        [Test]
        public void
            GetTotalModifier_WithInvalidInstanceId_Throws()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.GetTotalModifier(
                    default(InstanceId)));
        }

        [Test]
        public void
            AddModifier_WithPositiveDelta_RegistersModifier()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                4);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.HasModifier(
                    cardInstanceId),
                Is.True);

            Assert.That(
                registry.GetTotalModifier(
                    cardInstanceId),
                Is.EqualTo(4));
        }

        [Test]
        public void
            AddModifier_WithNegativeDelta_RegistersModifier()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                -2);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.GetTotalModifier(
                    cardInstanceId),
                Is.EqualTo(-2));
        }

        [Test]
        public void
            AddModifier_MultipleTimesForSameCard_CombinesModifiers()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                4);

            registry.AddModifier(
                cardInstanceId,
                2);

            registry.AddModifier(
                cardInstanceId,
                -1);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.GetTotalModifier(
                    cardInstanceId),
                Is.EqualTo(5));
        }

        [Test]
        public void
            AddModifier_ForDifferentCards_KeepsIndependentTotals()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var firstCardInstanceId =
                new InstanceId(100);

            var secondCardInstanceId =
                new InstanceId(200);

            registry.AddModifier(
                firstCardInstanceId,
                4);

            registry.AddModifier(
                secondCardInstanceId,
                2);

            registry.AddModifier(
                secondCardInstanceId,
                1);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.GetTotalModifier(
                    firstCardInstanceId),
                Is.EqualTo(4));

            Assert.That(
                registry.GetTotalModifier(
                    secondCardInstanceId),
                Is.EqualTo(3));
        }

        [Test]
        public void
            AddModifier_WhenCombinedModifierOverflows_ThrowsWithoutReplacingPreviousTotal()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                int.MaxValue);

            Assert.Throws<OverflowException>(
                () => registry.AddModifier(
                    cardInstanceId,
                    1));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.GetTotalModifier(
                    cardInstanceId),
                Is.EqualTo(
                    int.MaxValue));
        }

        [Test]
        public void
            AddModifier_WhenModifiersCancel_KeepsRegisteredZeroTotal()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var cardInstanceId =
                new InstanceId(100);

            registry.AddModifier(
                cardInstanceId,
                3);

            registry.AddModifier(
                cardInstanceId,
                -3);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.HasModifier(
                    cardInstanceId),
                Is.True);

            Assert.That(
                registry.GetTotalModifier(
                    cardInstanceId),
                Is.Zero);
        }
    }
}