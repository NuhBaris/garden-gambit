using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatResultDamageResolver
    {
        private readonly
            CombatSideResultContributionResolver
            _contributionResolver;

        public CombatResultDamageResolver()
        {
            _contributionResolver =
                new CombatSideResultContributionResolver();
        }

        public CombatResultDamageCalculation Resolve(
            CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            var playerSide =
                state.GetSide(
                    CombatSide.Player);

            var enemySide =
                state.GetSide(
                    CombatSide.Enemy);

            var playerContribution =
                _contributionResolver.Resolve(
                    playerSide);

            var enemyContribution =
                _contributionResolver.Resolve(
                    enemySide);

            return new CombatResultDamageCalculation(
                playerContribution,
                enemyContribution);
        }
    }
}