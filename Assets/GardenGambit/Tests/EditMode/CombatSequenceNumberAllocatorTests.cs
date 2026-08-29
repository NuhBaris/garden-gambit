using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSequenceNumberAllocatorTests
    {
        [Test]
        public void NewAllocator_StartsBeforeFirstSequence()
        {
            var allocator =
                new CombatSequenceNumberAllocator();

            Assert.That(
                allocator.LastIssuedValue,
                Is.Zero);

            Assert.That(
                allocator.CanAllocate,
                Is.True);
        }

        [Test]
        public void Allocate_FirstCall_ReturnsOne()
        {
            var allocator =
                new CombatSequenceNumberAllocator();

            var sequenceNumber =
                allocator.Allocate();

            Assert.That(
                sequenceNumber,
                Is.EqualTo(
                    new CombatSequenceNumber(1)));

            Assert.That(
                allocator.LastIssuedValue,
                Is.EqualTo(1));
        }

        [Test]
        public void Allocate_MultipleCalls_ReturnsSequentialNumbers()
        {
            var allocator =
                new CombatSequenceNumberAllocator();

            var first = allocator.Allocate();
            var second = allocator.Allocate();
            var third = allocator.Allocate();

            Assert.That(
                first,
                Is.EqualTo(
                    new CombatSequenceNumber(1)));

            Assert.That(
                second,
                Is.EqualTo(
                    new CombatSequenceNumber(2)));

            Assert.That(
                third,
                Is.EqualTo(
                    new CombatSequenceNumber(3)));
        }

        [Test]
        public void Constructor_WithLastIssuedValue_ResumesAfterValue()
        {
            var allocator =
                new CombatSequenceNumberAllocator(50);

            var sequenceNumber =
                allocator.Allocate();

            Assert.That(
                sequenceNumber,
                Is.EqualTo(
                    new CombatSequenceNumber(51)));
        }

        [Test]
        public void Constructor_WithNegativeLastIssuedValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatSequenceNumberAllocator(-1));
        }

        [Test]
        public void Allocate_AtMaximumValue_ExhaustsWithoutOverflow()
        {
            var allocator =
                new CombatSequenceNumberAllocator(
                    long.MaxValue - 1);

            var finalSequence =
                allocator.Allocate();

            Assert.That(
                finalSequence,
                Is.EqualTo(
                    new CombatSequenceNumber(
                        long.MaxValue)));

            Assert.That(
                allocator.CanAllocate,
                Is.False);

            Assert.Throws<InvalidOperationException>(
                () => allocator.Allocate());

            Assert.That(
                allocator.LastIssuedValue,
                Is.EqualTo(long.MaxValue));
        }
    }
}