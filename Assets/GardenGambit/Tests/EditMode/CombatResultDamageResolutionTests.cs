using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultDamageResolutionTests
    {
        [Test]
        public void Constructor_WithoutModification_PreservesBaseDamage()
        {
            var calculation =
                CreateCalculation(
                    baseDamageToPlayer: 4,
                    baseDamageToEnemy: 6);

            var resolution =
                new CombatResultDamageResolution(
                    calculation,
                    4,
                    6);

            Assert.That(
                resolution.IsValid,
                Is.True);

            Assert.That(
                resolution.Calculation.IsValid,
                Is.True);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.Zero);

            Assert.That(
                resolution.EnemyDamageDelta,
                Is.Zero);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void Constructor_WithReductions_CalculatesPreventedDamage()
        {
            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 4,
                        baseDamageToEnemy: 6),
                    3,
                    4);

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.EqualTo(-1L));

            Assert.That(
                resolution.EnemyDamageDelta,
                Is.EqualTo(-2L));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.EqualTo(1L));

            Assert.That(
                resolution.PreventedDamageForEnemy,
                Is.EqualTo(2L));

            Assert.That(
                resolution.AddedDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution.AddedDamageToEnemy,
                Is.Zero);

            Assert.That(
                resolution.IsPlayerDamageReduced,
                Is.True);

            Assert.That(
                resolution.IsEnemyDamageReduced,
                Is.True);

            Assert.That(
                resolution.IsPlayerDamageIncreased,
                Is.False);

            Assert.That(
                resolution.IsEnemyDamageIncreased,
                Is.False);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.True);
        }

        [Test]
        public void Constructor_WithIncreases_CalculatesAddedDamage()
        {
            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 4,
                        baseDamageToEnemy: 6),
                    7,
                    8);

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.EqualTo(3L));

            Assert.That(
                resolution.EnemyDamageDelta,
                Is.EqualTo(2L));

            Assert.That(
                resolution.AddedDamageToPlayer,
                Is.EqualTo(3L));

            Assert.That(
                resolution.AddedDamageToEnemy,
                Is.EqualTo(2L));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.Zero);

            Assert.That(
                resolution.PreventedDamageForEnemy,
                Is.Zero);

            Assert.That(
                resolution.IsPlayerDamageIncreased,
                Is.True);

            Assert.That(
                resolution.IsEnemyDamageIncreased,
                Is.True);

            Assert.That(
                resolution.IsPlayerDamageReduced,
                Is.False);

            Assert.That(
                resolution.IsEnemyDamageReduced,
                Is.False);
        }

        [Test]
        public void Constructor_WithMixedChanges_TracksEachSideIndependently()
        {
            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 4,
                        baseDamageToEnemy: 6),
                    2,
                    9);

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.EqualTo(-2L));

            Assert.That(
                resolution.EnemyDamageDelta,
                Is.EqualTo(3L));

            Assert.That(
                resolution.PreventedDamageForPlayer,
                Is.EqualTo(2L));

            Assert.That(
                resolution.AddedDamageToEnemy,
                Is.EqualTo(3L));

            Assert.That(
                resolution.IsPlayerDamageReduced,
                Is.True);

            Assert.That(
                resolution.IsEnemyDamageIncreased,
                Is.True);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.True);
        }

        [Test]
        public void Constructor_WithZeroDamage_IsValidAndUnmodified()
        {
            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 0,
                        baseDamageToEnemy: 0),
                    0,
                    0);

            Assert.That(
                resolution.IsValid,
                Is.True);

            Assert.That(
                resolution.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resolution
                    .ResolvedIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        [Test]
        public void Constructor_WithMaximumResolvedDamage_UsesLongDelta()
        {
            var resolution =
                new CombatResultDamageResolution(
                    CreateCalculation(
                        baseDamageToPlayer: 0,
                        baseDamageToEnemy: 0),
                    int.MaxValue,
                    0);

            Assert.That(
                resolution.PlayerDamageDelta,
                Is.EqualTo(
                    (long)int.MaxValue));

            Assert.That(
                resolution.AddedDamageToPlayer,
                Is.EqualTo(
                    (long)int.MaxValue));

            Assert.That(
                resolution.IsPlayerDamageIncreased,
                Is.True);

            Assert.That(
                resolution.IsValid,
                Is.True);
        }

        [Test]
        public void Constructor_WithInvalidCalculation_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultDamageResolution(
                        default(
                            CombatResultDamageCalculation),
                        4,
                        6));
        }

        [Test]
        public void Constructor_WithNegativePlayerDamage_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatResultDamageResolution(
                        CreateCalculation(
                            baseDamageToPlayer: 4,
                            baseDamageToEnemy: 6),
                        -1,
                        6));
        }

        [Test]
        public void Constructor_WithNegativeEnemyDamage_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatResultDamageResolution(
                        CreateCalculation(
                            baseDamageToPlayer: 4,
                            baseDamageToEnemy: 6),
                        4,
                        -1));
        }

        [Test]
        public void DefaultValue_IsInvalid()
        {
            var resolution =
                default(
                    CombatResultDamageResolution);

            Assert.That(
                resolution.IsValid,
                Is.False);

            Assert.That(
                resolution.HasAnyDamageModification,
                Is.False);
        }

        private static CombatResultDamageCalculation
            CreateCalculation(
                int baseDamageToPlayer,
                int baseDamageToEnemy)
        {
            var playerContribution =
                CreateContribution(
                    CombatSide.Player,
                    baseDamageToEnemy);

            var enemyContribution =
                CreateContribution(
                    CombatSide.Enemy,
                    baseDamageToPlayer);

            return new CombatResultDamageCalculation(
                playerContribution,
                enemyContribution);
        }

        private static CombatSideResultContribution
            CreateContribution(
                CombatSide side,
                int contribution)
        {
            var survivorCount =
                contribution > 0
                    ? 1
                    : 0;

            return new CombatSideResultContribution(
                side,
                survivorCount,
                contribution,
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}