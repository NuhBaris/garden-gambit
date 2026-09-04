using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarTransferApplicationPreview
    {
        public CombatAltarTransferApplicationPreview(
            CombatAltarTransferSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(
                    nameof(snapshot));
            }

            if (snapshot.DonorCard.CurrentHp !=
                snapshot.DonorPreviousHp)
            {
                throw new InvalidOperationException(
                    "Altar donor HP changed after the " +
                    "transfer snapshot was created.");
            }

            if (snapshot.IsWarAltar &&
                snapshot.DonorCard.Attack !=
                snapshot.TransferAmount)
            {
                throw new InvalidOperationException(
                    "War Altar donor Attack changed after " +
                    "the transfer snapshot was created.");
            }

            var recipientCard =
                snapshot.RecipientCard;

            var recipientPreviousHpCapacity =
                recipientCard.HpCapacity;

            var recipientPreviousHp =
                recipientCard.CurrentHp;

            var recipientPreviousAttack =
                recipientCard.Attack;

            var recipientCurrentHpCapacity =
                recipientPreviousHpCapacity;

            var recipientCurrentHp =
                recipientPreviousHp;

            var recipientCurrentAttack =
                recipientPreviousAttack;

            if (snapshot.IsSacrificialAltar)
            {
                recipientCurrentHpCapacity =
                    checked(
                        recipientPreviousHpCapacity +
                        snapshot.TransferAmount);

                recipientCurrentHp =
                    checked(
                        recipientPreviousHp +
                        snapshot.TransferAmount);
            }
            else
            {
                recipientCurrentAttack =
                    checked(
                        recipientPreviousAttack +
                        snapshot.TransferAmount);
            }

            Snapshot = snapshot;

            RecipientPreviousHpCapacity =
                recipientPreviousHpCapacity;

            RecipientCurrentHpCapacity =
                recipientCurrentHpCapacity;

            RecipientPreviousHp =
                recipientPreviousHp;

            RecipientCurrentHp =
                recipientCurrentHp;

            RecipientPreviousAttack =
                recipientPreviousAttack;

            RecipientCurrentAttack =
                recipientCurrentAttack;
        }

        public CombatAltarTransferSnapshot Snapshot
        {
            get;
        }

        public CombatSlotEnhanceKind AltarKind =>
            Snapshot.AltarKind;

        public bool IsSacrificialAltar =>
            Snapshot.IsSacrificialAltar;

        public bool IsWarAltar =>
            Snapshot.IsWarAltar;

        public InstanceId DonorInstanceId =>
            Snapshot.DonorInstanceId;

        public BoardPosition DonorPosition =>
            Snapshot.DonorPosition;

        public int DonorPreviousHp =>
            Snapshot.DonorPreviousHp;

        public int DonorCurrentHp =>
            0;

        public InstanceId RecipientInstanceId =>
            Snapshot.RecipientInstanceId;

        public BoardPosition RecipientPosition =>
            Snapshot.RecipientPosition;

        public int TransferAmount =>
            Snapshot.TransferAmount;

        public int RecipientPreviousHpCapacity
        {
            get;
        }

        public int RecipientCurrentHpCapacity
        {
            get;
        }

        public int RecipientPreviousHp
        {
            get;
        }

        public int RecipientCurrentHp
        {
            get;
        }

        public int RecipientPreviousAttack
        {
            get;
        }

        public int RecipientCurrentAttack
        {
            get;
        }

        public bool HasRecipientHpStatGain =>
            IsSacrificialAltar &&
            TransferAmount > 0;

        public bool HasRecipientAttackGain =>
            IsWarAltar &&
            TransferAmount > 0;
    }
}