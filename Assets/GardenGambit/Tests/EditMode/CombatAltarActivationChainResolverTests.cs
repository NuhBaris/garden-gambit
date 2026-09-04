using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationChainResolverTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationChainResolver(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.ResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationChainResolver(
                        environment.State,
                        null,
                        environment.EventLog,
                        environment.ResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationChainResolver(
                        environment.State,
                        environment.MetadataFactory,
                        null,
                        environment.ResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullResolutionEngine_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationChainResolver(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void TryActivateAndCompleteChain_WithSacrificialAltar_CompletesDeathRemoval()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteChain(
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
                environment.RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.PlayerSide.GetCardAt(
                    environment.RecipientPosition),
                Is.SameAs(
                    environment.RecipientCard));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.Throws<KeyNotFoundException>(
                () => environment.PlayerSide.Cards
                    .GetCard(
                        environment.DonorCard
                            .InstanceId));

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
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Resolver
                    .ActiveAltarEvent,
                Is.Null);

            Assert.That(
                environment.Resolver
                    .HasPendingResolution,
                Is.False);

            Assert.That(
                environment.EventQueue.HasPending,
                Is.False);
        }

        [Test]
        public void TryActivateAndCompleteChain_WithWarAltar_CompletesDeathRemoval()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.WarAltar);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteChain(
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
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.PlayerSide.Cards.Count,
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
                environment.Resolver
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void TryActivateAndCompleteChain_WithoutRecipient_ReturnsNullWithoutStartingChain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    includeRecipient: false);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteChain(
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
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.False);
        }

        [Test]
        public void TryActivateAndCompleteChain_WithoutAltarEnhance_ReturnsNullWithoutStartingChain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.None);

            var altarEvent =
                environment.Resolver
                    .TryActivateAndCompleteChain(
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
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.False);
        }

        [Test]
        public void TryActivateAndCompleteChain_WhenPassBudgetIsExhausted_PreservesChainForRetry()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 1,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Resolver.HasActiveChain,
                Is.True);

            var activeAltarEvent =
                environment.Resolver
                    .ActiveAltarEvent;

            Assert.That(
                activeAltarEvent,
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
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

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

            var resumedAltarEvent =
                environment.Resolver
                    .ResumeActiveChain(
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resumedAltarEvent,
                Is.SameAs(activeAltarEvent));

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
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

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
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResumeActiveChain_WithoutActiveChain_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ResumeActiveChain(
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivateAndCompleteChain_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        null,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivateAndCompleteChain_WithInvalidDonorPosition_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        environment.CombatStartedEvent,
                        default(BoardPosition),
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivateAndCompleteChain_WithInvalidPassBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 0,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivateAndCompleteChain_WithInvalidEventBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 0,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivateAndCompleteChain_WithInvalidTriggerBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryActivateAndCompleteChain(
                        environment.CombatStartedEvent,
                        environment.DonorPosition,
                        maximumPassCount: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 0));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
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
                        new CombatSlotState[0]),
                    new CombatCardRegistry(
                        new CombatCardState[0]),
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
                CreateMetadataFactory();

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
                    new ICombatTriggerSource[0]);

            var resolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            var resolver =
                new CombatAltarActivationChainResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    resolutionEngine);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                CombatStartedEvent =
                    combatStartedEvent,
                ResolutionEngine =
                    resolutionEngine,
                Resolver = resolver,
                DonorPosition = donorPosition,
                RecipientPosition =
                    recipientPosition,
                DonorCard = donorCard,
                RecipientCard =
                    includeRecipient
                        ? recipientCard
                        : null
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

            public CombatSideState PlayerSide
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

            public BoardPosition DonorPosition
            {
                get;
                set;
            }

            public BoardPosition RecipientPosition
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