using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartCardSnapshotTests
    {
        [Test]
        public void Constructor_WithNullCard_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartCardSnapshot(
                        null,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1)));
        }

        [Test]
        public void
            Constructor_WithInvalidPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartCardSnapshot(
                        CreateCard(
                            currentHp: 5),
                        default(BoardPosition)));
        }

        [Test]
        public void
            Constructor_CopiesCardIdentityAndPosition()
        {
            var card =
                CreateCard(
                    currentHp: 5);

            var position =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    3);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    position);

            Assert.That(
                snapshot.DefinitionId,
                Is.EqualTo(
                    card.DefinitionId));

            Assert.That(
                snapshot.InstanceId,
                Is.EqualTo(
                    card.InstanceId));

            Assert.That(
                snapshot.Position,
                Is.EqualTo(
                    position));
        }

        [Test]
        public void Constructor_CopiesCombatStats()
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "snapshot-card"),
                    new InstanceId(101),
                    new CardRank(8),
                    hpCapacity: 12,
                    currentHp: 7,
                    armor: 4,
                    attack: 6);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        2));

            Assert.That(
                snapshot.Rank,
                Is.EqualTo(
                    new CardRank(8)));

            Assert.That(
                snapshot.HpCapacity,
                Is.EqualTo(12));

            Assert.That(
                snapshot.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                snapshot.Armor,
                Is.EqualTo(4));

            Assert.That(
                snapshot.Attack,
                Is.EqualTo(6));
        }

        [Test]
        public void
            Constructor_WithPlayerPosition_SetsDerivedLocation()
        {
            var snapshot =
                new CombatBattleStartCardSnapshot(
                    CreateCard(
                        currentHp: 5),
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        4));

            Assert.That(
                snapshot.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                snapshot.Row,
                Is.EqualTo(
                    BoardRow.Back));

            Assert.That(
                snapshot.Column,
                Is.EqualTo(
                    new BoardColumn(4)));
        }

        [Test]
        public void
            Constructor_WithEnemyPosition_SetsDerivedLocation()
        {
            var snapshot =
                new CombatBattleStartCardSnapshot(
                    CreateCard(
                        currentHp: 5),
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        5));

            Assert.That(
                snapshot.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                snapshot.Row,
                Is.EqualTo(
                    BoardRow.Front));

            Assert.That(
                snapshot.Column,
                Is.EqualTo(
                    new BoardColumn(5)));
        }

        [Test]
        public void
            Constructor_WithLivingCard_SetsAliveFlags()
        {
            var snapshot =
                new CombatBattleStartCardSnapshot(
                    CreateCard(
                        currentHp: 1),
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        1));

            Assert.That(
                snapshot.WasAlive,
                Is.True);

            Assert.That(
                snapshot.WasAtDeathThreshold,
                Is.False);
        }

        [Test]
        public void
            Constructor_WithZeroHpCard_SetsDeathThresholdFlags()
        {
            var snapshot =
                new CombatBattleStartCardSnapshot(
                    CreateCard(
                        currentHp: 0),
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        1));

            Assert.That(
                snapshot.WasAlive,
                Is.False);

            Assert.That(
                snapshot.WasAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void
            Snapshot_AfterCardHpChanges_KeepsOriginalValues()
        {
            var card =
                CreateCard(
                    currentHp: 5);

            var snapshot =
                new CombatBattleStartCardSnapshot(
                    card,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        1));

            card.SetCurrentHpToZero();

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(0));

            Assert.That(
                snapshot.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                snapshot.WasAlive,
                Is.True);

            Assert.That(
                snapshot.WasAtDeathThreshold,
                Is.False);
        }

        private static CombatCardState CreateCard(
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    "snapshot-card"),
                new InstanceId(101),
                new CardRank(6),
                hpCapacity: 10,
                currentHp: currentHp,
                armor: 3,
                attack: 4);
        }

        private static BoardPosition CreatePosition(
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