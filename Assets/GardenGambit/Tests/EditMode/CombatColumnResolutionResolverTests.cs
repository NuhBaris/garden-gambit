using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnResolutionResolverTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatColumnResolutionResolver(
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
                    new CombatColumnResolutionResolver(
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
                    new CombatColumnResolutionResolver(
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
                    new CombatColumnResolutionResolver(
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
                    new CombatColumnResolutionResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.EventQueue,
                        null));
        }

        [Test]
        public void ResolveStartedColumn_WithNullColumnEvent_ThrowsWithoutDrainingPendingWork()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        null,
                        2,
                        4,
                        16,
                        1));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WithInvalidBudgets_ThrowsWithoutDrainingPendingWork()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        0,
                        4,
                        16,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        2,
                        0,
                        16,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        0,
                        1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        16,
                        0));

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WithUnloggedColumnEvent_ThrowsBeforeDrainingPendingWork()
        {
            var environment =
                CreateEnvironment();

            var unloggedColumnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment.ParentEvent
                                .Metadata),
                    environment.Column);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        unloggedColumnEvent,
                        2,
                        4,
                        16,
                        1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WithDifferentLoggedReference_ThrowsBeforeDrainingPendingWork()
        {
            var environment =
                CreateEnvironment();

            var differentReference =
                new ColumnStartedCombatEvent(
                    environment.ColumnStartedEvent
                        .Metadata,
                    environment.Column);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        differentReference,
                        2,
                        4,
                        16,
                        1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WithNonCombatStartedParent_ThrowsBeforeDrainingPendingWork()
        {
            var environment =
                CreateEnvironment(
                    useCombatStartedParent: false);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        16,
                        1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            AssertInitialStateUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WhenLaterColumnWasLogged_ThrowsWithoutDrainingLaterEvent()
        {
            var environment =
                CreateEnvironment();

            environment.Resolver
                .CompletePendingResolution(
                    4,
                    16,
                    1);

            var laterColumnEvent =
                new ColumnStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateChild(
                            environment.CombatStartedEvent
                                .Metadata),
                    new BoardColumn(2));

            environment.EventLog.Append(
                laterColumnEvent);

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        16,
                        1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            AssertFrontCardsUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WithPendingLifecycleAndNoFrontline_DrainsEventsAndReturnsZero()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            var exchangeCount =
                environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        2,
                        4,
                        16,
                        1);

            Assert.That(
                exchangeCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            AssertFrontCardsUnchanged(
                environment);
        }

        [Test]
        public void ResolveStartedColumn_WithPendingLifecycleAndLethalExchange_DrainsAndResolvesColumn()
        {
            var environment =
                CreateEnvironment(
                    playerFrontHp: 10,
                    enemyFrontHp: 5,
                    playerFrontAttack: 5,
                    enemyFrontAttack: 1);

            var exchangeCount =
                environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        1,
                        8,
                        32,
                        4);

            Assert.That(
                exchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
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
                        environment.EnemyPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog
                    .CardTombstones.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResolveStartedColumn_WhenInitialDrainBudgetIsExhausted_AllowsRetryWithSameColumnEvent()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        1,
                        1,
                        1,
                        1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.True);

            AssertFrontCardsUnchanged(
                environment);

            var exchangeCount =
                environment.Resolver
                    .ResolveStartedColumn(
                        environment.ColumnStartedEvent,
                        1,
                        4,
                        8,
                        1);

            Assert.That(
                exchangeCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventQueue.ProcessedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            AssertFrontCardsUnchanged(
                environment);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerFrontOccupied = true,
                bool enemyFrontOccupied = true,
                int playerFrontHp = 10,
                int enemyFrontHp = 5,
                int playerFrontAttack = 5,
                int enemyFrontAttack = 1,
                bool useCombatStartedParent = true)
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
                        playerFrontHp,
                        playerFrontAttack)
                    : null;

            var enemyCard =
                enemyFrontOccupied
                    ? CreateCard(
                        "card.enemy",
                        200,
                        enemyFrontHp,
                        enemyFrontAttack)
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

            CombatEvent parentEvent;
            CombatStartedCombatEvent
                combatStartedEvent = null;

            if (useCombatStartedParent)
            {
                combatStartedEvent =
                    new CombatStartedCombatEvent(
                        metadataFactory.CreateRoot());

                parentEvent =
                    combatStartedEvent;
            }
            else
            {
                parentEvent =
                    new TestCombatEvent(
                        metadataFactory.CreateRoot());
            }

            eventLog.Append(
                parentEvent);

            var columnStartedEvent =
                new ColumnStartedCombatEvent(
                    metadataFactory.CreateChild(
                        parentEvent.Metadata),
                    column);

            eventLog.Append(
                columnStartedEvent);

            return new TestEnvironment
            {
                State = state,
                Column = column,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerPosition = playerPosition,
                EnemyPosition = enemyPosition,
                InitialPlayerHp = playerFrontHp,
                InitialEnemyHp = enemyFrontHp,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry = sourceRegistry,
                ParentEvent = parentEvent,
                CombatStartedEvent =
                    combatStartedEvent,
                ColumnStartedEvent =
                    columnStartedEvent,
                InitialProcessedEventCount =
                    eventQueue.ProcessedCount,
                Resolver =
                    new CombatColumnResolutionResolver(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
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

            AssertFrontCardsUnchanged(
                environment);
        }

        private static void AssertFrontCardsUnchanged(
            TestEnvironment environment)
        {
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

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public BoardColumn Column { get; set; }

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

            public CombatEvent ParentEvent
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

            public int InitialProcessedEventCount
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
        }
    }
}