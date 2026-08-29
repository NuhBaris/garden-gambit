using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatDeathChainFinalizerTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathChainFinalizer(
                        null,
                        eventLog,
                        new CombatEventQueue(eventLog)));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var queueLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathChainFinalizer(
                        CreateMetadataFactory(),
                        null,
                        new CombatEventQueue(queueLog)));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatDeathChainFinalizer(
                        CreateMetadataFactory(),
                        new CombatEventLog(),
                        null));
        }

        [Test]
        public void CompletePendingDeathChains_WithNullState_ThrowsWithoutScanning()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: true,
                    includeEnemyCard: false);

            Assert.Throws<ArgumentNullException>(
                () => environment.Finalizer
                    .CompletePendingDeathChains(null));

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.Zero);
        }

        [Test]
        public void CompletePendingDeathChains_WithPendingEvent_ThrowsWithoutChangingStateOrCursor()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: true,
                    includeEnemyCard: false);

            AppendDeathEvent(
                environment,
                environment.PlayerCard,
                environment.PlayerFrontPosition);

            Assert.Throws<InvalidOperationException>(
                () => environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State));

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.Zero);

            Assert.That(
                environment.Finalizer.UnscannedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void CompletePendingDeathChains_WithNonDeathEvent_AdvancesCursorWithoutCompletingChain()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: true,
                    includeEnemyCard: false);

            var combatEvent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(
                combatEvent);

            ConsumeAllPendingEvents(environment);

            var completedCount =
                environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State);

            Assert.That(
                completedCount,
                Is.Zero);

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Finalizer.UnscannedEventCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void CompletePendingDeathChains_WithDeath_RemovesCardAndLeavesGeneratedEventUnscanned()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: true,
                    includeEnemyCard: false);

            AppendDeathEvent(
                environment,
                environment.PlayerCard,
                environment.PlayerFrontPosition);

            ConsumeAllPendingEvents(environment);

            var completedCount =
                environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State);

            Assert.That(
                completedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Finalizer.UnscannedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.EventQueue.HasPending,
                Is.True);
        }

        [Test]
        public void CompletePendingDeathChains_AfterGeneratedEventIsConsumed_DoesNotRepeatDeath()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: true,
                    includeEnemyCard: false);

            AppendDeathEvent(
                environment,
                environment.PlayerCard,
                environment.PlayerFrontPosition);

            ConsumeAllPendingEvents(environment);

            Assert.That(
                environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State),
                Is.EqualTo(1));

            ConsumeAllPendingEvents(environment);

            var secondCompletedCount =
                environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State);

            Assert.That(
                secondCompletedCount,
                Is.Zero);

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Finalizer.UnscannedEventCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void CompletePendingDeathChains_WhenCompletionThrows_RetryStartsFromSameDeath()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: false,
                    includeEnemyCard: false);

            AppendDeathEvent(
                environment,
                environment.PlayerCard,
                environment.PlayerFrontPosition);

            ConsumeAllPendingEvents(environment);

            Assert.Throws<InvalidOperationException>(
                () => environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State));

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.Zero);

            Assert.That(
                environment.Finalizer.UnscannedEventCount,
                Is.EqualTo(1));

            environment.PlayerSide.PlaceCard(
                environment.PlayerFrontPosition,
                environment.PlayerCard.InstanceId);

            var completedCount =
                environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State);

            Assert.That(
                completedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);
        }

        [Test]
        public void CompletePendingDeathChains_WithMutualDeaths_CompletesBothInLogOrder()
        {
            var environment =
                CreateEnvironment(
                    placePlayerCard: true,
                    includeEnemyCard: true);

            AppendDeathEvent(
                environment,
                environment.PlayerCard,
                environment.PlayerFrontPosition);

            AppendDeathEvent(
                environment,
                environment.EnemyCard,
                environment.EnemyFrontPosition);

            ConsumeAllPendingEvents(environment);

            var completedCount =
                environment.Finalizer
                    .CompletePendingDeathChains(
                        environment.State);

            Assert.That(
                completedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Finalizer.ScannedEventCount,
                Is.EqualTo(2));

            Assert.That(
                environment.Finalizer.UnscannedEventCount,
                Is.EqualTo(2));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.EnemySide.Cards.Count,
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EnemySide.Board
                    .GetSlot(
                        environment.EnemyFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventLog.Events[3],
                Is.TypeOf<DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventQueue.PendingCount,
                Is.EqualTo(2));
        }

        private static TestEnvironment CreateEnvironment(
            bool placePlayerCard,
            bool includeEnemyCard)
        {
            var playerFrontPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerBackPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var enemyFrontPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var enemyBackPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    new BoardColumn(1));

            var playerCard =
                CreateCard(
                    "player-card",
                    100);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    200);

            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerFrontPosition,
                    playerBackPosition,
                    new SlotId(1),
                    new SlotId(2),
                    playerCard,
                    placePlayerCard);

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    enemyFrontPosition,
                    enemyBackPosition,
                    new SlotId(3),
                    new SlotId(4),
                    enemyCard,
                    includeEnemyCard);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(eventLog);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                EnemySide = enemySide,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                PlayerFrontPosition =
                    playerFrontPosition,
                EnemyFrontPosition =
                    enemyFrontPosition,
                MetadataFactory =
                    metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                Finalizer =
                    new CombatDeathChainFinalizer(
                        metadataFactory,
                        eventLog,
                        eventQueue)
            };
        }

        private static CombatSideState CreateSideState(
            CombatSide side,
            BoardPosition frontPosition,
            BoardPosition backPosition,
            SlotId frontSlotId,
            SlotId backSlotId,
            CombatCardState card,
            bool placeCard)
        {
            var frontSlot =
                new CombatSlotState(
                    frontSlotId,
                    frontPosition,
                    placeCard
                        ? card.InstanceId
                        : (InstanceId?)null);

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
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                0,
                0,
                3);
        }

        private static DeathCombatEvent AppendDeathEvent(
            TestEnvironment environment,
            CombatCardState card,
            BoardPosition position)
        {
            var deathEvent =
                new DeathCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot(),
                    card.InstanceId,
                    position,
                    3,
                    card.CurrentHp);

            environment.EventLog.Append(
                deathEvent);

            return deathEvent;
        }

        private static void ConsumeAllPendingEvents(
            TestEnvironment environment)
        {
            while (environment.EventQueue.HasPending)
            {
                environment.EventQueue.DequeueNext();
            }
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
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
            public CombatState State
            {
                get;
                set;
            }

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatSideState EnemySide
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

            public BoardPosition PlayerFrontPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyFrontPosition
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

            public CombatEventQueue EventQueue
            {
                get;
                set;
            }

            public CombatDeathChainFinalizer Finalizer
            {
                get;
                set;
            }
        }
    }
}