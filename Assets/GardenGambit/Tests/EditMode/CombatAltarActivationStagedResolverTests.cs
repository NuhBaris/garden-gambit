using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationStagedResolverTests
    {
        [Test]
        public void
            TryStartActivation_WithSacrificialAltar_AppendsAltarThenHpGainAndKeepsDonorAlive()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var executionState =
                environment.Resolver
                    .TryStartActivation(
                        environment.State,
                        environment.CombatStartedEvent,
                        environment.DonorPosition);

            Assert.That(
                executionState,
                Is.Not.Null);

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                executionState.AltarEvent,
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

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
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    executionState.AltarEvent));

            var hpGainEvent =
                environment.EventLog.Events[2]
                    as HpGainCombatEvent;

            Assert.That(
                hpGainEvent,
                Is.Not.Null);

            Assert.That(
                hpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.RecipientCard
                        .InstanceId));

            Assert.That(
                hpGainEvent.TargetPosition,
                Is.EqualTo(
                    environment.RecipientPosition));

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(4));

            Assert.That(
                hpGainEvent.CapacityGainedAmount,
                Is.EqualTo(4));

            Assert.That(
                hpGainEvent.IsHpStatGain,
                Is.True);

            Assert.That(
                hpGainEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    executionState.AltarEvent
                        .Metadata.EventId));
        }

        [Test]
        public void
            TryStartActivation_WithWarAltar_AppliesAttackWithoutHpGainOrDonorDeath()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var executionState =
                environment.Resolver
                    .TryStartActivation(
                        environment.State,
                        environment.CombatStartedEvent,
                        environment.DonorPosition);

            Assert.That(
                executionState,
                Is.Not.Null);

            Assert.That(
                executionState.AltarEvent,
                Is.TypeOf<
                    WarAltarActivatedCombatEvent>());

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    executionState.AltarEvent));
        }

        [Test]
        public void
            TryStartActivation_WithoutRecipient_ReturnsNullWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    includeRecipient: false);

            var executionState =
                environment.Resolver
                    .TryStartActivation(
                        environment.State,
                        environment.CombatStartedEvent,
                        environment.DonorPosition);

            Assert.That(
                executionState,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryStartActivation_CalledTwice_DoesNotRepeatTransfer()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            environment.Resolver
                .TryStartActivation(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Resolver
                        .TryStartActivation(
                            environment.State,
                            environment
                                .CombatStartedEvent,
                            environment.DonorPosition));

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
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void
            StartDonorDeath_WithNullExecutionState_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () =>
                    environment.Resolver
                        .StartDonorDeath(
                            null));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            StartDonorDeath_BeforeTransferTriggersResolved_ThrowsWithoutKillingDonor()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var executionState =
                environment.Resolver
                    .TryStartActivation(
                        environment.State,
                        environment.CombatStartedEvent,
                        environment.DonorPosition);

            Assert.Throws<InvalidOperationException>(
                () =>
                    environment.Resolver
                        .StartDonorDeath(
                            executionState));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .TransferApplied));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void
            StartDonorDeath_AfterSacrificialTransferTriggers_AppendsDeathAfterHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var executionState =
                environment.Resolver
                    .TryStartActivation(
                        environment.State,
                        environment.CombatStartedEvent,
                        environment.DonorPosition);

            executionState
                .MarkTransferTriggersResolved();

            var deathEvent =
                environment.Resolver
                    .StartDonorDeath(
                        executionState);

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .DonorDeathStarted));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    executionState.AltarEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<HpGainCombatEvent>());

            Assert.That(
                environment.EventLog.Events[3],
                Is.SameAs(
                    deathEvent));

            Assert.That(
                deathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    executionState.AltarEvent
                        .Metadata.EventId));
        }

        [Test]
        public void
            StartDonorDeath_AfterWarTransferTriggers_AppendsDeathWithoutHpGain()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var executionState =
                environment.Resolver
                    .TryStartActivation(
                        environment.State,
                        environment.CombatStartedEvent,
                        environment.DonorPosition);

            executionState
                .MarkTransferTriggersResolved();

            var deathEvent =
                environment.Resolver
                    .StartDonorDeath(
                        executionState);

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatAltarActivationExecutionStage
                        .DonorDeathStarted));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    executionState.AltarEvent));

            Assert.That(
                environment.EventLog.Events[2],
                Is.SameAs(
                    deathEvent));
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatSlotEnhanceKind enhanceKind,
                bool includeRecipient = true)
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(2));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
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

            return new TestEnvironment
            {
                State =
                    state,

                EventLog =
                    eventLog,

                CombatStartedEvent =
                    combatStartedEvent,

                Resolver =
                    new CombatAltarActivationResolver(
                        metadataFactory,
                        eventLog),

                DonorPosition =
                    donorPosition,

                RecipientPosition =
                    recipientPosition,

                DonorCard =
                    donorCard,

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

        private sealed class TestEnvironment
        {
            public CombatState State
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

            public CombatAltarActivationResolver Resolver
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