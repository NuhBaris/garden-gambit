using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnNormalAttackResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnNormalAttackResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnNormalAttackResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void TryResolveExchange_WithNullState_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryResolveExchange(
                        null,
                        environment.ColumnStartedEvent));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithNullColumnEvent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        null));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithUnloggedColumnEvent_ThrowsEvenWhenFrontsAreEmpty()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false);

            var unloggedColumnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment.ParentEvent
                                .Metadata),
                    environment.Column);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        unloggedColumnEvent));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithDifferentLoggedReference_ThrowsEvenWhenFrontsAreEmpty()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false);

            var differentReference =
                new ColumnStartedCombatEvent(
                    environment.ColumnStartedEvent
                        .Metadata,
                    environment.Column);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        differentReference));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithNonCombatStartedParent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false,
                    useCombatStartedParent: false);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        environment.ColumnStartedEvent));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithoutPlayerFrontCard_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        environment.ColumnStartedEvent);

            Assert.That(
                exchangeEvent,
                Is.Null);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithoutEnemyFrontCard_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    enemyFrontOccupied: false);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        environment.ColumnStartedEvent);

            Assert.That(
                exchangeEvent,
                Is.Null);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchange_WithLivingFrontCards_AppliesDamageAndLogsHierarchy()
        {
            var environment =
                CreateEnvironment();

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        environment.ColumnStartedEvent);

            Assert.That(
                exchangeEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(exchangeEvent));

            Assert.That(
                exchangeEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                exchangeEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ColumnStartedEvent
                        .Metadata.EventId));

            Assert.That(
                exchangeEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            var damageToEnemyEvent =
                environment.EventLog.Events[3]
                    as DamageAppliedCombatEvent;

            var damageToPlayerEvent =
                environment.EventLog.Events[4]
                    as DamageAppliedCombatEvent;

            Assert.That(
                damageToEnemyEvent,
                Is.Not.Null);

            Assert.That(
                damageToPlayerEvent,
                Is.Not.Null);

            Assert.That(
                damageToEnemyEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    exchangeEvent.Metadata.EventId));

            Assert.That(
                damageToPlayerEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    exchangeEvent.Metadata.EventId));

            Assert.That(
                damageToEnemyEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.EnemyCard.InstanceId));

            Assert.That(
                damageToPlayerEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void TryResolveExchange_WithMutualLethalDamage_LogsDamagesBeforeDeaths()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 3,
                    playerAttack: 3,
                    enemyAttack: 4);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchange(
                        environment.State,
                        environment.ColumnStartedEvent);

            Assert.That(
                exchangeEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            var damageToEnemyEvent =
                environment.EventLog.Events[3]
                    as DamageAppliedCombatEvent;

            var damageToPlayerEvent =
                environment.EventLog.Events[4]
                    as DamageAppliedCombatEvent;

            var playerDeathEvent =
                environment.EventLog.Events[5]
                    as DeathCombatEvent;

            var enemyDeathEvent =
                environment.EventLog.Events[6]
                    as DeathCombatEvent;

            Assert.That(
                damageToEnemyEvent,
                Is.Not.Null);

            Assert.That(
                damageToPlayerEvent,
                Is.Not.Null);

            Assert.That(
                playerDeathEvent,
                Is.Not.Null);

            Assert.That(
                enemyDeathEvent,
                Is.Not.Null);

            Assert.That(
                playerDeathEvent.InstanceId,
                Is.EqualTo(
                    environment.PlayerCard.InstanceId));

            Assert.That(
                enemyDeathEvent.InstanceId,
                Is.EqualTo(
                    environment.EnemyCard.InstanceId));

            Assert.That(
                playerDeathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    damageToPlayerEvent
                        .Metadata.EventId));

            Assert.That(
                enemyDeathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    damageToEnemyEvent
                        .Metadata.EventId));

            Assert.That(
                playerDeathEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                enemyDeathEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerFrontOccupied = true,
                bool enemyFrontOccupied = true,
                int playerCurrentHp = 10,
                int enemyCurrentHp = 10,
                int playerAttack = 3,
                int enemyAttack = 4,
                bool useCombatStartedParent = true)
        {
            var column =
                new BoardColumn(1);

            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column);

            var playerCard =
                playerFrontOccupied
                    ? CreateCard(
                        "card.player",
                        100,
                        playerCurrentHp,
                        playerAttack)
                    : null;

            var enemyCard =
                enemyFrontOccupied
                    ? CreateCard(
                        "card.enemy",
                        200,
                        enemyCurrentHp,
                        enemyAttack)
                    : null;

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new SlotId(1),
                        playerPosition,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(2),
                        enemyPosition,
                        enemyCard));

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            CombatEvent parentEvent;
            CombatStartedCombatEvent
                combatStartedEvent = null;

            if (useCombatStartedParent)
            {
                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadataFactory.CreateRoot());

                parentEvent =
                    combatStartedEvent;
            }
            else
            {
                parentEvent =
                    new TestCombatEvent(
                        metadataFactory.CreateRoot());
            }

            eventLog.Append(
                parentEvent);

            var columnStartedEvent =
                new ColumnStartedCombatEvent(
                    metadataFactory.CreateChild(
                        parentEvent.Metadata),
                    column);

            eventLog.Append(
                columnStartedEvent);

            return new TestEnvironment
            {
                State = state,
                Column = column,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                InitialPlayerHp = playerCurrentHp,
                InitialEnemyHp = enemyCurrentHp,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                ParentEvent = parentEvent,
                CombatStartedEvent =
                    combatStartedEvent,
                ColumnStartedEvent =
                    columnStartedEvent,
                Resolver =
                    new CombatColumnNormalAttackResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position,
            CombatCardState card)
        {
            CombatSlotState slot;
            CombatCardRegistry cards;

            if (card == null)
            {
                slot =
                    new CombatSlotState(
                        slotId,
                        position);

                cards =
                    new CombatCardRegistry(
                        new CombatCardState[0]);
            }
            else
            {
                slot =
                    new CombatSlotState(
                        slotId,
                        position,
                        card.InstanceId);

                cards =
                    new CombatCardRegistry(
                        new[] { card });
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[] { slot }),
                cards,
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private static void AssertEnvironmentUnchanged(
            TestEnvironment environment)
        {
            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            var enemySide =
                environment.State.GetSide(
                    CombatSide.Enemy);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.EqualTo(
                    environment.PlayerCard != null));

            Assert.That(
                enemySide.Board.GetSlot(
                        environment.EnemyPosition)
                    .IsOccupied,
                Is.EqualTo(
                    environment.EnemyCard != null));

            if (environment.PlayerCard != null)
            {
                Assert.That(
                    environment.PlayerCard.CurrentHp,
                    Is.EqualTo(
                        environment.InitialPlayerHp));

                Assert.That(
                    playerSide.GetCardAt(
                        environment.PlayerPosition),
                    Is.SameAs(
                        environment.PlayerCard));
            }

            if (environment.EnemyCard != null)
            {
                Assert.That(
                    environment.EnemyCard.CurrentHp,
                    Is.EqualTo(
                        environment.InitialEnemyHp));

                Assert.That(
                    enemySide.GetCardAt(
                        environment.EnemyPosition),
                    Is.SameAs(
                        environment.EnemyCard));
            }
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public BoardColumn Column { get; set; }

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

            public int InitialPlayerHp { get; set; }

            public int InitialEnemyHp { get; set; }

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

            public CombatEvent ParentEvent
            {
                get;
                set;
            }

            public CombatStartedCombatEvent
                CombatStartedEvent
            {
                get;
                set;
            }

            public ColumnStartedCombatEvent
                ColumnStartedEvent
            {
                get;
                set;
            }

            public CombatColumnNormalAttackResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}