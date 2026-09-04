using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarTransferApplicationPreviewTests
    {
        [Test]
        public void Constructor_WithSacrificialAltar_CalculatesHpStatGain()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            var preview =
                new CombatAltarTransferApplicationPreview(
                    snapshot);

            Assert.That(
                preview.Snapshot,
                Is.SameAs(
                    snapshot));

            Assert.That(
                preview.IsSacrificialAltar,
                Is.True);

            Assert.That(
                preview.IsWarAltar,
                Is.False);

            Assert.That(
                preview.TransferAmount,
                Is.EqualTo(6));

            Assert.That(
                preview.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                preview.DonorCurrentHp,
                Is.Zero);

            Assert.That(
                preview.RecipientPreviousHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.RecipientCurrentHpCapacity,
                Is.EqualTo(16));

            Assert.That(
                preview.RecipientPreviousHp,
                Is.EqualTo(5));

            Assert.That(
                preview.RecipientCurrentHp,
                Is.EqualTo(11));

            Assert.That(
                preview.RecipientPreviousAttack,
                Is.EqualTo(3));

            Assert.That(
                preview.RecipientCurrentAttack,
                Is.EqualTo(3));

            Assert.That(
                preview.HasRecipientHpStatGain,
                Is.True);

            Assert.That(
                preview.HasRecipientAttackGain,
                Is.False);

            Assert.That(
                snapshot.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WithWarAltar_CalculatesAttackGain()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            var preview =
                new CombatAltarTransferApplicationPreview(
                    snapshot);

            Assert.That(
                preview.IsWarAltar,
                Is.True);

            Assert.That(
                preview.TransferAmount,
                Is.EqualTo(4));

            Assert.That(
                preview.DonorPreviousHp,
                Is.EqualTo(6));

            Assert.That(
                preview.DonorCurrentHp,
                Is.Zero);

            Assert.That(
                preview.RecipientPreviousHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.RecipientCurrentHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.RecipientPreviousHp,
                Is.EqualTo(5));

            Assert.That(
                preview.RecipientCurrentHp,
                Is.EqualTo(5));

            Assert.That(
                preview.RecipientPreviousAttack,
                Is.EqualTo(3));

            Assert.That(
                preview.RecipientCurrentAttack,
                Is.EqualTo(7));

            Assert.That(
                preview.HasRecipientHpStatGain,
                Is.False);

            Assert.That(
                preview.HasRecipientAttackGain,
                Is.True);

            Assert.That(
                snapshot.RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void Constructor_WithZeroAttackWarAltar_LeavesRecipientAttackUnchanged()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 0,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            var preview =
                new CombatAltarTransferApplicationPreview(
                    snapshot);

            Assert.That(
                preview.TransferAmount,
                Is.Zero);

            Assert.That(
                preview.RecipientPreviousAttack,
                Is.EqualTo(3));

            Assert.That(
                preview.RecipientCurrentAttack,
                Is.EqualTo(3));

            Assert.That(
                preview.HasRecipientAttackGain,
                Is.False);

            Assert.That(
                preview.DonorCurrentHp,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithNullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatAltarTransferApplicationPreview(
                        null));
        }

        [Test]
        public void Constructor_WhenDonorHpChangedAfterSnapshot_ThrowsWithoutChangingRecipient()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            snapshot.DonorCard.ApplyHpStatGain(
                1);

            Assert.Throws<InvalidOperationException>(
                () => _ =
                    new CombatAltarTransferApplicationPreview(
                        snapshot));

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                snapshot.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WhenWarDonorAttackChangedAfterSnapshot_ThrowsWithoutChangingRecipient()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            snapshot.DonorCard.ApplyAttackGain(
                1);

            Assert.Throws<InvalidOperationException>(
                () => _ =
                    new CombatAltarTransferApplicationPreview(
                        snapshot));

            Assert.That(
                snapshot.DonorCard.Attack,
                Is.EqualTo(5));

            Assert.That(
                snapshot.RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WhenSacrificialHpCapacityWouldOverflow_ThrowsWithoutMutation()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 1,
                    donorAttack: 4,
                    recipientHpCapacity: int.MaxValue,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            Assert.Throws<OverflowException>(
                () => _ =
                    new CombatAltarTransferApplicationPreview(
                        snapshot));

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(1));

            Assert.That(
                snapshot.RecipientCard.HpCapacity,
                Is.EqualTo(
                    int.MaxValue));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WhenWarAttackWouldOverflow_ThrowsWithoutMutation()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 1,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: int.MaxValue);

            Assert.Throws<OverflowException>(
                () => _ =
                    new CombatAltarTransferApplicationPreview(
                        snapshot));

            Assert.That(
                snapshot.DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                snapshot.DonorCard.Attack,
                Is.EqualTo(1));

            Assert.That(
                snapshot.RecipientCard.Attack,
                Is.EqualTo(
                    int.MaxValue));
        }

        [Test]
        public void Preview_AfterRecipientChanges_PreservesCalculatedValues()
        {
            var snapshot =
                CreateSnapshot(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4,
                    recipientHpCapacity: 10,
                    recipientCurrentHp: 5,
                    recipientAttack: 3);

            var preview =
                new CombatAltarTransferApplicationPreview(
                    snapshot);

            snapshot.RecipientCard.ApplyHpStatGain(
                1);

            Assert.That(
                snapshot.RecipientCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                snapshot.RecipientCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                preview.RecipientPreviousHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.RecipientCurrentHpCapacity,
                Is.EqualTo(16));

            Assert.That(
                preview.RecipientPreviousHp,
                Is.EqualTo(5));

            Assert.That(
                preview.RecipientCurrentHp,
                Is.EqualTo(11));
        }

        private static CombatAltarTransferSnapshot
            CreateSnapshot(
                CombatSlotEnhanceKind altarKind,
                int donorCurrentHp,
                int donorAttack,
                int recipientHpCapacity,
                int recipientCurrentHp,
                int recipientAttack)
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

            var donorHpCapacity =
                donorCurrentHp > 10
                    ? donorCurrentHp
                    : 10;

            var donorCard =
                new CombatCardState(
                    new DefinitionId("donor-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    donorHpCapacity,
                    donorCurrentHp,
                    0,
                    donorAttack);

            var recipientCard =
                new CombatCardState(
                    new DefinitionId("recipient-card"),
                    new InstanceId(200),
                    new CardRank(2),
                    recipientHpCapacity,
                    recipientCurrentHp,
                    0,
                    recipientAttack);

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
    }
}