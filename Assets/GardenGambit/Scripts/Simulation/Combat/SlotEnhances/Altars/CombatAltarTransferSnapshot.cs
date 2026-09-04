using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAltarTransferSnapshot
    {
        public CombatAltarTransferSnapshot(
            CombatAltarActivationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (context.DonorCard
                    .IsAtDeathThreshold)
            {
                throw new InvalidOperationException(
                    "Altar transfer snapshot requires " +
                    "a living donor card.");
            }

            Context = context;

            DonorPreviousHp =
                context.DonorCard.CurrentHp;

            TransferAmount =
                context.IsSacrificialAltar
                    ? context.DonorCard.CurrentHp
                    : context.DonorCard.Attack;
        }

        public CombatAltarActivationContext Context
        {
            get;
        }

        public CombatSlotEnhanceKind AltarKind =>
            Context.AltarKind;

        public bool IsSacrificialAltar =>
            Context.IsSacrificialAltar;

        public bool IsWarAltar =>
            Context.IsWarAltar;

        public BoardPosition DonorPosition =>
            Context.DonorPosition;

        public CombatCardState DonorCard =>
            Context.DonorCard;

        public InstanceId DonorInstanceId =>
            Context.DonorInstanceId;

        public int DonorPreviousHp
        {
            get;
        }

        public CombatAltarRecipient Recipient =>
            Context.Recipient;

        public BoardPosition RecipientPosition =>
            Context.RecipientPosition;

        public CombatCardState RecipientCard =>
            Context.RecipientCard;

        public InstanceId RecipientInstanceId =>
            Context.RecipientInstanceId;

        public int TransferAmount
        {
            get;
        }

        public bool HasPositiveTransfer =>
            TransferAmount > 0;
    }
}