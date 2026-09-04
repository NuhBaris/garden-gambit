using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatResolutionRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        null,
                        metadataFactory,
                        eventLog,
                        new CombatEventQueue(eventLog),
                        CreateSourceRegistry()));
        }

        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        CreateEmptyState(),
                        null,
                        eventLog,
                        new CombatEventQueue(eventLog),
                        CreateSourceRegistry()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var queueEventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        CreateEmptyState(),
                        CreateMetadataFactory(),
                        null,
                        new CombatEventQueue(
                            queueEventLog),
                        CreateSourceRegistry()));
        }

        [Test]
        public void Constructor_WithNullEventQueue_Throws()
        {
            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        CreateEmptyState(),
                        CreateMetadataFactory(),
                        eventLog,
                        null,
                        CreateSourceRegistry()));
        }

        [Test]
        public void Constructor_WithNullSourceRegistry_Throws()
        {
            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        CreateEmptyState(),
                        CreateMetadataFactory(),
                        eventLog,
                        new CombatEventQueue(eventLog),
                        null));
        }

        [Test]
        public void StartAndResolveCombat_WithInvalidBudget_ThrowsBeforeStartingCombat()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: false,
                    enemyHasCard: false);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        0,
                        100,
                        100,
                        100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Null);

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));
        }

        [Test]
        public void ResumeActiveCombat_WithoutActiveCombat_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: false,
                    enemyHasCard: false);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveCombat(
                        10,
                        100,
                        100,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.Zero);
        }

        [Test]
        public void StartAndResolveCombat_WithEmptyBoards_CompletesDrawWithoutExchanges()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: false,
                    enemyHasCard: false);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombat(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.IsDraw,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Null);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingColumnResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .CombatResultCalculated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatCompleted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleStartStageStarted),
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(11));

            Assert.That(
                environment.EventLog.Events[
                    environment.EventLog.Count - 1],
                Is.SameAs(completedEvent));
        }

        [Test]
        public void StartAndResolveCombat_WithPlayerOnlySurvivor_AppliesResultDamageAndCompletesVictory()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: false,
                    playerRank: 3,
                    playerMultiplier: 2);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombat(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                completedEvent.BattleHealthDifference,
                Is.EqualTo(6L));

            Assert.That(
                completedEvent.WinningMargin,
                Is.EqualTo(6L));

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
                environment.Runner
                    .ResolvedExchangeCount,
                Is.Zero);

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleHealthChanged),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleStartStageStarted),
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(12));

            var resultEvent =
                GetSingleResultEvent(
                    environment.EventLog);

            Assert.That(
                resultEvent
                    .BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                resultEvent
                    .BaseIncomingDamageToEnemy,
                Is.EqualTo(6));

            Assert.That(
                completedEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    resultEvent.Metadata.EventId));
        }

        [Test]
        public void StartAndResolveCombat_WhenExchangeBudgetIsExhausted_PreservesCombatForResume()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerRank: 2,
                    enemyRank: 2,
                    playerHp: 7,
                    enemyHp: 7,
                    playerAttack: 3,
                    enemyAttack: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        1,
                        100,
                        100,
                        100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CombatCompleted),
                Is.Zero);
        }

        [Test]
        public void ResumeActiveCombat_AfterExchangeBudgetExhaustion_CompletesWithoutRepeatingExchange()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: true,
                    enemyHasCard: true,
                    playerRank: 2,
                    enemyRank: 2,
                    playerHp: 7,
                    enemyHp: 7,
                    playerAttack: 3,
                    enemyAttack: 3);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        1,
                        100,
                        100,
                        100));

            var combatStartedEvent =
                environment.Runner
                    .ActiveCombatStartedEvent;

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombat(
                        10,
                        100,
                        100,
                        100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.Draw));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .ActiveCombatStartedEvent,
                Is.Null);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(3));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(3));

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
                environment.EventLog.CardTombstones
                    .Contains(
                        environment.PlayerCard.InstanceId),
                Is.True);

            Assert.That(
                environment.EventLog.CardTombstones
                    .Contains(
                        environment.EnemyCard.InstanceId),
                Is.True);

            Assert.That(
                environment.EventLog.Events[0],
                Is.SameAs(combatStartedEvent));

            Assert.That(
                environment.EventLog.Events[
                    environment.EventLog.Count - 1],
                Is.SameAs(completedEvent));
        }

        [Test]
        public void StartAndResolveCombat_AfterCompletedCombat_ThrowsWithoutChangingCompletedState()
        {
            var environment =
                CreateEnvironment(
                    playerHasCard: false,
                    enemyHasCard: false);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombat(
                        10,
                        100,
                        100,
                        100);

            var eventCount =
                environment.EventLog.Count;

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        10,
                        100,
                        100,
                        100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(eventCount));

            Assert.That(
                environment.EventLog.Events[
                    eventCount - 1],
                Is.SameAs(completedEvent));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .BattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerHasCard,
                bool enemyHasCard,
                int playerRank = 2,
                int enemyRank = 2,
                int playerMultiplier = 1,
                int enemyMultiplier = 1,
                int playerHp = 7,
                int enemyHp = 7,
                int playerAttack = 3,
                int enemyAttack = 3)
        {
            CombatCardState playerCard;
            CombatCardState enemyCard;

            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerHasCard,
                    playerRank,
                    playerMultiplier,
                    playerHp,
                    playerAttack,
                    new SlotId(1),
                    new SlotId(2),
                    new InstanceId(100),
                    "player-card",
                    out playerCard);

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    enemyHasCard,
                    enemyRank,
                    enemyMultiplier,
                    enemyHp,
                    enemyAttack,
                    new SlotId(3),
                    new SlotId(4),
                    new InstanceId(200),
                    "enemy-card",
                    out enemyCard);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                CreateSourceRegistry();

            return new TestEnvironment
            {
                State = state,
                PlayerCard = playerCard,
                EnemyCard = enemyCard,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                SourceRegistry = sourceRegistry,
                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
        }

        private static CombatState CreateEmptyState()
        {
            return new CombatState(
                CreateEmptySideState(
                    CombatSide.Player),
                CreateEmptySideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState
            CreateEmptySideState(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                bool hasCard,
                int rank,
                int attackMultiplier,
                int hp,
                int attack,
                SlotId frontSlotId,
                SlotId backSlotId,
                InstanceId instanceId,
                string definitionId,
                out CombatCardState card)
        {
            if (!hasCard)
            {
                card = null;

                return CreateEmptySideState(
                    side);
            }

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

            card =
                new CombatCardState(
                    new DefinitionId(definitionId),
                    instanceId,
                    new CardRank(rank),
                    hp,
                    hp,
                    0,
                    attack);

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
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    attackMultiplier));
        }

        private static CombatResultCalculatedCombatEvent
            GetSingleResultEvent(
                CombatEventLog eventLog)
        {
            CombatResultCalculatedCombatEvent
                resultEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as CombatResultCalculatedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (resultEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple result events were found.");
                }

                resultEvent =
                    candidate;
            }

            if (resultEvent == null)
            {
                throw new InvalidOperationException(
                    "Result event was not found.");
            }

            return resultEvent;
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
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    count++;
                }
            }

            return count;
        }

        private static CombatTriggerSourceRegistry
            CreateSourceRegistry()
        {
            return new CombatTriggerSourceRegistry(
                Array.Empty<ICombatTriggerSource>());
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatState State
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

            public CombatResolutionRunner Runner
            {
                get;
                set;
            }
        }
    }
}