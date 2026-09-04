using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackPreparationColumnTests
    {
        [Test]
        public void PrepareInColumn_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .PrepareInColumn(
                        null,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void PrepareInColumn_WithNullColumnEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .PrepareInColumn(
                        environment.State,
                        null,
                        environment.PlayerPosition,
                        environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void PrepareInColumn_WithUnloggedColumnEvent_Throws()
        {
            var sourceEnvironment =
                CreateEnvironment();

            var unrelatedMetadataFactory =
                CreateMetadataFactory();

            var unrelatedEventLog =
                new CombatEventLog();

            var resolver =
                new
                    CombatNormalAttackPreparationResolver(
                        unrelatedMetadataFactory,
                        unrelatedEventLog);

            Assert.Throws<ArgumentException>(
                () => resolver.PrepareInColumn(
                    sourceEnvironment.State,
                    sourceEnvironment
                        .ColumnStartedEvent,
                    sourceEnvironment.PlayerPosition,
                    sourceEnvironment.EnemyPosition));

            Assert.That(
                unrelatedEventLog.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void PrepareInColumn_WithWrongPlayerColumn_Throws()
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
                    .PrepareInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        wrongPlayerPosition,
                        environment.EnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void PrepareInColumn_WithWrongEnemyColumn_Throws()
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
                    .PrepareInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        wrongEnemyPosition));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void PrepareInColumn_CreatesExchangeChildOfColumn()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver
                    .PrepareInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition);

            Assert.That(
                batch.ExchangeEvent
                    .Metadata.HasParent,
                Is.True);

            Assert.That(
                batch.ExchangeEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.ColumnStartedEvent
                        .Metadata.EventId));

            Assert.That(
                batch.ExchangeEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                batch.ExchangeEvent
                    .Metadata.IsTriggerRoot,
                Is.False);
        }

        [Test]
        public void PrepareInColumn_CreatesAttacksAsExchangeChildren()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver
                    .PrepareInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition);

            Assert.That(
                batch.PlayerAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    batch.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                batch.EnemyAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    batch.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                batch.PlayerAttackEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                batch.EnemyAttackEvent
                    .Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));
        }

        [Test]
        public void PrepareInColumn_AppendsDeterministicEventOrder()
        {
            var environment =
                CreateEnvironment();

            var batch =
                environment.Resolver
                    .PrepareInColumn(
                        environment.State,
                        environment.ColumnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(
                    environment.CombatStartedEvent));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    environment.ColumnStartedEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    batch.ExchangeEvent));

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(
                    batch.PlayerAttackEvent));

            Assert.That(
                environment.EventLog.Events[4],
                Is.SameAs(
                    batch.EnemyAttackEvent));
        }

        [Test]
        public void PrepareInColumn_DoesNotApplyDamage()
        {
            var environment =
                CreateEnvironment();

            var previousPlayerHp =
                environment.PlayerCard.CurrentHp;

            var previousEnemyHp =
                environment.EnemyCard.CurrentHp;

            environment.Resolver.PrepareInColumn(
                environment.State,
                environment.ColumnStartedEvent,
                environment.PlayerPosition,
                environment.EnemyPosition);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    previousPlayerHp));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    previousEnemyHp));

            Assert.That(
                environment.PlayerCard
                    .IsAtDeathThreshold,
                Is.False);

            Assert.That(
                environment.EnemyCard
                    .IsAtDeathThreshold,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment()
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
                new CombatCardState(
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(1),
                    new CardRank(4),
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: 0,
                    attack: 5);

            var enemyCard =
                new CombatCardState(
                    new DefinitionId(
                        "enemy-card"),
                    new InstanceId(101),
                    new CardRank(6),
                    hpCapacity: 10,
                    currentHp: 10,
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
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog)
                    .Start(state);

            var columnStartedEvent =
                new CombatColumnStartResolver(
                    metadataFactory,
                    eventLog)
                    .StartColumn(
                        state,
                        combatStartedEvent,
                        column);

            return new TestEnvironment
            {
                State =
                    state,
                PlayerCard =
                    playerCard,
                EnemyCard =
                    enemyCard,
                PlayerPosition =
                    playerPosition,
                EnemyPosition =
                    enemyPosition,
                CombatStartedEvent =
                    combatStartedEvent,
                ColumnStartedEvent =
                    columnStartedEvent,
                EventLog =
                    eventLog,
                Resolver =
                    new
                        CombatNormalAttackPreparationResolver(
                            metadataFactory,
                            eventLog)
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatState State
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

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatNormalAttackPreparationResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}