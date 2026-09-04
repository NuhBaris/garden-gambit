using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivatedEventFactoryTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivatedEventFactory(
                        null));
        }

        [Test]
        public void Create_WithNullCombatStartedEvent_Throws()
        {
            var factory =
                new CombatAltarActivatedEventFactory(
                    CreateMetadataFactory());

            Assert.Throws<ArgumentNullException>(
                () => factory.Create(
                    null,
                    CreateSnapshot(
                        CombatSlotEnhanceKind
                            .SacrificialAltar)));
        }

        [Test]
        public void Create_WithNullSnapshot_Throws()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                CreateCombatStartedEvent(
                    metadataFactory);

            var factory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            Assert.Throws<ArgumentNullException>(
                () => factory.Create(
                    combatStartedEvent,
                    null));
        }

        [Test]
        public void Create_CalledTwice_AllocatesDistinctIncreasingMetadata()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                CreateCombatStartedEvent(
                    metadataFactory);

            var factory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            var firstEvent =
                factory.Create(
                    combatStartedEvent,
                    CreateSnapshot(
                        CombatSlotEnhanceKind
                            .SacrificialAltar));

            var secondEvent =
                factory.Create(
                    combatStartedEvent,
                    CreateSnapshot(
                        CombatSlotEnhanceKind
                            .WarAltar));

            Assert.That(
                secondEvent.Metadata.EventId,
                Is.Not.EqualTo(
                    firstEvent.Metadata.EventId));

            Assert.That(
                secondEvent.Metadata.SequenceNo,
                Is.GreaterThan(
                    firstEvent.Metadata.SequenceNo));

            Assert.That(
                firstEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                secondEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                firstEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                secondEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.TriggerRootId));
        }

        [Test]
        public void Create_WithSacrificialSnapshot_ReturnsSacrificialEvent()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                CreateCombatStartedEvent(
                    metadataFactory);

            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var factory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            var combatEvent =
                factory.Create(
                    combatStartedEvent,
                    snapshot);

            var altarEvent =
                combatEvent
                    as
                    SacrificialAltarActivatedCombatEvent;

            Assert.That(
                altarEvent,
                Is.Not.Null);

            Assert.That(
                altarEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .SacrificialAltarActivated));

            Assert.That(
                altarEvent.DonorInstanceId,
                Is.EqualTo(
                    snapshot.DonorInstanceId));

            Assert.That(
                altarEvent.DonorPosition,
                Is.EqualTo(
                    snapshot.DonorPosition));

            Assert.That(
                altarEvent.RecipientInstanceId,
                Is.EqualTo(
                    snapshot.RecipientInstanceId));

            Assert.That(
                altarEvent.RecipientPosition,
                Is.EqualTo(
                    snapshot.RecipientPosition));

            Assert.That(
                altarEvent.TransferredHp,
                Is.EqualTo(6));

            Assert.That(
                altarEvent.DonorPreviousHp,
                Is.EqualTo(6));

            AssertChildMetadata(
                combatStartedEvent,
                altarEvent);
        }

        [Test]
        public void Create_WithWarSnapshot_ReturnsWarEvent()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                CreateCombatStartedEvent(
                    metadataFactory);

            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var factory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            var combatEvent =
                factory.Create(
                    combatStartedEvent,
                    snapshot);

            var altarEvent =
                combatEvent
                    as WarAltarActivatedCombatEvent;

            Assert.That(
                altarEvent,
                Is.Not.Null);

            Assert.That(
                altarEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.WarAltarActivated));

            Assert.That(
                altarEvent.DonorInstanceId,
                Is.EqualTo(
                    snapshot.DonorInstanceId));

            Assert.That(
                altarEvent.RecipientInstanceId,
                Is.EqualTo(
                    snapshot.RecipientInstanceId));

            Assert.That(
                altarEvent.TransferredAttack,
                Is.EqualTo(4));

            Assert.That(
                altarEvent.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                altarEvent.HasPositiveTransfer,
                Is.True);

            AssertChildMetadata(
                combatStartedEvent,
                altarEvent);
        }

        [Test]
        public void Create_WithZeroAttackWarSnapshot_AllowsEvent()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                CreateCombatStartedEvent(
                    metadataFactory);

            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 0);

            var factory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            var altarEvent =
                factory.Create(
                    combatStartedEvent,
                    snapshot)
                    as WarAltarActivatedCombatEvent;

            Assert.That(
                altarEvent,
                Is.Not.Null);

            Assert.That(
                altarEvent.TransferredAttack,
                Is.Zero);

            Assert.That(
                altarEvent.HasPositiveTransfer,
                Is.False);

            Assert.That(
                altarEvent.DonorPreviousHp,
                Is.EqualTo(6));
        }

        [Test]
        public void Create_DoesNotMutateDonorOrRecipient()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var combatStartedEvent =
                CreateCombatStartedEvent(
                    metadataFactory);

            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var factory =
                new CombatAltarActivatedEventFactory(
                    metadataFactory);

            factory.Create(
                combatStartedEvent,
                snapshot);

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.DonorCard.Attack,
                Is.EqualTo(4));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                snapshot.RecipientCard.Attack,
                Is.EqualTo(3));
        }

        private static void AssertChildMetadata(
            CombatStartedCombatEvent
                combatStartedEvent,
            CombatEvent childEvent)
        {
            Assert.That(
                childEvent.Metadata.HasParent,
                Is.True);

            Assert.That(
                childEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                childEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                childEvent.Metadata.SequenceNo,
                Is.GreaterThan(
                    combatStartedEvent
                        .Metadata.SequenceNo));
        }

        private static CombatStartedCombatEvent
            CreateCombatStartedEvent(
                CombatEventMetadataFactory
                    metadataFactory)
        {
            return new CombatStartedCombatEvent(
                metadataFactory.CreateRoot());
        }

        private static CombatAltarTransferSnapshot
            CreateSnapshot(
                CombatSlotEnhanceKind altarKind,
                int donorCurrentHp = 6,
                int donorAttack = 4)
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(3));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(3));

            var donorCard =
                new CombatCardState(
                    new DefinitionId("donor-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    10,
                    donorCurrentHp,
                    0,
                    donorAttack);

            var recipientCard =
                new CombatCardState(
                    new DefinitionId("recipient-card"),
                    new InstanceId(200),
                    new CardRank(2),
                    10,
                    5,
                    0,
                    3);

            var recipient =
                new CombatAltarRecipient(
                    recipientPosition,
                    recipientCard);

            var context =
                new CombatAltarActivationContext(
                    altarKind,
                    donorPosition,
                    donorCard,
                    recipient);

            return new CombatAltarTransferSnapshot(
                context);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }
    }
}