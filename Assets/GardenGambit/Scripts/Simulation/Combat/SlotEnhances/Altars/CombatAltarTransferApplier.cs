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
            if (preview == null)
            {
                throw new ArgumentNullException(
                    nameof(preview));
            }

            ValidateStateStillMatches(
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

            snapshot.DonorCard
                .SetCurrentHpToZero();

            return preview;
        }

        private static void ValidateStateStillMatches(
            CombatAltarTransferApplicationPreview
                preview)
        {
            var snapshot =
                preview.Snapshot;

            var donorCard =
                snapshot.DonorCard;

            var recipientCard =
                snapshot.RecipientCard;

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
    }
}