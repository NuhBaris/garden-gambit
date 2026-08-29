using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatOutcomeTests
    {
        [Test]
        public void Values_HaveStableNumericContract()
        {
            Assert.That(
                (int)CombatOutcome.Unspecified,
                Is.EqualTo(0));

            Assert.That(
                (int)CombatOutcome.PlayerVictory,
                Is.EqualTo(1));

            Assert.That(
                (int)CombatOutcome.EnemyVictory,
                Is.EqualTo(2));

            Assert.That(
                (int)CombatOutcome.Draw,
                Is.EqualTo(3));

            Assert.That(
                CombatOutcome.PlayerVictory,
                Is.Not.EqualTo(
                    CombatOutcome.EnemyVictory));

            Assert.That(
                CombatOutcome.PlayerVictory,
                Is.Not.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                CombatOutcome.EnemyVictory,
                Is.Not.EqualTo(
                    CombatOutcome.Draw));
        }
    }
}