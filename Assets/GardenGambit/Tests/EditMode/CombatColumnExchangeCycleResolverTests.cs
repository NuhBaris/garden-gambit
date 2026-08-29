using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnExchangeCycleResolverTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeCycleResolver(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeCycleResolver(
                        environment.State,
                        null,
                        environment.EventLog,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeCycleResolver(
                        environment.State,
                        environment.MetadataFactory,
                        null,
                        environment.EventQueue,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeCycleResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null,
                        environment.SourceRegistry));
        }

        [Test]
        public void Constructor_WithNullSourceRegistry_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeCycleResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        null));
        }

        [Test]
        public void TryResolveExchangeAndCompleteChain_WithInvalidBudgets_ThrowsBeforeApplyingDamage()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        0,
                        8,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        4,
                        0,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        4,
                        8,
                        0));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchangeAndCompleteChain_WithPendingWork_ThrowsBeforeStartingExchange()
        {
            var environment =
                CreateEnvironment(
                    drainInitialEvents: false);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        4,
                        8,
                        1));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchangeAndCompleteChain_WithoutPlayerFront_ReturnsNullWithoutAddingWork()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        4,
                        8,
                        1);

            Assert.That(
                exchangeEvent,
                Is.Null);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryResolveExchangeAndCompleteChain_WithNonlethalExchange_DrainsAllGeneratedEvents()
        {
            var environment =
                CreateEnvironment();

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        8,
                        32,
                        8);

            Assert.That(
                exchangeEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(5));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(exchangeEvent));

            Assert.That(
                environment.EventLog.Events[3],
                Is.TypeOf<
                    DamageAppliedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[4],
                Is.TypeOf<
                    DamageAppliedCombatEvent>());
        }

        [Test]
        public void TryResolveExchangeAndCompleteChain_WithMutualDeath_RemovesBothCardsAndDrainsRemovalEvents()
        {
            var environment =
                CreateEnvironment(
                    playerCurrentHp: 4,
                    enemyCurrentHp: 3,
                    playerAttack: 3,
                    enemyAttack: 4);

            var exchangeEvent =
                environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        8,
                        64,
                        8);

            Assert.That(
                exchangeEvent,
                Is.Not.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(9));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(9));

            Assert.That(
                environment.EventLog.Events[5],
                Is.TypeOf<DeathCombatEvent>());

            Assert.That(
                environment.EventLog.Events[6],
                Is.TypeOf<DeathCombatEvent>());

            Assert.That(
                environment.EventLog.Events[7],
                Is.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventLog.Events[8],
                Is.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Board.GetSlot(
                        environment.EnemyPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void TryResolveExchangeAndCompleteChain_WhenBudgetIsExhausted_PreservesWorkForExplicitRetry()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        1,
                        1,
                        1));

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
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryResolveExchangeAndCompleteChain(
                        environment.ColumnStartedEvent,
                        4,
                        8,
                        1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(7));

            var processedOnRetry =
                environment.Resolver
                    .CompletePendingResolution(
                        4,
                        8,
                        1);

            Assert.That(
                processedOnRetry,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(5));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void CompletePendingResolution_WithNoWork_ReturnsZero()
        {
            var environment =
                CreateEnvironment();

            var processedEventCount =
                environment.Resolver
                    .CompletePendingResolution(
                        4,
                        8,
                        1);

            Assert.That(
                processedEventCount,
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            AssertEnvironmentUnchanged(
                environment);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerFrontOccupied = true,
                bool enemyFrontOccupied = true,
                int playerCurrentHp = 10,
                int enemyCurrentHp = 10,
                int playerAttack = 3,
                int enemyAttack = 4,
                bool drainInitialEvents = true)
        {
            var column =
                new BoardColumn(1);

            var playerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var playerBackPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column);

            var enemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column);

            var enemyBackPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    column);

            var playerCard =
                playerFrontOccupied
                    ? CreateCard(
                        "card.player",
                        100,
                        playerCurrentHp,
                        playerAttack)
                    : null;

            var enemyCard =
                enemyFrontOccupied
                    ? CreateCard(
                        "card.enemy",
                        200,
                        enemyCurrentHp,
                        enemyAttack)
                    : null;

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new SlotId(1),
                        new SlotId(2),
                        playerPosition,
                        playerBackPosition,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(3),
                        new SlotId(4),
                        enemyPosition,
                        enemyBackPosition,
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

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                combatStartedEvent);

            var columnStartedEvent =
                new ColumnStartedCombatEvent(
                    metadataFactory.CreateChild(
                        combatStartedEvent.Metadata),
                    column);

            eventLog.Append(
                columnStartedEvent);

            var resolver =
                new CombatColumnExchangeCycleResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            if (drainInitialEvents)
            {
                resolver.CompletePendingResolution(
                    4,
                    8,
                    1);
            }

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                InitialPlayerHp = playerCurrentHp,
                InitialEnemyHp = enemyCurrentHp,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry = sourceRegistry,
                ColumnStartedEvent =
                    columnStartedEvent,
                InitialProcessedEventCount =
                    eventQueue.ProcessedCount,
                Resolver = resolver
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId frontSlotId,
            SlotId backSlotId,
            BoardPosition frontPosition,
            BoardPosition backPosition,
            CombatCardState frontCard)
        {
            CombatSlotState frontSlot;
            CombatCardRegistry cards;

            if (frontCard == null)
            {
                frontSlot =
                    new CombatSlotState(
                        frontSlotId,
                        frontPosition);

                cards =
                    new CombatCardRegistry(
                        new CombatCardState[0]);
            }
            else
            {
                frontSlot =
                    new CombatSlotState(
                        frontSlotId,
                        frontPosition,
                        frontCard.InstanceId);

                cards =
                    new CombatCardRegistry(
                        new[] { frontCard });
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        frontSlot,
                        new CombatSlotState(
                            backSlotId,
                            backPosition)
                    }),
                cards,
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

        private static void AssertEnvironmentUnchanged(
            TestEnvironment environment)
        {
            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(
                    environment
                        .InitialProcessedEventCount));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            var enemySide =
                environment.State.GetSide(
                    CombatSide.Enemy);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.PlayerPosition)
                    .IsOccupied,
                Is.EqualTo(
                    environment.PlayerCard != null));

            Assert.That(
                enemySide.Board.GetSlot(
                        environment.EnemyPosition)
                    .IsOccupied,
                Is.EqualTo(
                    environment.EnemyCard != null));

            if (environment.PlayerCard != null)
            {
                Assert.That(
                    environment.PlayerCard.CurrentHp,
                    Is.EqualTo(
                        environment.InitialPlayerHp));

                Assert.That(
                    playerSide.GetCardAt(
                        environment.PlayerPosition),
                    Is.SameAs(
                        environment.PlayerCard));
            }

            if (environment.EnemyCard != null)
            {
                Assert.That(
                    environment.EnemyCard.CurrentHp,
                    Is.EqualTo(
                        environment.InitialEnemyHp));

                Assert.That(
                    enemySide.GetCardAt(
                        environment.EnemyPosition),
                    Is.SameAs(
                        environment.EnemyCard));
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

            public int InitialPlayerHp { get; set; }

            public int InitialEnemyHp { get; set; }

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

            public CombatTriggerSourceRegistry
                SourceRegistry
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

            public int InitialProcessedEventCount
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
        }
    }
}