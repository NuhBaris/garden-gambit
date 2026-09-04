using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnResolutionStagedTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnResolutionResolver(
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
                    .HasPendingResolution,
                Is.False);

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
        }

        [Test]
        public void
            ResolveStartedColumnStaged_WithNullColumnEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        null,
                        10,
                        10,
                        10,
                        10));
        }

        [Test]
        public void
            ResolveStartedColumnStaged_WithInvalidBudgets_ThrowsWithoutCreatingEvents()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        0,
                        10,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        0,
                        10,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        0,
                        10));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
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
            ResolveStartedColumnStaged_WithLethalExchange_ResolvesColumn()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 3,
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

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
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResolveStartedColumnStaged_AppliesSourceModifier()
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
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveStartedColumnStaged(
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
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.NormalAttack),
                Is.EqualTo(2));
        }

        [Test]
        public void
            ResolveStartedColumnStaged_WhenTriggerBudgetExhausts_RetainsActiveAttack()
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
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

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
                firstHandler.ResolveCount +
                secondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(3));
        }

        [Test]
        public void
            ResolveStartedColumnStaged_AfterTriggerBudgetFailure_CompletesPendingAttackWithoutCreatingDuplicate()
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
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        1));

            var activeBatch =
                environment.Resolver
                    .ActiveNormalAttackBatch;

            var resumedExchangeCount =
                environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                resumedExchangeCount,
                Is.Zero);

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    activeBatch.ExchangeEvent));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);
        }

        [Test]
        public void
            CompletePendingResolution_ResumesStagedAttackWithoutRepeatingModifier()
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
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
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
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));
        }

        [Test]
        public void
            ResolveStartedColumnStaged_WhenLaterColumnExists_ThrowsBeforeResolvingOlderColumn()
        {
            var environment =
                CreateEnvironment();

            var laterColumnEvent =
                new CombatColumnStartResolver(
                    environment.MetadataFactory,
                    environment.EventLog)
                    .StartColumn(
                        environment.State,
                        environment.CombatStartedEvent,
                        new BoardColumn(2));

            Assert.That(
                laterColumnEvent,
                Is.Not.Null);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveStartedColumnStaged(
                        environment.ColumnStartedEvent,
                        10,
                        10,
                        10,
                        10));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasActiveNormalAttackExecution,
                Is.False);
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
                int playerCurrentHp = 10,
                int playerAttack = 3,
                int enemyCurrentHp = 3,
                int enemyAttack = 1)
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
                        playerCard,
                        new SlotId(1),
                        new SlotId(2),
                        new SlotId(3),
                        new SlotId(4)),
                    CreateSide(
                        CombatSide.Enemy,
                        enemyCard,
                        new SlotId(5),
                        new SlotId(6),
                        new SlotId(7),
                        new SlotId(8)));

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
                new CombatColumnResolutionResolver(
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
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                EventResolutionEngine =
                    eventResolutionEngine,
                Resolver = resolver,
                CombatStartedEvent =
                    combatStartedEvent,
                ColumnStartedEvent =
                    columnStartedEvent
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState frontCard,
            SlotId columnOneFrontSlotId,
            SlotId columnOneBackSlotId,
            SlotId columnTwoFrontSlotId,
            SlotId columnTwoBackSlotId)
        {
            var columnOne =
                new BoardColumn(1);

            var columnTwo =
                new BoardColumn(2);

            var slots =
                new[]
                {
                    new CombatSlotState(
                        columnOneFrontSlotId,
                        new BoardPosition(
                            side,
                            BoardRow.Front,
                            columnOne),
                        frontCard.InstanceId),

                    new CombatSlotState(
                        columnOneBackSlotId,
                        new BoardPosition(
                            side,
                            BoardRow.Back,
                            columnOne)),

                    new CombatSlotState(
                        columnTwoFrontSlotId,
                        new BoardPosition(
                            side,
                            BoardRow.Front,
                            columnTwo)),

                    new CombatSlotState(
                        columnTwoBackSlotId,
                        new BoardPosition(
                            side,
                            BoardRow.Back,
                            columnTwo))
                };

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    new[] { frontCard }),
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

            public CombatColumnResolutionResolver
                Resolver
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
        }
    }
}