using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class AttackMultiplierTests
    {
        [TestCase(AttackMultiplier.BaseValue)]
        [TestCase(10)]
        public void Constructor_WithValidValue_CreatesMultiplier(
            int value)
        {
            var multiplier =
                new AttackMultiplier(value);

            Assert.That(
                multiplier.Value,
                Is.EqualTo(value));

            Assert.That(multiplier.IsValid, Is.True);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void Constructor_WithValueBelowMinimum_Throws(
            int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new AttackMultiplier(value));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var multiplier =
                default(AttackMultiplier);

            Assert.That(multiplier.IsValid, Is.False);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var first = new AttackMultiplier(2);
            var second = new AttackMultiplier(2);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);

            Assert.That(
                first.GetHashCode(),
                Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void DifferentValues_AreNotEqual()
        {
            var lower = new AttackMultiplier(2);
            var higher = new AttackMultiplier(3);

            Assert.That(lower, Is.Not.EqualTo(higher));
            Assert.That(lower == higher, Is.False);
            Assert.That(lower != higher, Is.True);
        }

        [Test]
        public void OrderingOperators_FollowNumericValue()
        {
            var lower = new AttackMultiplier(2);
            var higher = new AttackMultiplier(3);

            Assert.That(lower < higher, Is.True);
            Assert.That(lower <= higher, Is.True);
            Assert.That(higher > lower, Is.True);
            Assert.That(higher >= lower, Is.True);
        }

        [Test]
        public void ToString_ReturnsInvariantNumericValue()
        {
            var multiplier =
                new AttackMultiplier(10);

            Assert.That(
                multiplier.ToString(),
                Is.EqualTo("10"));
        }
    }
}