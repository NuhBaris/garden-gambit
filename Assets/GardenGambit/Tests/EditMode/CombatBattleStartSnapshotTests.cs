using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartSnapshotTests
    {
        [Test]
        public void Constructor_WithValidSides_SetsState()
        {
            var player =
                CreateSideSnapshot(
                    CombatSide.Player,
                    CreateCardSnapshot(
                        "player-card",
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1));

            var enemy =
                CreateSideSnapshot(
                    CombatSide.Enemy,
                    CreateCardSnapshot(
                        "enemy-card-1",
                        2,
                        CombatSide.Enemy,
                        BoardRow.Front,
                        1),
                    CreateCardSnapshot(
                        "enemy-card-2",
                        3,
                        CombatSide.Enemy,
                        BoardRow.Back,
                        2));

            var snapshot =
                new CombatBattleStartSnapshot(
                    player,
                    enemy);

            Assert.That(
                snapshot.Player,
                Is.SameAs(player));

            Assert.That(
                snapshot.Enemy,
                Is.SameAs(enemy));

            Assert.That(
                snapshot.TotalCardCount,
                Is.EqualTo(3));
        }

        [Test]
        public void Constructor_WithNullPlayer_Throws()
        {
            var enemy =
                CreateSideSnapshot(
                    CombatSide.Enemy);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartSnapshot(
                        null,
                        enemy));
        }

        [Test]
        public void Constructor_WithNullEnemy_Throws()
        {
            var player =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatBattleStartSnapshot(
                        player,
                        null));
        }

        [Test]
        public void Constructor_WithWrongPlayerSide_Throws()
        {
            var wrongPlayer =
                CreateSideSnapshot(
                    CombatSide.Enemy);

            var enemy =
                CreateSideSnapshot(
                    CombatSide.Enemy);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSnapshot(
                        wrongPlayer,
                        enemy));
        }

        [Test]
        public void Constructor_WithWrongEnemySide_Throws()
        {
            var player =
                CreateSideSnapshot(
                    CombatSide.Player);

            var wrongEnemy =
                CreateSideSnapshot(
                    CombatSide.Player);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSnapshot(
                        player,
                        wrongEnemy));
        }

        [Test]
        public void Constructor_WithDuplicateCrossSideInstanceId_Throws()
        {
            var player =
                CreateSideSnapshot(
                    CombatSide.Player,
                    CreateCardSnapshot(
                        "player-card",
                        1,
                        CombatSide.Player,
                        BoardRow.Front,
                        1));

            var enemy =
                CreateSideSnapshot(
                    CombatSide.Enemy,
                    CreateCardSnapshot(
                        "enemy-card",
                        1,
                        CombatSide.Enemy,
                        BoardRow.Front,
                        1));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatBattleStartSnapshot(
                        player,
                        enemy));
        }

        [Test]
        public void GetSide_WithPlayer_ReturnsPlayerSnapshot()
        {
            var snapshot =
                CreateSnapshot();

            var result =
                snapshot.GetSide(
                    CombatSide.Player);

            Assert.That(
                result,
                Is.SameAs(
                    snapshot.Player));
        }

        [Test]
        public void GetSide_WithEnemy_ReturnsEnemySnapshot()
        {
            var snapshot =
                CreateSnapshot();

            var result =
                snapshot.GetSide(
                    CombatSide.Enemy);

            Assert.That(
                result,
                Is.SameAs(
                    snapshot.Enemy));
        }

        [Test]
        public void GetSide_WithInvalidSide_Throws()
        {
            var snapshot =
                CreateSnapshot();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => snapshot.GetSide(
                    default(CombatSide)));
        }

        [Test]
        public void GetOpposingSide_WithPlayer_ReturnsEnemy()
        {
            var snapshot =
                CreateSnapshot();

            var result =
                snapshot.GetOpposingSide(
                    CombatSide.Player);

            Assert.That(
                result,
                Is.SameAs(
                    snapshot.Enemy));
        }

        [Test]
        public void GetOpposingSide_WithEnemy_ReturnsPlayer()
        {
            var snapshot =
                CreateSnapshot();

            var result =
                snapshot.GetOpposingSide(
                    CombatSide.Enemy);

            Assert.That(
                result,
                Is.SameAs(
                    snapshot.Player));
        }

        [Test]
        public void GetOpposingSide_WithInvalidSide_Throws()
        {
            var snapshot =
                CreateSnapshot();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => snapshot.GetOpposingSide(
                    default(CombatSide)));
        }

        private static CombatBattleStartSnapshot
            CreateSnapshot()
        {
            return new CombatBattleStartSnapshot(
                CreateSideSnapshot(
                    CombatSide.Player),
                CreateSideSnapshot(
                    CombatSide.Enemy));
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
            CreateCardSnapshot(
                string definitionId,
                long instanceId,
                CombatSide side,
                BoardRow row,
                int column)
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        definitionId),
                    new InstanceId(
                        instanceId),
                    new CardRank(2),
                    hpCapacity: 10,
                    currentHp: 5,
                    armor: 1,
                    attack: 3);

            return new CombatBattleStartCardSnapshot(
                card,
                new BoardPosition(
                    side,
                    row,
                    new BoardColumn(column)));
        }
    }
}