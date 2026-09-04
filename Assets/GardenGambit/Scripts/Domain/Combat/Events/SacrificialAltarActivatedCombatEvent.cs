using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        SacrificialAltarActivatedCombatEvent :
        CombatEvent
    {
        public SacrificialAltarActivatedCombatEvent(
            CombatEventMetadata metadata,
            InstanceId donorInstanceId,
            BoardPosition donorPosition,
            InstanceId recipientInstanceId,
            BoardPosition recipientPosition,
            int transferredHp)
            : base(
                metadata,
                CombatEventKind
                    .SacrificialAltarActivated)
        {
            if (!donorInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Sacrificial Altar event requires " +
                    "a valid donor InstanceId.",
                    nameof(donorInstanceId));
            }

            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "Sacrificial Altar event requires " +
                    "a valid donor position.",
                    nameof(donorPosition));
            }

            if (!recipientInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Sacrificial Altar event requires " +
                    "a valid recipient InstanceId.",
                    nameof(recipientInstanceId));
            }

            if (!recipientPosition.IsValid)
            {
                throw new ArgumentException(
                    "Sacrificial Altar event requires " +
                    "a valid recipient position.",
                    nameof(recipientPosition));
            }

            if (donorInstanceId ==
                recipientInstanceId)
            {
                throw new ArgumentException(
                    "Sacrificial Altar donor and " +
                    "recipient must be different cards.",
                    nameof(recipientInstanceId));
            }

            if (donorPosition.Side !=
                recipientPosition.Side)
            {
                throw new ArgumentException(
                    "Sacrificial Altar donor and " +
                    "recipient must belong to the same side.",
                    nameof(recipientPosition));
            }

            if (donorPosition.Column !=
                recipientPosition.Column)
            {
                throw new ArgumentException(
                    "Sacrificial Altar donor and " +
                    "recipient must belong to the same column.",
                    nameof(recipientPosition));
            }

            if (donorPosition.Row ==
                recipientPosition.Row)
            {
                throw new ArgumentException(
                    "Sacrificial Altar recipient must occupy " +
                    "the opposite board row.",
                    nameof(recipientPosition));
            }

            if (transferredHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(transferredHp),
                    transferredHp,
                    "Sacrificial Altar transferred HP must " +
                    "be greater than zero.");
            }

            DonorInstanceId = donorInstanceId;
            DonorPosition = donorPosition;
            RecipientInstanceId =
                recipientInstanceId;

            RecipientPosition =
                recipientPosition;

            TransferredHp =
                transferredHp;
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

        public int TransferredHp
        {
            get;
        }

        public int DonorPreviousHp =>
            TransferredHp;
    }
}