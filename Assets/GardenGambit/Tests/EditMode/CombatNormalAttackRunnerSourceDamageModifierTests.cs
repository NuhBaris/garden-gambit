using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackRunnerSourceDamageModifierTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventResolutionEngine,
                        null));
        }

        [Test]
        public void
            Constructor_WithInjectedRegistry_ExposesSameRegistry()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var environment =
                CreateEnvironment(
                    modifierRegistry);

            Assert.That(
                environment.Runner
                    .SourceDamageModifierRegistry,
                Is.SameAs(modifierRegistry));
        }

        [Test]
        public void
            DefaultConstructor_CreatesRegistryAndUsesBaseDamage()
        {
            var environment =
                CreateEnvironment(
                    useDefaultRunnerConstructor: true);

            Assert.That(
                environment.Runner
                    .SourceDamageModifierRegistry,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .SourceDamageModifierRegistry.Count,
                Is.Zero);

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    10,
                    10,
                    10);

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.EqualTo(3));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void
            CallbacklessExecution_AppliesPlayerAttackModifierCreatedByTrigger()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    CombatSide.Player,
                    2);

            var source =
                CreateTriggerSource(
                    handler,
                    0);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[] { source });

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    10,
                    10,
                    10);

            Assert.That(
                handler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                modifierRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.EqualTo(5));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void
            CallbacklessExecution_AppliesEnemyAttackModifierCreatedByTrigger()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    CombatSide.Enemy,
                    3);

            var source =
                CreateTriggerSource(
                    handler,
                    0);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[] { source });

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    10,
                    10,
                    10);

            Assert.That(
                handler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.EqualTo(3));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToPlayer,
                Is.EqualTo(7));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(3));
        }

        [Test]
        public void
            CallbacklessExecution_KeepsPlayerAndEnemyModifiersIndependent()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    null,
                    1);

            var source =
                CreateTriggerSource(
                    handler,
                    0);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[] { source });

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    10,
                    10,
                    10);

            Assert.That(
                handler.ResolveCount,
                Is.EqualTo(2));

            Assert.That(
                modifierRegistry.Count,
                Is.EqualTo(2));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.EqualTo(4));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToPlayer,
                Is.EqualTo(5));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void
            CallbacklessExecution_ClampsNegativeModifiedDamageToZero()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    CombatSide.Player,
                    -10);

            var source =
                CreateTriggerSource(
                    handler,
                    0);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[] { source });

            var application =
                environment.Runner.StartAndResolve(
                    environment.PlayerPosition,
                    environment.EnemyPosition,
                    10,
                    10,
                    10);

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.Zero);

            Assert.That(
                application.Resolution
                    .ResolvedDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void
            CallbackExecution_UsesExplicitResolverInsteadOfRegistryResolver()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    CombatSide.Player,
                    5);

            var source =
                CreateTriggerSource(
                    handler,
                    0);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[] { source });

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
                handler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    application.Batch
                        .PlayerAttackEvent
                        .Metadata.EventId),
                Is.EqualTo(5));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.EqualTo(3));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));
        }

        [Test]
        public void
            ResumeAfterTriggerBudgetExhaustion_DoesNotRepeatAppliedModifier()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var firstHandler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    CombatSide.Player,
                    1);

            var secondHandler =
                new AddSourceDamageModifierHandler(
                    modifierRegistry,
                    CombatSide.Player,
                    1);

            var firstSource =
                CreateTriggerSource(
                    firstHandler,
                    0);

            var secondSource =
                CreateTriggerSource(
                    secondHandler,
                    1);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[]
                    {
                        firstSource,
                        secondSource
                    });

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolve(
                        environment.PlayerPosition,
                        environment.EnemyPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 10,
                        maximumTriggerCountPerEvent: 1));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                firstHandler.ResolveCount +
                secondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    environment.Runner
                        .ActiveBatch
                        .PlayerAttackEvent
                        .Metadata.EventId),
                Is.EqualTo(1));

            var activeBatch =
                environment.Runner.ActiveBatch;

            var application =
                environment.Runner
                    .ResumeActiveExecution(
                        10,
                        10,
                        10);

            Assert.That(
                application.Batch,
                Is.SameAs(activeBatch));

            Assert.That(
                firstHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                secondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    application.Batch
                        .PlayerAttackEvent
                        .Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                application.Resolution
                    .ResolvedDamageToEnemy,
                Is.EqualTo(5));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.Runner.HasActiveExecution,
                Is.False);
        }

        private static CombatTriggerHandlerSource
            CreateTriggerSource(
                ICombatTriggerHandler handler,
                int horizontalOrder)
        {
            var orderKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Card,
                    CombatSide.Player,
                    horizontalOrder,
                    0);

            return new CombatTriggerHandlerSource(
                new FixedCombatTriggerOrderKeyProvider(
                    orderKey),
                handler);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry = null,
                ICombatTriggerSource[] sources = null,
                bool useDefaultRunnerConstructor = false)
        {
            if (modifierRegistry == null)
            {
                modifierRegistry =
                    new
                        CombatNormalAttackSourceDamageModifierRegistry();
            }

            if (sources == null)
            {
                sources =
                    new ICombatTriggerSource[0];
            }

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
                    10,
                    3);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    200,
                    10,
                    4);

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
                    sources);

            var eventResolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            CombatNormalAttackRunner runner;

            if (useDefaultRunnerConstructor)
            {
                runner =
                    new CombatNormalAttackRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventResolutionEngine);
            }
            else
            {
                runner =
                    new CombatNormalAttackRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventResolutionEngine,
                        modifierRegistry);
            }

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

        private sealed class
            AddSourceDamageModifierHandler :
            CombatEventTriggerHandler<
                NormalAttackCombatEvent>
        {
            private readonly
                CombatNormalAttackSourceDamageModifierRegistry
                _modifierRegistry;

            private readonly CombatSide?
                _attackerSide;

            private readonly int
                _damageDelta;

            public AddSourceDamageModifierHandler(
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry,
                CombatSide? attackerSide,
                int damageDelta)
            {
                _modifierRegistry =
                    modifierRegistry;

                _attackerSide =
                    attackerSide;

                _damageDelta =
                    damageDelta;
            }

            public int ResolveCount
            {
                get;
                private set;
            }

            protected override bool CanTriggerTyped(
                CombatState state,
                NormalAttackCombatEvent
                    sourceEvent)
            {
                if (!_attackerSide.HasValue)
                {
                    return true;
                }

                return sourceEvent.AttackerSide ==
                       _attackerSide.Value;
            }

            protected override void ResolveTyped(
                CombatState state,
                NormalAttackCombatEvent
                    sourceEvent)
            {
                _modifierRegistry.AddModifier(
                    sourceEvent.Metadata.EventId,
                    _damageDelta);

                ResolveCount =
                    checked(
                        ResolveCount + 1);
            }
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