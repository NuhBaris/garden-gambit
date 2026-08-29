using System;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        ProtectiveSealResultDamageResolverTests
    {
        [Test]
        public void Resolve_WithNoActiveSeals_ReturnsIncomingDamage()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    20,
                    0);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(20));
        }

        [Test]
        public void Resolve_WithOneSeal_ReducesDamageByFivePercent()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    20,
                    1);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(19));
        }

        [Test]
        public void Resolve_WithFractionalResult_RoundsUp()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    21,
                    1);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(20));
        }

        [Test]
        public void Resolve_WithMultipleSeals_AppliesSequentialRounding()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    100,
                    2);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(91));
        }

        [Test]
        public void Resolve_WithOneDamage_DoesNotRoundToZero()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    1,
                    1);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithZeroDamage_ReturnsZero()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    0,
                    5);

            Assert.That(
                resolvedDamage,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithMaximumDamage_UsesLongWithoutOverflow()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            var resolvedDamage =
                resolver.Resolve(
                    int.MaxValue,
                    1);

            Assert.That(
                resolvedDamage,
                Is.EqualTo(2040109465));
        }

        [Test]
        public void Resolve_WithNegativeIncomingDamage_Throws()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => resolver.Resolve(
                    -1,
                    1));
        }

        [Test]
        public void Resolve_WithNegativeSealCount_Throws()
        {
            var resolver =
                new ProtectiveSealResultDamageResolver();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => resolver.Resolve(
                    20,
                    -1));
        }
    }
}