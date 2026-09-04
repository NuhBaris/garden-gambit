using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarRecipientPositionResolver
    {
        public BoardPosition Resolve(
            BoardPosition donorPosition)
        {
            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "Altar recipient resolution requires " +
                    "a valid donor board position.",
                    nameof(donorPosition));
            }

            var recipientRow =
                donorPosition.Row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

            return new BoardPosition(
                donorPosition.Side,
                recipientRow,
                donorPosition.Column);
        }
    }
}