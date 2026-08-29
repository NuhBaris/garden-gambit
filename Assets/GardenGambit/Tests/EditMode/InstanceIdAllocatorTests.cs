using System;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class InstanceIdAllocatorTests
    {
        [Test]
        public void NewAllocator_AllocatesFromOne()
        {
            var allocator = new InstanceIdAllocator();

            var allocatedId = allocator.Allocate();

            Assert.That(allocatedId.Value, Is.EqualTo(1L));
            Assert.That(allocator.LastIssuedValue, Is.EqualTo(1L));
            Assert.That(allocator.IsExhausted, Is.False);
        }

        [Test]
        public void ConsecutiveAllocations_AreStrictlyIncreasing()
        {
            var allocator = new InstanceIdAllocator();

            var first = allocator.Allocate();
            var second = allocator.Allocate();
            var third = allocator.Allocate();

            Assert.That(first.Value, Is.EqualTo(1L));
            Assert.That(second.Value, Is.EqualTo(2L));
            Assert.That(third.Value, Is.EqualTo(3L));
            Assert.That(first < second, Is.True);
            Assert.That(second < third, Is.True);
        }

        [Test]
        public void Constructor_WithLastIssuedValue_ResumesFromNextValue()
        {
            var allocator = new InstanceIdAllocator(50);

            var allocatedId = allocator.Allocate();

            Assert.That(allocatedId.Value, Is.EqualTo(51L));
            Assert.That(allocator.LastIssuedValue, Is.EqualTo(51L));
        }

        [TestCase(-1L)]
        [TestCase(long.MinValue)]
        public void Constructor_WithNegativeValue_Throws(
            long lastIssuedValue)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new InstanceIdAllocator(lastIssuedValue);
            });
        }

        [Test]
        public void ExhaustedAllocator_ThrowsWithoutChangingState()
        {
            var allocator = new InstanceIdAllocator(long.MaxValue);

            Assert.That(allocator.IsExhausted, Is.True);

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = allocator.Allocate();
            });

            Assert.That(
                allocator.LastIssuedValue,
                Is.EqualTo(long.MaxValue));
        }
    }
}