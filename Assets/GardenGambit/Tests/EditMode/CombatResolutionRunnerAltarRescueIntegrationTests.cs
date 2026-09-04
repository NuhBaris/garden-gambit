using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerAltarRescueIntegrationTests
    {
        [Test]
        public void StartAndResolveCombat_WhenAltarDonorIsRescued_KeepsDonorAtOneHp()
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
                    donorCard.InstanceId);

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
                recipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                recipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                donorCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(donorPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.PlayerSide.GetCardAt(
                    donorPosition),
                Is.SameAs(donorCard));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(2));

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

            var deathIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.Death,
                    occurrence: 0);

            var rescueIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.Rescue,
                    occurrence: 0);

            var columnIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted,
                    occurrence: 0);

            Assert.That(
                rescueIndex,
                Is.GreaterThan(deathIndex));

            Assert.That(
                columnIndex,
                Is.GreaterThan(rescueIndex));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void StartAndResolveCombat_WhenFirstAltarDonorIsRescued_CompletesRescueBeforeNextAltar()
        {
            var firstDonorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 1);

            var firstRecipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 1);

            var secondDonorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 3);

            var secondRecipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 3);

            var firstDonor =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var firstRecipient =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var secondDonor =
                CreateCard(
                    300,
                    currentHp: 7,
                    attack: 5);

            var secondRecipient =
                CreateCard(
                    400,
                    currentHp: 6,
                    attack: 2);

            var environment =
                CreateEnvironment(
                    new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            secondDonorPosition,
                            secondDonor,
                            CombatSlotEnhanceKind
                                .WarAltar),

                        CreateOccupiedSlot(
                            2,
                            secondRecipientPosition,
                            secondRecipient,
                            CombatSlotEnhanceKind.None),

                        CreateOccupiedSlot(
                            3,
                            firstDonorPosition,
                            firstDonor,
                            CombatSlotEnhanceKind
                                .SacrificialAltar),

                        CreateOccupiedSlot(
                            4,
                            firstRecipientPosition,
                            firstRecipient,
                            CombatSlotEnhanceKind.None)
                    },
                    new[]
                    {
                        firstDonor,
                        firstRecipient,
                        secondDonor,
                        secondRecipient
                    },
                    firstDonor.InstanceId);

            StartCombat(
                environment.Runner);

            Assert.That(
                environment.Runner
                    .ResolvedAltarActivationCount,
                Is.EqualTo(2));

            Assert.That(
                firstDonor.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                firstRecipient.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                firstRecipient.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                secondRecipient.Attack,
                Is.EqualTo(7));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(firstDonorPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(secondDonorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.PlayerSide.Cards.Count,
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
                    CombatEventKind
                        .WarAltarActivated),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Death),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.Rescue),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            var rescueIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind.Rescue,
                    occurrence: 0);

            var secondAltarIndex =
                IndexOfKind(
                    environment.EventLog,
                    CombatEventKind
                        .WarAltarActivated,
                    occurrence: 0);

            Assert.That(
                rescueIndex,
                Is.GreaterThanOrEqualTo(0));

            Assert.That(
                secondAltarIndex,
                Is.GreaterThan(rescueIndex));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
        }

        [Test]
        public void ResumeActiveCombat_WhenRescueChainExhaustsBudget_DoesNotRepeatAltarOrRescue()
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
                    donorCard.InstanceId);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveCombat(
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 1,
                        maximumEventCountPerPass: 1,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasActiveAltarResolution,
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
                donorCard.CurrentHp,
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
                    CombatEventKind.Rescue),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.Zero);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.Runner.HasActiveCombat,
                Is.False);
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
                InstanceId rescueInstanceId)
        {
            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerSlots,
                    playerCards);

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    new CombatSlotState[0],
                    new CombatCardState[0]);

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

            var rescueHandler =
                new TargetedAltarRescueHandler(
                    metadataFactory,
                    eventLog,
                    rescueInstanceId);

            var orderKeyProvider =
                new FixedCombatTriggerOrderKeyProvider(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Slot,
                        CombatSide.Player,
                        horizontalOrder: 0,
                        verticalOrder: 0));

            var rescueSource =
                new CombatTriggerHandlerSource(
                    orderKeyProvider,
                    rescueHandler);

            var sourceRegistry =
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        rescueSource
                    });

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
            CombatEventKind kind,
            int occurrence)
        {
            var currentOccurrence = 0;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                if (eventLog.Events[index].Kind !=
                    kind)
                {
                    continue;
                }

                if (currentOccurrence ==
                    occurrence)
                {
                    return index;
                }

                currentOccurrence++;
            }

            return -1;
        }

        private sealed class
            TargetedAltarRescueHandler :
            CombatRescueTriggerHandler
        {
            private readonly InstanceId
                _rescueInstanceId;

            public TargetedAltarRescueHandler(
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog,
                InstanceId rescueInstanceId)
                : base(
                    metadataFactory,
                    eventLog)
            {
                if (!rescueInstanceId.IsValid)
                {
                    throw new ArgumentException(
                        "A valid Rescue InstanceId " +
                        "is required.",
                        nameof(rescueInstanceId));
                }

                _rescueInstanceId =
                    rescueInstanceId;
            }

            protected override bool CanRescue(
                CombatState state,
                DeathCombatEvent sourceEvent)
            {
                return sourceEvent.InstanceId ==
                       _rescueInstanceId;
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