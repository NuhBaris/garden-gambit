using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class WarBannerCountResolver
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
                    "War Banner count requires Player " +
                    "or Enemy side.");
            }

            var sideState =
                state.GetSide(
                    side);

            return Resolve(
                sideState);
        }

        public int Resolve(
            CombatSideState sideState)
        {
            if (sideState == null)
            {
                throw new ArgumentNullException(
                    nameof(sideState));
            }

            var activeWarBannerCount = 0;

            foreach (var slot in
                     sideState.Board.Slots)
            {
                if (!slot.HasWarBanner)
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

                activeWarBannerCount =
                    checked(
                        activeWarBannerCount + 1);
            }

            return activeWarBannerCount;
        }
    }
}