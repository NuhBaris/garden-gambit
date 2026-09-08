using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationStagedChainResolverTests
    {
        [Test]
        public void
            TryActivateAndCompleteStagedChain_WithSacrificialAltar_CompletesHpGainAndDeath()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteStagedChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                altarEvent,
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

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
                environment.EventLog.Events[1],
                Is.SameAs(
                    altarEvent));

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
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Resolver
                    .HasStagedExecution,
                Is.False);

            Assert.That(
                environment.Resolver
                    .ActiveExecutionState,
                Is.Null);

            Assert.That(
                environment.Resolver.ActiveStage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .Unspecified));

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            TryActivateAndCompleteStagedChain_WithWarAltar_CompletesWithoutHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteStagedChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                altarEvent,
                Is.TypeOf<
                    WarAltarActivatedCombatEvent>());

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

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
                environment.Resolver.HasActiveChain,
                Is.False);

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            TryActivateAndCompleteStagedChain_WithoutRecipient_ReturnsNullWithoutStartingChain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    includeRecipient: false);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteStagedChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                altarEvent,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Resolver
                    .HasStagedExecution,
                Is.False);
        }

        [Test]
        public void
            StagedChain_WhenTransferDrainExhaustsBudget_ResumeDoesNotRepeatHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Resolver
                        .TryActivateAndCompleteStagedChain(
                            environment.CombatStartedEvent,
                            environment.DonorPosition,
                            maximumPassCount: 1,
                            maximumEventCountPerPass: 1,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.True);

            Assert.That(
                environment.Resolver
                    .HasStagedExecution,
                Is.True);

            Assert.That(
                environment.Resolver.ActiveStage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            var activeAltarEvent =
                environment.Resolver
                    .ActiveAltarEvent;

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

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
                Is.Zero);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Resolver
                        .TryActivateAndCompleteStagedChain(
                            environment.CombatStartedEvent,
                            environment.DonorPosition,
                            maximumPassCount: 10,
                            maximumEventCountPerPass: 100,
                            maximumTriggerCountPerEvent: 100));

            var resumedEvent =
                environment.Resolver
                    .ResumeActiveChain(
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resumedEvent,
                Is.SameAs(
                    activeAltarEvent));

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
                environment.Resolver.HasActiveChain,
                Is.False);

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            StagedChain_WhenDeathDrainExhaustsBudget_ResumeDoesNotRepeatDeath()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Resolver
                        .TryActivateAndCompleteStagedChain(
                            environment.CombatStartedEvent,
                            environment.DonorPosition,
                            maximumPassCount: 1,
                            maximumEventCountPerPass: 100,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.True);

            Assert.That(
                environment.Resolver.ActiveStage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .DonorDeathStarted));

            var activeAltarEvent =
                environment.Resolver
                    .ActiveAltarEvent;

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

            var resumedEvent =
                environment.Resolver
                    .ResumeActiveChain(
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resumedEvent,
                Is.SameAs(
                    activeAltarEvent));

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
                environment.Resolver.HasActiveChain,
                Is.False);

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);
        }

        [Test]
        public void
            TryActivateAndCompleteStagedChain_WithInvalidBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    environment.Resolver
                        .TryActivateAndCompleteStagedChain(
                            environment.CombatStartedEvent,
                            environment.DonorPosition,
                            maximumPassCount: 0,
                            maximumEventCountPerPass: 100,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver.HasActiveChain,
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
                bool includeRecipient = true)
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

            CombatSlotState[] playerSlots;
            CombatCardState[] playerCards;

            if (includeRecipient)
            {
                playerSlots =
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
                    };

                playerCards =
                    new[]
                    {
                        donorCard,
                        recipientCard
                    };
            }
            else
            {
                playerSlots =
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            donorPosition,
                            donorCard.InstanceId,
                            enhanceKind),

                        new CombatSlotState(
                            new SlotId(2),
                            recipientPosition)
                    };

                playerCards =
                    new[]
                    {
                        donorCard
                    };
            }

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        playerSlots),
                    new CombatCardRegistry(
                        playerCards),
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
                PlayerSide =
                    playerSide,

                DonorCard =
                    donorCard,

                RecipientCard =
                    includeRecipient
                        ? recipientCard
                        : null,

                DonorPosition =
                    donorPosition,

                EventLog =
                    eventLog,

                CombatStartedEvent =
                    combatStartedEvent,

                ResolutionEngine =
                    resolutionEngine,

                Resolver =
                    new
                        CombatAltarActivationChainResolver(
                            state,
                            metadataFactory,
                            eventLog,
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

            public CombatStartedCombatEvent
                CombatStartedEvent
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

            public CombatAltarActivationChainResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}