using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationResolver(
                        CreateMetadataFactory(),
                        null));
        }

        [Test]
        public void TryActivate_WithSacrificialAltar_AppliesTransferAndLogsEvent()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var altarEvent =
                environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

            Assert.That(
                altarEvent,
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

            var sacrificialEvent =
                (SacrificialAltarActivatedCombatEvent)
                    altarEvent;

            Assert.That(
                sacrificialEvent.TransferredHp,
                Is.EqualTo(4));

            Assert.That(
                sacrificialEvent.DonorInstanceId,
                Is.EqualTo(
                    environment.DonorCard.InstanceId));

            Assert.That(
                sacrificialEvent.RecipientInstanceId,
                Is.EqualTo(
                    environment.RecipientCard.InstanceId));

            Assert.That(
                sacrificialEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.CombatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

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
                environment.EventLog.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(altarEvent));

            var deathEvent =
                environment.EventLog.Events[2]
                    as DeathCombatEvent;

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.InstanceId,
                Is.EqualTo(
                    environment.DonorCard.InstanceId));

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);

            Assert.That(
                deathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    altarEvent.Metadata.EventId));
        }

        [Test]
        public void TryActivate_WithWarAltar_AppliesTransferAndLogsEvent()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.WarAltar);

            var altarEvent =
                environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

            Assert.That(
                altarEvent,
                Is.TypeOf<
                    WarAltarActivatedCombatEvent>());

            var warEvent =
                (WarAltarActivatedCombatEvent)
                    altarEvent;

            Assert.That(
                warEvent.TransferredAttack,
                Is.EqualTo(6));

            Assert.That(
                warEvent.DonorPreviousHp,
                Is.EqualTo(4));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

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
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(altarEvent));

            var deathEvent =
                environment.EventLog.Events[2]
                    as DeathCombatEvent;

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.InstanceId,
                Is.EqualTo(
                    environment.DonorCard.InstanceId));

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);

            Assert.That(
                deathEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    altarEvent.Metadata.EventId));
        }

        [Test]
        public void TryActivate_WithZeroAttackWarAltar_StillSetsDonorToZeroAndLogsEvent()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.WarAltar,
                    donorAttack: 0);

            var altarEvent =
                environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

            var warEvent =
                altarEvent as
                    WarAltarActivatedCombatEvent;

            Assert.That(
                warEvent,
                Is.Not.Null);

            Assert.That(
                warEvent.TransferredAttack,
                Is.Zero);

            Assert.That(
                warEvent.HasPositiveTransfer,
                Is.False);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(3));

            var deathEvent =
                environment.EventLog.Events[2]
                    as DeathCombatEvent;

            Assert.That(
                deathEvent,
                Is.Not.Null);

            Assert.That(
                deathEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                deathEvent.CurrentHp,
                Is.Zero);
        }

        [Test]
        public void TryActivate_WithoutRecipient_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    includeRecipient: false);

            var altarEvent =
                environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

            Assert.That(
                altarEvent,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.DonorCard.Attack,
                Is.EqualTo(6));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WithoutAltarEnhance_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind.None);

            var altarEvent =
                environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

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
        }

        [Test]
        public void TryActivate_WithDeathThresholdDonor_ReturnsNullWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 0);

            var altarEvent =
                environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition);

            Assert.That(
                altarEvent,
                Is.Null);

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WithNullState_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.TryActivate(
                    null,
                    environment.CombatStartedEvent,
                    environment.DonorPosition));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WithNullCombatStartedEvent_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver.TryActivate(
                    environment.State,
                    null,
                    environment.DonorPosition));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WithInvalidDonorPosition_Throws()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    default(BoardPosition)));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WithUnloggedCombatStartedEvent_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var unloggedEvent =
                new CombatStartedCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.TryActivate(
                    environment.State,
                    unloggedEvent,
                    environment.DonorPosition));

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
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WithDifferentCombatStartedEventInstance_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            var differentInstance =
                new CombatStartedCombatEvent(
                    environment.CombatStartedEvent
                        .Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver.TryActivate(
                    environment.State,
                    differentInstance,
                    environment.DonorPosition));

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
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void TryActivate_WhenDonorAlreadyActivated_ThrowsWithoutApplyingSecondTransfer()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            environment.Resolver.TryActivate(
                environment.State,
                environment.CombatStartedEvent,
                environment.DonorPosition);

            environment.DonorCard.Heal(1);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(1));

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
                Is.TypeOf<
                    SacrificialAltarActivatedCombatEvent>());

            Assert.That(
                environment.EventLog.Events[2],
                Is.TypeOf<DeathCombatEvent>());
        }

        [Test]
        public void TryActivate_WhenTransferWouldOverflow_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 1,
                    recipientHpCapacity: int.MaxValue,
                    recipientCurrentHp: 5);

            Assert.Throws<OverflowException>(
                () => environment.Resolver.TryActivate(
                    environment.State,
                    environment.CombatStartedEvent,
                    environment.DonorPosition));

            Assert.That(
                environment.DonorCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                environment.RecipientCard.HpCapacity,
                Is.EqualTo(int.MaxValue));

            Assert.That(
                environment.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatSlotEnhanceKind enhanceKind,
                bool includeRecipient = true,
                int donorCurrentHp = 4,
                int donorAttack = 6,
                int recipientHpCapacity = 10,
                int recipientCurrentHp = 5,
                int recipientAttack = 3)
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
                    currentHp: donorCurrentHp,
                    attack: donorAttack);

            var recipientCard =
                CreateCard(
                    instanceId: 200,
                    hpCapacity: recipientHpCapacity,
                    currentHp: recipientCurrentHp,
                    attack: recipientAttack);

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

            return new TestEnvironment
            {
                State = state,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                CombatStartedEvent =
                    combatStartedEvent,
                Resolver =
                    new CombatAltarActivationResolver(
                        metadataFactory,
                        eventLog),
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

            public CombatAltarActivationResolver
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