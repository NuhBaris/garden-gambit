using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSequenceNumberTests
    {
        [TestCase(1L)]
        [TestCase(long.MaxValue)]
        public void Constructor_WithPositiveValue_CreatesValidNumber(
            long value)
        {
            var sequenceNumber =
                new CombatSequenceNumber(value);

            Assert.That(
                sequenceNumber.Value,
                Is.EqualTo(value));

            Assert.That(sequenceNumber.IsValid, Is.True);
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(long.MinValue)]
        public void Constructor_WithNonPositiveValue_Throws(
            long value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSequenceNumber(value));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var sequenceNumber =
                default(CombatSequenceNumber);

            Assert.That(
                sequenceNumber.IsValid,
                Is.False);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var first =
                new CombatSequenceNumber(10);

            var second =
                new CombatSequenceNumber(10);

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
            var lower =
                new CombatSequenceNumber(10);

            var higher =
                new CombatSequenceNumber(11);

            Assert.That(lower, Is.Not.EqualTo(higher));
            Assert.That(lower == higher, Is.False);
            Assert.That(lower != higher, Is.True);
        }

        [Test]
        public void OrderingOperators_FollowNumericSequence()
        {
            var lower =
                new CombatSequenceNumber(10);

            var higher =
                new CombatSequenceNumber(11);

            Assert.That(lower < higher, Is.True);
            Assert.That(lower <= higher, Is.True);
            Assert.That(higher > lower, Is.True);
            Assert.That(higher >= lower, Is.True);

            Assert.That(
                lower.CompareTo(higher),
                Is.LessThan(0));
        }

        [Test]
        public void ToString_ReturnsInvariantNumericValue()
        {
            var sequenceNumber =
                new CombatSequenceNumber(123);

            Assert.That(
                sequenceNumber.ToString(),
                Is.EqualTo("123"));
        }
    }
}