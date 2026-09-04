using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatNormalAttackRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackRunner(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventResolutionEngine));
        }

        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackRunner(
                        environment.State,
                        null,
                        environment.EventLog,
                        environment.EventResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackRunner(
                        environment.State,
                        environment.MetadataFactory,
                        null,
                        environment.EventResolutionEngine));
        }

        [Test]
        public void
            Constructor_WithNullEventResolutionEngine_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void
            Constructor_StartsWithoutActiveExecution()
        {
            var environment =
                CreateEnvironment();

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);

            Assert.That(
                environment.Runner.ActiveExecutionState,
                Is.Null);

            Assert.That(
                environment.Runner.ActiveBatch,
                Is.Null);

            Assert.That(
                environment.Runner.ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Unspecified));
        }

        [Test]
        public void
            StartAndResolve_WithInvalidRequest_ThrowsBeforePreparingAttack()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        0,
                        10,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        0,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        10,
                        0,
                        attackEvent =>
                            attackEvent.BaseDamage));

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        10,
                        10,
                        null));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);
        }

        [Test]
        public void
            StartAndResolve_CompletesStandaloneAttackAndClearsActiveExecution()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10,
                    playerAttack: 3,
                    enemyAttack: 4);

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    10,
                    10,
                    10,
                    attackEvent =>
                        attackEvent.BaseDamage);

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                application.Batch.ExchangeEvent
                    .Metadata.IsTriggerRoot,
                Is.True);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);

            Assert.That(
                environment.Runner.ActiveExecutionState,
                Is.Null);

            Assert.That(
                environment.Runner.ActiveBatch,
                Is.Null);

            Assert.That(
                environment.Runner.ActiveStage,
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
            StartAndResolveInColumn_CreatesExchangeAsColumnChild()
        {
            var environment =
                CreateEnvironment();

            var combatStartResolver =
                new CombatStartResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var combatStartedEvent =
                combatStartResolver.Start(
                    environment.State);

            var columnStartResolver =
                new CombatColumnStartResolver(
                    environment.MetadataFactory,
                    environment.EventLog);

            var columnStartedEvent =
                columnStartResolver.StartColumn(
                    environment.State,
                    combatStartedEvent,
                    new BoardColumn(1));

            environment.EventResolutionEngine.Drain(
                10,
                10,
                10);

            var application =
                environment.Runner
                    .StartAndResolveInColumn(
                        columnStartedEvent,
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        10,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage);

            Assert.That(
                application.Batch.ExchangeEvent
                    .Metadata.HasParent,
                Is.True);

            Assert.That(
                application.Batch.ExchangeEvent
                    .Metadata.ParentEventId.Value,
                Is.EqualTo(
                    columnStartedEvent
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
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.Runner.HasActiveExecution,
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
                () => environment.Runner
                    .ResumeActiveExecution(
                        10,
                        10,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);
        }

        [Test]
        public void
            StartAndResolve_WithPendingEventWork_ThrowsBeforePreparingAttack()
        {
            var environment =
                CreateEnvironment();

            var pendingEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                pendingEvent);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.True);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        10,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(pendingEvent));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);
        }

        [Test]
        public void
            StartAndResolve_WhenTriggerDrainExhaustsBudget_RetainsPreparedExecution()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        maximumPassCount: 1,
                        maximumEventCountPerPass: 1,
                        maximumTriggerCountPerEvent: 10,
                        resolveDamage:
                            attackEvent =>
                                attackEvent.BaseDamage));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveExecutionState,
                Is.Not.Null);

            Assert.That(
                environment.Runner.ActiveBatch,
                Is.Not.Null);

            Assert.That(
                environment.Runner.ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));
        }

        [Test]
        public void
            StartAndResolve_WithActiveExecution_ThrowsWithoutPreparingSecondExchange()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        1,
                        1,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            var activeBatch =
                environment.Runner.ActiveBatch;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        10,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void
            ResumeActiveExecution_AfterTriggerDrainBudgetExhaustion_CompletesWithoutRepeatingExchange()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        1,
                        1,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage));

            var activeBatch =
                environment.Runner.ActiveBatch;

            var application =
                environment.Runner
                    .ResumeActiveExecution(
                        10,
                        10,
                        10,
                        attackEvent =>
                            attackEvent.BaseDamage);

            Assert.That(
                application.Batch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);

            Assert.That(
                environment.Runner.ActiveBatch,
                Is.Null);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveExecution_AfterDamageCalculationFailure_DoesNotRepeatAttackEvents()
        {
            var environment =
                CreateEnvironment();

            var initialCallbackCount = 0;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        10,
                        10,
                        10,
                        attackEvent =>
                        {
                            initialCallbackCount++;

                            throw new InvalidOperationException(
                                "Test damage calculation failure.");
                        }));

            Assert.That(
                initialCallbackCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            var activeBatch =
                environment.Runner.ActiveBatch;

            var retryCallbackCount = 0;

            var application =
                environment.Runner
                    .ResumeActiveExecution(
                        10,
                        10,
                        10,
                        attackEvent =>
                        {
                            retryCallbackCount++;

                            return attackEvent.BaseDamage;
                        });

            Assert.That(
                retryCallbackCount,
                Is.EqualTo(2));

            Assert.That(
                application.Batch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveExecution_AfterFinalDrainBudgetExhaustion_DoesNotRepeatDamage()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 3,
                    playerAttack: 3,
                    enemyAttack: 4);

            var callbackCount = 0;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        maximumPassCount: 1,
                        maximumEventCountPerPass: 3,
                        maximumTriggerCountPerEvent: 10,
                        resolveDamage:
                            attackEvent =>
                            {
                                callbackCount++;

                                return attackEvent
                                    .BaseDamage;
                            }));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .DamageApplied));

            Assert.That(
                environment.Runner
                    .ActiveExecutionState
                    .DamageApplication
                    .DidBothDie,
                Is.True);

            Assert.That(
                callbackCount,
                Is.EqualTo(2));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

            var activeBatch =
                environment.Runner.ActiveBatch;

            var damageApplication =
                environment.Runner
                    .ActiveExecutionState
                    .DamageApplication;

            var completedApplication =
                environment.Runner
                    .ResumeActiveExecution(
                        20,
                        20,
                        20,
                        attackEvent =>
                        {
                            callbackCount++;

                            return attackEvent.BaseDamage;
                        });

            Assert.That(
                callbackCount,
                Is.EqualTo(2));

            Assert.That(
                completedApplication,
                Is.SameAs(damageApplication));

            Assert.That(
                completedApplication.Batch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
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

            var runner =
                new CombatNormalAttackRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);

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
                Runner = runner
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

            var frontSlot =
                new CombatSlotState(
                    frontSlotId,
                    frontPosition,
                    card.InstanceId);

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

            public CombatNormalAttackRunner Runner
            {
                get;
                set;
            }
        }
    }
}