using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnExchangeLoopStagedTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeLoopResolver(
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
            ResolveAvailableStagedExchanges_WithInvalidBudgets_ThrowsWithoutCreatingEvents()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        0,
                        10,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        0,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        0,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        0));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void
            ResolveAvailableStagedExchanges_WithoutBothFrontlines_ReturnsZero()
        {
            var environment =
                CreateEnvironment(
                    includePlayerFront: true,
                    includeEnemyFront: false);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        10);

            Assert.That(
                exchangeCount,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResolveAvailableStagedExchanges_WhenFirstExchangeEndsCombat_ReturnsOne()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 3,
                    enemyFrontCurrentHp: 3,
                    enemyFrontAttack: 4);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResolveAvailableStagedExchanges_WithEnemyBackCard_ResolvesTwoSequentialExchanges()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 3,
                    enemyFrontCurrentHp: 3,
                    enemyFrontAttack: 1,
                    includeEnemyBack: true,
                    enemyBackCurrentHp: 3,
                    enemyBackAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(2));

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(4));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(4));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CardAdvanced),
                Is.EqualTo(1));
        }

        [Test]
        public void
            ResolveAvailableStagedExchanges_SourceModifierCanEndExchangeLoop()
        {
            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var handler =
                new AddPlayerDamageModifierHandler(
                    modifierRegistry,
                    1);

            var environment =
                CreateEnvironment(
                    modifierRegistry,
                    new[]
                    {
                        CreateTriggerSource(
                            handler,
                            0)
                    },
                    playerAttack: 2,
                    enemyFrontCurrentHp: 3,
                    enemyFrontAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));
        }

        [Test]
        public void
            ResolveAvailableStagedExchanges_WhenTriggerBudgetExhausts_RetainsActiveExecution()
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
                    },
                    playerAttack: 1,
                    enemyFrontCurrentHp: 3,
                    enemyFrontAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        maximumExchangeCount: 10,
                        maximumPassCountPerExchange: 10,
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
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyFrontCard.CurrentHp,
                Is.EqualTo(3));
        }

        [Test]
        public void
            ResolveAvailableStagedExchanges_WithPendingExecution_ThrowsWithoutCreatingSecondExchange()
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
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        1));

            var activeBatch =
                environment.Resolver
                    .ActiveNormalAttackBatch;

            var exchangeCountBeforeRetry =
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        10));

            Assert.That(
                environment.Resolver
                    .ActiveNormalAttackBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(
                    exchangeCountBeforeRetry));
        }

        [Test]
        public void
            CompletePendingResolution_ResumesLoopAttackWithoutRepeatingModifier()
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
                    },
                    playerAttack: 1,
                    enemyFrontCurrentHp: 3,
                    enemyFrontAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        1));

            var activeBatch =
                environment.Resolver
                    .ActiveNormalAttackBatch;

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
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            var remainingExchangeCount =
                environment.Resolver
                    .ResolveAvailableStagedExchanges(
                        environment.ColumnStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                remainingExchangeCount,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));
        }

        private static int CountEvents(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind != kind)
                {
                    continue;
                }

                count =
                    checked(
                        count + 1);
            }

            return count;
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
                bool includePlayerFront = true,
                bool includeEnemyFront = true,
                int playerCurrentHp = 10,
                int playerAttack = 3,
                int enemyFrontCurrentHp = 3,
                int enemyFrontAttack = 1,
                bool includeEnemyBack = false,
                int enemyBackCurrentHp = 3,
                int enemyBackAttack = 1)
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

            CombatCardState playerFrontCard = null;
            CombatCardState enemyFrontCard = null;
            CombatCardState enemyBackCard = null;

            if (includePlayerFront)
            {
                playerFrontCard =
                    CreateCard(
                        "card.player.front",
                        100,
                        playerCurrentHp,
                        playerAttack);
            }

            if (includeEnemyFront)
            {
                enemyFrontCard =
                    CreateCard(
                        "card.enemy.front",
                        200,
                        enemyFrontCurrentHp,
                        enemyFrontAttack);
            }

            if (includeEnemyBack)
            {
                enemyBackCard =
                    CreateCard(
                        "card.enemy.back",
                        201,
                        enemyBackCurrentHp,
                        enemyBackAttack);
            }

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new SlotId(1),
                        new SlotId(2),
                        playerFrontCard,
                        null),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(3),
                        new SlotId(4),
                        enemyFrontCard,
                        enemyBackCard));

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
                new CombatColumnExchangeLoopResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    modifierRegistry);

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
                        new BoardColumn(1));

            eventResolutionEngine.Drain(
                20,
                20,
                20);

            return new TestEnvironment
            {
                State = state,
                PlayerFrontCard =
                    playerFrontCard,
                EnemyFrontCard =
                    enemyFrontCard,
                MetadataFactory =
                    metadataFactory,
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
            CombatCardState frontCard,
            CombatCardState backCard)
        {
            var frontPosition =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1));

            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    new BoardColumn(1));

            CombatSlotState frontSlot;
            CombatSlotState backSlot;

            var cards =
                new List<CombatCardState>();

            if (frontCard == null)
            {
                frontSlot =
                    new CombatSlotState(
                        frontSlotId,
                        frontPosition);
            }
            else
            {
                frontSlot =
                    new CombatSlotState(
                        frontSlotId,
                        frontPosition,
                        frontCard.InstanceId);

                cards.Add(
                    frontCard);
            }

            if (backCard == null)
            {
                backSlot =
                    new CombatSlotState(
                        backSlotId,
                        backPosition);
            }
            else
            {
                backSlot =
                    new CombatSlotState(
                        backSlotId,
                        backPosition,
                        backCard.InstanceId);

                cards.Add(
                    backCard);
            }

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

            public CombatCardState PlayerFrontCard
            {
                get;
                set;
            }

            public CombatCardState EnemyFrontCard
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

            public CombatColumnExchangeLoopResolver
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