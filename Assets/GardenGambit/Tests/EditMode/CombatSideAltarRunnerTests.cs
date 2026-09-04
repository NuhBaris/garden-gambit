using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatSideAltarRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSideAltarRunner(
                        null,
                        environment.MetadataFactory,
                        environment.EventLog,
                        environment.ResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSideAltarRunner(
                        environment.State,
                        null,
                        environment.EventLog,
                        environment.ResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSideAltarRunner(
                        environment.State,
                        environment.MetadataFactory,
                        null,
                        environment.ResolutionEngine));
        }

        [Test]
        public void Constructor_WithNullResolutionEngine_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatSideAltarRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void StartAndResolveSide_WithTwoAltars_ResolvesLeftToRight()
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
                    });

            var resolvedCount =
                environment.Runner.StartAndResolveSide(
                    environment.CombatStartedEvent,
                    CombatSide.Player,
                    maximumPassCountPerAltar: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(2));

            var firstAltarEvent =
                environment.EventLog.Events[1]
                    as
                    SacrificialAltarActivatedCombatEvent;

            Assert.That(
                firstAltarEvent,
                Is.Not.Null);

            Assert.That(
                firstAltarEvent.DonorPosition,
                Is.EqualTo(firstDonorPosition));

            var secondAltarEvent =
                environment.EventLog.Events[4]
                    as WarAltarActivatedCombatEvent;

            Assert.That(
                secondAltarEvent,
                Is.Not.Null);

            Assert.That(
                secondAltarEvent.DonorPosition,
                Is.EqualTo(secondDonorPosition));

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
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(7));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveSide_WithFrontAndBackAltars_ResolvesFrontThenSkipsEmptiedBackSlot()
        {
            var frontPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 2);

            var backPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 2);

            var frontDonor =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var backCard =
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
                            backPosition,
                            backCard,
                            CombatSlotEnhanceKind
                                .WarAltar),

                        CreateOccupiedSlot(
                            2,
                            frontPosition,
                            frontDonor,
                            CombatSlotEnhanceKind
                                .SacrificialAltar)
                    },
                    new[]
                    {
                        frontDonor,
                        backCard
                    });

            var resolvedCount =
                environment.Runner.StartAndResolveSide(
                    environment.CombatStartedEvent,
                    CombatSide.Player,
                    maximumPassCountPerAltar: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                backCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                backCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.PlayerSide.GetCardAt(
                    frontPosition),
                Is.SameAs(backCard));

            Assert.That(
                environment.PlayerSide.Board
                    .GetSlot(backPosition)
                    .IsOccupied,
                Is.False);

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
                environment.Runner.HasActiveSide,
                Is.False);
        }

        [Test]
        public void StartAndResolveSide_WithoutRecipient_SkipsAltar()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    includeRecipient: false);

            var resolvedCount =
                environment.Runner.StartAndResolveSide(
                    environment.CombatStartedEvent,
                    CombatSide.Player,
                    maximumPassCountPerAltar: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.Zero);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);
        }

        [Test]
        public void StartAndResolveSide_WithoutAltars_ReturnsZero()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind.None);

            var resolvedCount =
                environment.Runner.StartAndResolveSide(
                    environment.CombatStartedEvent,
                    CombatSide.Player,
                    maximumPassCountPerAltar: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.Zero);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);
        }

        [Test]
        public void StartAndResolveSide_WithEnemyAltar_ResolvesEnemySide()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Enemy,
                    CombatSlotEnhanceKind.WarAltar);

            var resolvedCount =
                environment.Runner.StartAndResolveSide(
                    environment.CombatStartedEvent,
                    CombatSide.Enemy,
                    maximumPassCountPerAltar: 10,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                environment.EnemySide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EnemySide.Board
                    .GetSlot(
                        environment.DonorPosition)
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
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveSide_WhenBudgetIsExhausted_ResumeDoesNotRepeatActivation()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 1,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.True);

            Assert.That(
                environment.Runner.ActiveSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.True);

            Assert.That(
                environment.Runner
                    .NextAltarPositionIndex,
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

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            var resolvedCount =
                environment.Runner.ResumeActiveSide(
                    maximumPassCountPerAltar: 10,
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
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Runner.ActiveSide,
                Is.Null);

            Assert.That(
                environment.Runner
                    .HasPendingResolution,
                Is.False);
        }

        [Test]
        public void ResumeActiveSide_WithoutActiveSide_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveSide(
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void StartAndResolveSide_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        null,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
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
        public void StartAndResolveSide_WithInvalidSide_Throws()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        environment.CombatStartedEvent,
                        default(CombatSide),
                        maximumPassCountPerAltar: 10,
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
        public void StartAndResolveSide_WithInvalidPassBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 0,
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
        public void StartAndResolveSide_WithInvalidEventBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
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
        public void StartAndResolveSide_WithInvalidTriggerBudget_ThrowsBeforeActivation()
        {
            var environment =
                CreateSingleAltarEnvironment(
                    CombatSide.Player,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveSide(
                        environment.CombatStartedEvent,
                        CombatSide.Player,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 0));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateSingleAltarEnvironment(
                CombatSide side,
                CombatSlotEnhanceKind enhanceKind,
                bool includeRecipient = true)
        {
            var donorPosition =
                CreatePosition(
                    side,
                    BoardRow.Back,
                    column: 2);

            var recipientPosition =
                CreatePosition(
                    side,
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

            var slots =
                includeRecipient
                    ? new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            donorPosition,
                            donorCard,
                            enhanceKind),

                        CreateOccupiedSlot(
                            2,
                            recipientPosition,
                            recipientCard,
                            CombatSlotEnhanceKind.None)
                    }
                    : new[]
                    {
                        CreateOccupiedSlot(
                            1,
                            donorPosition,
                            donorCard,
                            enhanceKind),

                        new CombatSlotState(
                            new SlotId(2),
                            recipientPosition)
                    };

            var cards =
                includeRecipient
                    ? new[]
                    {
                        donorCard,
                        recipientCard
                    }
                    : new[]
                    {
                        donorCard
                    };

            TestEnvironment environment;

            if (side == CombatSide.Player)
            {
                environment =
                    CreateEnvironment(
                        slots,
                        cards);
            }
            else
            {
                environment =
                    CreateEnvironment(
                        new CombatSlotState[0],
                        new CombatCardState[0],
                        slots,
                        cards);
            }

            environment.DonorPosition =
                donorPosition;

            environment.RecipientPosition =
                recipientPosition;

            environment.DonorCard =
                donorCard;

            environment.RecipientCard =
                includeRecipient
                    ? recipientCard
                    : null;

            return environment;
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatSlotState[] playerSlots,
                CombatCardState[] playerCards,
                CombatSlotState[] enemySlots = null,
                CombatCardState[] enemyCards = null)
        {
            if (enemySlots == null)
            {
                enemySlots =
                    new CombatSlotState[0];
            }

            if (enemyCards == null)
            {
                enemyCards =
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
                    enemySlots,
                    enemyCards);

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

            var runner =
                new CombatSideAltarRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    resolutionEngine);

            return new TestEnvironment
            {
                State = state,
                PlayerSide = playerSide,
                EnemySide = enemySide,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                EventQueue = eventQueue,
                CombatStartedEvent =
                    combatStartedEvent,
                ResolutionEngine =
                    resolutionEngine,
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

            public CombatSideState EnemySide
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

            public CombatSideAltarRunner Runner
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