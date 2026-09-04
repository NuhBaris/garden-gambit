using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnNormalAttackRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnNormalAttackRunner(
                        null,
                        environment.NormalAttackRunner));
        }

        [Test]
        public void
            Constructor_WithNullNormalAttackRunner_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnNormalAttackRunner(
                        environment.State,
                        null));
        }

        [Test]
        public void
            Constructor_StartsWithoutActiveExecution()
        {
            var environment =
                CreateEnvironment();

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.False);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveExecutionState,
                Is.Null);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveBatch,
                Is.Null);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Unspecified));
        }

        [Test]
        public void
            TryStartAndResolve_WithNullColumnEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        null,
                        10,
                        10,
                        10));
        }

        [Test]
        public void
            TryStartAndResolve_WithInvalidBudgets_ThrowsBeforeCreatingAttack()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        0,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        10,
                        0,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        0));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.False);
        }

        [Test]
        public void
            TryStartAndResolve_WithoutPlayerFrontline_ReturnsNullWithoutCreatingEvents()
        {
            var environment =
                CreateEnvironment(
                    includePlayerCard: false,
                    includeEnemyCard: true);

            var application =
                environment.ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10);

            Assert.That(
                application,
                Is.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.False);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            TryStartAndResolve_WithoutEnemyFrontline_ReturnsNullWithoutCreatingEvents()
        {
            var environment =
                CreateEnvironment(
                    includePlayerCard: true,
                    includeEnemyCard: false);

            var application =
                environment.ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10);

            Assert.That(
                application,
                Is.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.False);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            TryStartAndResolve_WithBothFrontlines_ResolvesNestedNormalAttack()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10,
                    playerAttack: 3,
                    enemyAttack: 4);

            var application =
                environment.ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10);

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                application.Batch.ExchangeEvent
                    .Metadata.HasParent,
                Is.True);

            Assert.That(
                application.Batch.ExchangeEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.ColumnStartedEvent
                        .Metadata.EventId));

            Assert.That(
                application.Batch.PlayerAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    application.Batch.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                application.Batch.EnemyAttackEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    application.Batch.ExchangeEvent
                        .Metadata.EventId));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.False);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveExecution_WithoutActiveExecution_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .ColumnAttackRunner
                    .ResumeActiveExecution(
                        10,
                        10,
                        10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void
            TryStartAndResolve_WhenBudgetExhausts_RetainsPreparedExecution()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        maximumPassCount: 1,
                        maximumEventCountPerPass: 1,
                        maximumTriggerCountPerEvent: 10));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.True);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveExecutionState,
                Is.Not.Null);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveBatch,
                Is.Not.Null);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));
        }

        [Test]
        public void
            TryStartAndResolve_WithActiveExecution_ThrowsWithoutCreatingSecondExchange()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        1,
                        1,
                        10));

            var activeBatch =
                environment.ColumnAttackRunner
                    .ActiveBatch;

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10));

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void
            ResumeActiveExecution_AfterBudgetExhaustion_CompletesWithoutRepeatingExchange()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        1,
                        1,
                        10));

            var activeBatch =
                environment.ColumnAttackRunner
                    .ActiveBatch;

            var application =
                environment.ColumnAttackRunner
                    .ResumeActiveExecution(
                        10,
                        10,
                        10);

            Assert.That(
                application.Batch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.False);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveBatch,
                Is.Null);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Unspecified));

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveExecution_WithInvalidBudgets_PreservesActiveExecution()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .ColumnAttackRunner
                    .TryStartAndResolve(
                        environment.ColumnStartedEvent,
                        1,
                        1,
                        10));

            var activeBatch =
                environment.ColumnAttackRunner
                    .ActiveBatch;

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment
                    .ColumnAttackRunner
                    .ResumeActiveExecution(
                        0,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment
                    .ColumnAttackRunner
                    .ResumeActiveExecution(
                        10,
                        0,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment
                    .ColumnAttackRunner
                    .ResumeActiveExecution(
                        10,
                        10,
                        0));

            Assert.That(
                environment.ColumnAttackRunner
                    .HasActiveExecution,
                Is.True);

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.ColumnAttackRunner
                    .ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includePlayerCard = true,
                bool includeEnemyCard = true,
                int playerCurrentHp = 10,
                int enemyCurrentHp = 10,
                int playerAttack = 3,
                int enemyAttack = 4)
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

            CombatCardState playerCard = null;
            CombatCardState enemyCard = null;

            if (includePlayerCard)
            {
                playerCard =
                    CreateCard(
                        "card.player",
                        100,
                        playerCurrentHp,
                        playerAttack);
            }

            if (includeEnemyCard)
            {
                enemyCard =
                    CreateCard(
                        "card.enemy",
                        200,
                        enemyCurrentHp,
                        enemyAttack);
            }

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new SlotId(1),
                        new SlotId(2),
                        playerPosition,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(3),
                        new SlotId(4),
                        enemyPosition,
                        enemyCard));

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[0]);

            var eventResolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var normalAttackRunner =
                new CombatNormalAttackRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);

            var columnAttackRunner =
                new CombatColumnNormalAttackRunner(
                    state,
                    normalAttackRunner);

            var combatStartResolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            var combatStartedEvent =
                combatStartResolver.Start(
                    state);

            var columnStartResolver =
                new CombatColumnStartResolver(
                    metadataFactory,
                    eventLog);

            var columnStartedEvent =
                columnStartResolver.StartColumn(
                    state,
                    combatStartedEvent,
                    new BoardColumn(1));

            eventResolutionEngine.Drain(
                10,
                10,
                10);

            return new TestEnvironment
            {
                State = state,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventResolutionEngine =
                    eventResolutionEngine,
                NormalAttackRunner =
                    normalAttackRunner,
                ColumnAttackRunner =
                    columnAttackRunner,
                ColumnStartedEvent =
                    columnStartedEvent
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId frontSlotId,
            SlotId backSlotId,
            BoardPosition frontPosition,
            CombatCardState card)
        {
            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    frontPosition.Column);

            CombatSlotState frontSlot;
            CombatCardState[] cards;

            if (card == null)
            {
                frontSlot =
                    new CombatSlotState(
                        frontSlotId,
                        frontPosition);

                cards =
                    new CombatCardState[0];
            }
            else
            {
                frontSlot =
                    new CombatSlotState(
                        frontSlotId,
                        frontPosition,
                        card.InstanceId);

                cards =
                    new[] { card };
            }

            var backSlot =
                new CombatSlotState(
                    backSlotId,
                    backPosition);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        frontSlot,
                        backSlot
                    }),
                new CombatCardRegistry(
                    cards),
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

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

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

            public CombatEventResolutionEngine
                EventResolutionEngine
            {
                get;
                set;
            }

            public CombatNormalAttackRunner
                NormalAttackRunner
            {
                get;
                set;
            }

            public CombatColumnNormalAttackRunner
                ColumnAttackRunner
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
        }
    }
}