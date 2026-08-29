using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class BoardColumnTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void Constructor_WithValidValue_PreservesValue(int value)
        {
            var column = new BoardColumn(value);

            Assert.That(column.Value, Is.EqualTo(value));
            Assert.That(column.IsValid, Is.True);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(6)]
        [TestCase(int.MaxValue)]
        public void Constructor_WithOutOfRangeValue_Throws(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new BoardColumn(value);
            });
        }

        [Test]
        public void EqualValues_AreEqual()
        {
            var left = new BoardColumn(3);
            var right = new BoardColumn(3);

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left == right, Is.True);
            Assert.That(left != right, Is.False);
            Assert.That(
                left.GetHashCode(),
                Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void Columns_AreOrderedFromOneToFive()
        {
            var first = new BoardColumn(1);
            var fifth = new BoardColumn(5);

            Assert.That(first.CompareTo(fifth), Is.LessThan(0));
            Assert.That(first < fifth, Is.True);
            Assert.That(fifth > first, Is.True);
            Assert.That(first <= fifth, Is.True);
            Assert.That(fifth >= first, Is.True);
        }

        [Test]
        public void DefaultInstance_IsInvalid()
        {
            var column = default(BoardColumn);

            Assert.That(column.Value, Is.Zero);
            Assert.That(column.IsValid, Is.False);
        }
    }
}