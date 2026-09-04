using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnRunnerStagedTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnRunner(
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
                environment.Runner
                    .SourceDamageModifierRegistry,
                Is.SameAs(modifierRegistry));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveNormalAttackBatch,
                Is.Null);

            Assert.That(
                environment.Runner
                    .ActiveNormalAttackStage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Unspecified));
        }

        [Test]
        public void
            StartAndResolveColumnStaged_WithLethalExchange_CompletesAndClearsColumn()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 3,
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            var exchangeCount =
                environment.Runner
                    .StartAndResolveColumnStaged(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
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
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(1));

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
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveColumnStaged_AppliesSourceModifier()
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
                environment.Runner
                    .StartAndResolveColumnStaged(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
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
                modifierRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartAndResolveColumnStaged_WhenTriggerBudgetExhausts_RetainsStagedColumn()
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
                () => environment.Runner
                    .StartAndResolveColumnStaged(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        10,
                        10,
                        10,
                        1));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveNormalAttackExecution,
                Is.True);

            Assert.That(
                environment.Runner
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
            ResumeActiveColumn_WithStagedColumn_ThrowsWithoutChangingActiveState()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            var activeColumnEvent =
                environment.Runner
                    .ActiveColumnEvent;

            var activeBatch =
                environment.Runner
                    .ActiveNormalAttackBatch;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveColumn(
                        10,
                        20,
                        20,
                        20));

            Assert.That(
                environment.Runner
                    .ActiveColumnEvent,
                Is.SameAs(activeColumnEvent));

            Assert.That(
                environment.Runner
                    .ActiveNormalAttackBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.True);
        }

        [Test]
        public void
            ResumeActiveColumnStaged_AfterBudgetFailure_CompletesWithoutRepeatingColumnOrExchange()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            var activeColumnEvent =
                environment.Runner
                    .ActiveColumnEvent;

            var activeBatch =
                environment.Runner
                    .ActiveNormalAttackBatch;

            var resumedExchangeCount =
                environment.Runner
                    .ResumeActiveColumnStaged(
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                resumedExchangeCount,
                Is.Zero);

            Assert.That(
                environment.FirstHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.SecondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(activeColumnEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    activeBatch.ExchangeEvent));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveColumnStaged_WithLegacyColumn_ThrowsWithoutChangingMode()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 1,
                    enemyCurrentHp: 10,
                    enemyAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveColumn(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        maximumExchangeCount: 10,
                        maximumPassCountPerExchange: 1,
                        maximumEventCountPerPass: 1,
                        maximumTriggerCountPerEvent: 10));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.False);

            var activeColumnEvent =
                environment.Runner
                    .ActiveColumnEvent;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveColumnStaged(
                        10,
                        20,
                        20,
                        20));

            Assert.That(
                environment.Runner
                    .ActiveColumnEvent,
                Is.SameAs(activeColumnEvent));

            Assert.That(
                environment.Runner
                    .ActiveColumnUsesStagedNormalAttack,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveColumnStaged_WithActiveColumn_ThrowsWithoutStartingSecondColumn()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            var activeColumnEvent =
                environment.Runner
                    .ActiveColumnEvent;

            var columnEventCount =
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveColumnStaged(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        10,
                        20,
                        20,
                        20));

            Assert.That(
                environment.Runner
                    .ActiveColumnEvent,
                Is.SameAs(activeColumnEvent));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(
                    columnEventCount));
        }

        [Test]
        public void
            CompletePendingResolution_ThenResumeStagedColumn_ClearsColumnWithoutRepeatingExchange()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            var activeBatch =
                environment.Runner
                    .ActiveNormalAttackBatch;

            var processedEventCount =
                environment.Runner
                    .CompletePendingResolution(
                        20,
                        20,
                        20);

            Assert.That(
                processedEventCount,
                Is.GreaterThan(0));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);

            var resumedExchangeCount =
                environment.Runner
                    .ResumeActiveColumnStaged(
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                resumedExchangeCount,
                Is.Zero);

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
                environment.FirstHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.SecondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);
        }

        private static TestEnvironment
            CreateStagedBudgetFailureEnvironment()
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
                    playerCurrentHp: 10,
                    playerAttack: 1,
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            environment.FirstHandler =
                firstHandler;

            environment.SecondHandler =
                secondHandler;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveColumnStaged(
                        environment.CombatStartedEvent,
                        new BoardColumn(1),
                        10,
                        10,
                        10,
                        1));

            return environment;
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
                        new SlotId(1),
                        new SlotId(2),
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(3),
                        new SlotId(4),
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

            var runner =
                new CombatColumnRunner(
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
                Runner = runner,
                CombatStartedEvent =
                    combatStartedEvent
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId frontSlotId,
            SlotId backSlotId,
            CombatCardState card)
        {
            var column =
                new BoardColumn(1);

            var frontSlot =
                new CombatSlotState(
                    frontSlotId,
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        column),
                    card.InstanceId);

            var backSlot =
                new CombatSlotState(
                    backSlotId,
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        column));

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

            public CombatColumnRunner Runner
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

            public AddPlayerDamageModifierHandler
                FirstHandler
            {
                get;
                set;
            }

            public AddPlayerDamageModifierHandler
                SecondHandler
            {
                get;
                set;
            }
        }
    }
}