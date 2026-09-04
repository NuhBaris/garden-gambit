using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAltarRecipient
    {
        public CombatAltarRecipient(
            BoardPosition position,
            CombatCardState card)
        {
            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "Altar recipient requires a valid " +
                    "board position.",
                    nameof(position));
            }

            if (card == null)
            {
                throw new ArgumentNullException(
                    nameof(card));
            }

            Position = position;
            Card = card;
        }

        public BoardPosition Position
        {
            get;
        }

        public CombatCardState Card
        {
            get;
        }

        public InstanceId InstanceId =>
            Card.InstanceId;
    }
}