using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnExchangeCycleStagedTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeCycleResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventResolutionEngine,
                        null));
        }

        [Test]
        public void
            Constructor_WithInjectedRegistry_ExposesRegistryAndStartsInactive()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var environment =
                CreateEnvironment(
                    modifierRegistry);

            Assert.That(
                environment.Resolver
                    .SourceDamageModifierRegistry,
                Is.SameAs(modifierRegistry));

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackExecutionState,
                Is.Null);

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackBatch,
                Is.Null);

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Unspecified));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            TryResolveStagedExchange_WithNullColumnEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        null,
                        10,
                        10,
                        10));
        }

        [Test]
        public void
            TryResolveStagedExchange_WithInvalidBudgets_ThrowsWithoutCreatingEvents()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        0,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        10,
                        0,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        0));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);
        }

        [Test]
        public void
            TryResolveStagedExchange_WithoutBothFrontlines_ReturnsNull()
        {
            var environment =
                CreateEnvironment(
                    includePlayerCard: true,
                    includeEnemyCard: false);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10);

            Assert.That(
                exchangeEvent,
                Is.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            TryResolveStagedExchange_WithBothFrontlines_AppendsSemanticEventOrder()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    enemyCurrentHp: 10,
                    playerAttack: 3,
                    enemyAttack: 4);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10);

            Assert.That(
                exchangeEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Events[0].Kind,
                Is.EqualTo(
                    CombatEventKind.CombatStarted));

            Assert.That(
                environment.EventLog.Events[1].Kind,
                Is.EqualTo(
                    CombatEventKind.ColumnStarted));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(exchangeEvent));

            Assert.That(
                environment.EventLog.Events[3].Kind,
                Is.EqualTo(
                    CombatEventKind.NormalAttack));

            Assert.That(
                environment.EventLog.Events[4].Kind,
                Is.EqualTo(
                    CombatEventKind.NormalAttack));

            Assert.That(
                environment.EventLog.Events[5].Kind,
                Is.EqualTo(
                    CombatEventKind.DamageApplied));

            Assert.That(
                environment.EventLog.Events[6].Kind,
                Is.EqualTo(
                    CombatEventKind.DamageApplied));

            var playerAttackEvent =
                environment.EventLog.Events[3]
                    as NormalAttackCombatEvent;

            var enemyAttackEvent =
                environment.EventLog.Events[4]
                    as NormalAttackCombatEvent;

            Assert.That(
                playerAttackEvent,
                Is.Not.Null);

            Assert.That(
                enemyAttackEvent,
                Is.Not.Null);

            Assert.That(
                playerAttackEvent.IsPlayerAttack,
                Is.True);

            Assert.That(
                enemyAttackEvent.IsEnemyAttack,
                Is.True);

            Assert.That(
                playerAttackEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    exchangeEvent.Metadata.EventId));

            Assert.That(
                enemyAttackEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    exchangeEvent.Metadata.EventId));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            TryResolveStagedExchange_AppliesModifierCreatedDuringTriggerDrain()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddPlayerDamageModifierHandler(
                    modifierRegistry,
                    2);

            var source =
                CreateTriggerSource(
                    handler,
                    0);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[] { source });

            environment.Resolver
                .TryResolveStagedExchangeAndCompleteChain(
                    environment.ColumnStartedEvent,
                    10,
                    10,
                    10);

            var playerAttackEvent =
                environment.EventLog.Events[3]
                    as NormalAttackCombatEvent;

            Assert.That(
                handler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                playerAttackEvent,
                Is.Not.Null);

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    playerAttackEvent
                        .Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void
            TryResolveStagedExchange_WhenTriggerBudgetExhausts_RetainsPreparedExecution()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var firstHandler =
                new AddPlayerDamageModifierHandler(
                    modifierRegistry,
                    1);

            var secondHandler =
                new AddPlayerDamageModifierHandler(
                    modifierRegistry,
                    1);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[]
                    {
                        CreateTriggerSource(
                            firstHandler,
                            0),
                        CreateTriggerSource(
                            secondHandler,
                            1)
                    });

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 10,
                        maximumTriggerCountPerEvent: 1));

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.True);

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            Assert.That(
                firstHandler.ResolveCount +
                secondHandler.ResolveCount,
                Is.EqualTo(1));

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
            CompletePendingResolution_ResumesStagedExecutionWithoutRepeatingExchangeOrModifier()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var firstHandler =
                new AddPlayerDamageModifierHandler(
                    modifierRegistry,
                    1);

            var secondHandler =
                new AddPlayerDamageModifierHandler(
                    modifierRegistry,
                    1);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[]
                    {
                        CreateTriggerSource(
                            firstHandler,
                            0),
                        CreateTriggerSource(
                            secondHandler,
                            1)
                    });

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        1));

            var activeBatch =
                environment.Resolver
                    .ActiveNormalAttackBatch;

            var processedEventCount =
                environment.Resolver
                    .CompletePendingResolution(
                        10,
                        10,
                        10);

            Assert.That(
                processedEventCount,
                Is.GreaterThan(0));

            Assert.That(
                firstHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                secondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                modifierRegistry.GetTotalModifier(
                    activeBatch.PlayerAttackEvent
                        .Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    activeBatch.ExchangeEvent));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            LegacyExchangePath_WithActiveStagedExecution_ThrowsWithoutCreatingSecondExchange()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        1,
                        1,
                        10));

            var activeBatch =
                environment.Resolver
                    .ActiveNormalAttackBatch;

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10));

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void
            CompletePendingResolution_AfterFinalDrainFailure_DoesNotApplyDamageTwice()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 3,
                    playerAttack: 3,
                    enemyAttack: 4);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveStagedExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        maximumPassCount: 1,
                        maximumEventCountPerPass: 3,
                        maximumTriggerCountPerEvent: 10));

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .DamageApplied));

            var damageApplication =
                environment.Resolver
                    .ActiveNormalAttackExecutionState
                    .DamageApplication;

            Assert.That(
                damageApplication.DidBothDie,
                Is.True);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

            var processedEventCount =
                environment.Resolver
                    .CompletePendingResolution(
                        20,
                        20,
                        20);

            Assert.That(
                processedEventCount,
                Is.GreaterThan(0));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            CompletePendingResolution_WithoutPendingWork_ReturnsZero()
        {
            var environment =
                CreateEnvironment();

            var processedEventCount =
                environment.Resolver
                    .CompletePendingResolution(
                        10,
                        10,
                        10);

            Assert.That(
                processedEventCount,
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        private static CombatTriggerHandlerSource
            CreateTriggerSource(
                ICombatTriggerHandler handler,
                int horizontalOrder)
        {
            return new CombatTriggerHandlerSource(
                new FixedCombatTriggerOrderKeyProvider(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Card,
                        CombatSide.Player,
                        horizontalOrder,
                        0)),
                handler);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry = null,
                ICombatTriggerSource[] sources = null,
                bool includePlayerCard = true,
                bool includeEnemyCard = true,
                int playerCurrentHp = 10,
                int enemyCurrentHp = 10,
                int playerAttack = 3,
                int enemyAttack = 4)
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
                    sources);

            var eventResolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var resolver =
                new CombatColumnExchangeCycleResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    modifierRegistry);

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
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventResolutionEngine =
                    eventResolutionEngine,
                Resolver = resolver,
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

        private sealed class
            AddPlayerDamageModifierHandler :
            CombatEventTriggerHandler<
                NormalAttackCombatEvent>
        {
            private readonly
                CombatNormalAttackSourceDamageModifierRegistry
                _modifierRegistry;

            private readonly int
                _damageDelta;

            public AddPlayerDamageModifierHandler(
                CombatNormalAttackSourceDamageModifierRegistry
                    modifierRegistry,
                int damageDelta)
            {
                _modifierRegistry =
                    modifierRegistry;

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
                return sourceEvent.IsPlayerAttack;
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

            public CombatColumnExchangeCycleResolver
                Resolver
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