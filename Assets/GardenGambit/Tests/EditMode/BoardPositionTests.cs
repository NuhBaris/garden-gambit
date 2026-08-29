using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class BoardPositionTests
    {
        [Test]
        public void AllTwentyBoardPositions_AreValid()
        {
            var sides = new[]
            {
                CombatSide.Player,
                CombatSide.Enemy
            };

            var rows = new[]
            {
                BoardRow.Front,
                BoardRow.Back
            };

            var positionCount = 0;

            foreach (var side in sides)
            {
                foreach (var row in rows)
                {
                    for (
                        var columnValue = BoardColumn.MinimumValue;
                        columnValue <= BoardColumn.MaximumValue;
                        columnValue++)
                    {
                        var column = new BoardColumn(columnValue);
                        var position = new BoardPosition(
                            side,
                            row,
                            column);

                        Assert.That(position.IsValid, Is.True);
                        Assert.That(position.Side, Is.EqualTo(side));
                        Assert.That(position.Row, Is.EqualTo(row));
                        Assert.That(
                            position.Column,
                            Is.EqualTo(column));

                        positionCount++;
                    }
                }
            }

            Assert.That(positionCount, Is.EqualTo(20));
        }

        [TestCase(CombatSide.Unspecified)]
        [TestCase((CombatSide)999)]
        public void Constructor_WithInvalidSide_Throws(
            CombatSide side)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));
            });
        }

        [TestCase(BoardRow.Unspecified)]
        [TestCase((BoardRow)999)]
        public void Constructor_WithInvalidRow_Throws(
            BoardRow row)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new BoardPosition(
                    CombatSide.Player,
                    row,
                    new BoardColumn(1));
            });
        }

        [Test]
        public void Constructor_WithInvalidColumn_Throws()
        {
            var invalidColumn = default(BoardColumn);

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    invalidColumn);
            });
        }

        [Test]
        public void EqualPositions_AreEqual()
        {
            var left = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(3));

            var right = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(3));

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left == right, Is.True);
            Assert.That(left != right, Is.False);
            Assert.That(
                left.GetHashCode(),
                Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void ChangingAnyComponent_ChangesPosition()
        {
            var origin = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(3));

            var differentSide = new BoardPosition(
                CombatSide.Enemy,
                BoardRow.Front,
                new BoardColumn(3));

            var differentRow = new BoardPosition(
                CombatSide.Player,
                BoardRow.Back,
                new BoardColumn(3));

            var differentColumn = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(4));

            Assert.That(origin, Is.Not.EqualTo(differentSide));
            Assert.That(origin, Is.Not.EqualTo(differentRow));
            Assert.That(origin, Is.Not.EqualTo(differentColumn));
        }

        [Test]
        public void DefaultInstance_IsInvalid()
        {
            var position = default(BoardPosition);

            Assert.That(position.IsValid, Is.False);
        }
    }
}