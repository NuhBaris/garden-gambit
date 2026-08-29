using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnExchangeLoopResolverTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnExchangeLoopResolver(
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
                    new CombatColumnExchangeLoopResolver(
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
                    new CombatColumnExchangeLoopResolver(
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
                    new CombatColumnExchangeLoopResolver(
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
                    new CombatColumnExchangeLoopResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        null));
        }

        [Test]
        public void ResolveAvailableExchanges_WithNullColumnEvent_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .ResolveAvailableExchanges(
                        null,
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveAvailableExchanges_WithInvalidBudgets_ThrowsBeforeStartingExchange()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        0,
                        4,
                        16,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        2,
                        0,
                        16,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        0,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        16,
                        0));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveAvailableExchanges_WithPendingResolution_ThrowsBeforeStartingExchange()
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
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveAvailableExchanges_WithOnlyPlayerBackCard_ReturnsZeroWithoutAdvancingIt()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    playerBackOccupied: true);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        4,
                        8,
                        32,
                        4);

            Assert.That(
                exchangeCount,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .GetCardAt(
                        environment.PlayerBackPosition),
                Is.SameAs(
                    environment.PlayerBackCard));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveAvailableExchanges_WithOneSidedLethalExchange_ReturnsOneAtExactBudget()
        {
            var environment =
                CreateEnvironment(
                    playerFrontHp: 10,
                    enemyFrontHp: 5,
                    playerFrontAttack: 5,
                    enemyFrontAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        1,
                        8,
                        32,
                        4);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Board.GetSlot(
                        environment.EnemyFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(7));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResolveAvailableExchanges_AfterMutualDeath_AdvancesBackCardsAndResolvesSecondExchange()
        {
            var environment =
                CreateEnvironment(
                    playerBackOccupied: true,
                    enemyBackOccupied: true,
                    playerFrontHp: 1,
                    playerBackHp: 1,
                    enemyFrontHp: 1,
                    enemyBackHp: 1,
                    playerFrontAttack: 1,
                    playerBackAttack: 1,
                    enemyFrontAttack: 1,
                    enemyBackAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        2,
                        8,
                        64,
                        8);

            Assert.That(
                exchangeCount,
                Is.EqualTo(2));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(2));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(4));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(4));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(4));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.CardAdvanced),
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(18));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(4));

            var playerSide =
                environment.State.GetSide(
                    CombatSide.Player);

            var enemySide =
                environment.State.GetSide(
                    CombatSide.Enemy);

            Assert.That(
                playerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                enemySide.Cards.Count,
                Is.Zero);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                playerSide.Board.GetSlot(
                        environment.PlayerBackPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                enemySide.Board.GetSlot(
                        environment.EnemyFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                enemySide.Board.GetSlot(
                        environment.EnemyBackPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(18));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResolveAvailableExchanges_WhenExchangeBudgetIsExhausted_ThrowsWithCompletedChainAndNoPendingWork()
        {
            var environment =
                CreateEnvironment(
                    playerFrontHp: 10,
                    enemyFrontHp: 10,
                    playerFrontAttack: 0,
                    enemyFrontAttack: 0);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveAvailableExchanges(
                        environment.ColumnStartedEvent,
                        1,
                        8,
                        32,
                        4));

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyFrontCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            Assert.That(
                CountEventsOfKind(
                    environment.EventLog,
                    CombatEventKind.DamageApplied),
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.Zero);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerFrontOccupied = true,
                bool playerBackOccupied = false,
                bool enemyFrontOccupied = true,
                bool enemyBackOccupied = false,
                int playerFrontHp = 10,
                int playerBackHp = 5,
                int enemyFrontHp = 10,
                int enemyBackHp = 5,
                int playerFrontAttack = 3,
                int playerBackAttack = 1,
                int enemyFrontAttack = 4,
                int enemyBackAttack = 1,
                bool drainInitialEvents = true)
        {
            var column =
                new BoardColumn(1);

            var playerFrontPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var playerBackPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column);

            var enemyFrontPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column);

            var enemyBackPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    column);

            var playerFrontCard =
                playerFrontOccupied
                    ? CreateCard(
                        "card.player.front",
                        100,
                        playerFrontHp,
                        playerFrontAttack)
                    : null;

            var playerBackCard =
                playerBackOccupied
                    ? CreateCard(
                        "card.player.back",
                        101,
                        playerBackHp,
                        playerBackAttack)
                    : null;

            var enemyFrontCard =
                enemyFrontOccupied
                    ? CreateCard(
                        "card.enemy.front",
                        200,
                        enemyFrontHp,
                        enemyFrontAttack)
                    : null;

            var enemyBackCard =
                enemyBackOccupied
                    ? CreateCard(
                        "card.enemy.back",
                        201,
                        enemyBackHp,
                        enemyBackAttack)
                    : null;

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        playerFrontPosition,
                        playerBackPosition,
                        new SlotId(1),
                        new SlotId(2),
                        playerFrontCard,
                        playerBackCard),
                    CreateSide(
                        CombatSide.Enemy,
                        enemyFrontPosition,
                        enemyBackPosition,
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
                new CombatColumnExchangeLoopResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            if (drainInitialEvents)
            {
                resolver.CompletePendingResolution(
                    4,
                    16,
                    4);
            }

            return new TestEnvironment
            {
                State = state,
                PlayerFrontCard =
                    playerFrontCard,
                PlayerBackCard =
                    playerBackCard,
                EnemyFrontCard =
                    enemyFrontCard,
                EnemyBackCard =
                    enemyBackCard,
                PlayerFrontPosition =
                    playerFrontPosition,
                PlayerBackPosition =
                    playerBackPosition,
                EnemyFrontPosition =
                    enemyFrontPosition,
                EnemyBackPosition =
                    enemyBackPosition,
                InitialPlayerFrontHp =
                    playerFrontHp,
                InitialPlayerBackHp =
                    playerBackHp,
                InitialEnemyFrontHp =
                    enemyFrontHp,
                InitialEnemyBackHp =
                    enemyBackHp,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry =
                    sourceRegistry,
                ColumnStartedEvent =
                    columnStartedEvent,
                InitialProcessedEventCount =
                    eventQueue.ProcessedCount,
                Resolver = resolver
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            BoardPosition frontPosition,
            BoardPosition backPosition,
            SlotId frontSlotId,
            SlotId backSlotId,
            CombatCardState frontCard,
            CombatCardState backCard)
        {
            var cards =
                new List<CombatCardState>();

            if (frontCard != null)
            {
                cards.Add(frontCard);
            }

            if (backCard != null)
            {
                cards.Add(backCard);
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        CreateSlot(
                            frontSlotId,
                            frontPosition,
                            frontCard),
                        CreateSlot(
                            backSlotId,
                            backPosition,
                            backCard)
                    }),
                new CombatCardRegistry(cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSlotState CreateSlot(
            SlotId slotId,
            BoardPosition position,
            CombatCardState card)
        {
            if (card == null)
            {
                return new CombatSlotState(
                    slotId,
                    position);
            }

            return new CombatSlotState(
                slotId,
                position,
                card.InstanceId);
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

        private static int CountEventsOfKind(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            var count = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertInitialStateUnchanged(
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

            AssertCardUnchanged(
                environment.State.GetSide(
                    CombatSide.Player),
                environment.PlayerFrontPosition,
                environment.PlayerFrontCard,
                environment.InitialPlayerFrontHp);

            AssertCardUnchanged(
                environment.State.GetSide(
                    CombatSide.Player),
                environment.PlayerBackPosition,
                environment.PlayerBackCard,
                environment.InitialPlayerBackHp);

            AssertCardUnchanged(
                environment.State.GetSide(
                    CombatSide.Enemy),
                environment.EnemyFrontPosition,
                environment.EnemyFrontCard,
                environment.InitialEnemyFrontHp);

            AssertCardUnchanged(
                environment.State.GetSide(
                    CombatSide.Enemy),
                environment.EnemyBackPosition,
                environment.EnemyBackCard,
                environment.InitialEnemyBackHp);
        }

        private static void AssertCardUnchanged(
            CombatSideState side,
            BoardPosition position,
            CombatCardState card,
            int initialHp)
        {
            var slot =
                side.Board.GetSlot(position);

            Assert.That(
                slot.IsOccupied,
                Is.EqualTo(card != null));

            if (card == null)
            {
                return;
            }

            Assert.That(
                card.CurrentHp,
                Is.EqualTo(initialHp));

            Assert.That(
                side.GetCardAt(position),
                Is.SameAs(card));
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public CombatCardState PlayerFrontCard
            {
                get;
                set;
            }

            public CombatCardState PlayerBackCard
            {
                get;
                set;
            }

            public CombatCardState EnemyFrontCard
            {
                get;
                set;
            }

            public CombatCardState EnemyBackCard
            {
                get;
                set;
            }

            public BoardPosition PlayerFrontPosition
            {
                get;
                set;
            }

            public BoardPosition PlayerBackPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyFrontPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyBackPosition
            {
                get;
                set;
            }

            public int InitialPlayerFrontHp { get; set; }

            public int InitialPlayerBackHp { get; set; }

            public int InitialEnemyFrontHp { get; set; }

            public int InitialEnemyBackHp { get; set; }

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

            public CombatColumnExchangeLoopResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}