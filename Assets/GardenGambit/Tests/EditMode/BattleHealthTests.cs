using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class BattleHealthTests
    {
        [TestCase(BattleHealth.NormalBaselineValue)]
        [TestCase(0)]
        [TestCase(-5)]
        public void Constructor_AcceptsAnyIntegerValue(
            int value)
        {
            var battleHealth =
                new BattleHealth(value);

            Assert.That(
                battleHealth.Value,
                Is.EqualTo(value));
        }

        [Test]
        public void ApplyDamage_ReducesValueWithoutChangingOriginal()
        {
            var original = new BattleHealth(20);

            var result =
                original.ApplyDamage(5);

            Assert.That(original.Value, Is.EqualTo(20));
            Assert.That(result.Value, Is.EqualTo(15));
        }

        [Test]
        public void ApplyDamage_CanProduceNegativeValue()
        {
            var original = new BattleHealth(5);

            var result =
                original.ApplyDamage(8);

            Assert.That(result.Value, Is.EqualTo(-3));
        }

        [Test]
        public void ApplyDamage_WithZero_DoesNotChangeValue()
        {
            var original = new BattleHealth(20);

            var result =
                original.ApplyDamage(0);

            Assert.That(result, Is.EqualTo(original));
        }

        [Test]
        public void ApplyDamage_WithNegativeAmount_Throws()
        {
            var original = new BattleHealth(20);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => original.ApplyDamage(-1));

            Assert.That(original.Value, Is.EqualTo(20));
        }

        [Test]
        public void ApplyDamage_WhenValueWouldUnderflow_Throws()
        {
            var original =
                new BattleHealth(int.MinValue);

            Assert.Throws<OverflowException>(
                () => original.ApplyDamage(1));

            Assert.That(
                original.Value,
                Is.EqualTo(int.MinValue));
        }

        [Test]
        public void ApplyGain_IncreasesValueWithoutChangingOriginal()
        {
            var original = new BattleHealth(20);

            var result =
                original.ApplyGain(5);

            Assert.That(original.Value, Is.EqualTo(20));
            Assert.That(result.Value, Is.EqualTo(25));
        }

        [Test]
        public void ApplyGain_CanIncreaseNegativeValue()
        {
            var original = new BattleHealth(-5);

            var result =
                original.ApplyGain(3);

            Assert.That(result.Value, Is.EqualTo(-2));
        }

        [Test]
        public void ApplyGain_WithZero_DoesNotChangeValue()
        {
            var original = new BattleHealth(20);

            var result =
                original.ApplyGain(0);

            Assert.That(result, Is.EqualTo(original));
        }

        [Test]
        public void ApplyGain_WithNegativeAmount_Throws()
        {
            var original = new BattleHealth(20);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => original.ApplyGain(-1));

            Assert.That(original.Value, Is.EqualTo(20));
        }

        [Test]
        public void ApplyGain_WhenValueWouldOverflow_Throws()
        {
            var original =
                new BattleHealth(int.MaxValue);

            Assert.Throws<OverflowException>(
                () => original.ApplyGain(1));

            Assert.That(
                original.Value,
                Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var first = new BattleHealth(20);
            var second = new BattleHealth(20);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);

            Assert.That(
                first.GetHashCode(),
                Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void OrderingOperators_FollowNumericValue()
        {
            var lower = new BattleHealth(-5);
            var higher = new BattleHealth(20);

            Assert.That(lower < higher, Is.True);
            Assert.That(lower <= higher, Is.True);
            Assert.That(higher > lower, Is.True);
            Assert.That(higher >= lower, Is.True);
        }

        [Test]
        public void ToString_ReturnsInvariantNumericValue()
        {
            var battleHealth =
                new BattleHealth(-5);

            Assert.That(
                battleHealth.ToString(),
                Is.EqualTo("-5"));
        }
    }
}