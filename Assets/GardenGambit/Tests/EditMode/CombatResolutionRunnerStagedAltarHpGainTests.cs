using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerStagedAltarHpGainTests
    {
        [Test]
        public void
            StartAndResolveCombatStaged_WithSacrificialAltar_OrdersHpGainBeforeDeathAndLaterStages()
        {
            var environment =
                CreateEnvironment(
                    includeHpGainObserver: false);

            var completedEvent =
                environment.Runner
                    .StartAndResolveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
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

            var slotStageIndex =
                FindStageIndex(
                    environment.EventLog,
                    CombatBattleStartStage.Slot);

            var altarIndex =
                FindEventIndex(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated);

            var hpGainIndex =
                FindEventIndex(
                    environment.EventLog,
                    CombatEventKind.HpGain);

            var deathIndex =
                FindEventIndex(
                    environment.EventLog,
                    CombatEventKind.Death);

            var removalIndex =
                FindEventIndex(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval);

            var petStageIndex =
                FindStageIndex(
                    environment.EventLog,
                    CombatBattleStartStage.Pet);

            var cardStageIndex =
                FindStageIndex(
                    environment.EventLog,
                    CombatBattleStartStage.Card);

            var firstColumnIndex =
                FindEventIndex(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted);

            Assert.That(
                slotStageIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                altarIndex,
                Is.GreaterThan(slotStageIndex));

            Assert.That(
                hpGainIndex,
                Is.GreaterThan(altarIndex));

            Assert.That(
                deathIndex,
                Is.GreaterThan(hpGainIndex));

            Assert.That(
                removalIndex,
                Is.GreaterThan(deathIndex));

            Assert.That(
                petStageIndex,
                Is.GreaterThan(removalIndex));

            Assert.That(
                cardStageIndex,
                Is.GreaterThan(petStageIndex));

            Assert.That(
                firstColumnIndex,
                Is.GreaterThan(cardStageIndex));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveCombatStaged_HpGainTriggerObservesLivingDonor()
        {
            var environment =
                CreateEnvironment(
                    includeHpGainObserver: true);

            environment.Runner
                .StartAndResolveCombatStaged(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.Observer,
                Is.Not.Null);

            Assert.That(
                environment.Observer.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.Observer
                    .DonorHpWhenResolved,
                Is.EqualTo(4));

            Assert.That(
                environment.Observer
                    .DonorWasStillInBoardSlot,
                Is.True);

            Assert.That(
                environment.Observer
                    .ObservedHpGainEvent,
                Is.Not.Null);

            Assert.That(
                environment.Observer
                    .ObservedHpGainEvent
                    .TargetInstanceId,
                Is.EqualTo(
                    environment.RecipientCard
                        .InstanceId));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

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


            var observedHpGainEvent =
                environment.Observer.ObservedHpGainEvent;

            Assert.That(
                observedHpGainEvent.SourceInstanceId,
                Is.EqualTo(
                    environment.DonorCard.InstanceId));

            Assert.That(
                observedHpGainEvent.IsFromAnotherSource,
                Is.True);

            Assert.That(
                observedHpGainEvent.IsSelfSource,
                Is.False);

        }

        [Test]
        public void
            ResumeActiveCombatStaged_AfterAltarBudgetExhaustion_DoesNotRepeatHpGainOrDeath()
        {
            var environment =
                CreateEnvironment(
                    includeHpGainObserver: false);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Runner
                        .StartAndResolveCombatStaged(
                            maximumExchangeCountPerColumn: 10,
                            maximumPassCountPerExchange: 1,
                            maximumEventCountPerPass: 1,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedNormalAttack,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveCombatUsesStagedAltarResolution,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveBattleStartResolution,
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
                Is.Zero);

            var completedEvent =
                environment.Runner
                    .ResumeActiveCombatStaged(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 100,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent,
                Is.Not.Null);

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
                environment.Runner.HasActiveCombat,
                Is.False);

            var hpGainIndex = FindEventIndex(
                environment.EventLog,
                CombatEventKind.HpGain);

            Assert.That(
                hpGainIndex,
                Is.GreaterThanOrEqualTo(0));

            var hpGainEvent =
                environment.EventLog.Events[hpGainIndex]
                    as HpGainCombatEvent;

            Assert.That(
                hpGainEvent,
                Is.Not.Null);

            Assert.That(
                hpGainEvent.SourceInstanceId,
                Is.EqualTo(
                    environment.DonorCard.InstanceId));

            Assert.That(
                hpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.RecipientCard.InstanceId));

            Assert.That(
                hpGainEvent.IsFromAnotherSource,
                Is.True);

            Assert.That(
                hpGainEvent.IsSelfSource,
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

        private static int FindEventIndex(
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

        private static int FindStageIndex(
            CombatEventLog eventLog,
            CombatBattleStartStage stage)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var stageEvent =
                    eventLog.Events[index]
                        as
                            BattleStartStageStartedCombatEvent;

                if (stageEvent != null &&
                    stageEvent.Stage == stage)
                {
                    return index;
                }
            }

            return -1;
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeHpGainObserver)
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
                                CombatSlotEnhanceKind
                                    .SacrificialAltar),

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

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            HpGainObserverHandler observer =
                null;

            ICombatTriggerSource[] sources;

            if (includeHpGainObserver)
            {
                observer =
                    new HpGainObserverHandler(
                        donorCard,
                        donorPosition,
                        recipientCard.InstanceId);

                sources =
                    new ICombatTriggerSource[]
                    {
                        new CombatTriggerHandlerSource(
                            new
                                FixedCombatTriggerOrderKeyProvider(
                                    new CombatTriggerOrderKey(
                                        CombatTriggerSourceKind
                                            .Slot,
                                        CombatSide.Player,
                                        horizontalOrder: 0,
                                        verticalOrder: 0)),
                            observer)
                    };
            }
            else
            {
                sources =
                    Array.Empty<
                        ICombatTriggerSource>();
            }

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            return new TestEnvironment
            {
                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry),

                PlayerSide =
                    playerSide,

                EventLog =
                    eventLog,

                DonorCard =
                    donorCard,

                RecipientCard =
                    recipientCard,

                DonorPosition =
                    donorPosition,

                Observer =
                    observer
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

        private sealed class
            HpGainObserverHandler :
            CombatEventTriggerHandler<
                HpGainCombatEvent>
        {
            private readonly CombatCardState
                _donorCard;

            private readonly BoardPosition
                _donorPosition;

            private readonly InstanceId
                _recipientInstanceId;

            public HpGainObserverHandler(
                CombatCardState donorCard,
                BoardPosition donorPosition,
                InstanceId recipientInstanceId)
            {
                _donorCard =
                    donorCard;

                _donorPosition =
                    donorPosition;

                _recipientInstanceId =
                    recipientInstanceId;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public int DonorHpWhenResolved
            {
                get;
                private set;
            }

            public bool DonorWasStillInBoardSlot
            {
                get;
                private set;
            }

            public HpGainCombatEvent
                ObservedHpGainEvent
            {
                get;
                private set;
            }

            protected override bool CanTriggerTyped(
                CombatState state,
                HpGainCombatEvent sourceEvent)
            {
                return sourceEvent.TargetInstanceId ==
                    _recipientInstanceId;
            }

            protected override void ResolveTyped(
                CombatState state,
                HpGainCombatEvent sourceEvent)
            {
                ResolveCallCount++;

                DonorHpWhenResolved =
                    _donorCard.CurrentHp;

                var donorSlot =
                    state.GetSide(
                            _donorPosition.Side)
                        .Board.GetSlot(
                            _donorPosition);

                DonorWasStillInBoardSlot =
                    donorSlot.IsOccupied &&
                    donorSlot.OccupantInstanceId.Value ==
                    _donorCard.InstanceId;

                ObservedHpGainEvent =
                    sourceEvent;
            }
        }

        private sealed class TestEnvironment
        {
            public CombatResolutionRunner Runner
            {
                get;
                set;
            }

            public CombatSideState PlayerSide
            {
                get;
                set;
            }

            public CombatEventLog EventLog
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

            public HpGainObserverHandler Observer
            {
                get;
                set;
            }
        }
    }
}