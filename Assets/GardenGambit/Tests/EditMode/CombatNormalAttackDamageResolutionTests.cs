using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackDamageResolutionTests
    {
        [Test]
        public void Constructor_WithBaseDamage_SetsState()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var resolution =
                new CombatNormalAttackDamageResolution(
                    batch,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            Assert.That(
                resolution.Batch,
                Is.SameAs(batch));

            Assert.That(
                resolution.BaseDamageToEnemy,
                Is.EqualTo(5));

            Assert.That(
                resolution.BaseDamageToPlayer,
                Is.EqualTo(7));

            Assert.That(
                resolution.ResolvedDamageToEnemy,
                Is.EqualTo(5));

            Assert.That(
                resolution.ResolvedDamageToPlayer,
                Is.EqualTo(7));

            Assert.That(
                resolution.PlayerAttackDamageDelta,
                Is.EqualTo(0L));

            Assert.That(
                resolution.EnemyAttackDamageDelta,
                Is.EqualTo(0L));
        }

        [Test]
        public void Constructor_WithPlayerBonus_SetsPositiveDelta()
        {
            var resolution =
                new CombatNormalAttackDamageResolution(
                    CreateBatch(
                        playerAttack: 5,
                        enemyAttack: 7),
                    resolvedDamageToEnemy: 8,
                    resolvedDamageToPlayer: 7);

            Assert.That(
                resolution.PlayerAttackDamageDelta,
                Is.EqualTo(3L));

            Assert.That(
                resolution.EnemyAttackDamageDelta,
                Is.EqualTo(0L));
        }

        [Test]
        public void Constructor_WithEnemyReduction_SetsNegativeDelta()
        {
            var resolution =
                new CombatNormalAttackDamageResolution(
                    CreateBatch(
                        playerAttack: 5,
                        enemyAttack: 7),
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 2);

            Assert.That(
                resolution.PlayerAttackDamageDelta,
                Is.EqualTo(0L));

            Assert.That(
                resolution.EnemyAttackDamageDelta,
                Is.EqualTo(-5L));
        }

        [Test]
        public void Constructor_WithZeroDamage_SetsNoDamageFlags()
        {
            var resolution =
                new CombatNormalAttackDamageResolution(
                    CreateBatch(
                        playerAttack: 5,
                        enemyAttack: 7),
                    resolvedDamageToEnemy: 0,
                    resolvedDamageToPlayer: 0);

            Assert.That(
                resolution.HasDamageToEnemy,
                Is.False);

            Assert.That(
                resolution.HasDamageToPlayer,
                Is.False);

            Assert.That(
                resolution.HasMutualDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithMutualDamage_SetsMutualFlag()
        {
            var resolution =
                new CombatNormalAttackDamageResolution(
                    CreateBatch(
                        playerAttack: 5,
                        enemyAttack: 7),
                    resolvedDamageToEnemy: 3,
                    resolvedDamageToPlayer: 4);

            Assert.That(
                resolution.HasDamageToEnemy,
                Is.True);

            Assert.That(
                resolution.HasDamageToPlayer,
                Is.True);

            Assert.That(
                resolution.HasMutualDamage,
                Is.True);
        }

        [Test]
        public void Constructor_WithOneSidedDamage_SetsSideFlags()
        {
            var resolution =
                new CombatNormalAttackDamageResolution(
                    CreateBatch(
                        playerAttack: 5,
                        enemyAttack: 7),
                    resolvedDamageToEnemy: 3,
                    resolvedDamageToPlayer: 0);

            Assert.That(
                resolution.HasDamageToEnemy,
                Is.True);

            Assert.That(
                resolution.HasDamageToPlayer,
                Is.False);

            Assert.That(
                resolution.HasMutualDamage,
                Is.False);
        }

        [Test]
        public void Constructor_WithNullBatch_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackDamageResolution(
                        null,
                        resolvedDamageToEnemy: 5,
                        resolvedDamageToPlayer: 7));
        }

        [Test]
        public void Constructor_WithNegativeDamageToEnemy_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new
                        CombatNormalAttackDamageResolution(
                            CreateBatch(
                                playerAttack: 5,
                                enemyAttack: 7),
                            resolvedDamageToEnemy: -1,
                            resolvedDamageToPlayer: 7));
        }

        [Test]
        public void Constructor_WithNegativeDamageToPlayer_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new
                        CombatNormalAttackDamageResolution(
                            CreateBatch(
                                playerAttack: 5,
                                enemyAttack: 7),
                            resolvedDamageToEnemy: 5,
                            resolvedDamageToPlayer: -1));
        }

        [Test]
        public void Constructor_WithExtremeDeltas_UsesLong()
        {
            var maximumPositive =
                new CombatNormalAttackDamageResolution(
                    CreateBatch(
                        playerAttack: 0,
                        enemyAttack: int.MaxValue),
                    resolvedDamageToEnemy:
                        int.MaxValue,
                    resolvedDamageToPlayer: 0);

            Assert.That(
                maximumPositive
                    .PlayerAttackDamageDelta,
                Is.EqualTo(
                    (long)int.MaxValue));

            Assert.That(
                maximumPositive
                    .EnemyAttackDamageDelta,
                Is.EqualTo(
                    -(long)int.MaxValue));
        }

        private static CombatNormalAttackEventBatch
            CreateBatch(
                int playerAttack,
                int enemyAttack)
        {
            var exchangeEventId =
                new CombatEventId(1);

            var exchangeMetadata =
                new CombatEventMetadata(
                    exchangeEventId,
                    new CombatSequenceNumber(1),
                    null,
                    exchangeEventId);

            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerInstanceId =
                new InstanceId(1);

            var enemyInstanceId =
                new InstanceId(101);

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    exchangeMetadata,
                    playerInstanceId,
                    playerPosition,
                    playerAttack,
                    enemyInstanceId,
                    enemyPosition,
                    enemyAttack);

            var playerAttackEvent =
                new NormalAttackCombatEvent(
                    new CombatEventMetadata(
                        new CombatEventId(2),
                        new CombatSequenceNumber(2),
                        exchangeEventId,
                        exchangeEventId),
                    playerInstanceId,
                    playerPosition,
                    enemyInstanceId,
                    enemyPosition,
                    playerAttack);

            var enemyAttackEvent =
                new NormalAttackCombatEvent(
                    new CombatEventMetadata(
                        new CombatEventId(3),
                        new CombatSequenceNumber(3),
                        exchangeEventId,
                        exchangeEventId),
                    enemyInstanceId,
                    enemyPosition,
                    playerInstanceId,
                    playerPosition,
                    enemyAttack);

            return new CombatNormalAttackEventBatch(
                exchangeEvent,
                playerAttackEvent,
                enemyAttackEvent);
        }
    }
}