using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultDamageCalculationTests
    {
        [Test]
        public void Constructor_WithValidContributions_MapsFullDamageToOpposingSides()
        {
            var playerContribution =
                CreateContribution(
                    CombatSide.Player,
                    2,
                    10,
                    3);

            var enemyContribution =
                CreateContribution(
                    CombatSide.Enemy,
                    1,
                    4,
                    2);

            var calculation =
                new CombatResultDamageCalculation(
                    playerContribution,
                    enemyContribution);

            Assert.That(
                calculation.IsValid,
                Is.True);

            Assert.That(
                calculation.PlayerContribution.Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(
                calculation.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(30));

            Assert.That(
                calculation.EnemyContribution.Side,
                Is.EqualTo(CombatSide.Enemy));

            Assert.That(
                calculation.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(8));

            Assert.That(
                calculation.BaseIncomingDamageToPlayer,
                Is.EqualTo(8));

            Assert.That(
                calculation.BaseIncomingDamageToEnemy,
                Is.EqualTo(30));

            Assert.That(
                calculation.HasIncomingDamageToPlayer,
                Is.True);

            Assert.That(
                calculation.HasIncomingDamageToEnemy,
                Is.True);

            Assert.That(
                calculation.HasMutualIncomingDamage,
                Is.True);
        }

        [Test]
        public void Constructor_WithBothZeroContributions_ProducesNoIncomingDamage()
        {
            var calculation =
                new CombatResultDamageCalculation(
                    CreateContribution(
                        CombatSide.Player,
                        0,
                        0,
                        2),
                    CreateContribution(
                        CombatSide.Enemy,
                        0,
                        0,
                        3));

            Assert.That(
                calculation.IsValid,
                Is.True);

            Assert.That(
                calculation.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                calculation.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                calculation.HasIncomingDamageToPlayer,
                Is.False);

            Assert.That(
                calculation.HasIncomingDamageToEnemy,
                Is.False);

            Assert.That(
                calculation.HasMutualIncomingDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithOnlyEnemyContribution_DamagesOnlyPlayer()
        {
            var calculation =
                new CombatResultDamageCalculation(
                    CreateContribution(
                        CombatSide.Player,
                        0,
                        0,
                        1),
                    CreateContribution(
                        CombatSide.Enemy,
                        2,
                        9,
                        2));

            Assert.That(
                calculation.BaseIncomingDamageToPlayer,
                Is.EqualTo(18));

            Assert.That(
                calculation.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                calculation.HasIncomingDamageToPlayer,
                Is.True);

            Assert.That(
                calculation.HasIncomingDamageToEnemy,
                Is.False);

            Assert.That(
                calculation.HasMutualIncomingDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithOnlyPlayerContribution_DamagesOnlyEnemy()
        {
            var calculation =
                new CombatResultDamageCalculation(
                    CreateContribution(
                        CombatSide.Player,
                        3,
                        12,
                        2),
                    CreateContribution(
                        CombatSide.Enemy,
                        0,
                        0,
                        1));

            Assert.That(
                calculation.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                calculation.BaseIncomingDamageToEnemy,
                Is.EqualTo(24));

            Assert.That(
                calculation.HasIncomingDamageToPlayer,
                Is.False);

            Assert.That(
                calculation.HasIncomingDamageToEnemy,
                Is.True);

            Assert.That(
                calculation.HasMutualIncomingDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithInvalidPlayerContribution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultDamageCalculation(
                        default(
                            CombatSideResultContribution),
                        CreateContribution(
                            CombatSide.Enemy,
                            1,
                            5,
                            1)));
        }

        [Test]
        public void Constructor_WithEnemyContributionAsPlayerContribution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultDamageCalculation(
                        CreateContribution(
                            CombatSide.Enemy,
                            1,
                            5,
                            1),
                        CreateContribution(
                            CombatSide.Enemy,
                            1,
                            5,
                            1)));
        }

        [Test]
        public void Constructor_WithInvalidEnemyContribution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultDamageCalculation(
                        CreateContribution(
                            CombatSide.Player,
                            1,
                            5,
                            1),
                        default(
                            CombatSideResultContribution)));
        }

        [Test]
        public void Constructor_WithPlayerContributionAsEnemyContribution_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultDamageCalculation(
                        CreateContribution(
                            CombatSide.Player,
                            1,
                            5,
                            1),
                        CreateContribution(
                            CombatSide.Player,
                            1,
                            5,
                            1)));
        }

        [Test]
        public void DefaultValue_IsInvalidAndHasNoIncomingDamage()
        {
            var calculation =
                default(
                    CombatResultDamageCalculation);

            Assert.That(
                calculation.IsValid,
                Is.False);

            Assert.That(
                calculation.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                calculation.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                calculation.HasIncomingDamageToPlayer,
                Is.False);

            Assert.That(
                calculation.HasIncomingDamageToEnemy,
                Is.False);

            Assert.That(
                calculation.HasMutualIncomingDamage,
                Is.False);
        }

        private static CombatSideResultContribution
            CreateContribution(
                CombatSide side,
                int survivorCount,
                int totalRankContribution,
                int attackMultiplier)
        {
            return new CombatSideResultContribution(
                side,
                survivorCount,
                totalRankContribution,
                new AttackMultiplier(
                    attackMultiplier));
        }
    }
}