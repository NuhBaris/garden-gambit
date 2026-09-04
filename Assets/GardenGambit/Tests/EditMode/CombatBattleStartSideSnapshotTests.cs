using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartSideSnapshotTests
    {
        [Test]
        public void Constructor_WithValidCards_SetsState()
        {
            var firstCard =
                CreateSnapshot(
                    "snapshot-card-1",
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var secondCard =
                CreateSnapshot(
                    "snapshot-card-2",
                    2,
                    CombatSide.Player,
                    BoardRow.Back,
                    2);

            var snapshot =
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    new[]
                    {
                        firstCard,
                        secondCard
                    });

            Assert.That(
                snapshot.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                snapshot.Count,
                Is.EqualTo(2));

            Assert.That(
                snapshot.Cards[0],
                Is.SameAs(firstCard));

            Assert.That(
                snapshot.Cards[1],
                Is.SameAs(secondCard));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatBattleStartSideSnapshot(
                        default(CombatSide),
                        Array.Empty<
                            CombatBattleStartCardSnapshot>()));
        }

        [Test]
        public void Constructor_WithNullCards_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartSideSnapshot(
                        CombatSide.Player,
                        null));
        }

        [Test]
        public void Constructor_WithNullCardSnapshot_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSideSnapshot(
                        CombatSide.Player,
                        new CombatBattleStartCardSnapshot[]
                        {
                            null
                        }));
        }

        [Test]
        public void Constructor_WithWrongSideCard_Throws()
        {
            var enemyCard =
                CreateSnapshot(
                    "enemy-card",
                    1,
                    CombatSide.Enemy,
                    BoardRow.Front,
                    1);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSideSnapshot(
                        CombatSide.Player,
                        new[]
                        {
                            enemyCard
                        }));
        }

        [Test]
        public void Constructor_WithDuplicateInstanceId_Throws()
        {
            var firstCard =
                CreateSnapshot(
                    "first-card",
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var secondCard =
                CreateSnapshot(
                    "second-card",
                    1,
                    CombatSide.Player,
                    BoardRow.Back,
                    2);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSideSnapshot(
                        CombatSide.Player,
                        new[]
                        {
                            firstCard,
                            secondCard
                        }));
        }

        [Test]
        public void Constructor_WithDuplicatePosition_Throws()
        {
            var firstCard =
                CreateSnapshot(
                    "first-card",
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var secondCard =
                CreateSnapshot(
                    "second-card",
                    2,
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSideSnapshot(
                        CombatSide.Player,
                        new[]
                        {
                            firstCard,
                            secondCard
                        }));
        }

        [Test]
        public void GetCard_WithExistingInstanceId_ReturnsCard()
        {
            var card =
                CreateSnapshot(
                    "snapshot-card",
                    1,
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player,
                    card);

            var result =
                snapshot.GetCard(
                    card.InstanceId);

            Assert.That(
                result,
                Is.SameAs(card));
        }

        [Test]
        public void GetCard_WithInvalidInstanceId_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<ArgumentException>(
                () => snapshot.GetCard(
                    default(InstanceId)));
        }

        [Test]
        public void GetCard_WithMissingInstanceId_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<KeyNotFoundException>(
                () => snapshot.GetCard(
                    new InstanceId(999)));
        }

        [Test]
        public void GetCardAt_WithExistingPosition_ReturnsCard()
        {
            var card =
                CreateSnapshot(
                    "snapshot-card",
                    1,
                    CombatSide.Player,
                    BoardRow.Back,
                    3);

            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player,
                    card);

            var result =
                snapshot.GetCardAt(
                    card.Position);

            Assert.That(
                result,
                Is.SameAs(card));
        }

        [Test]
        public void GetCardAt_WithInvalidPosition_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<ArgumentException>(
                () => snapshot.GetCardAt(
                    default(BoardPosition)));
        }

        [Test]
        public void GetCardAt_WithWrongSidePosition_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            Assert.Throws<ArgumentException>(
                () => snapshot.GetCardAt(
                    enemyPosition));
        }

        [Test]
        public void GetCardAt_WithMissingPosition_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            var missingPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            Assert.Throws<KeyNotFoundException>(
                () => snapshot.GetCardAt(
                    missingPosition));
        }

        [Test]
        public void CountInRow_CountsAllCardsInRequestedRow()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player,
                    CreateSnapshot(
                        "front-1",
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1),
                    CreateSnapshot(
                        "front-2",
                        2,
                        CombatSide.Player,
                        BoardRow.Front,
                        2),
                    CreateSnapshot(
                        "back-1",
                        3,
                        CombatSide.Player,
                        BoardRow.Back,
                        1));

            Assert.That(
                snapshot.CountInRow(
                    BoardRow.Front),
                Is.EqualTo(2));

            Assert.That(
                snapshot.CountInRow(
                    BoardRow.Back),
                Is.EqualTo(1));
        }

        [Test]
        public void CountLivingInRow_ExcludesDeadCards()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player,
                    CreateSnapshot(
                        "living-front",
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1,
                        5),
                    CreateSnapshot(
                        "dead-front",
                        2,
                        CombatSide.Player,
                        BoardRow.Front,
                        2,
                        0),
                    CreateSnapshot(
                        "living-back",
                        3,
                        CombatSide.Player,
                        BoardRow.Back,
                        1,
                        5));

            Assert.That(
                snapshot.CountLivingInRow(
                    BoardRow.Front),
                Is.EqualTo(1));

            Assert.That(
                snapshot.CountLivingInRow(
                    BoardRow.Back),
                Is.EqualTo(1));
        }

        [Test]
        public void CountInRow_WithInvalidRow_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => snapshot.CountInRow(
                    default(BoardRow)));
        }

        [Test]
        public void CountLivingInRow_WithInvalidRow_Throws()
        {
            var snapshot =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => snapshot.CountLivingInRow(
                    default(BoardRow)));
        }

        private static CombatBattleStartSideSnapshot
            CreateSideSnapshot(
                CombatSide side,
                params CombatBattleStartCardSnapshot[]
                    cards)
        {
            return new CombatBattleStartSideSnapshot(
                side,
                cards);
        }

        private static CombatBattleStartCardSnapshot
            CreateSnapshot(
                string definitionId,
                long instanceId,
                CombatSide side,
                BoardRow row,
                int column,
                int currentHp = 5)
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        definitionId),
                    new InstanceId(
                        instanceId),
                    new CardRank(2),
                    hpCapacity: 10,
                    currentHp: currentHp,
                    armor: 1,
                    attack: 3);

            var position =
                new BoardPosition(
                    side,
                    row,
                    new BoardColumn(column));

            return new CombatBattleStartCardSnapshot(
                card,
                position);
        }
    }
}