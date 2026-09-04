using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        WarAltarActivatedCombatEvent :
        CombatEvent
    {
        public WarAltarActivatedCombatEvent(
            CombatEventMetadata metadata,
            InstanceId donorInstanceId,
            BoardPosition donorPosition,
            InstanceId recipientInstanceId,
            BoardPosition recipientPosition,
            int transferredAttack,
            int donorPreviousHp)
            : base(
                metadata,
                CombatEventKind.WarAltarActivated)
        {
            if (!donorInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "War Altar event requires a valid " +
                    "donor InstanceId.",
                    nameof(donorInstanceId));
            }

            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "War Altar event requires a valid " +
                    "donor position.",
                    nameof(donorPosition));
            }

            if (!recipientInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "War Altar event requires a valid " +
                    "recipient InstanceId.",
                    nameof(recipientInstanceId));
            }

            if (!recipientPosition.IsValid)
            {
                throw new ArgumentException(
                    "War Altar event requires a valid " +
                    "recipient position.",
                    nameof(recipientPosition));
            }

            if (donorInstanceId ==
                recipientInstanceId)
            {
                throw new ArgumentException(
                    "War Altar donor and recipient must " +
                    "be different cards.",
                    nameof(recipientInstanceId));
            }

            if (donorPosition.Side !=
                recipientPosition.Side)
            {
                throw new ArgumentException(
                    "War Altar donor and recipient must " +
                    "belong to the same side.",
                    nameof(recipientPosition));
            }

            if (donorPosition.Column !=
                recipientPosition.Column)
            {
                throw new ArgumentException(
                    "War Altar donor and recipient must " +
                    "belong to the same column.",
                    nameof(recipientPosition));
            }

            if (donorPosition.Row ==
                recipientPosition.Row)
            {
                throw new ArgumentException(
                    "War Altar recipient must occupy the " +
                    "opposite board row.",
                    nameof(recipientPosition));
            }

            if (transferredAttack < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(transferredAttack),
                    transferredAttack,
                    "War Altar transferred Attack cannot " +
                    "be negative.");
            }

            if (donorPreviousHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(donorPreviousHp),
                    donorPreviousHp,
                    "War Altar donor previous HP must be " +
                    "greater than zero.");
            }

            DonorInstanceId = donorInstanceId;
            DonorPosition = donorPosition;

            RecipientInstanceId =
                recipientInstanceId;

            RecipientPosition =
                recipientPosition;

            TransferredAttack =
                transferredAttack;

            DonorPreviousHp =
                donorPreviousHp;
        }

        public InstanceId DonorInstanceId
        {
            get;
        }

        public BoardPosition DonorPosition
        {
            get;
        }

        public InstanceId RecipientInstanceId
        {
            get;
        }

        public BoardPosition RecipientPosition
        {
            get;
        }

        public int TransferredAttack
        {
            get;
        }

        public int DonorPreviousHp
        {
            get;
        }

        public bool HasPositiveTransfer =>
            TransferredAttack > 0;
    }
}