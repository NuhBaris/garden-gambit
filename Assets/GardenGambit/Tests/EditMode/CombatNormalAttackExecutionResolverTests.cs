using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackExecutionResolverTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackExecutionResolver(
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
                    new CombatNormalAttackExecutionResolver(
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
                    new CombatNormalAttackExecutionResolver(
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
                    new CombatNormalAttackExecutionResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void Continue_WithNullExecutionState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Continue(
                    null,
                    10,
                    10,
                    10,
                    attackEvent =>
                        attackEvent.BaseDamage));
        }

        [Test]
        public void
            Continue_WithInvalidMaximumPassCount_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    0,
                    10,
                    10,
                    attackEvent =>
                        attackEvent.BaseDamage));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));
        }

        [Test]
        public void
            Continue_WithInvalidMaximumEventCount_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    0,
                    10,
                    attackEvent =>
                        attackEvent.BaseDamage));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));
        }

        [Test]
        public void
            Continue_WithInvalidMaximumTriggerCount_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    0,
                    attackEvent =>
                        attackEvent.BaseDamage));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));
        }

        [Test]
        public void Continue_WithNullDamageResolver_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    10,
                    null));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));
        }

        [Test]
        public void
            Continue_FromPrepared_ResolvesTriggersBeforeApplyingDamage()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10,
                    playerAttack: 3,
                    enemyAttack: 4);

            var callbackOrder =
                new List<CombatSide>();

            var application =
                environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    10,
                    attackEvent =>
                    {
                        Assert.That(
                            environment.EventQueue
                                .HasPending,
                            Is.False);

                        callbackOrder.Add(
                            attackEvent.AttackerSide);

                        return attackEvent.IsPlayerAttack
                            ? 6
                            : 2;
                    });

            Assert.That(
                callbackOrder,
                Is.EqualTo(
                    new[]
                    {
                        CombatSide.Player,
                        CombatSide.Enemy
                    }));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Completed));

            Assert.That(
                environment.ExecutionState
                    .IsCompleted,
                Is.True);

            Assert.That(
                environment.ExecutionState
                    .DamageApplication,
                Is.SameAs(application));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            Continue_WhenAlreadyCompleted_ReturnsExistingApplicationWithoutRepeatingWork()
        {
            var environment =
                CreateEnvironment();

            var firstApplication =
                environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    10,
                    attackEvent =>
                        attackEvent.BaseDamage);

            var playerHpAfterCompletion =
                environment.PlayerCard.CurrentHp;

            var enemyHpAfterCompletion =
                environment.EnemyCard.CurrentHp;

            var eventCountAfterCompletion =
                environment.EventLog.Count;

            var callbackCount = 0;

            var secondApplication =
                environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    10,
                    attackEvent =>
                    {
                        callbackCount++;

                        return attackEvent.BaseDamage;
                    });

            Assert.That(
                secondApplication,
                Is.SameAs(firstApplication));

            Assert.That(
                callbackCount,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(
                    playerHpAfterCompletion));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(
                    enemyHpAfterCompletion));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCountAfterCompletion));

            Assert.That(
                environment.ExecutionState
                    .IsCompleted,
                Is.True);
        }

        [Test]
        public void
            Continue_WhenDamageCalculationThrows_RetryDoesNotRepeatAttackTriggerDrain()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    10,
                    attackEvent =>
                    {
                        throw new InvalidOperationException(
                            "Test damage calculation failure.");
                    }));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));

            Assert.That(
                environment.ExecutionState
                    .HasDamageApplication,
                Is.False);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);

            var retryCallbackCount = 0;

            var application =
                environment.Resolver.Continue(
                    environment.ExecutionState,
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
                application,
                Is.Not.Null);

            Assert.That(
                environment.ExecutionState
                    .IsCompleted,
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
        }

        [Test]
        public void
            Continue_WhenAttackTriggerDrainExhaustsBudget_RetryContinuesFromPrepared()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10);

            var callbackCount = 0;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    maximumPassCount: 1,
                    maximumEventCountPerPass: 1,
                    maximumTriggerCountPerEvent: 10,
                    resolveDamage:
                        attackEvent =>
                        {
                            callbackCount++;

                            return attackEvent.BaseDamage;
                        }));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                environment.ExecutionState
                    .HasDamageApplication,
                Is.False);

            Assert.That(
                callbackCount,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventResolutionEngine
                    .HasPendingWork,
                Is.True);

            var application =
                environment.Resolver.Continue(
                    environment.ExecutionState,
                    10,
                    10,
                    10,
                    attackEvent =>
                    {
                        callbackCount++;

                        return attackEvent.BaseDamage;
                    });

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                callbackCount,
                Is.EqualTo(2));

            Assert.That(
                environment.ExecutionState
                    .IsCompleted,
                Is.True);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));
        }

        [Test]
        public void
            Continue_WhenFinalDeathChainDrainExhaustsBudget_RetryDoesNotRepeatDamage()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 3,
                    playerAttack: 3,
                    enemyAttack: 4);

            var callbackCount = 0;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.Continue(
                    environment.ExecutionState,
                    maximumPassCount: 1,
                    maximumEventCountPerPass: 3,
                    maximumTriggerCountPerEvent: 10,
                    resolveDamage:
                        attackEvent =>
                        {
                            callbackCount++;

                            return attackEvent.BaseDamage;
                        }));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .DamageApplied));

            Assert.That(
                environment.ExecutionState
                    .HasDamageApplication,
                Is.True);

            Assert.That(
                environment.ExecutionState
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

            var damageApplication =
                environment.ExecutionState
                    .DamageApplication;

            environment.Resolver.Continue(
                environment.ExecutionState,
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
                environment.ExecutionState
                    .IsCompleted,
                Is.True);

            Assert.That(
                environment.ExecutionState
                    .DamageApplication,
                Is.SameAs(damageApplication));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

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
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Front);

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

            var preparationResolver =
                new
                    CombatNormalAttackPreparationResolver(
                        metadataFactory,
                        eventLog);

            var batch =
                preparationResolver.Prepare(
                    state,
                    playerPosition,
                    enemyPosition);

            var executionState =
                new CombatNormalAttackExecutionState(
                    batch);

            var resolver =
                new CombatNormalAttackExecutionResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                EventResolutionEngine =
                    eventResolutionEngine,
                ExecutionState = executionState,
                Resolver = resolver
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

        private static BoardPosition CreatePosition(
            CombatSide side,
            BoardRow row)
        {
            return new BoardPosition(
                side,
                row,
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

            public CombatEventQueue EventQueue
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

            public CombatNormalAttackExecutionState
                ExecutionState
            {
                get;
                set;
            }

            public CombatNormalAttackExecutionResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}