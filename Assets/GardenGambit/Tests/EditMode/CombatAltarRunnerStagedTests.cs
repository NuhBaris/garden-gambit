using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarRunnerStagedTests
    {
        [Test]
        public void
            StartAndResolveAllAltarsStaged_WithBothSides_PreservesPlayerEnemyOrder()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveAllAltarsStaged(
                        environment.CombatStartedEvent,
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
                environment.EnemyRecipient.HpCapacity,
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

            var playerAltarEvent =
                environment.EventLog.Events[1]
                    as
                        SacrificialAltarActivatedCombatEvent;

            var playerHpGainEvent =
                environment.EventLog.Events[2]
                    as HpGainCombatEvent;

            var enemyAltarEvent =
                environment.EventLog.Events[5]
                    as
                        SacrificialAltarActivatedCombatEvent;

            var enemyHpGainEvent =
                environment.EventLog.Events[6]
                    as HpGainCombatEvent;

            Assert.That(
                playerAltarEvent,
                Is.Not.Null);

            Assert.That(
                enemyAltarEvent,
                Is.Not.Null);

            Assert.That(
                playerAltarEvent.DonorPosition.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                enemyAltarEvent.DonorPosition.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                playerHpGainEvent.TargetPosition.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                enemyHpGainEvent.TargetPosition.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner
                    .UsesStagedActivationChain,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveAllAltarsStaged_WithSacrificialAndWar_EmitsHpGainOnlyForSacrificial()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    CombatSlotEnhanceKind
                        .WarAltar);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveAllAltarsStaged(
                        environment.CombatStartedEvent,
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
                environment.EnemyRecipient.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyRecipient.CurrentHp,
                Is.EqualTo(5));

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
                    CombatEventKind.HpGain),
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
                Is.EqualTo(8));

            Assert.That(
                environment.EventLog.Events[1],
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<HpGainCombatEvent>());

            Assert.That(
                environment.EventLog.Events[5],
                Is.TypeOf<
                    WarAltarActivatedCombatEvent>());
        }

        [Test]
        public void
            StartAndResolveAllAltarsStaged_WhenPlayerBudgetExhausts_ResumeCompletesBothSidesWithoutRepeating()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Runner
                        .StartAndResolveAllAltarsStaged(
                            environment
                                .CombatStartedEvent,
                            maximumPassCountPerAltar: 1,
                            maximumEventCountPerPass: 1,
                            maximumTriggerCountPerEvent: 100));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
                Is.True);

            Assert.That(
                environment.Runner
                    .UsesStagedActivationChain,
                Is.True);

            Assert.That(
                environment.Runner.NextSideIndex,
                Is.Zero);

            Assert.That(
                environment.Runner.ActiveSide,
                Is.EqualTo(
                    CombatSide.Player));

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
                environment.PlayerRecipient.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                environment.EnemyRecipient.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                CountEvents(
                    environment.EventLog,
                    CombatEventKind.HpGain),
                Is.EqualTo(1));

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
                environment.EnemyRecipient.HpCapacity,
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
                environment.Runner
                    .HasActiveResolution,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveSide,
                Is.False);

            Assert.That(
                environment.Runner.HasActiveChain,
                Is.False);
        }

        [Test]
        public void
            StartAndResolveAllAltarsStaged_WithoutAltars_ReturnsZero()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.None,
                    CombatSlotEnhanceKind.None);

            var resolvedCount =
                environment.Runner
                    .StartAndResolveAllAltarsStaged(
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
                environment.PlayerRecipient.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.EnemyRecipient.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runner
                    .HasActiveResolution,
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
                CombatSlotEnhanceKind
                    playerEnhanceKind,
                CombatSlotEnhanceKind
                    enemyEnhanceKind)
        {
            CombatCardState playerDonor;
            CombatCardState playerRecipient;

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    playerEnhanceKind,
                    donorInstanceId: 100,
                    recipientInstanceId: 200,
                    firstSlotId: 1,
                    out playerDonor,
                    out playerRecipient);

            CombatCardState enemyDonor;
            CombatCardState enemyRecipient;

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    enemyEnhanceKind,
                    donorInstanceId: 300,
                    recipientInstanceId: 400,
                    firstSlotId: 3,
                    out enemyDonor,
                    out enemyRecipient);

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
                    new CombatAltarRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        resolutionEngine),

                CombatStartedEvent =
                    combatStartedEvent,

                EventLog =
                    eventLog,

                PlayerDonor =
                    playerDonor,

                PlayerRecipient =
                    playerRecipient,

                EnemyDonor =
                    enemyDonor,

                EnemyRecipient =
                    enemyRecipient
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatSlotEnhanceKind enhanceKind,
            long donorInstanceId,
            long recipientInstanceId,
            long firstSlotId,
            out CombatCardState donorCard,
            out CombatCardState recipientCard)
        {
            var column =
                new BoardColumn(1);

            var donorPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    column);

            var recipientPosition =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    column);

            donorCard =
                CreateCard(
                    donorInstanceId,
                    currentHp: 4,
                    attack: 6);

            recipientCard =
                CreateCard(
                    recipientInstanceId,
                    currentHp: 5,
                    attack: 3);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(firstSlotId),
                            donorPosition,
                            donorCard.InstanceId,
                            enhanceKind),

                        new CombatSlotState(
                            new SlotId(firstSlotId + 1),
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

        private sealed class TestEnvironment
        {
            public CombatAltarRunner Runner
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