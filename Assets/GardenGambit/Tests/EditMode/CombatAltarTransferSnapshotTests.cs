using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarTransferSnapshotTests
    {
        [Test]
        public void Constructor_WithSacrificialAltar_SnapshotsCurrentHp()
        {
            var context =
                CreateContext(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            Assert.That(
                snapshot.Context,
                Is.SameAs(
                    context));

            Assert.That(
                snapshot.AltarKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind
                        .SacrificialAltar));

            Assert.That(
                snapshot.IsSacrificialAltar,
                Is.True);

            Assert.That(
                snapshot.IsWarAltar,
                Is.False);

            Assert.That(
                snapshot.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.TransferAmount,
                Is.EqualTo(6));

            Assert.That(
                snapshot.HasPositiveTransfer,
                Is.True);

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WithWarAltar_SnapshotsCurrentAttack()
        {
            var context =
                CreateContext(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            Assert.That(
                snapshot.AltarKind,
                Is.EqualTo(
                    CombatSlotEnhanceKind.WarAltar));

            Assert.That(
                snapshot.IsWarAltar,
                Is.True);

            Assert.That(
                snapshot.IsSacrificialAltar,
                Is.False);

            Assert.That(
                snapshot.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.TransferAmount,
                Is.EqualTo(4));

            Assert.That(
                snapshot.HasPositiveTransfer,
                Is.True);

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.DonorCard.Attack,
                Is.EqualTo(4));

            Assert.That(
                snapshot.RecipientCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void Constructor_WithZeroAttackWarAltar_AllowsSnapshot()
        {
            var context =
                CreateContext(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 0);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            Assert.That(
                snapshot.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.TransferAmount,
                Is.Zero);

            Assert.That(
                snapshot.HasPositiveTransfer,
                Is.False);

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void SacrificialSnapshot_AfterDonorHpChanges_PreservesOriginalValues()
        {
            var context =
                CreateContext(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            context.DonorCard.ApplyHpStatGain(
                2);

            Assert.That(
                context.DonorCard.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                snapshot.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.TransferAmount,
                Is.EqualTo(6));
        }

        [Test]
        public void WarSnapshot_AfterDonorAttackChanges_PreservesOriginalValue()
        {
            var context =
                CreateContext(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            context.DonorCard.ApplyAttackGain(
                3);

            Assert.That(
                context.DonorCard.Attack,
                Is.EqualTo(7));

            Assert.That(
                snapshot.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.TransferAmount,
                Is.EqualTo(4));
        }

        [Test]
        public void Constructor_WithNullContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarTransferSnapshot(
                        null));
        }

        [Test]
        public void Constructor_WithDeathThresholdDonor_Throws()
        {
            var context =
                CreateContext(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 0,
                    donorAttack: 4);

            Assert.Throws<InvalidOperationException>(
                () => _ =
                    new CombatAltarTransferSnapshot(
                        context));

            Assert.That(
                context.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                context.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        private static CombatAltarActivationContext
            CreateContext(
                CombatSlotEnhanceKind altarKind,
                int donorCurrentHp,
                int donorAttack)
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

            return new CombatAltarActivationContext(
                altarKind,
                donorPosition,
                donorCard,
                recipient);
        }
    }
}