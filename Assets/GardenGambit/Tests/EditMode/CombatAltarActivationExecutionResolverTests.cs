using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationExecutionResolverTests
    {
        [Test]
        public void
            Constructor_WithNullActivationResolver_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionResolver(
                            null,
                            environment.ResolutionEngine));
        }

        [Test]
        public void
            Constructor_WithNullResolutionEngine_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        CombatAltarActivationExecutionResolver(
                            environment.ActivationResolver,
                            null));
        }

        [Test]
        public void
            Continue_WithNullExecutionState_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () =>
                    environment.ExecutionResolver
                        .Continue(
                            null,
                            maximumPassCount: 10,
                            maximumEventCountPerPass: 100,
                            maximumTriggerCountPerEvent: 100));
        }

        [Test]
        public void
        Continue_WithSacrificialAltar_CompletesHpGainBeforeDeathChain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                environment.ExecutionResolver
                    .Continue(
                        environment.ExecutionState,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                altarEvent,
                Is.SameAs(
                    environment.ExecutionState
                        .AltarEvent));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .Completed));

            Assert.That(
                environment.ExecutionState.IsCompleted,
                Is.True);

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CardAdvanced),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(6));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<HpGainCombatEvent>());

            Assert.That(
                environment.EventLog.Events[3],
                Is.TypeOf<DeathCombatEvent>());

            Assert.That(
                environment.EventLog.Events[4],
                Is.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventLog.Events[5],
                Is.TypeOf<
                    CardAdvancedCombatEvent>());

            var advancedFrontSlot =
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition);

            Assert.That(
                advancedFrontSlot.IsOccupied,
                Is.True);

            Assert.That(
                advancedFrontSlot
                    .OccupantInstanceId.Value,
                Is.EqualTo(
                    environment.RecipientCard
                        .InstanceId));

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
    Continue_WithWarAltar_CompletesWithoutHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .WarAltar);

            environment.ExecutionResolver
                .Continue(
                    environment.ExecutionState,
                    maximumPassCount: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.ExecutionState.IsCompleted,
                Is.True);

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

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
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.CardAdvanced),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    WarAltarActivatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<DeathCombatEvent>());

            Assert.That(
                environment.EventLog.Events[3],
                Is.TypeOf<
                    DeathRemovalCombatEvent>());

            Assert.That(
                environment.EventLog.Events[4],
                Is.TypeOf<
                    CardAdvancedCombatEvent>());

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            Continue_WhenAlreadyCompleted_DoesNotRepeatWork()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var firstResult =
                environment.ExecutionResolver
                    .Continue(
                        environment.ExecutionState,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            var eventCountAfterCompletion =
                environment.EventLog.Count;

            var recipientCapacityAfterCompletion =
                environment.RecipientCard.HpCapacity;

            var secondResult =
                environment.ExecutionResolver
                    .Continue(
                        environment.ExecutionState,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                secondResult,
                Is.SameAs(
                    firstResult));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(
                    eventCountAfterCompletion));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(
                    recipientCapacityAfterCompletion));

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
        }

        [Test]
        public void
            Continue_WhenTransferDrainExhaustsBudget_RetryDoesNotRepeatHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.ExecutionResolver
                        .Continue(
                            environment.ExecutionState,
                            maximumPassCount: 1,
                            maximumEventCountPerPass: 1,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.ExecutionState.Stage,
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
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(9));

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

            environment.ExecutionResolver
                .Continue(
                    environment.ExecutionState,
                    maximumPassCount: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.ExecutionState.IsCompleted,
                Is.True);

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(9));

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
        }

        [Test]
        public void
            Continue_WhenDeathDrainExhaustsBudget_RetryDoesNotRepeatDeath()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.ExecutionResolver
                        .Continue(
                            environment.ExecutionState,
                            maximumPassCount: 1,
                            maximumEventCountPerPass: 100,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.ExecutionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .DonorDeathStarted));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

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

            environment.ExecutionResolver
                .Continue(
                    environment.ExecutionState,
                    maximumPassCount: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.ExecutionState.IsCompleted,
                Is.True);

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

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
                    BoardRow.Front,
                    new BoardColumn(2));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(2));

            var donorCard =
                CreateCard(
                    instanceId: 100,
                    hpCapacity: 10,
                    currentHp: 4,
                    attack: 6);

            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    hpCapacity: 10,
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

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    Array.Empty<
                        ICombatTriggerSource>());

            var resolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var activationResolver =
                new CombatAltarActivationResolver(
                    metadataFactory,
                    eventLog);

            var executionState =
                activationResolver
                    .TryStartActivation(
                        state,
                        combatStartedEvent,
                        donorPosition);

            return new TestEnvironment
            {
                PlayerSide =
                    playerSide,

                DonorCard =
                    donorCard,

                RecipientCard =
                    recipientCard,

                DonorPosition =
                    donorPosition,

                EventLog =
                    eventLog,

                ResolutionEngine =
                    resolutionEngine,

                ActivationResolver =
                    activationResolver,

                ExecutionState =
                    executionState,

                ExecutionResolver =
                    new
                        CombatAltarActivationExecutionResolver(
                            activationResolver,
                            resolutionEngine)
            };
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int hpCapacity,
            int currentHp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(2),
                hpCapacity,
                currentHp,
                armor: 0,
                attack: attack);
        }

        private sealed class TestEnvironment
        {
            public CombatSideState PlayerSide
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

            public BoardPosition DonorPosition
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

            public CombatAltarActivationResolver
                ActivationResolver
            {
                get;
                set;
            }

            public CombatAltarActivationExecutionState
                ExecutionState
            {
                get;
                set;
            }

            public CombatAltarActivationExecutionResolver
                ExecutionResolver
            {
                get;
                set;
            }
        }
    }
}