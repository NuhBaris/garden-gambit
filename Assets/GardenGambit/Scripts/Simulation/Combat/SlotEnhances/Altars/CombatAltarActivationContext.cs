using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAltarActivationContext
    {
        public CombatAltarActivationContext(
            CombatSlotEnhanceKind altarKind,
            BoardPosition donorPosition,
            CombatCardState donorCard,
            CombatAltarRecipient recipient)
        {
            if (altarKind !=
                    CombatSlotEnhanceKind
                        .SacrificialAltar &&
                altarKind !=
                    CombatSlotEnhanceKind.WarAltar)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(altarKind),
                    altarKind,
                    "Altar activation requires a " +
                    "Sacrificial Altar or War Altar.");
            }

            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "Altar activation requires a valid " +
                    "donor position.",
                    nameof(donorPosition));
            }

            if (donorCard == null)
            {
                throw new ArgumentNullException(
                    nameof(donorCard));
            }

            if (recipient == null)
            {
                throw new ArgumentNullException(
                    nameof(recipient));
            }

            if (recipient.Position.Side !=
                donorPosition.Side)
            {
                throw new ArgumentException(
                    "Altar donor and recipient must belong " +
                    "to the same combat side.",
                    nameof(recipient));
            }

            if (recipient.Position.Column !=
                donorPosition.Column)
            {
                throw new ArgumentException(
                    "Altar donor and recipient must belong " +
                    "to the same board column.",
                    nameof(recipient));
            }

            if (recipient.Position.Row ==
                donorPosition.Row)
            {
                throw new ArgumentException(
                    "Altar recipient must occupy the " +
                    "opposite board row.",
                    nameof(recipient));
            }

            if (recipient.InstanceId ==
                donorCard.InstanceId)
            {
                throw new ArgumentException(
                    "Altar donor and recipient must be " +
                    "different card instances.",
                    nameof(recipient));
            }

            AltarKind = altarKind;
            DonorPosition = donorPosition;
            DonorCard = donorCard;
            Recipient = recipient;
        }

        public CombatSlotEnhanceKind AltarKind
        {
            get;
        }

        public bool IsSacrificialAltar =>
            AltarKind ==
            CombatSlotEnhanceKind
                .SacrificialAltar;

        public bool IsWarAltar =>
            AltarKind ==
            CombatSlotEnhanceKind.WarAltar;

        public BoardPosition DonorPosition
        {
            get;
        }

        public CombatCardState DonorCard
        {
            get;
        }

        public InstanceId DonorInstanceId =>
            DonorCard.InstanceId;

        public CombatAltarRecipient Recipient
        {
            get;
        }

        public BoardPosition RecipientPosition =>
            Recipient.Position;

        public CombatCardState RecipientCard =>
            Recipient.Card;

        public InstanceId RecipientInstanceId =>
            Recipient.InstanceId;
    }
}