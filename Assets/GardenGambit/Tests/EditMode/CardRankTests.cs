using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CardRankTests
    {
        [TestCase(CardRank.MinimumValue)]
        [TestCase(CardRank.MaximumValue)]
        public void Constructor_WithBoundaryValue_CreatesValidRank(
            int value)
        {
            var rank = new CardRank(value);

            Assert.That(rank.Value, Is.EqualTo(value));
            Assert.That(rank.IsValid, Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(15)]
        [TestCase(int.MaxValue)]
        public void Constructor_WithOutOfRangeValue_Throws(
            int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new CardRank(value));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var rank = default(CardRank);

            Assert.That(rank.IsValid, Is.False);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var first = new CardRank(10);
            var second = new CardRank(10);

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
            var lower = new CardRank(10);
            var higher = new CardRank(11);

            Assert.That(lower, Is.Not.EqualTo(higher));
            Assert.That(lower == higher, Is.False);
            Assert.That(lower != higher, Is.True);
        }

        [Test]
        public void OrderingOperators_FollowNumericRank()
        {
            var lower = new CardRank(10);
            var higher = new CardRank(11);

            Assert.That(lower < higher, Is.True);
            Assert.That(lower <= higher, Is.True);
            Assert.That(higher > lower, Is.True);
            Assert.That(higher >= lower, Is.True);
        }

        [Test]
        public void ToString_ReturnsNumericRank()
        {
            var rank = new CardRank(14);

            Assert.That(rank.ToString(), Is.EqualTo("14"));
        }
    }
}