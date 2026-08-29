using System;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class InstanceIdTests
    {
        [Test]
        public void Constructor_WithPositiveValue_PreservesValue()
        {
            var instanceId = new InstanceId(42);

            Assert.That(instanceId.Value, Is.EqualTo(42L));
            Assert.That(instanceId.IsValid, Is.True);
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var left = new InstanceId(42);
            var right = new InstanceId(42);

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
            var first = new InstanceId(41);
            var second = new InstanceId(42);

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first != second, Is.True);
        }

        [Test]
        public void LowerValue_SortsBeforeHigherValue()
        {
            var lower = new InstanceId(41);
            var higher = new InstanceId(42);

            Assert.That(lower.CompareTo(higher), Is.LessThan(0));
            Assert.That(lower < higher, Is.True);
            Assert.That(higher > lower, Is.True);
            Assert.That(lower <= higher, Is.True);
            Assert.That(higher >= lower, Is.True);
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(long.MinValue)]
        public void Constructor_WithNonPositiveValue_Throws(long value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new InstanceId(value);
            });
        }

        [Test]
        public void DefaultInstance_IsInvalid()
        {
            var instanceId = default(InstanceId);

            Assert.That(instanceId.Value, Is.Zero);
            Assert.That(instanceId.IsValid, Is.False);
        }
    }
}