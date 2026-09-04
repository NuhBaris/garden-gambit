using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackDamageResolutionResolverTests
    {
        [Test]
        public void Resolve_WithNullBatch_Throws()
        {
            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(null));
        }

        [Test]
        public void Resolve_WithoutCallback_UsesBaseDamage()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    batch);

            Assert.That(
                resolution.Batch,
                Is.SameAs(batch));

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
        public void Resolve_WithNullCallback_Throws()
        {
            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(
                    CreateBatch(
                        playerAttack: 5,
                        enemyAttack: 7),
                    null));
        }

        [Test]
        public void Resolve_WithCallback_UsesResolvedDamage()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            var resolution =
                resolver.Resolve(
                    batch,
                    attackEvent =>
                        attackEvent.IsPlayerAttack
                            ? 8
                            : 3);

            Assert.That(
                resolution.ResolvedDamageToEnemy,
                Is.EqualTo(8));

            Assert.That(
                resolution.ResolvedDamageToPlayer,
                Is.EqualTo(3));

            Assert.That(
                resolution.PlayerAttackDamageDelta,
                Is.EqualTo(3L));

            Assert.That(
                resolution.EnemyAttackDamageDelta,
                Is.EqualTo(-4L));
        }

        [Test]
        public void Resolve_WithCallback_ProcessesPlayerBeforeEnemy()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var processedEvents =
                new List<
                    NormalAttackCombatEvent>();

            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            resolver.Resolve(
                batch,
                attackEvent =>
                {
                    processedEvents.Add(
                        attackEvent);

                    return attackEvent.BaseDamage;
                });

            Assert.That(
                processedEvents.Count,
                Is.EqualTo(2));

            Assert.That(
                processedEvents[0],
                Is.SameAs(
                    batch.PlayerAttackEvent));

            Assert.That(
                processedEvents[1],
                Is.SameAs(
                    batch.EnemyAttackEvent));
        }

        [Test]
        public void Resolve_WithNegativePlayerDamage_StopsBeforeEnemy()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var callbackCallCount = 0;

            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(
                    batch,
                    attackEvent =>
                    {
                        callbackCallCount++;

                        return attackEvent.IsPlayerAttack
                            ? -1
                            : attackEvent.BaseDamage;
                    }));

            Assert.That(
                callbackCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithNegativeEnemyDamage_ThrowsAfterBoth()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var callbackCallCount = 0;

            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(
                    batch,
                    attackEvent =>
                    {
                        callbackCallCount++;

                        return attackEvent.IsEnemyAttack
                            ? -1
                            : attackEvent.BaseDamage;
                    }));

            Assert.That(
                callbackCallCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WhenCallbackThrows_PropagatesException()
        {
            var batch =
                CreateBatch(
                    playerAttack: 5,
                    enemyAttack: 7);

            var callbackCallCount = 0;

            var resolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            Assert.Throws<TestResolutionException>(
                () => resolver.Resolve(
                    batch,
                    attackEvent =>
                    {
                        callbackCallCount++;

                        throw new
                            TestResolutionException();
                    }));

            Assert.That(
                callbackCallCount,
                Is.EqualTo(1));
        }

        private static CombatNormalAttackEventBatch
            CreateBatch(
                int playerAttack,
                int enemyAttack)
        {
            var exchangeEventId =
                new CombatEventId(1);

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
                    new CombatEventMetadata(
                        exchangeEventId,
                        new CombatSequenceNumber(1),
                        null,
                        exchangeEventId),
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

        private sealed class
            TestResolutionException :
            Exception
        {
        }
    }
}