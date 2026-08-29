using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatOutcomeResolverTests
    {
        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var resolver =
                new CombatOutcomeResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(null));
        }

        [Test]
        public void Resolve_WhenPlayerHealthIsHigher_ReturnsPlayerVictory()
        {
            var state =
                CreateState(
                    13,
                    8);

            var resolver =
                new CombatOutcomeResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.IsValid,
                Is.True);

            Assert.That(
                result.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(13)));

            Assert.That(
                result.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(8)));

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                result.BattleHealthDifference,
                Is.EqualTo(5L));

            Assert.That(
                result.WinningMargin,
                Is.EqualTo(5L));
        }

        [Test]
        public void Resolve_WhenEnemyHealthIsHigher_ReturnsEnemyVictory()
        {
            var state =
                CreateState(
                    4,
                    11);

            var resolver =
                new CombatOutcomeResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    CombatOutcome.EnemyVictory));

            Assert.That(
                result.BattleHealthDifference,
                Is.EqualTo(-7L));

            Assert.That(
                result.WinningMargin,
                Is.EqualTo(7L));

            Assert.That(
                result.IsEnemyVictory,
                Is.True);
        }

        [Test]
        public void Resolve_WhenHealthValuesAreEqual_ReturnsDraw()
        {
            var state =
                CreateState(
                    10,
                    10);

            var resolver =
                new CombatOutcomeResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                result.BattleHealthDifference,
                Is.Zero);

            Assert.That(
                result.WinningMargin,
                Is.Zero);

            Assert.That(
                result.IsDraw,
                Is.True);
        }

        [Test]
        public void Resolve_WithNegativeHealthValues_UsesNumericComparison()
        {
            var state =
                CreateState(
                    -3,
                    -7);

            var resolver =
                new CombatOutcomeResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                result.BattleHealthDifference,
                Is.EqualTo(4L));

            Assert.That(
                result.WinningMargin,
                Is.EqualTo(4L));
        }

        [Test]
        public void Resolve_WithMaximumDifference_UsesLongWithoutOverflow()
        {
            var state =
                CreateState(
                    int.MaxValue,
                    int.MinValue);

            var resolver =
                new CombatOutcomeResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                result.BattleHealthDifference,
                Is.EqualTo(4294967295L));

            Assert.That(
                result.WinningMargin,
                Is.EqualTo(4294967295L));
        }

        [Test]
        public void Resolve_WhenCalledRepeatedly_IsDeterministicAndDoesNotChangeState()
        {
            var state =
                CreateState(
                    17,
                    9);

            var playerSide =
                state.GetSide(
                    CombatSide.Player);

            var enemySide =
                state.GetSide(
                    CombatSide.Enemy);

            var previousPlayerBattleHealth =
                playerSide.BattleHealth;

            var previousEnemyBattleHealth =
                enemySide.BattleHealth;

            var resolver =
                new CombatOutcomeResolver();

            var firstResult =
                resolver.Resolve(state);

            var secondResult =
                resolver.Resolve(state);

            Assert.That(
                firstResult.Outcome,
                Is.EqualTo(
                    secondResult.Outcome));

            Assert.That(
                firstResult.PlayerBattleHealth,
                Is.EqualTo(
                    secondResult.PlayerBattleHealth));

            Assert.That(
                firstResult.EnemyBattleHealth,
                Is.EqualTo(
                    secondResult.EnemyBattleHealth));

            Assert.That(
                firstResult.BattleHealthDifference,
                Is.EqualTo(
                    secondResult
                        .BattleHealthDifference));

            Assert.That(
                playerSide.BattleHealth,
                Is.EqualTo(
                    previousPlayerBattleHealth));

            Assert.That(
                enemySide.BattleHealth,
                Is.EqualTo(
                    previousEnemyBattleHealth));

            Assert.That(
                playerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                enemySide.Cards.Count,
                Is.Zero);

            Assert.That(
                playerSide.Board.SlotCount,
                Is.Zero);

            Assert.That(
                enemySide.Board.SlotCount,
                Is.Zero);
        }

        private static CombatState CreateState(
            int playerBattleHealth,
            int enemyBattleHealth)
        {
            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    playerBattleHealth);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    enemyBattleHealth);

            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            int battleHealth)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    battleHealth),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}