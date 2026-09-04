using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackDamageApplicationResolverTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplicationResolver(
                            null,
                            new CombatEventLog()));
        }

        [Test]
        public void
            Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatNormalAttackDamageApplicationResolver(
                            CreateMetadataFactory(),
                            null));
        }

        [Test]
        public void Apply_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            var resolution =
                CreateResolution(
                    environment,
                    3,
                    4);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Apply(
                    null,
                    resolution));
        }

        [Test]
        public void Apply_WithNullResolution_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Apply(
                    environment.State,
                    null));
        }

        [Test]
        public void
            Apply_UsesResolvedDamageValues()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10,
                    playerAttack: 3,
                    enemyAttack: 4);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 6,
                    resolvedDamageToPlayer: 2);

            var application =
                environment.Resolver.Apply(
                    environment.State,
                    resolution);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                application.Resolution,
                Is.SameAs(resolution));

            Assert.That(
                application.Batch,
                Is.SameAs(environment.Batch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void
            Apply_CreatesDamageEventsAsNormalAttackChildren()
        {
            var environment =
                CreateEnvironment();

            var resolution =
                CreateResolution(
                    environment,
                    2,
                    3);

            var application =
                environment.Resolver.Apply(
                    environment.State,
                    resolution);

            Assert.That(
                application.DamageToEnemyEvent
                    .Metadata.HasParent,
                Is.True);

            Assert.That(
                application.DamageToEnemyEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.Batch
                        .PlayerAttackEvent
                        .Metadata.EventId));

            Assert.That(
                application.DamageToPlayerEvent
                    .Metadata.HasParent,
                Is.True);

            Assert.That(
                application.DamageToPlayerEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.Batch
                        .EnemyAttackEvent
                        .Metadata.EventId));

            Assert.That(
                application.DamageToEnemyEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.Batch.ExchangeEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                application.DamageToPlayerEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.Batch.ExchangeEvent
                        .Metadata.TriggerRootId));
        }

        [Test]
        public void
            Apply_WithZeroDamage_DoesNotChangeHpOrCreateDeath()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 7,
                    enemyCurrentHp: 8);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 0,
                    resolvedDamageToPlayer: 0);

            var application =
                environment.Resolver.Apply(
                    environment.State,
                    resolution);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(8));

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

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void
            Apply_WhenPlayerReachesDeathThreshold_CreatesPlayerDeath()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 10);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 2,
                    resolvedDamageToPlayer: 4);

            var application =
                environment.Resolver.Apply(
                    environment.State,
                    resolution);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

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
                application.PlayerDeathEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    application.DamageToPlayerEvent
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(6));

            Assert.That(
                environment.EventLog.Events[5],
                Is.SameAs(
                    application.PlayerDeathEvent));
        }

        [Test]
        public void
            Apply_WhenEnemyReachesDeathThreshold_CreatesEnemyDeath()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 5);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 2);

            var application =
                environment.Resolver.Apply(
                    environment.State,
                    resolution);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

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
                application.EnemyDeathEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    application.DamageToEnemyEvent
                        .Metadata.EventId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(6));

            Assert.That(
                environment.EventLog.Events[5],
                Is.SameAs(
                    application.EnemyDeathEvent));
        }

        [Test]
        public void
            Apply_WithMutualDeath_AppendsDeterministicEventOrder()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 5);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 5,
                    resolvedDamageToPlayer: 4);

            var application =
                environment.Resolver.Apply(
                    environment.State,
                    resolution);

            Assert.That(
                application.DidPlayerDie,
                Is.True);

            Assert.That(
                application.DidEnemyDie,
                Is.True);

            Assert.That(
                application.DidBothDie,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.Batch.ExchangeEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    environment.Batch
                        .PlayerAttackEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    environment.Batch
                        .EnemyAttackEvent));

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(
                    application.DamageToEnemyEvent));

            Assert.That(
                environment.EventLog.Events[4],
                Is.SameAs(
                    application.DamageToPlayerEvent));

            Assert.That(
                environment.EventLog.Events[5],
                Is.SameAs(
                    application.PlayerDeathEvent));

            Assert.That(
                environment.EventLog.Events[6],
                Is.SameAs(
                    application.EnemyDeathEvent));
        }

        [Test]
        public void
            Apply_WhenCalledTwice_ThrowsWithoutRepeatingDamage()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 3,
                    resolvedDamageToPlayer: 4);

            environment.Resolver.Apply(
                environment.State,
                resolution);

            var playerHpAfterFirstApplication =
                environment.PlayerCard.CurrentHp;

            var enemyHpAfterFirstApplication =
                environment.EnemyCard.CurrentHp;

            var eventCountAfterFirstApplication =
                environment.EventLog.Count;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Apply(
                    environment.State,
                    resolution));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    playerHpAfterFirstApplication));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    enemyHpAfterFirstApplication));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCountAfterFirstApplication));
        }

        [Test]
        public void
            Apply_WithUnloggedBatch_ThrowsWithoutApplyingDamage()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10);

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 3,
                    resolvedDamageToPlayer: 4);

            var emptyEventLog =
                new CombatEventLog();

            var resolver =
                new
                    CombatNormalAttackDamageApplicationResolver(
                        environment.MetadataFactory,
                        emptyEventLog);

            Assert.Throws<ArgumentException>(
                () => resolver.Apply(
                    environment.State,
                    resolution));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                emptyEventLog.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void
            Apply_WhenCurrentCardsDoNotMatchBatch_ThrowsWithoutAppendingEvents()
        {
            var environment =
                CreateEnvironment();

            var resolution =
                CreateResolution(
                    environment,
                    resolvedDamageToEnemy: 3,
                    resolvedDamageToPlayer: 4);

            var differentPlayerCard =
                CreateCard(
                    "card.different.player",
                    101,
                    10,
                    4);

            var differentEnemyCard =
                CreateCard(
                    "card.different.enemy",
                    201,
                    10,
                    3);

            var differentState =
                CreateState(
                    environment.PlayerPosition,
                    differentPlayerCard,
                    environment.EnemyPosition,
                    differentEnemyCard);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Apply(
                    differentState,
                    resolution));

            Assert.That(
                differentPlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                differentEnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        private static
            CombatNormalAttackDamageResolution
            CreateResolution(
                TestEnvironment environment,
                int resolvedDamageToEnemy,
                int resolvedDamageToPlayer)
        {
            return new
                CombatNormalAttackDamageResolution(
                    environment.Batch,
                    resolvedDamageToEnemy,
                    resolvedDamageToPlayer);
        }

        private static TestEnvironment
            CreateEnvironment(
                int playerCurrentHp = 10,
                int enemyCurrentHp = 10,
                int playerAttack = 3,
                int enemyAttack = 4)
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "card.player",
                    100,
                    playerCurrentHp,
                    playerAttack);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    200,
                    enemyCurrentHp,
                    enemyAttack);

            var state =
                CreateState(
                    playerPosition,
                    playerCard,
                    enemyPosition,
                    enemyCard);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var preparationResolver =
                new CombatNormalAttackPreparationResolver(
                    metadataFactory,
                    eventLog);

            var batch =
                preparationResolver.Prepare(
                    state,
                    playerPosition,
                    enemyPosition);

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                Batch = batch,
                Resolver =
                    new
                        CombatNormalAttackDamageApplicationResolver(
                            metadataFactory,
                            eventLog)
            };
        }

        private static CombatState CreateState(
            BoardPosition playerPosition,
            CombatCardState playerCard,
            BoardPosition enemyPosition,
            CombatCardState enemyCard)
        {
            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    new SlotId(1),
                    playerPosition,
                    playerCard);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    new SlotId(2),
                    enemyPosition,
                    enemyCard);

            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position,
            CombatCardState card)
        {
            var slot =
                new CombatSlotState(
                    slotId,
                    position,
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

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                0,
                attack);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public CombatCardState PlayerCard
            {
                get;
                set;
            }

            public CombatCardState EnemyCard
            {
                get;
                set;
            }

            public BoardPosition PlayerPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyPosition
            {
                get;
                set;
            }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatNormalAttackEventBatch Batch
            {
                get;
                set;
            }

            public
                CombatNormalAttackDamageApplicationResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}