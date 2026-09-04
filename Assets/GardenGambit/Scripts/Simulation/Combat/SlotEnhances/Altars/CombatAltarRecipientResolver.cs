using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarRecipientResolver
    {
        private readonly
            CombatAltarRecipientPositionResolver
            _positionResolver;

        public CombatAltarRecipientResolver()
        {
            _positionResolver =
                new CombatAltarRecipientPositionResolver();
        }

        public CombatAltarRecipient TryResolve(
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
                    "Altar recipient resolution requires " +
                    "a valid donor position.",
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

            var recipientPosition =
                _positionResolver.Resolve(
                    donorPosition);

            var recipientSlot =
                FindSlot(
                    sideState.Board,
                    recipientPosition);

            if (recipientSlot == null ||
                !recipientSlot.IsOccupied)
            {
                return null;
            }

            var recipientCard =
                sideState.Cards.GetCard(
                    recipientSlot
                        .OccupantInstanceId.Value);

            return new CombatAltarRecipient(
                recipientPosition,
                recipientCard);
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