using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatOutcomeCalculationTests
    {
        [Test]
        public void Constructor_WhenPlayerHealthIsHigher_ReturnsPlayerVictory()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(13),
                    new BattleHealth(8));

            Assert.That(
                calculation.IsValid,
                Is.True);

            Assert.That(
                calculation.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(13)));

            Assert.That(
                calculation.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(8)));

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.EqualTo(5L));

            Assert.That(
                calculation.WinningMargin,
                Is.EqualTo(5L));

            Assert.That(
                calculation.IsPlayerVictory,
                Is.True);

            Assert.That(
                calculation.IsEnemyVictory,
                Is.False);

            Assert.That(
                calculation.IsDraw,
                Is.False);
        }

        [Test]
        public void Constructor_WhenEnemyHealthIsHigher_ReturnsEnemyVictory()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(4),
                    new BattleHealth(11));

            Assert.That(
                calculation.IsValid,
                Is.True);

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.EnemyVictory));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.EqualTo(-7L));

            Assert.That(
                calculation.WinningMargin,
                Is.EqualTo(7L));

            Assert.That(
                calculation.IsPlayerVictory,
                Is.False);

            Assert.That(
                calculation.IsEnemyVictory,
                Is.True);

            Assert.That(
                calculation.IsDraw,
                Is.False);
        }

        [Test]
        public void Constructor_WhenHealthValuesAreEqual_ReturnsDraw()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(10),
                    new BattleHealth(10));

            Assert.That(
                calculation.IsValid,
                Is.True);

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.Zero);

            Assert.That(
                calculation.WinningMargin,
                Is.Zero);

            Assert.That(
                calculation.IsPlayerVictory,
                Is.False);

            Assert.That(
                calculation.IsEnemyVictory,
                Is.False);

            Assert.That(
                calculation.IsDraw,
                Is.True);
        }

        [Test]
        public void Constructor_WithNegativeHealthValues_UsesNormalNumericComparison()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(-3),
                    new BattleHealth(-7));

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.EqualTo(4L));

            Assert.That(
                calculation.WinningMargin,
                Is.EqualTo(4L));

            Assert.That(
                calculation.IsPlayerVictory,
                Is.True);
        }

        [Test]
        public void Constructor_WithEqualNegativeHealthValues_ReturnsDraw()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(-12),
                    new BattleHealth(-12));

            Assert.That(
                calculation.IsValid,
                Is.True);

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.Zero);

            Assert.That(
                calculation.WinningMargin,
                Is.Zero);

            Assert.That(
                calculation.IsDraw,
                Is.True);
        }

        [Test]
        public void Constructor_WithMaximumPositiveDifference_UsesLongWithoutOverflow()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(int.MaxValue),
                    new BattleHealth(int.MinValue));

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.EqualTo(4294967295L));

            Assert.That(
                calculation.WinningMargin,
                Is.EqualTo(4294967295L));

            Assert.That(
                calculation.IsPlayerVictory,
                Is.True);
        }

        [Test]
        public void Constructor_WithMaximumNegativeDifference_UsesLongWithoutOverflow()
        {
            var calculation =
                new CombatOutcomeCalculation(
                    new BattleHealth(int.MinValue),
                    new BattleHealth(int.MaxValue));

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.EnemyVictory));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.EqualTo(-4294967295L));

            Assert.That(
                calculation.WinningMargin,
                Is.EqualTo(4294967295L));

            Assert.That(
                calculation.IsEnemyVictory,
                Is.True);
        }

        [Test]
        public void DefaultValue_IsInvalidAndUnspecified()
        {
            var calculation =
                default(
                    CombatOutcomeCalculation);

            Assert.That(
                calculation.IsValid,
                Is.False);

            Assert.That(
                calculation.Outcome,
                Is.EqualTo(
                    CombatOutcome.Unspecified));

            Assert.That(
                calculation.BattleHealthDifference,
                Is.Zero);

            Assert.That(
                calculation.WinningMargin,
                Is.Zero);

            Assert.That(
                calculation.IsPlayerVictory,
                Is.False);

            Assert.That(
                calculation.IsEnemyVictory,
                Is.False);

            Assert.That(
                calculation.IsDraw,
                Is.False);
        }
    }
}