using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarTransferApplierTests
    {
        [Test]
        public void Apply_WithSacrificialAltar_AppliesHpGainThenSetsDonorToZero()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var applier =
                new CombatAltarTransferApplier();

            var result =
                applier.Apply(
                    preview);

            Assert.That(
                result,
                Is.SameAs(
                    preview));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.HpCapacity,
                Is.EqualTo(16));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.CurrentHp,
                Is.EqualTo(11));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                preview.Snapshot
                    .DonorCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.Snapshot
                    .DonorCard.Attack,
                Is.EqualTo(4));

            Assert.That(
                preview.Snapshot
                    .DonorCard.IsAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void Apply_WithWarAltar_AppliesAttackGainThenSetsDonorToZero()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            var applier =
                new CombatAltarTransferApplier();

            var result =
                applier.Apply(
                    preview);

            Assert.That(
                result,
                Is.SameAs(
                    preview));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.Attack,
                Is.EqualTo(7));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                preview.Snapshot
                    .DonorCard.Attack,
                Is.EqualTo(4));

            Assert.That(
                preview.Snapshot
                    .DonorCard.IsAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void Apply_WithZeroAttackWarAltar_SkipsGainAndSetsDonorToZero()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 0);

            var applier =
                new CombatAltarTransferApplier();

            applier.Apply(
                preview);

            Assert.That(
                preview.Snapshot
                    .RecipientCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                preview.Snapshot
                    .DonorCard.IsAtDeathThreshold,
                Is.True);
        }

        [Test]
        public void Apply_WithNullPreview_Throws()
        {
            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<ArgumentNullException>(
                () => applier.Apply(
                    null));
        }

        [Test]
        public void Apply_WhenDonorHpChanged_ThrowsWithoutApplyingAltar()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            preview.Snapshot.DonorCard.Heal(
                1);

            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<InvalidOperationException>(
                () => applier.Apply(
                    preview));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void Apply_WhenWarDonorAttackChanged_ThrowsWithoutApplyingAltar()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            preview.Snapshot.DonorCard
                .ApplyAttackGain(
                    1);

            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<InvalidOperationException>(
                () => applier.Apply(
                    preview));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                preview.Snapshot
                    .DonorCard.Attack,
                Is.EqualTo(5));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void Apply_WhenRecipientHpCapacityChanged_ThrowsWithoutApplyingAltar()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            preview.Snapshot.RecipientCard
                .ApplyHpStatGain(
                    1);

            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<InvalidOperationException>(
                () => applier.Apply(
                    preview));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void Apply_WhenRecipientCurrentHpChanged_ThrowsWithoutApplyingAltar()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind
                        .SacrificialAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            preview.Snapshot.RecipientCard.Heal(
                1);

            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<InvalidOperationException>(
                () => applier.Apply(
                    preview));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.CurrentHp,
                Is.EqualTo(6));
        }

        [Test]
        public void Apply_WhenRecipientAttackChanged_ThrowsWithoutApplyingAltar()
        {
            var preview =
                CreatePreview(
                    CombatSlotEnhanceKind.WarAltar,
                    donorCurrentHp: 6,
                    donorAttack: 4);

            preview.Snapshot.RecipientCard
                .ApplyAttackGain(
                    1);

            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<InvalidOperationException>(
                () => applier.Apply(
                    preview));

            Assert.That(
                preview.Snapshot
                    .DonorCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                preview.Snapshot
                    .RecipientCard.Attack,
                Is.EqualTo(4));
        }

        private static
            CombatAltarTransferApplicationPreview
            CreatePreview(
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

            var context =
                new CombatAltarActivationContext(
                    altarKind,
                    donorPosition,
                    donorCard,
                    recipient);

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            return
                new
                    CombatAltarTransferApplicationPreview(
                        snapshot);
        }
    }
}