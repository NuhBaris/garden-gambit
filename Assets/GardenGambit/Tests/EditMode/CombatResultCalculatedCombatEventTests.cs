using System;
using GardenGambit.Domain.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultCalculatedCombatEventTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsCompleteSnapshot()
        {
            var metadata =
                CreateMetadata();

            var calculation =
                CreateCalculation(
                    2,
                    10,
                    3,
                    1,
                    4,
                    2);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    metadata,
                    calculation,
                    7,
                    28);

            Assert.That(
                resultEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .CombatResultCalculated));

            Assert.That(
                resultEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                resultEvent.Calculation.IsValid,
                Is.True);

            Assert.That(
                resultEvent.PlayerContribution.Side,
                Is.EqualTo(CombatSide.Player));

            Assert.That(
                resultEvent.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(30));

            Assert.That(
                resultEvent.EnemyContribution.Side,
                Is.EqualTo(CombatSide.Enemy));

            Assert.That(
                resultEvent.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(8));

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(8));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(30));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToPlayer,
                Is.EqualTo(7));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToEnemy,
                Is.EqualTo(28));

            Assert.That(
                resultEvent.HasResolvedDamageToPlayer,
                Is.True);

            Assert.That(
                resultEvent.HasResolvedDamageToEnemy,
                Is.True);

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
                Is.True);
        }

        [Test]
        public void Constructor_WithZeroContributions_AllowsZeroResolvedDamage()
        {
            var calculation =
                CreateCalculation(
                    0,
                    0,
                    2,
                    0,
                    0,
                    3);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    calculation,
                    0,
                    0);

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.ResolvedIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resultEvent.ResolvedIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.HasResolvedDamageToPlayer,
                Is.False);

            Assert.That(
                resultEvent.HasResolvedDamageToEnemy,
                Is.False);

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithReducedResolvedDamage_AllowsModifierResult()
        {
            var calculation =
                CreateCalculation(
                    1,
                    10,
                    2,
                    1,
                    5,
                    2);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    calculation,
                    9,
                    19);

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(10));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToPlayer,
                Is.EqualTo(9));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToEnemy,
                Is.EqualTo(19));
        }

        [Test]
        public void Constructor_WithResolvedDamageAboveBase_AllowsAmplification()
        {
            var calculation =
                CreateCalculation(
                    1,
                    10,
                    2,
                    1,
                    5,
                    2);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    calculation,
                    12,
                    25);

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(10));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(20));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToPlayer,
                Is.EqualTo(12));

            Assert.That(
                resultEvent.ResolvedIncomingDamageToEnemy,
                Is.EqualTo(25));

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
                Is.True);
        }

        [Test]
        public void Constructor_WithOneSidedResolvedDamage_SetsFlags()
        {
            var calculation =
                CreateCalculation(
                    0,
                    0,
                    1,
                    1,
                    6,
                    2);

            var resultEvent =
                new CombatResultCalculatedCombatEvent(
                    CreateMetadata(),
                    calculation,
                    12,
                    0);

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(12));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                resultEvent.HasResolvedDamageToPlayer,
                Is.True);

            Assert.That(
                resultEvent.HasResolvedDamageToEnemy,
                Is.False);

            Assert.That(
                resultEvent.HasMutualResolvedDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            var calculation =
                CreateCalculation(
                    1,
                    5,
                    1,
                    1,
                    5,
                    1);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultCalculatedCombatEvent(
                        default(CombatEventMetadata),
                        calculation,
                        5,
                        5));
        }

        [Test]
        public void Constructor_WithInvalidCalculation_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatResultCalculatedCombatEvent(
                        CreateMetadata(),
                        default(
                            CombatResultDamageCalculation),
                        0,
                        0));
        }

        [Test]
        public void Constructor_WithNegativePlayerDamage_Throws()
        {
            var calculation =
                CreateCalculation(
                    1,
                    5,
                    1,
                    1,
                    5,
                    1);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatResultCalculatedCombatEvent(
                        CreateMetadata(),
                        calculation,
                        -1,
                        5));
        }

        [Test]
        public void Constructor_WithNegativeEnemyDamage_Throws()
        {
            var calculation =
                CreateCalculation(
                    1,
                    5,
                    1,
                    1,
                    5,
                    1);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatResultCalculatedCombatEvent(
                        CreateMetadata(),
                        calculation,
                        5,
                        -1));
        }

        private static CombatResultDamageCalculation
            CreateCalculation(
                int playerSurvivorCount,
                int playerRankContribution,
                int playerAttackMultiplier,
                int enemySurvivorCount,
                int enemyRankContribution,
                int enemyAttackMultiplier)
        {
            var playerContribution =
                new CombatSideResultContribution(
                    CombatSide.Player,
                    playerSurvivorCount,
                    playerRankContribution,
                    new AttackMultiplier(
                        playerAttackMultiplier));

            var enemyContribution =
                new CombatSideResultContribution(
                    CombatSide.Enemy,
                    enemySurvivorCount,
                    enemyRankContribution,
                    new AttackMultiplier(
                        enemyAttackMultiplier));

            return new CombatResultDamageCalculation(
                playerContribution,
                enemyContribution);
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }
    }
}