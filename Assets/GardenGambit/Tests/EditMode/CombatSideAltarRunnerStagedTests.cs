using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatSideAltarRunnerStagedTests
    {
        [Test]
        public void
            StartAndResolveSideStaged_WithSacrificialAltar_EmitsHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveSideStaged(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCards[0]
                    .HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.RecipientCards[0]
                    .CurrentHp,
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
                environment.EventLog.Count,
                Is.EqualTo(5));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner
                    .UsesStagedActivationChain,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveSideStaged_WithWarAltar_DoesNotEmitHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveSideStaged(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCards[0]
                    .Attack,
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
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveSideStaged_WithTwoAltars_PreservesColumnOrder()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    includeSecondAltar: true);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveSideStaged(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.RecipientCards[0]
                    .HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.RecipientCards[1]
                    .HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.EqualTo(2));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
                Is.EqualTo(2));

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
                environment.EventLog.Count,
                Is.EqualTo(9));

            var firstAltarEvent =
                environment.EventLog.Events[1]
                    as
                        SacrificialAltarActivatedCombatEvent;

            var firstHpGainEvent =
                environment.EventLog.Events[2]
                    as HpGainCombatEvent;

            var secondAltarEvent =
                environment.EventLog.Events[5]
                    as
                        SacrificialAltarActivatedCombatEvent;

            var secondHpGainEvent =
                environment.EventLog.Events[6]
                    as HpGainCombatEvent;

            Assert.That(
                firstAltarEvent,
                Is.Not.Null);

            Assert.That(
                secondAltarEvent,
                Is.Not.Null);

            Assert.That(
                firstAltarEvent.DonorInstanceId,
                Is.EqualTo(
                    environment.DonorCards[0]
                        .InstanceId));

            Assert.That(
                secondAltarEvent.DonorInstanceId,
                Is.EqualTo(
                    environment.DonorCards[1]
                        .InstanceId));

            Assert.That(
                firstHpGainEvent,
                Is.Not.Null);

            Assert.That(
                secondHpGainEvent,
                Is.Not.Null);

            Assert.That(
                firstHpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.RecipientCards[0]
                        .InstanceId));

            Assert.That(
                secondHpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.RecipientCards[1]
                        .InstanceId));
        }

        [Test]
        public void
            StartAndResolveSideStaged_WhenBudgetExhausts_ResumePreservesStagedMode()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Runner
                        .StartAndResolveSideStaged(
                            environment
                                .CombatStartedEvent,
                            CombatSide.Player,
                            maximumPassCountPerAltar: 1,
                            maximumEventCountPerPass: 1,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.True);

            Assert.That(
                environment.Runner
                    .UsesStagedActivationChain,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.True);

            Assert.That(
                environment.Runner
                    .HasStagedExecution,
                Is.True);

            Assert.That(
                environment.Runner
                    .ActiveExecutionStage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                environment.Runner
                    .NextAltarPositionIndex,
                Is.Zero);

            Assert.That(
                environment.Runner
                    .ResolvedActivationCount,
                Is.Zero);

            Assert.That(
                environment.DonorCards[0]
                    .CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCards[0]
                    .HpCapacity,
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
                Is.Zero);

            var resolvedCount =
                environment.Runner
                    .ResumeActiveSide(
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCards[0]
                    .HpCapacity,
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
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Runner
                    .UsesStagedActivationChain,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveSideStaged_WithoutAltar_ReturnsZero()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.None);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveSideStaged(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.Zero);

            Assert.That(
                environment.DonorCards[0]
                    .CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCards[0]
                    .HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
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
                CombatSlotEnhanceKind enhanceKind,
                bool includeSecondAltar = false)
        {
            var slots =
                new List<CombatSlotState>();

            var cards =
                new List<CombatCardState>();

            var donorCards =
                new List<CombatCardState>();

            var recipientCards =
                new List<CombatCardState>();

            AddColumn(
                slots,
                cards,
                donorCards,
                recipientCards,
                columnValue: 1,
                donorInstanceId: 101,
                recipientInstanceId: 201,
                enhanceKind: enhanceKind,
                firstSlotId: 1);

            if (includeSecondAltar)
            {
                AddColumn(
                    slots,
                    cards,
                    donorCards,
                    recipientCards,
                    columnValue: 2,
                    donorInstanceId: 102,
                    recipientInstanceId: 202,
                    enhanceKind: enhanceKind,
                    firstSlotId: 3);
            }

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        slots),
                    new CombatCardRegistry(
                        cards),
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
                    new CombatSideAltarRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        resolutionEngine),

                CombatStartedEvent =
                    combatStartedEvent,

                EventLog =
                    eventLog,

                DonorCards =
                    donorCards,

                RecipientCards =
                    recipientCards
            };
        }

        private static void AddColumn(
            ICollection<CombatSlotState> slots,
            ICollection<CombatCardState> cards,
            ICollection<CombatCardState> donorCards,
            ICollection<CombatCardState>
                recipientCards,
            int columnValue,
            long donorInstanceId,
            long recipientInstanceId,
            CombatSlotEnhanceKind enhanceKind,
            long firstSlotId)
        {
            var column =
                new BoardColumn(
                    columnValue);

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column);

            var donorCard =
                CreateCard(
                    donorInstanceId);

            var recipientCard =
                CreateCard(
                    recipientInstanceId);

            slots.Add(
                new CombatSlotState(
                    new SlotId(firstSlotId),
                    donorPosition,
                    donorCard.InstanceId,
                    enhanceKind));

            slots.Add(
                new CombatSlotState(
                    new SlotId(firstSlotId + 1),
                    recipientPosition,
                    recipientCard.InstanceId));

            cards.Add(
                donorCard);

            cards.Add(
                recipientCard);

            donorCards.Add(
                donorCard);

            recipientCards.Add(
                recipientCard);
        }

        private static CombatCardState CreateCard(
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(instanceId),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: instanceId < 200
                    ? 4
                    : 5,
                armor: 0,
                attack: instanceId < 200
                    ? 6
                    : 3);
        }

        private sealed class TestEnvironment
        {
            public CombatSideAltarRunner Runner
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

            public IReadOnlyList<CombatCardState>
                DonorCards
            {
                get;
                set;
            }

            public IReadOnlyList<CombatCardState>
                RecipientCards
            {
                get;
                set;
            }
        }
    }
}