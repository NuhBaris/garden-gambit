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
            : this(
                new
                    CombatFinalRankModifierRegistry())
        {
        }

        public CombatResultDamageResolver(
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
        {
            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(finalRankModifierRegistry));
            }

            _contributionResolver =
                new
                    CombatSideResultContributionResolver(
                        finalRankModifierRegistry);
        }

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _contributionResolver
                    .FinalRankModifierRegistry;

        public CombatResultDamageCalculation Resolve(
            CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            var playerContribution =
                _contributionResolver.Resolve(
                    state.GetSide(
                        CombatSide.Player));

            var enemyContribution =
                _contributionResolver.Resolve(
                    state.GetSide(
                        CombatSide.Enemy));

            return new CombatResultDamageCalculation(
                playerContribution,
                enemyContribution);
        }
    }
}