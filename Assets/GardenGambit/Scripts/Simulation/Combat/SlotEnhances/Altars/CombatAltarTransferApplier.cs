using System;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAltarTransferApplier
    {
        public CombatAltarTransferApplicationPreview
            Apply(
                CombatAltarTransferApplicationPreview
                    preview)
        {
            EnsureCanApplyRecipientTransfer(
                preview);

            var snapshot =
                preview.Snapshot;

            if (preview.IsSacrificialAltar)
            {
                snapshot.RecipientCard
                    .ApplyHpStatGain(
                        preview.TransferAmount);
            }
            else if (preview.HasRecipientAttackGain)
            {
                snapshot.RecipientCard
                    .ApplyAttackGain(
                        preview.TransferAmount);
            }

            return ApplyDonorDeathThreshold(
                preview);
        }

        public void EnsureCanApplyRecipientTransfer(
            CombatAltarTransferApplicationPreview
                preview)
        {
            if (preview == null)
            {
                throw new ArgumentNullException(
                    nameof(preview));
            }

            ValidateStateBeforeRecipientTransfer(
                preview);
        }

        public CombatAltarTransferApplicationPreview
            ApplyWarRecipientTransfer(
                CombatAltarTransferApplicationPreview
                    preview)
        {
            EnsureCanApplyRecipientTransfer(
                preview);

            if (!preview.IsWarAltar)
            {
                throw new ArgumentException(
                    "War Altar recipient transfer requires " +
                    "a War Altar transfer preview.",
                    nameof(preview));
            }

            if (preview.HasRecipientAttackGain)
            {
                preview.Snapshot.RecipientCard
                    .ApplyAttackGain(
                        preview.TransferAmount);
            }

            return preview;
        }

        public CombatAltarTransferApplicationPreview
            ApplyDonorDeathThreshold(
                CombatAltarTransferApplicationPreview
                    preview)
        {
            if (preview == null)
            {
                throw new ArgumentNullException(
                    nameof(preview));
            }

            ValidateStateAfterRecipientTransfer(
                preview);

            preview.Snapshot.DonorCard
                .SetCurrentHpToZero();

            return preview;
        }

        private static void
            ValidateStateBeforeRecipientTransfer(
                CombatAltarTransferApplicationPreview
                    preview)
        {
            var snapshot =
                preview.Snapshot;

            var donorCard =
                snapshot.DonorCard;

            var recipientCard =
                snapshot.RecipientCard;

            ValidateUnchangedDonor(
                preview,
                donorCard);

            if (recipientCard.HpCapacity !=
                preview.RecipientPreviousHpCapacity)
            {
                throw new InvalidOperationException(
                    "Altar recipient HP Capacity no longer " +
                    "matches the transfer preview.");
            }

            if (recipientCard.CurrentHp !=
                preview.RecipientPreviousHp)
            {
                throw new InvalidOperationException(
                    "Altar recipient Current HP no longer " +
                    "matches the transfer preview.");
            }

            if (recipientCard.Attack !=
                preview.RecipientPreviousAttack)
            {
                throw new InvalidOperationException(
                    "Altar recipient Attack no longer " +
                    "matches the transfer preview.");
            }
        }

        private static void
            ValidateStateAfterRecipientTransfer(
                CombatAltarTransferApplicationPreview
                    preview)
        {
            var snapshot =
                preview.Snapshot;

            var donorCard =
                snapshot.DonorCard;

            var recipientCard =
                snapshot.RecipientCard;

            ValidateUnchangedDonor(
                preview,
                donorCard);

            if (recipientCard.HpCapacity !=
                preview.RecipientCurrentHpCapacity)
            {
                throw new InvalidOperationException(
                    "Altar recipient HP Capacity does not " +
                    "match the applied transfer.");
            }

            if (recipientCard.CurrentHp !=
                preview.RecipientCurrentHp)
            {
                throw new InvalidOperationException(
                    "Altar recipient Current HP does not " +
                    "match the applied transfer.");
            }

            if (recipientCard.Attack !=
                preview.RecipientCurrentAttack)
            {
                throw new InvalidOperationException(
                    "Altar recipient Attack does not match " +
                    "the applied transfer.");
            }
        }

        private static void ValidateUnchangedDonor(
            CombatAltarTransferApplicationPreview
                preview,
            GardenGambit.Domain.Combat.CombatCardState
                donorCard)
        {
            if (donorCard.CurrentHp !=
                preview.DonorPreviousHp)
            {
                throw new InvalidOperationException(
                    "Altar donor HP no longer matches " +
                    "the transfer preview.");
            }

            if (preview.IsWarAltar &&
                donorCard.Attack !=
                preview.TransferAmount)
            {
                throw new InvalidOperationException(
                    "War Altar donor Attack no longer " +
                    "matches the transfer preview.");
            }
        }
    }
}