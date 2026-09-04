using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarActivationContextResolver
    {
        private readonly CombatAltarRecipientResolver
            _recipientResolver;

        public CombatAltarActivationContextResolver()
        {
            _recipientResolver =
                new CombatAltarRecipientResolver();
        }

        public CombatAltarActivationContext
            TryResolve(
                CombatSideState sideState,
                BoardPosition donorPosition)
        {
            if (sideState == null)
            {
                throw new ArgumentNullException(
                    nameof(sideState));
            }

            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "Altar activation requires a valid " +
                    "donor position.",
                    nameof(donorPosition));
            }

            if (donorPosition.Side !=
                sideState.Side)
            {
                throw new ArgumentException(
                    "Altar donor position must belong to " +
                    "the supplied combat side.",
                    nameof(donorPosition));
            }

            var donorSlot =
                FindSlot(
                    sideState.Board,
                    donorPosition);

            if (donorSlot == null ||
                !donorSlot.IsOccupied)
            {
                return null;
            }

            if (!donorSlot.HasSacrificialAltar &&
                !donorSlot.HasWarAltar)
            {
                return null;
            }

            var donorCard =
                sideState.Cards.GetCard(
                    donorSlot
                        .OccupantInstanceId.Value);

            if (donorCard.IsAtDeathThreshold)
            {
                return null;
            }

            var recipient =
                _recipientResolver.TryResolve(
                    sideState,
                    donorPosition);

            if (recipient == null)
            {
                return null;
            }

            return new CombatAltarActivationContext(
                donorSlot.EnhanceKind,
                donorPosition,
                donorCard,
                recipient);
        }

        private static CombatSlotState FindSlot(
            CombatBoardState board,
            BoardPosition position)
        {
            foreach (var slot in board.Slots)
            {
                if (slot.Position ==
                    position)
                {
                    return slot;
                }
            }

            return null;
        }
    }
}