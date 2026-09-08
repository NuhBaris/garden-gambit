using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatBattleStartRunnerStagedAltarTests
    {
        [Test]
        public void
            StartAndResolveBattleStartStaged_WithSacrificialAltar_CompletesAltarBeforePetAndCardStages()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveBattleStartStaged(
                        environment.CombatStartedEvent,
                        maximumPassCountPerStage: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleStartStageStarted),
                Is.EqualTo(3));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
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
                environment.EventLog.Count,
                Is.EqualTo(8));

            var slotStageEvent =
                environment.EventLog.Events[1]
                    as
                        BattleStartStageStartedCombatEvent;

            var petStageEvent =
                environment.EventLog.Events[6]
                    as
                        BattleStartStageStartedCombatEvent;

            var cardStageEvent =
                environment.EventLog.Events[7]
                    as
                        BattleStartStageStartedCombatEvent;

            Assert.That(
                slotStageEvent,
                Is.Not.Null);

            Assert.That(
                slotStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[3],
                Is.TypeOf<HpGainCombatEvent>());

            Assert.That(
                environment.EventLog.Events[4],
                Is.TypeOf<DeathCombatEvent>());

            Assert.That(
                environment.EventLog.Events[5],
                Is.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                petStageEvent,
                Is.Not.Null);

            Assert.That(
                petStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                cardStageEvent,
                Is.Not.Null);

            Assert.That(
                cardStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .UsesStagedAltarResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveStage,
                Is.False);

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveBattleStartStaged_WithWarAltar_CompletesWithoutHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveBattleStartStaged(
                        environment.CombatStartedEvent,
                        maximumPassCountPerStage: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleStartStageStarted),
                Is.EqualTo(3));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .WarAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
                Is.Zero);

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
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveBattleStartStaged_WithoutAltar_PreservesSlotPetCardOrder()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.None);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveBattleStartStaged(
                        environment.CombatStartedEvent,
                        maximumPassCountPerStage: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            var slotStageEvent =
                environment.EventLog.Events[1]
                    as
                        BattleStartStageStartedCombatEvent;

            var petStageEvent =
                environment.EventLog.Events[2]
                    as
                        BattleStartStageStartedCombatEvent;

            var cardStageEvent =
                environment.EventLog.Events[3]
                    as
                        BattleStartStageStartedCombatEvent;

            Assert.That(
                slotStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                petStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Pet));

            Assert.That(
                cardStageEvent.Stage,
                Is.EqualTo(
                    CombatBattleStartStage.Card));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
                Is.Zero);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveBattleStartStaged_WhenAltarBudgetExhausts_ResumeDoesNotRepeatHpGainOrStages()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Runner
                        .StartAndResolveBattleStartStaged(
                            environment
                                .CombatStartedEvent,
                            maximumPassCountPerStage: 1,
                            maximumEventCountPerPass: 1,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.True);

            Assert.That(
                environment.Runner
                    .UsesStagedAltarResolution,
                Is.True);

            Assert.That(
                environment.Runner.NextStage,
                Is.EqualTo(
                    CombatBattleStartStage.Slot));

            Assert.That(
                environment.Runner.HasActiveSlotStage,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasStagedAltarExecution,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveAltarExecutionStage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleStartStageStarted),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.Zero);

            var resolvedCount =
                environment.Runner
                    .ResumeActiveBattleStart(
                        maximumPassCountPerStage: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .BattleStartStageStarted),
                Is.EqualTo(3));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
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
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveStage,
                Is.False);

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
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
                if (eventLog.Events[index].Kind ==
                    kind)
                {
                    count++;
                }
            }

            return count;
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatSlotEnhanceKind enhanceKind)
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var donorCard =
                CreateCard(
                    instanceId: 100,
                    currentHp: 4,
                    attack: 6);

            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    currentHp: 5,
                    attack: 3);

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                donorPosition,
                                donorCard.InstanceId,
                                enhanceKind),

                            new CombatSlotState(
                                new SlotId(2),
                                recipientPosition,
                                recipientCard.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            donorCard,
                            recipientCard
                        }),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var enemySide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Enemy,
                        Array.Empty<
                            CombatSlotState>()),
                    new CombatCardRegistry(
                        Array.Empty<
                            CombatCardState>()),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new
                        CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog)
                    .Start(state);

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var resolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    new CombatTriggerSourceRegistry(
                        Array.Empty<
                            ICombatTriggerSource>()));

            return new TestEnvironment
            {
                Runner =
                    new CombatBattleStartRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        resolutionEngine),

                CombatStartedEvent =
                    combatStartedEvent,

                EventLog =
                    eventLog,

                ResolutionEngine =
                    resolutionEngine,

                DonorCard =
                    donorCard,

                RecipientCard =
                    recipientCard
            };
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

        private sealed class TestEnvironment
        {
            public CombatBattleStartRunner Runner
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

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatEventResolutionEngine
                ResolutionEngine
            {
                get;
                set;
            }

            public CombatCardState DonorCard
            {
                get;
                set;
            }

            public CombatCardState RecipientCard
            {
                get;
                set;
            }
        }
    }
}