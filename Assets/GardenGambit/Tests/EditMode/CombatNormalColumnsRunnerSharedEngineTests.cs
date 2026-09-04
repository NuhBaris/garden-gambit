using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalColumnsRunnerSharedEngineTests
    {
        [Test]
        public void Constructor_WithNullSharedEngine_Throws()
        {
            var environment =
                CreateEnvironment(
                    withPlayerAltar: false);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalColumnsRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void ResolveAllColumnsForStartedCombat_WithEmptyBoards_ResolvesFiveColumns()
        {
            var environment =
                CreateEnvironment(
                    withPlayerAltar: false);

            var resolvedExchangeCount =
                environment.NormalColumnsRunner
                    .ResolveAllColumnsForStartedCombat(
                        environment.CombatStartedEvent,
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedExchangeCount,
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
                environment.EventLog.Count,
                Is.EqualTo(6));

            Assert.That(
                environment.NormalColumnsRunner
                    .HasActiveCombat,
                Is.False);

            Assert.That(
                environment.NormalColumnsRunner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResolveAllColumnsForStartedCombat_AfterAltarChain_UsesSameEngineWithoutRepeatingDeathRemoval()
        {
            var environment =
                CreateEnvironment(
                    withPlayerAltar: true);

            var altarRunner =
                new CombatAltarRunner(
                    environment.State,
                    environment.MetadataFactory,
                    environment.EventLog,
                    environment.ResolutionEngine);

            var resolvedAltarCount =
                altarRunner.StartAndResolveAllAltars(
                    environment.CombatStartedEvent,
                    maximumPassCountPerAltar: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedAltarCount,
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

            var resolvedExchangeCount =
                environment.NormalColumnsRunner
                    .ResolveAllColumnsForStartedCombat(
                        environment.CombatStartedEvent,
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedExchangeCount,
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

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(9));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(
                        environment.DonorPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.ResolutionEngine
                    .HasPendingWork,
                Is.False);

            Assert.That(
                environment.NormalColumnsRunner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResolveAllColumnsForStartedCombat_WithUnloggedCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    withPlayerAltar: false);

            var unloggedEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment
                    .NormalColumnsRunner
                    .ResolveAllColumnsForStartedCombat(
                        unloggedEvent,
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.NormalColumnsRunner
                    .HasActiveCombat,
                Is.False);
        }

        [Test]
        public void ResolveAllColumnsForStartedCombat_WithDifferentEventInstance_Throws()
        {
            var environment =
                CreateEnvironment(
                    withPlayerAltar: false);

            var differentInstance =
                new CombatStartedCombatEvent(
                    environment.CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment
                    .NormalColumnsRunner
                    .ResolveAllColumnsForStartedCombat(
                        differentInstance,
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.NormalColumnsRunner
                    .HasActiveCombat,
                Is.False);
        }

        [Test]
        public void ResolveAllColumnsForStartedCombat_WhenColumnAlreadyStarted_Throws()
        {
            var environment =
                CreateEnvironment(
                    withPlayerAltar: false);

            new CombatColumnStartResolver(
                    environment.MetadataFactory,
                    environment.EventLog)
                .StartColumn(
                    environment.State,
                    environment.CombatStartedEvent,
                    new BoardColumn(1));

            Assert.Throws<InvalidOperationException>(
                () => environment
                    .NormalColumnsRunner
                    .ResolveAllColumnsForStartedCombat(
                        environment.CombatStartedEvent,
                        maximumExchangeCountPerColumn: 10,
                        maximumPassCountPerExchange: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.ColumnStarted),
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.NormalColumnsRunner
                    .HasActiveCombat,
                Is.False);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool withPlayerAltar)
        {
            CombatSlotState[] playerSlots;
            CombatCardState[] playerCards;

            CombatCardState donorCard = null;
            CombatCardState recipientCard = null;

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

            if (withPlayerAltar)
            {
                donorCard =
                    CreateCard(
                        instanceId: 100,
                        currentHp: 4,
                        attack: 6);

                recipientCard =
                    CreateCard(
                        instanceId: 200,
                        currentHp: 5,
                        attack: 3);

                playerSlots =
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
                    new CombatSlotState[0];

                playerCards =
                    new CombatCardState[0];
            }

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
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

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

            var combatStartedEvent =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog)
                    .Start(state);

            var normalColumnsRunner =
                new CombatNormalColumnsRunner(
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
                ResolutionEngine =
                    resolutionEngine,
                CombatStartedEvent =
                    combatStartedEvent,
                NormalColumnsRunner =
                    normalColumnsRunner,
                DonorCard = donorCard,
                RecipientCard =
                    recipientCard,
                DonorPosition =
                    donorPosition,
                RecipientPosition =
                    recipientPosition
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

            public CombatEventResolutionEngine
                ResolutionEngine
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

            public CombatNormalColumnsRunner
                NormalColumnsRunner
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

            public BoardPosition RecipientPosition
            {
                get;
                set;
            }
        }
    }
}