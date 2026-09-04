using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackExecutionResolverTargetReductionTests
    {
        [Test]
        public void Constructor_WithNullTargetResolver_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackExecutionResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventResolutionEngine,
                        null));
        }

        [Test]
        public void Constructor_ExposesExactTargetResolver()
        {
            var environment =
                CreateEnvironment();

            Assert.That(
                environment.Resolver
                    .TargetDamageReductionResolver,
                Is.SameAs(
                    environment.TargetResolver));
        }

        [Test]
        public void
            Continue_WithTargetRequest_ReducesDamageBeforeApplication()
        {
            var environment =
                CreateEnvironment();

            RegisterPlayerAttackTargetReduction(
                environment);

            var application =
                environment.Resolver.Continue(
                    environment.ExecutionState,
                    maximumPassCount: 10,
                    maximumEventCountPerPass: 10,
                    maximumTriggerCountPerEvent: 10,
                    resolveDamage:
                        attackEvent =>
                            attackEvent.BaseDamage);

            Assert.That(
                application,
                Is.Not.Null);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1001),
                        environment.EnemyCard
                            .InstanceId),
                Is.True);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.ExecutionState
                    .IsCompleted,
                Is.True);
        }

        [Test]
        public void
            Continue_WithZeroSourceDamage_DoesNotConsumeReductionUsage()
        {
            var environment =
                CreateEnvironment();

            RegisterPlayerAttackTargetReduction(
                environment);

            environment.Resolver.Continue(
                environment.ExecutionState,
                maximumPassCount: 10,
                maximumEventCountPerPass: 10,
                maximumTriggerCountPerEvent: 10,
                resolveDamage:
                    attackEvent =>
                        attackEvent.IsPlayerAttack
                            ? 0
                            : attackEvent.BaseDamage);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        new InstanceId(1001),
                        environment.EnemyCard
                            .InstanceId),
                Is.False);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.ExecutionState
                    .IsCompleted,
                Is.True);
        }

        private static void
            RegisterPlayerAttackTargetReduction(
                TestEnvironment environment)
        {
            var playerAttackEvent =
                environment.ExecutionState
                    .Batch.PlayerAttackEvent;

            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        playerAttackEvent.Metadata.EventId,
                        new InstanceId(1001),
                        playerAttackEvent.TargetInstanceId,
                        reductionAmount: 1);

            var wasRegistered =
                environment.ReductionRegistry
                    .TryRegister(
                        request);

            Assert.That(
                wasRegistered,
                Is.True);
        }

        private static TestEnvironment
            CreateEnvironment()
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
                    instanceId: 100,
                    attack: 3);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    instanceId: 200,
                    attack: 4);

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
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    Array.Empty<
                        ICombatTriggerSource>());

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

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        new
                            CombatPetCardTriggerUsageRegistry());

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var targetResolver =
                new
                    CombatNormalAttackTargetDamageReductionResolver(
                        reductionRegistry,
                        usageCommitter);

            var resolver =
                new CombatNormalAttackExecutionResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    targetResolver);

            return new TestEnvironment
            {
                State =
                    state,

                PlayerCard =
                    playerCard,

                EnemyCard =
                    enemyCard,

                MetadataFactory =
                    metadataFactory,

                EventLog =
                    eventLog,

                EventResolutionEngine =
                    eventResolutionEngine,

                ExecutionState =
                    executionState,

                UsageCommitter =
                    usageCommitter,

                ReductionRegistry =
                    reductionRegistry,

                TargetResolver =
                    targetResolver,

                Resolver =
                    resolver
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

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition,
                            card.InstanceId),

                        new CombatSlotState(
                            backSlotId,
                            backPosition)
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

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: attack);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
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

            public CombatNormalAttackExecutionState
                ExecutionState
            {
                get;
                set;
            }

            public CombatPetCardTriggerUsageCommitter
                UsageCommitter
            {
                get;
                set;
            }

            public
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry
            {
                get;
                set;
            }

            public
                CombatNormalAttackTargetDamageReductionResolver
                TargetResolver
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