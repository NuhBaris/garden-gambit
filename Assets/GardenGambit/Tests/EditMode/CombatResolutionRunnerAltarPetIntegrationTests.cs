using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerAltarPetIntegrationTests
    {
        [Test]
        public void
            StartAndResolveCombat_AfterAltarRemoval_PetObservesCompletedDeathChain()
        {
            var environment =
                CreateEnvironment(
                    rescueDonor: false);

            var completedEvent =
                StartCombat(
                    environment.Runner);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .DonorSlotOccupiedAtPetStart,
                Is.False);

            Assert.That(
                environment.PetHandler
                    .PlayerCardCountAtPetStart,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .DonorHpAtPetStart,
                Is.EqualTo(0));

            Assert.That(
                environment.PetHandler
                    .RecipientHpCapacityAtPetStart,
                Is.EqualTo(14));

            Assert.That(
                environment.PetHandler
                    .RecipientCurrentHpAtPetStart,
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

            var altarIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated);

            var removalIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval);

            var petStageIndex =
                IndexOfStage(
                    environment.EventLog,
                    CombatBattleStartStage.Pet);

            Assert.That(
                removalIndex,
                Is.GreaterThan(
                    altarIndex));

            Assert.That(
                petStageIndex,
                Is.GreaterThan(
                    removalIndex));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveCombat_AfterAltarRescue_PetObservesRescuedDonor()
        {
            var environment =
                CreateEnvironment(
                    rescueDonor: true);

            var completedEvent =
                StartCombat(
                    environment.Runner);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .DonorSlotOccupiedAtPetStart,
                Is.True);

            Assert.That(
                environment.PetHandler
                    .PlayerCardCountAtPetStart,
                Is.EqualTo(2));

            Assert.That(
                environment.PetHandler
                    .DonorHpAtPetStart,
                Is.EqualTo(1));

            Assert.That(
                environment.PetHandler
                    .RecipientHpCapacityAtPetStart,
                Is.EqualTo(14));

            Assert.That(
                environment.PetHandler
                    .RecipientCurrentHpAtPetStart,
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
                    CombatEventKind.Rescue),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.Zero);

            var rescueIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.Rescue);

            var petStageIndex =
                IndexOfStage(
                    environment.EventLog,
                    CombatBattleStartStage.Pet);

            Assert.That(
                petStageIndex,
                Is.GreaterThan(
                    rescueIndex));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool rescueDonor)
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(2));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            var donorCard =
                new CombatCardState(
                    new DefinitionId(
                        "altar-donor"),
                    new InstanceId(100),
                    new CardRank(2),
                    hpCapacity: 10,
                    currentHp: 4,
                    armor: 0,
                    attack: 6);

            var recipientCard =
                new CombatCardState(
                    new DefinitionId(
                        "altar-recipient"),
                    new InstanceId(200),
                    new CardRank(2),
                    hpCapacity: 10,
                    currentHp: 5,
                    armor: 0,
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
                CreateEmptySide(
                    CombatSide.Enemy);

            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "observing-pet"),
                    new InstanceId(900));

            var state =
                new CombatState(
                    playerSide,
                    enemySide,
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                pet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new CombatPetState[0])));

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var eventQueue =
                new CombatEventQueue(
                    eventLog);

            var petHandler =
                new ObservingPetHandler(
                    pet.InstanceId,
                    donorPosition,
                    donorCard,
                    recipientCard);

            var petSource =
                new CombatPetBattleStartTriggerSource(
                    petHandler);

            ICombatTriggerSource[] sources;

            if (rescueDonor)
            {
                var rescueHandler =
                    new TargetedRescueHandler(
                        metadataFactory,
                        eventLog,
                        donorCard.InstanceId);

                var rescueSource =
                    new CombatTriggerHandlerSource(
                        new
                            FixedCombatTriggerOrderKeyProvider(
                                new CombatTriggerOrderKey(
                                    CombatTriggerSourceKind
                                        .Slot,
                                    CombatSide.Player,
                                    0,
                                    0)),
                        rescueHandler);

                sources =
                    new ICombatTriggerSource[]
                    {
                        petSource,
                        rescueSource
                    };
            }
            else
            {
                sources =
                    new ICombatTriggerSource[]
                    {
                        petSource
                    };
            }

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    sources);

            return new TestEnvironment
            {
                PlayerSide = playerSide,
                DonorPosition = donorPosition,
                EventLog = eventLog,
                PetHandler = petHandler,
                Runner =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        eventQueue,
                        sourceRegistry)
            };
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

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
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

        private static int IndexOfStage(
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

        private sealed class ObservingPetHandler :
            CombatPetBattleStartTriggerHandler
        {
            private readonly BoardPosition
                _donorPosition;

            private readonly CombatCardState
                _donorCard;

            private readonly CombatCardState
                _recipientCard;

            public ObservingPetHandler(
                InstanceId petInstanceId,
                BoardPosition donorPosition,
                CombatCardState donorCard,
                CombatCardState recipientCard)
                : base(
                    CombatSide.Player,
                    petInstanceId)
            {
                _donorPosition =
                    donorPosition;

                _donorCard =
                    donorCard;

                _recipientCard =
                    recipientCard;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public bool DonorSlotOccupiedAtPetStart
            {
                get;
                private set;
            }

            public int PlayerCardCountAtPetStart
            {
                get;
                private set;
            }

            public int DonorHpAtPetStart
            {
                get;
                private set;
            }

            public int RecipientHpCapacityAtPetStart
            {
                get;
                private set;
            }

            public int RecipientCurrentHpAtPetStart
            {
                get;
                private set;
            }

            protected override bool
                CanTriggerAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                return true;
            }

            protected override void
                ResolveAtBattleStart(
                    CombatState state,
                    BattleStartStageStartedCombatEvent
                        sourceEvent,
                    CombatPetState pet)
            {
                ResolveCallCount++;

                var playerSide =
                    state.GetSide(
                        CombatSide.Player);

                DonorSlotOccupiedAtPetStart =
                    playerSide.Board
                        .GetSlot(
                            _donorPosition)
                        .IsOccupied;

                PlayerCardCountAtPetStart =
                    playerSide.Cards.Count;

                DonorHpAtPetStart =
                    _donorCard.CurrentHp;

                RecipientHpCapacityAtPetStart =
                    _recipientCard.HpCapacity;

                RecipientCurrentHpAtPetStart =
                    _recipientCard.CurrentHp;
            }
        }

        private sealed class TargetedRescueHandler :
            CombatRescueTriggerHandler
        {
            private readonly InstanceId
                _targetInstanceId;

            public TargetedRescueHandler(
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog,
                InstanceId targetInstanceId)
                : base(
                    metadataFactory,
                    eventLog)
            {
                _targetInstanceId =
                    targetInstanceId;
            }

            protected override bool CanRescue(
                CombatState state,
                DeathCombatEvent sourceEvent)
            {
                return sourceEvent.InstanceId ==
                       _targetInstanceId;
            }
        }

        private sealed class TestEnvironment
        {
            public CombatSideState PlayerSide
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

            public ObservingPetHandler PetHandler
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