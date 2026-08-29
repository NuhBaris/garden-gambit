using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatStateTests
    {
        [Test]
        public void Constructor_WithValidSides_SetsProperties()
        {
            var player =
                CreateSideState(CombatSide.Player);

            var enemy =
                CreateSideState(CombatSide.Enemy);

            var state =
                new CombatState(player, enemy);

            Assert.That(state.Player, Is.SameAs(player));
            Assert.That(state.Enemy, Is.SameAs(enemy));
        }

        [Test]
        public void Constructor_WithNullPlayer_Throws()
        {
            var enemy =
                CreateSideState(CombatSide.Enemy);

            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatState(
                    null,
                    enemy));
        }

        [Test]
        public void Constructor_WithNullEnemy_Throws()
        {
            var player =
                CreateSideState(CombatSide.Player);

            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatState(
                    player,
                    null));
        }

        [Test]
        public void Constructor_WithEnemyStateAsPlayer_Throws()
        {
            var invalidPlayer =
                CreateSideState(CombatSide.Enemy);

            var enemy =
                CreateSideState(CombatSide.Enemy);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatState(
                    invalidPlayer,
                    enemy));
        }

        [Test]
        public void Constructor_WithPlayerStateAsEnemy_Throws()
        {
            var player =
                CreateSideState(CombatSide.Player);

            var invalidEnemy =
                CreateSideState(CombatSide.Player);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatState(
                    player,
                    invalidEnemy));
        }

        [Test]
        public void Constructor_WithDuplicateCrossSideInstanceId_Throws()
        {
            var player = CreateSideState(
                CombatSide.Player,
                100,
                1);

            var enemy = CreateSideState(
                CombatSide.Enemy,
                100,
                2);

            Assert.Throws<ArgumentException>(
                () => _ = new CombatState(
                    player,
                    enemy));
        }

        [Test]
        public void Constructor_WithSameSlotIdOnDifferentSides_AllowsState()
        {
            var player = CreateSideState(
                CombatSide.Player,
                100,
                1);

            var enemy = CreateSideState(
                CombatSide.Enemy,
                101,
                1);

            var state =
                new CombatState(player, enemy);

            Assert.That(
                state.Player.Board.Slots[0].SlotId,
                Is.EqualTo(new SlotId(1)));

            Assert.That(
                state.Enemy.Board.Slots[0].SlotId,
                Is.EqualTo(new SlotId(1)));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void GetSide_WithValidSide_ReturnsRequestedState(
            CombatSide side)
        {
            var player =
                CreateSideState(CombatSide.Player);

            var enemy =
                CreateSideState(CombatSide.Enemy);

            var state =
                new CombatState(player, enemy);

            var expected =
                side == CombatSide.Player
                    ? player
                    : enemy;

            Assert.That(
                state.GetSide(side),
                Is.SameAs(expected));
        }

        [TestCase(CombatSide.Unspecified)]
        [TestCase((CombatSide)999)]
        public void GetSide_WithInvalidSide_Throws(
            CombatSide side)
        {
            var state = CreateCombatState();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.GetSide(side));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void GetOpposingSide_WithValidSide_ReturnsOpponent(
            CombatSide side)
        {
            var player =
                CreateSideState(CombatSide.Player);

            var enemy =
                CreateSideState(CombatSide.Enemy);

            var state =
                new CombatState(player, enemy);

            var expected =
                side == CombatSide.Player
                    ? enemy
                    : player;

            Assert.That(
                state.GetOpposingSide(side),
                Is.SameAs(expected));
        }

        [TestCase(CombatSide.Unspecified)]
        [TestCase((CombatSide)999)]
        public void GetOpposingSide_WithInvalidSide_Throws(
            CombatSide side)
        {
            var state = CreateCombatState();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.GetOpposingSide(side));
        }

        private static CombatState CreateCombatState()
        {
            return new CombatState(
                CreateSideState(CombatSide.Player),
                CreateSideState(CombatSide.Enemy));
        }

        private static CombatSideState CreateSideState(
            CombatSide side,
            long? cardInstanceId = null,
            long slotId = 1)
        {
            if (!cardInstanceId.HasValue)
            {
                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        new CombatSlotState[0]),
                    new CombatCardRegistry(
                        new CombatCardState[0]),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));
            }

            var card = new CombatCardState(
                new DefinitionId(
                    $"card.{cardInstanceId.Value}"),
                new InstanceId(
                    cardInstanceId.Value),
                new CardRank(2),
                7,
                7,
                2,
                3);

            var slot = new CombatSlotState(
                new SlotId(slotId),
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1)),
                card.InstanceId);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[] { slot }),
                new CombatCardRegistry(
                    new[] { card }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}