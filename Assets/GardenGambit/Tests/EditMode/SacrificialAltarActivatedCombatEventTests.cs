using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        SacrificialAltarActivatedCombatEventTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsSnapshot()
        {
            var metadata =
                CreateMetadata();

            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 3);

            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 3);

            var altarEvent =
                new SacrificialAltarActivatedCombatEvent(
                    metadata,
                    new InstanceId(100),
                    donorPosition,
                    new InstanceId(200),
                    recipientPosition,
                    transferredHp: 6);

            Assert.That(
                altarEvent.Kind,
                Is.EqualTo(
                    CombatEventKind
                        .SacrificialAltarActivated));

            Assert.That(
                altarEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                altarEvent.DonorInstanceId,
                Is.EqualTo(
                    new InstanceId(100)));

            Assert.That(
                altarEvent.DonorPosition,
                Is.EqualTo(
                    donorPosition));

            Assert.That(
                altarEvent.RecipientInstanceId,
                Is.EqualTo(
                    new InstanceId(200)));

            Assert.That(
                altarEvent.RecipientPosition,
                Is.EqualTo(
                    recipientPosition));

            Assert.That(
                altarEvent.TransferredHp,
                Is.EqualTo(6));

            Assert.That(
                altarEvent.DonorPreviousHp,
                Is.EqualTo(6));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        CreateValidRecipientPosition(),
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithInvalidDonorInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        CreateValidRecipientPosition(),
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithInvalidDonorPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        new InstanceId(200),
                        CreateValidRecipientPosition(),
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithInvalidRecipientInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        default(InstanceId),
                        CreateValidRecipientPosition(),
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithInvalidRecipientPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        default(BoardPosition),
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithSameCardInstance_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(100),
                        CreateValidRecipientPosition(),
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithRecipientOnDifferentSide_Throws()
        {
            var recipientPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    column: 3);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        recipientPosition,
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithRecipientInDifferentColumn_Throws()
        {
            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 4);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        recipientPosition,
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithRecipientInSameRow_Throws()
        {
            var recipientPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 3);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        recipientPosition,
                        transferredHp: 6));
        }

        [Test]
        public void Constructor_WithZeroTransferredHp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        CreateValidRecipientPosition(),
                        transferredHp: 0));
        }

        [Test]
        public void Constructor_WithNegativeTransferredHp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new SacrificialAltarActivatedCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateValidDonorPosition(),
                        new InstanceId(200),
                        CreateValidRecipientPosition(),
                        transferredHp: -1));
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private static BoardPosition
            CreateValidDonorPosition()
        {
            return CreatePosition(
                CombatSide.Player,
                BoardRow.Front,
                column: 3);
        }

        private static BoardPosition
            CreateValidRecipientPosition()
        {
            return CreatePosition(
                CombatSide.Player,
                BoardRow.Back,
                column: 3);
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
    }
}