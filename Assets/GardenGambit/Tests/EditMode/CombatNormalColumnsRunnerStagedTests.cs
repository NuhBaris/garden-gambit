using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalColumnsRunnerStagedTests
    {
        [Test]
        public void
            Constructor_WithNullSourceDamageModifierRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
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
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Null);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.Zero);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.Zero);
        }

        [Test]
        public void
            StartAndResolveAllColumnsStaged_WithEmptyBoards_CompletesFiveColumnsWithoutExchanges()
        {
            var environment =
                CreateEnvironment(
                    includePlayerCard: false,
                    includeEnemyCard: false);

            var exchangeCount =
                environment.Runner
                    .StartAndResolveAllColumnsStaged(
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.Zero);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.Zero);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            ResolveAllColumnsForStartedCombatStaged_UsesExistingCombatAndReturnsExchangeCount()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 3,
                    enemyCurrentHp: 3,
                    enemyAttack: 1);

            var combatStartedEvent =
                StartAndDrainCombat(
                    environment);

            var exchangeCount =
                environment.Runner
                    .ResolveAllColumnsForStartedCombatStaged(
                        combatStartedEvent,
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

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
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveAllColumnsStaged_AppliesSourceModifierAcrossSharedRegistry()
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
                    .StartAndResolveAllColumnsStaged(
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
                modifierRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));
        }

        [Test]
        public void
            StartAndResolveAllColumnsStaged_WhenTriggerBudgetExhausts_RetainsCombatColumnAndExchangeCount()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

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
                environment.Runner.NextColumnValue,
                Is.EqualTo(
                    BoardColumn.MinimumValue));

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
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
        }

        [Test]
        public void
            ResumeActiveCombat_WithStagedCombat_ThrowsWithoutChangingProgress()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            var activeCombatEvent =
                environment.Runner
                    .ActiveCombatStartedEvent;

            var activeColumnEvent =
                environment.Runner
                    .ActiveColumnEvent;

            var activeBatch =
                environment.Runner
                    .ActiveNormalAttackBatch;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCombat(
                        10,
                        20,
                        20,
                        20));

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.SameAs(activeCombatEvent));

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.SameAs(activeColumnEvent));

            Assert.That(
                environment.Runner
                    .ActiveNormalAttackBatch,
                Is.SameAs(activeBatch));

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.EqualTo(
                    BoardColumn.MinimumValue));
        }

        [Test]
        public void
            ResumeActiveCombatStaged_AfterBudgetFailure_CompletesRemainingColumnsWithoutRepeatingExchange()
        {
            var environment =
                CreateStagedBudgetFailureEnvironment();

            var activeBatch =
                environment.Runner
                    .ActiveNormalAttackBatch;

            var exchangeCount =
                environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.FirstHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.SecondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                FindFirstEvent(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.SameAs(
                    activeBatch.ExchangeEvent));

            Assert.That(
                environment.State.Enemy.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner.NextColumnValue,
                Is.Zero);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void
            CompletePendingResolution_ThenResumeStagedCombat_PreservesRetrySafeExchangeCount()
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
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveNormalAttackExecution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            var exchangeCount =
                environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        20,
                        20,
                        20);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                FindFirstEvent(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.SameAs(
                    activeBatch.ExchangeEvent));

            Assert.That(
                environment.FirstHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.SecondHandler.ResolveCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void
            ResumeActiveCombatStaged_WithLegacyCombat_ThrowsWithoutChangingMode()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 10,
                    playerAttack: 1,
                    enemyCurrentHp: 10,
                    enemyAttack: 1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllColumns(
                        maximumExchangeCountPerColumn: 1,
                        maximumPassCountPerExchange: 20,
                        maximumEventCountPerPass: 20,
                        maximumTriggerCountPerEvent: 20));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.False);

            var activeCombatEvent =
                environment.Runner
                    .ActiveCombatStartedEvent;

            var activeColumnEvent =
                environment.Runner
                    .ActiveColumnEvent;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCombatStaged(
                        10,
                        20,
                        20,
                        20));

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.SameAs(activeCombatEvent));

            Assert.That(
                environment.Runner.ActiveColumnEvent,
                Is.SameAs(activeColumnEvent));

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
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
                    .StartAndResolveAllColumnsStaged(
                        10,
                        10,
                        10,
                        1));

            return environment;
        }

        private static CombatStartedCombatEvent
            StartAndDrainCombat(
                TestEnvironment environment)
        {
            var combatStartedEvent =
                new CombatStartResolver(
                    environment.MetadataFactory,
                    environment.EventLog)
                    .Start(
                        environment.State);

            environment.EventResolutionEngine.Drain(
                20,
                20,
                20);

            return combatStartedEvent;
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

        private static CombatEvent FindFirstEvent(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind == kind)
                {
                    return eventLog.Events[index];
                }
            }

            return null;
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
                        1,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        101,
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
                new CombatNormalColumnsRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    modifierRegistry);

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
                Runner = runner
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            int firstSlotId,
            CombatCardState columnOneFrontCard)
        {
            var slots =
                new List<CombatSlotState>();

            var nextSlotId =
                firstSlotId;

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                     BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

                var frontPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        column);

                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        column);

                if (columnValue ==
                        BoardColumn.MinimumValue &&
                    columnOneFrontCard != null)
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition,
                            columnOneFrontCard
                                .InstanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition));
                }

                nextSlotId =
                    checked(
                        nextSlotId + 1);

                slots.Add(
                    new CombatSlotState(
                        new SlotId(
                            nextSlotId),
                        backPosition));

                nextSlotId =
                    checked(
                        nextSlotId + 1);
            }

            CombatCardState[] cards;

            if (columnOneFrontCard == null)
            {
                cards =
                    new CombatCardState[0];
            }
            else
            {
                cards =
                    new[]
                    {
                        columnOneFrontCard
                    };
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
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

            public CombatNormalColumnsRunner Runner
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