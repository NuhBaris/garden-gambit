using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatAltarRunnerTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarRunner(
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
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarRunner(
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
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarRunner(
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
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarRunner(
                        environment.State,
                        environment.MetadataFactory,
                        environment.EventLog,
                        null));
        }

        [Test]
        public void StartAndResolveAllAltars_WithBothSides_ResolvesPlayerBeforeEnemy()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(2));

            var playerAltarEvent =
                environment.EventLog.Events[1]
                    as
                    SacrificialAltarActivatedCombatEvent;

            Assert.That(
                playerAltarEvent,
                Is.Not.Null);

            Assert.That(
                playerAltarEvent.DonorPosition.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                playerAltarEvent.DonorPosition,
                Is.EqualTo(
                    environment.PlayerDonorPosition));

            var enemyAltarEvent =
                environment.EventLog.Events[4]
                    as WarAltarActivatedCombatEvent;

            Assert.That(
                enemyAltarEvent,
                Is.Not.Null);

            Assert.That(
                enemyAltarEvent.DonorPosition.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                enemyAltarEvent.DonorPosition,
                Is.EqualTo(
                    environment.EnemyDonorPosition));

            Assert.That(
                environment.PlayerRecipient.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.PlayerRecipient.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.EnemyRecipient.Attack,
                Is.EqualTo(9));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.EnemySide.Cards.Count,
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
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);

            Assert.That(
                environment.Runner
                    .HasPendingEventResolution,
                Is.False);

            Assert.That(
                environment.Runner.NextSide,
                Is.Null);
        }

        [Test]
        public void StartAndResolveAllAltars_WithoutAltars_ReturnsZero()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: false,
                    enemyHasAltar: false);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.Zero);

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllAltars_WithOnlyEnemyAltar_ResolvesEnemy()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: false,
                    enemyHasAltar: true);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.PlayerSide.Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EnemyRecipient.Attack,
                Is.EqualTo(9));

            Assert.That(
                environment.EnemySide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .WarAltarActivated),
                Is.EqualTo(1));

            var altarEvent =
                environment.EventLog.Events[1]
                    as WarAltarActivatedCombatEvent;

            Assert.That(
                altarEvent,
                Is.Not.Null);

            Assert.That(
                altarEvent.DonorPosition.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllAltars_WhenPlayerBudgetIsExhausted_ResumeContinuesWithEnemy()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 1,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.True);

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.True);

            Assert.That(
                environment.Runner.ActiveSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                environment.Runner.NextSide,
                Is.EqualTo(
                    CombatSide.Player));

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

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            var resolvedCount =
                environment.Runner
                    .ResumeActiveResolution(
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(2));

            Assert.That(
                environment.PlayerRecipient.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.PlayerRecipient.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                environment.EnemyRecipient.Attack,
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
                environment.Runner.HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.NextSide,
                Is.Null);
        }

        [Test]
        public void StartAndResolveAllAltars_WhenEnemyBudgetIsExhausted_ResumeDoesNotRepeatPlayer()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: false,
                    enemyHasAltar: true);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 1,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.True);

            Assert.That(
                environment.Runner.ActiveSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                environment.Runner.NextSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .SacrificialAltarActivated),
                Is.Zero);

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind
                        .WarAltarActivated),
                Is.EqualTo(1));

            var resolvedCount =
                environment.Runner
                    .ResumeActiveResolution(
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100);

            Assert.That(
                resolvedCount,
                Is.EqualTo(1));

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyRecipient.Attack,
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
                    CombatEventKind.Death),
                Is.EqualTo(1));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.DeathRemoval),
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllAltars_WithUnloggedCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            var unloggedEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        unloggedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner.HasActiveResolution,
                Is.False);
        }

        [Test]
        public void StartAndResolveAllAltars_WithDifferentCombatStartedEventInstance_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            var differentInstance =
                new CombatStartedCombatEvent(
                    environment.CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        differentInstance,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ResumeActiveResolution_WithoutActiveResolution_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<InvalidOperationException>(
                () => environment.Runner
                    .ResumeActiveResolution(
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void StartAndResolveAllAltars_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentNullException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        null,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void StartAndResolveAllAltars_WithInvalidPassBudget_ThrowsBeforeResolution()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 0,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.PlayerDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EnemyDonor.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void StartAndResolveAllAltars_WithInvalidEventBudget_ThrowsBeforeResolution()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 0,
                        maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void StartAndResolveAllAltars_WithInvalidTriggerBudget_ThrowsBeforeResolution()
        {
            var environment =
                CreateEnvironment(
                    playerHasAltar: true,
                    enemyHasAltar: true);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Runner
                    .StartAndResolveAllAltars(
                        environment.CombatStartedEvent,
                        maximumPassCountPerAltar: 10,
                        maximumEventCountPerPass: 100,
                        maximumTriggerCountPerEvent: 0));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerHasAltar,
                bool enemyHasAltar)
        {
            var playerDonorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(3));

            var playerRecipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(3));

            var enemyDonorPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    new BoardColumn(1));

            var enemyRecipientPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var playerDonor =
                CreateCard(
                    100,
                    currentHp: 4,
                    attack: 6);

            var playerRecipient =
                CreateCard(
                    200,
                    currentHp: 5,
                    attack: 3);

            var enemyDonor =
                CreateCard(
                    300,
                    currentHp: 4,
                    attack: 6);

            var enemyRecipient =
                CreateCard(
                    400,
                    currentHp: 5,
                    attack: 3);

            var playerSlots =
                new[]
                {
                    CreateOccupiedSlot(
                        1,
                        playerDonorPosition,
                        playerDonor,
                        playerHasAltar
                            ? CombatSlotEnhanceKind
                                .SacrificialAltar
                            : CombatSlotEnhanceKind.None),

                    CreateOccupiedSlot(
                        2,
                        playerRecipientPosition,
                        playerRecipient,
                        CombatSlotEnhanceKind.None)
                };

            var enemySlots =
                new[]
                {
                    CreateOccupiedSlot(
                        1,
                        enemyDonorPosition,
                        enemyDonor,
                        enemyHasAltar
                            ? CombatSlotEnhanceKind
                                .WarAltar
                            : CombatSlotEnhanceKind.None),

                    CreateOccupiedSlot(
                        2,
                        enemyRecipientPosition,
                        enemyRecipient,
                        CombatSlotEnhanceKind.None)
                };

            var playerSide =
                CreateSideState(
                    CombatSide.Player,
                    playerSlots,
                    new[]
                    {
                        playerDonor,
                        playerRecipient
                    });

            var enemySide =
                CreateSideState(
                    CombatSide.Enemy,
                    enemySlots,
                    new[]
                    {
                        enemyDonor,
                        enemyRecipient
                    });

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
                new CombatAltarRunner(
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
                CombatStartedEvent =
                    combatStartedEvent,
                ResolutionEngine =
                    resolutionEngine,
                Runner = runner,
                PlayerDonorPosition =
                    playerDonorPosition,
                EnemyDonorPosition =
                    enemyDonorPosition,
                PlayerDonor = playerDonor,
                PlayerRecipient =
                    playerRecipient,
                EnemyDonor = enemyDonor,
                EnemyRecipient =
                    enemyRecipient
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

            public CombatAltarRunner Runner
            {
                get;
                set;
            }

            public BoardPosition
                PlayerDonorPosition
            {
                get;
                set;
            }

            public BoardPosition
                EnemyDonorPosition
            {
                get;
                set;
            }

            public CombatCardState PlayerDonor
            {
                get;
                set;
            }

            public CombatCardState PlayerRecipient
            {
                get;
                set;
            }

            public CombatCardState EnemyDonor
            {
                get;
                set;
            }

            public CombatCardState EnemyRecipient
            {
                get;
                set;
            }
        }
    }
}