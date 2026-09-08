using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatAltarTransferApplierStageTests
    {
        [Test]
        public void
            EnsureCanApplyRecipientTransfer_WithNullPreview_Throws()
        {
            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<ArgumentNullException>(
                () =>
                    applier
                        .EnsureCanApplyRecipientTransfer(
                            null));
        }

        [Test]
        public void
            EnsureCanApplyRecipientTransfer_DoesNotMutateCards()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            data.Applier
                .EnsureCanApplyRecipientTransfer(
                    data.Preview);

            Assert.That(
                data.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                data.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                data.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                data.RecipientCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void
            ApplyWarRecipientTransfer_AppliesAttackWithoutKillingDonor()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .WarAltar);

            var result =
                data.Applier
                    .ApplyWarRecipientTransfer(
                        data.Preview);

            Assert.That(
                result,
                Is.SameAs(
                    data.Preview));

            Assert.That(
                data.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                data.RecipientCard.Attack,
                Is.EqualTo(9));

            Assert.That(
                data.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                data.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void
            ApplyWarRecipientTransfer_WithSacrificialPreview_ThrowsWithoutMutation()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<ArgumentException>(
                () =>
                    data.Applier
                        .ApplyWarRecipientTransfer(
                            data.Preview));

            Assert.That(
                data.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                data.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                data.RecipientCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                data.RecipientCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void
            ApplyDonorDeathThreshold_WithNullPreview_Throws()
        {
            var applier =
                new CombatAltarTransferApplier();

            Assert.Throws<ArgumentNullException>(
                () =>
                    applier
                        .ApplyDonorDeathThreshold(
                            null));
        }

        [Test]
        public void
            ApplyDonorDeathThreshold_BeforeSacrificialTransfer_Throws()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            Assert.Throws<InvalidOperationException>(
                () =>
                    data.Applier
                        .ApplyDonorDeathThreshold(
                            data.Preview));

            Assert.That(
                data.DonorCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                data.RecipientCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                data.RecipientCard.CurrentHp,
                Is.EqualTo(5));
        }

        [Test]
        public void
            ApplyDonorDeathThreshold_AfterSacrificialTransfer_KillsOnlyDonor()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .SacrificialAltar);

            data.RecipientCard.ApplyHpStatGain(
                data.Preview.TransferAmount);

            var result =
                data.Applier
                    .ApplyDonorDeathThreshold(
                        data.Preview);

            Assert.That(
                result,
                Is.SameAs(
                    data.Preview));

            Assert.That(
                data.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                data.RecipientCard.HpCapacity,
                Is.EqualTo(14));

            Assert.That(
                data.RecipientCard.CurrentHp,
                Is.EqualTo(9));

            Assert.That(
                data.RecipientCard.Attack,
                Is.EqualTo(3));
        }

        [Test]
        public void
            ApplyDonorDeathThreshold_CalledTwice_ThrowsWithoutRepeating()
        {
            var data =
                CreateData(
                    CombatSlotEnhanceKind
                        .WarAltar);

            data.Applier.ApplyWarRecipientTransfer(
                data.Preview);

            data.Applier.ApplyDonorDeathThreshold(
                data.Preview);

            Assert.Throws<InvalidOperationException>(
                () =>
                    data.Applier
                        .ApplyDonorDeathThreshold(
                            data.Preview));

            Assert.That(
                data.DonorCard.CurrentHp,
                Is.Zero);

            Assert.That(
                data.RecipientCard.Attack,
                Is.EqualTo(9));
        }

        private static TestData CreateData(
            CombatSlotEnhanceKind altarKind)
        {
            var donorPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var recipientPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var donorCard =
                CreateCard(
                    instanceId: 1,
                    hpCapacity: 10,
                    currentHp: 4,
                    attack: 6);

            var recipientCard =
                CreateCard(
                    instanceId: 2,
                    hpCapacity: 10,
                    currentHp: 5,
                    attack: 3);

            var context =
                new CombatAltarActivationContext(
                    altarKind,
                    donorPosition,
                    donorCard,
                    new CombatAltarRecipient(
                        recipientPosition,
                        recipientCard));

            var snapshot =
                new CombatAltarTransferSnapshot(
                    context);

            return new TestData
            {
                Applier =
                    new CombatAltarTransferApplier(),

                Preview =
                    new
                        CombatAltarTransferApplicationPreview(
                            snapshot),

                DonorCard =
                    donorCard,

                RecipientCard =
                    recipientCard
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

        private sealed class TestData
        {
            public CombatAltarTransferApplier Applier
            {
                get;
                set;
            }

            public CombatAltarTransferApplicationPreview
                Preview
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