using System;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class SlotIdTests
    {
        [Test]
        public void Constructor_WithPositiveValue_PreservesValue()
        {
            var slotId = new SlotId(7);

            Assert.That(slotId.Value, Is.EqualTo(7L));
            Assert.That(slotId.IsValid, Is.True);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var left = new SlotId(7);
            var right = new SlotId(7);

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left == right, Is.True);
            Assert.That(left != right, Is.False);
            Assert.That(
                left.GetHashCode(),
                Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void DifferentValues_AreNotEqual()
        {
            var first = new SlotId(7);
            var second = new SlotId(8);

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first != second, Is.True);
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(long.MinValue)]
        public void Constructor_WithNonPositiveValue_Throws(long value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new SlotId(value);
            });
        }

        [Test]
        public void DefaultInstance_IsInvalid()
        {
            var slotId = default(SlotId);

            Assert.That(slotId.Value, Is.Zero);
            Assert.That(slotId.IsValid, Is.False);
        }
    }
}