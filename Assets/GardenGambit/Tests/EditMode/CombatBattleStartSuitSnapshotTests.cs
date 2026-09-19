using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatBattleStartSuitSnapshotTests
    {
        [TestCase(CombatCardSuit.Fruit)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Nut)]
        [TestCase(CombatCardSuit.Drink)]
        public void CardSnapshot_CopiesSpecifiedSuitAndPredicates(
            CombatCardSuit suit)
        {
            var snapshot = CreateCardSnapshot(
                CombatSide.Player,
                BoardRow.Front,
                1,
                1,
                suit);

            Assert.That(snapshot.Suit, Is.EqualTo(suit));
            Assert.That(snapshot.HasSpecifiedSuit, Is.True);
            Assert.That(
                snapshot.IsFruit,
                Is.EqualTo(
                    suit == CombatCardSuit.Fruit));
            Assert.That(
                snapshot.IsVegetable,
                Is.EqualTo(
                    suit == CombatCardSuit.Vegetable));
            Assert.That(
                snapshot.IsNut,
                Is.EqualTo(
                    suit == CombatCardSuit.Nut));
            Assert.That(
                snapshot.IsDrink,
                Is.EqualTo(
                    suit == CombatCardSuit.Drink));
        }

        [Test]
        public void CardSnapshot_FromLegacyCard_KeepsUnspecifiedSuit()
        {
            var card = new CombatCardState(
                new DefinitionId("test.legacy_snapshot"),
                new InstanceId(1),
                new CardRank(4),
                CombatCardSeason.Spring,
                10,
                8,
                2,
                3);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    Position(
                        CombatSide.Player,
                        BoardRow.Front,
                        1));

            Assert.That(
                snapshot.Suit,
                Is.EqualTo(
                    CombatCardSuit.Unspecified));
            Assert.That(snapshot.HasSpecifiedSuit, Is.False);
            Assert.That(snapshot.IsFruit, Is.False);
            Assert.That(snapshot.IsVegetable, Is.False);
            Assert.That(snapshot.IsNut, Is.False);
            Assert.That(snapshot.IsDrink, Is.False);
        }

        [Test]
        public void SideSnapshot_CountsDistinctSpecifiedSuitsPerRow()
        {
            var snapshot = new CombatBattleStartSideSnapshot(
                CombatSide.Player,
                new[]
                {
                    CreateCardSnapshot(
                        CombatSide.Player,
                        BoardRow.Front,
                        1,
                        1,
                        CombatCardSuit.Fruit),
                    CreateCardSnapshot(
                        CombatSide.Player,
                        BoardRow.Front,
                        2,
                        2,
                        CombatCardSuit.Vegetable),
                    CreateCardSnapshot(
                        CombatSide.Player,
                        BoardRow.Front,
                        3,
                        3,
                        CombatCardSuit.Nut),
                    CreateCardSnapshot(
                        CombatSide.Player,
                        BoardRow.Front,
                        4,
                        4,
                        CombatCardSuit.Drink),
                    CreateCardSnapshot(
                        CombatSide.Player,
                        BoardRow.Front,
                        5,
                        5,
                        CombatCardSuit.Fruit),
                    CreateCardSnapshot(
                        CombatSide.Player,
                        BoardRow.Back,
                        1,
                        11,
                        CombatCardSuit.Nut)
                });

            Assert.That(
                snapshot.CountDistinctSpecifiedSuitsInRow(
                    BoardRow.Front),
                Is.EqualTo(4));
            Assert.That(
                snapshot.CountDistinctSpecifiedSuitsInRow(
                    BoardRow.Back),
                Is.EqualTo(1));
        }

        [Test]
        public void SideSnapshot_IgnoresUnspecifiedAndDuplicateSuits()
        {
            var snapshot = new CombatBattleStartSideSnapshot(
                CombatSide.Enemy,
                new[]
                {
                    CreateCardSnapshot(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        1,
                        1,
                        CombatCardSuit.Unspecified),
                    CreateCardSnapshot(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        2,
                        2,
                        CombatCardSuit.Drink),
                    CreateCardSnapshot(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        3,
                        3,
                        CombatCardSuit.Drink)
                });

            Assert.That(
                snapshot.CountDistinctSpecifiedSuitsInRow(
                    BoardRow.Back),
                Is.EqualTo(1));
            Assert.That(
                snapshot.CountDistinctSpecifiedSuitsInRow(
                    BoardRow.Front),
                Is.Zero);
        }

        [TestCase(0)]
        [TestCase(99)]
        public void SideSnapshot_WithInvalidRow_Throws(
            int rawRow)
        {
            var snapshot =
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                snapshot.CountDistinctSpecifiedSuitsInRow(
                    (BoardRow)rawRow));
        }

        private static CombatBattleStartCardSnapshot
            CreateCardSnapshot(
                CombatSide side,
                BoardRow row,
                int column,
                long instanceId,
                CombatCardSuit suit)
        {
            var card = new CombatCardState(
                new DefinitionId(
                    $"test.suit_snapshot.{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(6),
                suit,
                CombatCardSeason.Summer,
                10,
                7,
                2,
                4);

            return new CombatBattleStartCardSnapshot(
                card,
                Position(side, row, column));
        }

        private static BoardPosition Position(
            CombatSide side,
            BoardRow row,
            int column)
        {
            return new BoardPosition(
                side,
                row,
                new BoardColumn(column));
        }
    }
}
