using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerAltarIntegrationTests
    {
        [Test]
        public void StartAndResolveCombat_WithSacrificialAltar_ResolvesAltarBeforeColumns()
        {
            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 2);

            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 2);

            var donorCard =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var recipientCard =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            donorPosition,
                            donorCard,
                            CombatSlotEnhanceKind
                                .SacrificialAltar),

                        CreateOccupiedSlot(
                            2,
                            recipientPosition,
                            recipientCard,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        donorCard,
                        recipientCard
                    },
                    new CombatSlotState[0],
                    new CombatCardState[0]);

            var completedEvent =
                StartCombat(
                    environment.Runner);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                completedEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.CombatCompleted));

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.Zero);

            Assert.That(
                recipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                recipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(donorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));

            var altarIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated);

            var firstColumnIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted);

            Assert.That(
                altarIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                firstColumnIndex,
                Is.GreaterThan(altarIndex));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);
        }

        [Test]
        public void StartAndResolveCombat_WithBothSideAltars_ResolvesPlayerBeforeEnemyAndColumns()
        {
            var playerDonorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 4);

            var playerRecipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 4);

            var enemyDonorPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    column: 1);

            var enemyRecipientPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column: 1);

            var playerDonor =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var playerRecipient =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var enemyDonor =
                CreateCard(
                    300,
                    currentHp: 4,
                    attack: 6);

            var enemyRecipient =
                CreateCard(
                    400,
                    currentHp: 5,
                    attack: 3);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            playerDonorPosition,
                            playerDonor,
                            CombatSlotEnhanceKind
                                .SacrificialAltar),

                        CreateOccupiedSlot(
                            2,
                            playerRecipientPosition,
                            playerRecipient,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        playerDonor,
                        playerRecipient
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            enemyDonorPosition,
                            enemyDonor,
                            CombatSlotEnhanceKind
                                .WarAltar),

                        CreateOccupiedSlot(
                            2,
                            enemyRecipientPosition,
                            enemyRecipient,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        enemyDonor,
                        enemyRecipient
                    });

            StartCombat(
                environment.Runner);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(2));

            Assert.That(
                playerRecipient.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                playerRecipient.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                enemyRecipient.Attack,
                Is.EqualTo(9));

            var playerAltarIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated);

            var enemyAltarIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .WarAltarActivated);

            var firstColumnIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted);

            Assert.That(
                playerAltarIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                enemyAltarIndex,
                Is.GreaterThan(
                    playerAltarIndex));

            Assert.That(
                firstColumnIndex,
                Is.GreaterThan(
                    enemyAltarIndex));

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
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void StartAndResolveCombat_WithoutAltarRecipient_SkipsActivationAndKeepsDonor()
        {
            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 2);

            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 2);

            var donorCard =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            donorPosition,
                            donorCard,
                            CombatSlotEnhanceKind
                                .SacrificialAltar),

                        new CombatSlotState(
                            new SlotId(2),
                            recipientPosition)
                    },
                    new[]
                    {
                        donorCard
                    },
                    new CombatSlotState[0],
                    new CombatCardState[0]);

            StartCombat(
                environment.Runner);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.Zero);

            Assert.That(
                donorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(donorPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(5));
        }

        [Test]
        public void ResumeActiveCombat_AfterAltarBudgetExhaustion_CompletesWithoutRepeatingAltar()
        {
            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 2);

            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 2);

            var donorCard =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var recipientCard =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            donorPosition,
                            donorCard,
                            CombatSlotEnhanceKind
                                .SacrificialAltar),

                        CreateOccupiedSlot(
                            2,
                            recipientPosition,
                            recipientCard,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        donorCard,
                        recipientCard
                    },
                    new CombatSlotState[0],
                    new CombatCardState[0]);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 1,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveAltarSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(1));

            var completedEvent =
                environment.Runner.ResumeActiveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(1));

            Assert.That(
                recipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                recipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

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
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.False);
        }

        [Test]
        public void ResumeActiveCombat_AfterColumnBudgetExhaustion_DoesNotRepeatCompletedAltar()
        {
            var altarDonorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 2);

            var altarRecipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 2);

            var playerFighterPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 1);

            var playerBackPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 1);

            var enemyFighterPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column: 1);

            var enemyBackPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    column: 1);

            var altarDonor =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var altarRecipient =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var playerFighter =
                CreateCard(
                    300,
                    currentHp: 5,
                    attack: 3);

            var enemyFighter =
                CreateCard(
                    400,
                    currentHp: 5,
                    attack: 3);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            altarDonorPosition,
                            altarDonor,
                            CombatSlotEnhanceKind
                                .SacrificialAltar),

                        CreateOccupiedSlot(
                            2,
                            altarRecipientPosition,
                            altarRecipient,
                            CombatSlotEnhanceKind.None),

                        CreateOccupiedSlot(
                            3,
                            playerFighterPosition,
                            playerFighter,
                            CombatSlotEnhanceKind.None),

                        new CombatSlotState(
                            new SlotId(4),
                            playerBackPosition)
                    },
                    new[]
                    {
                        altarDonor,
                        altarRecipient,
                        playerFighter
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            enemyFighterPosition,
                            enemyFighter,
                            CombatSlotEnhanceKind.None),

                        new CombatSlotState(
                            new SlotId(2),
                            enemyBackPosition)
                    },
                    new[]
                    {
                        enemyFighter
                    });

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        maximumExchangeCountPerColumn: 1,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.True);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(1));

            var completedEvent =
                environment.Runner.ResumeActiveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner
                    .ResolvedExchangeCount,
                Is.EqualTo(2));

            Assert.That(
                altarRecipient.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                altarRecipient.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .NormalAttackExchange),
                Is.EqualTo(2));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveColumn,
                Is.False);
        }

        [Test]
        public void StartAndResolveCombat_WithZeroAttackWarAltar_StillCompletesDonorDeathChain()
        {
            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 2);

            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 2);

            var donorCard =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 0);

            var recipientCard =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            donorPosition,
                            donorCard,
                            CombatSlotEnhanceKind
                                .WarAltar),

                        CreateOccupiedSlot(
                            2,
                            recipientPosition,
                            recipientCard,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        donorCard,
                        recipientCard
                    },
                    new CombatSlotState[0],
                    new CombatCardState[0]);

            StartCombat(
                environment.Runner);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(1));

            Assert.That(
                recipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(donorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .WarAltarActivated),
                Is.EqualTo(1));

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
        }

        private static CombatCompletedCombatEvent
            StartCombat(
                CombatResolutionRunner runner)
        {
            return runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 10,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatSlotState[] playerSlots,
                CombatCardState[] playerCards,
                CombatSlotState[] enemySlots,
                CombatCardState[] enemyCards)
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerSlots,
                    playerCards);

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    enemySlots,
                    enemyCards);

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[0]);

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                EnemySide = enemySide,
                EventLog = eventLog,
                Runner = runner
            };
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                CombatSlotState[] slots,
                CombatCardState[] cards)
        {
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

        private static CombatSlotState
            CreateOccupiedSlot(
                int slotId,
                BoardPosition position,
                CombatCardState card,
                CombatSlotEnhanceKind enhanceKind)
        {
            return new CombatSlotState(
                new SlotId(slotId),
                position,
                card.InstanceId,
                enhanceKind);
        }

        private static BoardPosition CreatePosition(
            CombatSide side,
            BoardRow row,
            int column)
        {
            return new BoardPosition(
                side,
                row,
                new BoardColumn(column));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int currentHp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: currentHp,
                armor: 0,
                attack: attack);
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

        private static int IndexOfKind(
            CombatEventLog eventLog,
            CombatEventKind kind)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    return index;
                }
            }

            return -1;
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

            public CombatEventLog EventLog
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