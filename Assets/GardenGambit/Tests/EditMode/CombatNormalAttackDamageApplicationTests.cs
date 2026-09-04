using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackDamageApplicationTests
    {
        [Test]
        public void Constructor_WithoutDeaths_SetsState()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 20,
                    enemyHp: 20,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            var application =
                new CombatNormalAttackDamageApplication(
                    environment.Resolution,
                    environment.DamageToEnemyEvent,
                    environment.DamageToPlayerEvent,
                    environment.PlayerDeathEvent,
                    environment.EnemyDeathEvent);

            Assert.That(
                application.Resolution,
                Is.SameAs(
                    environment.Resolution));

            Assert.That(
                application.Batch,
                Is.SameAs(
                    environment.Resolution.Batch));

            Assert.That(
                application.DamageToEnemyEvent,
                Is.SameAs(
                    environment.DamageToEnemyEvent));

            Assert.That(
                application.DamageToPlayerEvent,
                Is.SameAs(
                    environment.DamageToPlayerEvent));

            Assert.That(
                application.PlayerDeathEvent,
                Is.Null);

            Assert.That(
                application.EnemyDeathEvent,
                Is.Null);

            Assert.That(
                application.DidPlayerDie,
                Is.False);

            Assert.That(
                application.DidEnemyDie,
                Is.False);

            Assert.That(
                application.DidBothDie,
                Is.False);
        }

        [Test]
        public void Constructor_WithPlayerDeath_SetsFlags()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 5,
                    enemyHp: 20,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            var application =
                CreateApplication(
                    environment);

            Assert.That(
                application.PlayerDeathEvent,
                Is.Not.Null);

            Assert.That(
                application.EnemyDeathEvent,
                Is.Null);

            Assert.That(
                application.DidPlayerDie,
                Is.True);

            Assert.That(
                application.DidEnemyDie,
                Is.False);

            Assert.That(
                application.DidBothDie,
                Is.False);
        }

        [Test]
        public void Constructor_WithEnemyDeath_SetsFlags()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 20,
                    enemyHp: 5,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            var application =
                CreateApplication(
                    environment);

            Assert.That(
                application.PlayerDeathEvent,
                Is.Null);

            Assert.That(
                application.EnemyDeathEvent,
                Is.Not.Null);

            Assert.That(
                application.DidPlayerDie,
                Is.False);

            Assert.That(
                application.DidEnemyDie,
                Is.True);

            Assert.That(
                application.DidBothDie,
                Is.False);
        }

        [Test]
        public void Constructor_WithBothDeaths_SetsMutualFlag()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 5,
                    enemyHp: 5,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            var application =
                CreateApplication(
                    environment);

            Assert.That(
                application.PlayerDeathEvent,
                Is.Not.Null);

            Assert.That(
                application.EnemyDeathEvent,
                Is.Not.Null);

            Assert.That(
                application.DidPlayerDie,
                Is.True);

            Assert.That(
                application.DidEnemyDie,
                Is.True);

            Assert.That(
                application.DidBothDie,
                Is.True);
        }

        [Test]
        public void Constructor_WithNullResolution_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 20,
                    enemyHp: 20,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplication(
                            null,
                            environment
                                .DamageToEnemyEvent,
                            environment
                                .DamageToPlayerEvent,
                            null,
                            null));
        }

        [Test]
        public void Constructor_WithNullDamageToEnemy_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 20,
                    enemyHp: 20,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplication(
                            environment.Resolution,
                            null,
                            environment
                                .DamageToPlayerEvent,
                            null,
                            null));
        }

        [Test]
        public void Constructor_WithNullDamageToPlayer_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 20,
                    enemyHp: 20,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplication(
                            environment.Resolution,
                            environment
                                .DamageToEnemyEvent,
                            null,
                            null,
                            null));
        }

        [Test]
        public void Constructor_WithSwappedDamageEvents_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 20,
                    enemyHp: 20,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplication(
                            environment.Resolution,
                            environment
                                .DamageToPlayerEvent,
                            environment
                                .DamageToEnemyEvent,
                            null,
                            null));
        }

        [Test]
        public void Constructor_WithSwappedDeathEvents_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHp: 5,
                    enemyHp: 5,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 7);

            Assert.That(
                environment.PlayerDeathEvent,
                Is.Not.Null);

            Assert.That(
                environment.EnemyDeathEvent,
                Is.Not.Null);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplication(
                            environment.Resolution,
                            environment
                                .DamageToEnemyEvent,
                            environment
                                .DamageToPlayerEvent,
                            environment
                                .EnemyDeathEvent,
                            environment
                                .PlayerDeathEvent));
        }

        private static
            CombatNormalAttackDamageApplication
            CreateApplication(
                TestEnvironment environment)
        {
            return new
                CombatNormalAttackDamageApplication(
                    environment.Resolution,
                    environment.DamageToEnemyEvent,
                    environment.DamageToPlayerEvent,
                    environment.PlayerDeathEvent,
                    environment.EnemyDeathEvent);
        }

        private static TestEnvironment
            CreateEnvironment(
                int playerHp,
                int enemyHp,
                int resolvedDamageToEnemy,
                int resolvedDamageToPlayer)
        {
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

            var playerCard =
                new CombatCardState(
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(1),
                    new CardRank(4),
                    hpCapacity: 20,
                    currentHp: playerHp,
                    armor: 0,
                    attack: 5);

            var enemyCard =
                new CombatCardState(
                    new DefinitionId(
                        "enemy-card"),
                    new InstanceId(101),
                    new CardRank(6),
                    hpCapacity: 20,
                    currentHp: enemyHp,
                    armor: 0,
                    attack: 7);

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new SlotId(1),
                        playerPosition,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(101),
                        enemyPosition,
                        enemyCard));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var batch =
                new
                    CombatNormalAttackPreparationResolver(
                        metadataFactory,
                        eventLog)
                    .Prepare(
                        state,
                        playerPosition,
                        enemyPosition);

            var resolution =
                new CombatNormalAttackDamageResolution(
                    batch,
                    resolvedDamageToEnemy,
                    resolvedDamageToPlayer);

            enemyCard.PreviewIncomingDamage(
                resolvedDamageToEnemy);

            playerCard.PreviewIncomingDamage(
                resolvedDamageToPlayer);

            var damageToEnemyMetadata =
                metadataFactory.CreateChild(
                    batch.PlayerAttackEvent.Metadata);

            var damageToPlayerMetadata =
                metadataFactory.CreateChild(
                    batch.EnemyAttackEvent.Metadata);

            var damageResolver =
                new CombatDamageResolver(
                    metadataFactory,
                    eventLog);

            var damageToEnemyEvent =
                damageResolver
                    .ApplyPreparedCardDamage(
                        state,
                        batch.PlayerAttackEvent,
                        playerPosition,
                        enemyPosition,
                        resolvedDamageToEnemy,
                        damageToEnemyMetadata);

            var damageToPlayerEvent =
                damageResolver
                    .ApplyPreparedCardDamage(
                        state,
                        batch.EnemyAttackEvent,
                        enemyPosition,
                        playerPosition,
                        resolvedDamageToPlayer,
                        damageToPlayerMetadata);

            var deathEventResolver =
                new CombatDeathEventResolver(
                    metadataFactory,
                    eventLog);

            var playerDeathEvent =
                deathEventResolver.AppendFromDamage(
                    damageToPlayerEvent);

            var enemyDeathEvent =
                deathEventResolver.AppendFromDamage(
                    damageToEnemyEvent);

            return new TestEnvironment
            {
                Resolution =
                    resolution,
                DamageToEnemyEvent =
                    damageToEnemyEvent,
                DamageToPlayerEvent =
                    damageToPlayerEvent,
                PlayerDeathEvent =
                    playerDeathEvent,
                EnemyDeathEvent =
                    enemyDeathEvent
            };
        }

        private static CombatSideState
            CreateSide(
                CombatSide side,
                SlotId slotId,
                BoardPosition position,
                CombatCardState card)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            slotId,
                            position,
                            card.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private sealed class TestEnvironment
        {
            public CombatNormalAttackDamageResolution
                Resolution
            {
                get;
                set;
            }

            public DamageAppliedCombatEvent
                DamageToEnemyEvent
            {
                get;
                set;
            }

            public DamageAppliedCombatEvent
                DamageToPlayerEvent
            {
                get;
                set;
            }

            public DeathCombatEvent PlayerDeathEvent
            {
                get;
                set;
            }

            public DeathCombatEvent EnemyDeathEvent
            {
                get;
                set;
            }
        }
    }
}