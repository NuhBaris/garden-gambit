using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ProtectiveSealCountResolver
    {
        public int Resolve(
            CombatState state,
            CombatSide side)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Protective Seal count requires " +
                    "Player or Enemy side.");
            }

            var sideState =
                state.GetSide(
                    side);

            var activeSealCount = 0;

            foreach (var slot in
                     sideState.Board.Slots)
            {
                if (!slot.HasProtectiveSeal)
                {
                    continue;
                }

                if (!slot.IsOccupied)
                {
                    continue;
                }

                var card =
                    sideState.GetCardAt(
                        slot.Position);

                if (card.IsAtDeathThreshold)
                {
                    continue;
                }

                activeSealCount =
                    checked(
                        activeSealCount + 1);
            }

            return activeSealCount;
        }
    }
}