using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        NormalAttackExchangeColumnTests
    {
        [Test]
        public void ResolveInColumn_WithNullState_ThrowsWithoutChangingLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        null,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithNullColumnEvent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        environment.State,
                        null,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithUnloggedColumnEvent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var unloggedColumnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment
                                .CombatStartedEvent
                                .Metadata),
                    environment.Column);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        environment.State,
                        unloggedColumnEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithDifferentLoggedReference_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var differentReference =
                new ColumnStartedCombatEvent(
                    environment.ColumnStartedEvent
                        .Metadata,
                    environment.ColumnStartedEvent
                        .Column);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        environment.State,
                        differentReference,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithNonCombatStartedParent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    useCombatStartedParent: false);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithPlayerPositionFromDifferentColumn_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var wrongPlayerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        wrongPlayerPosition,
                        environment.EnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithEnemyPositionFromDifferentColumn_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            var wrongEnemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(2));

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        wrongEnemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void ResolveInColumn_WithValidColumn_AppliesDamageAndLogsHierarchy()
        {
            var environment =
                CreateEnvironment();

            var exchangeEvent =
                environment.Resolver.ResolveInColumn(
                    environment.State,
                    environment.ColumnStartedEvent,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(exchangeEvent));

            Assert.That(
                exchangeEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .NormalAttackExchange));

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

            Assert.That(
                exchangeEvent.PlayerAttack,
                Is.EqualTo(3));

            Assert.That(
                exchangeEvent.EnemyAttack,
                Is.EqualTo(4));

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
                damageToEnemyEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                damageToPlayerEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

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
        public void ResolveInColumn_WithMutualLethalDamage_LogsBothDamagesBeforeDeaths()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 3,
                    playerAttack: 3,
                    enemyAttack: 4);

            var exchangeEvent =
                environment.Resolver.ResolveInColumn(
                    environment.State,
                    environment.ColumnStartedEvent,
                    environment.PlayerPosition,
                    environment.EnemyPosition);

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
                    exchangeEvent.Metadata
                        .TriggerRootId));

            Assert.That(
                enemyDeathEvent.Metadata
                    .TriggerRootId,
                Is.EqualTo(
                    exchangeEvent.Metadata
                        .TriggerRootId));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);
        }

        private static TestEnvironment
            CreateEnvironment(
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
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                Column = column,
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
                    new NormalAttackExchangeResolver(
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

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    environment.InitialPlayerHp));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    environment.InitialEnemyHp));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .GetCardAt(
                        environment.PlayerPosition),
                Is.SameAs(
                    environment.PlayerCard));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .GetCardAt(
                        environment.EnemyPosition),
                Is.SameAs(
                    environment.EnemyCard));
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

            public BoardColumn Column { get; set; }

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

            public NormalAttackExchangeResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}