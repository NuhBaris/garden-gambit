using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarActivationContextTests
    {
        [Test]
        public void Constructor_WithSacrificialAltar_SetsContext()
        {
            var donorCard =
                CreateCard(100);

            var recipientCard =
                CreateCard(200);

            var donorPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column: 3);

            var recipient =
                new CombatAltarRecipient(
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 3),
                    recipientCard);

            var context =
                new CombatAltarActivationContext(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorPosition,
                    donorCard,
                    recipient);

            Assert.That(
                context.AltarKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind
                        .SacrificialAltar));

            Assert.That(
                context.IsSacrificialAltar,
                Is.True);

            Assert.That(
                context.IsWarAltar,
                Is.False);

            Assert.That(
                context.DonorPosition,
                Is.EqualTo(
                    donorPosition));

            Assert.That(
                context.DonorCard,
                Is.SameAs(
                    donorCard));

            Assert.That(
                context.DonorInstanceId,
                Is.EqualTo(
                    new InstanceId(100)));

            Assert.That(
                context.Recipient,
                Is.SameAs(
                    recipient));

            Assert.That(
                context.RecipientCard,
                Is.SameAs(
                    recipientCard));

            Assert.That(
                context.RecipientInstanceId,
                Is.EqualTo(
                    new InstanceId(200)));

            Assert.That(
                context.RecipientPosition,
                Is.EqualTo(
                    recipient.Position));
        }

        [Test]
        public void Constructor_WithWarAltar_SetsContext()
        {
            var donorCard =
                CreateCard(100);

            var recipientCard =
                CreateCard(200);

            var context =
                new CombatAltarActivationContext(
                    CombatSlotEnhanceKind.WarAltar,
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        column: 4),
                    donorCard,
                    new CombatAltarRecipient(
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            column: 4),
                        recipientCard));

            Assert.That(
                context.AltarKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind.WarAltar));

            Assert.That(
                context.IsWarAltar,
                Is.True);

            Assert.That(
                context.IsSacrificialAltar,
                Is.False);

            Assert.That(
                context.DonorCard,
                Is.SameAs(
                    donorCard));

            Assert.That(
                context.RecipientCard,
                Is.SameAs(
                    recipientCard));
        }

        [Test]
        public void Constructor_WithNonAltarKind_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind.None,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        CreateCard(100),
                        CreateValidRecipient()));
        }

        [Test]
        public void Constructor_WithInvalidDonorPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind
                            .SacrificialAltar,
                        default(BoardPosition),
                        CreateCard(100),
                        CreateValidRecipient()));
        }

        [Test]
        public void Constructor_WithNullDonorCard_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind
                            .SacrificialAltar,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        null,
                        CreateValidRecipient()));
        }

        [Test]
        public void Constructor_WithNullRecipient_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind.WarAltar,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        CreateCard(100),
                        null));
        }

        [Test]
        public void Constructor_WithRecipientOnDifferentSide_Throws()
        {
            var recipient =
                new CombatAltarRecipient(
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        column: 3),
                    CreateCard(200));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind
                            .SacrificialAltar,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        CreateCard(100),
                        recipient));
        }

        [Test]
        public void Constructor_WithRecipientInDifferentColumn_Throws()
        {
            var recipient =
                new CombatAltarRecipient(
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 4),
                    CreateCard(200));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind
                            .SacrificialAltar,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        CreateCard(100),
                        recipient));
        }

        [Test]
        public void Constructor_WithRecipientInSameRow_Throws()
        {
            var recipient =
                new CombatAltarRecipient(
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 3),
                    CreateCard(200));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind.WarAltar,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        CreateCard(100),
                        recipient));
        }

        [Test]
        public void Constructor_WithSameCardInstance_Throws()
        {
            var donorCard =
                CreateCard(100);

            var recipient =
                new CombatAltarRecipient(
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 3),
                    donorCard);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatAltarActivationContext(
                        CombatSlotEnhanceKind
                            .SacrificialAltar,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            column: 3),
                        donorCard,
                        recipient));
        }

        [Test]
        public void Constructor_PreservesExactCardReferences()
        {
            var donorCard =
                CreateCard(100);

            var recipientCard =
                CreateCard(200);

            var recipient =
                new CombatAltarRecipient(
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        column: 3),
                    recipientCard);

            var context =
                new CombatAltarActivationContext(
                    CombatSlotEnhanceKind.WarAltar,
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        column: 3),
                    donorCard,
                    recipient);

            Assert.That(
                context.DonorCard,
                Is.SameAs(
                    donorCard));

            Assert.That(
                context.RecipientCard,
                Is.SameAs(
                    recipientCard));

            Assert.That(
                context.Recipient,
                Is.SameAs(
                    recipient));
        }

        private static CombatAltarRecipient
            CreateValidRecipient()
        {
            return new CombatAltarRecipient(
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    column: 3),
                CreateCard(200));
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
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId("test-card"),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                7,
                0,
                3);
        }
    }
}