using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventIdTests
    {
        [TestCase(1L)]
        [TestCase(long.MaxValue)]
        public void Constructor_WithPositiveValue_CreatesValidId(
            long value)
        {
            var eventId =
                new CombatEventId(value);

            Assert.That(
                eventId.Value,
                Is.EqualTo(value));

            Assert.That(eventId.IsValid, Is.True);
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(long.MinValue)]
        public void Constructor_WithNonPositiveValue_Throws(
            long value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new CombatEventId(value));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var eventId =
                default(CombatEventId);

            Assert.That(eventId.IsValid, Is.False);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var first = new CombatEventId(10);
            var second = new CombatEventId(10);

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
            var first = new CombatEventId(10);
            var second = new CombatEventId(11);

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first == second, Is.False);
            Assert.That(first != second, Is.True);
        }

        [Test]
        public void ToString_ReturnsInvariantNumericValue()
        {
            var eventId =
                new CombatEventId(123);

            Assert.That(
                eventId.ToString(),
                Is.EqualTo("123"));
        }
    }
}