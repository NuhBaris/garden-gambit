using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        WarBannerAttackMultiplierResolver
    {
        private readonly WarBannerCountResolver
            _warBannerCountResolver;

        public WarBannerAttackMultiplierResolver()
        {
            _warBannerCountResolver =
                new WarBannerCountResolver();
        }

        public AttackMultiplier Resolve(
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
                    "War Banner multiplier resolution " +
                    "requires Player or Enemy side.");
            }

            var sideState =
                state.GetSide(
                    side);

            return Resolve(
                sideState);
        }

        public AttackMultiplier Resolve(
            CombatSideState sideState)
        {
            if (sideState == null)
            {
                throw new ArgumentNullException(
                    nameof(sideState));
            }

            var activeWarBannerCount =
                _warBannerCountResolver.Resolve(
                    sideState);

            var resolvedMultiplierValue =
                checked(
                    sideState.AttackMultiplier.Value +
                    activeWarBannerCount);

            return new AttackMultiplier(
                resolvedMultiplierValue);
        }
    }
}