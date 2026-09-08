using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatSideResultContributionResolver
    {
        private readonly
            WarBannerAttackMultiplierResolver
            _warBannerAttackMultiplierResolver;

        private readonly
            CombatFinalRankContributionResolver
            _finalRankContributionResolver;

        public CombatSideResultContributionResolver()
            : this(
                new
                    CombatFinalRankModifierRegistry())
        {
        }

        public CombatSideResultContributionResolver(
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
        {
            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(finalRankModifierRegistry));
            }

            _warBannerAttackMultiplierResolver =
                new WarBannerAttackMultiplierResolver();

            _finalRankContributionResolver =
                new CombatFinalRankContributionResolver(
                    finalRankModifierRegistry);
        }

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _finalRankContributionResolver
                    .ModifierRegistry;

        public CombatSideResultContribution Resolve(
            CombatSideState sideState)
        {
            if (sideState == null)
            {
                throw new ArgumentNullException(
                    nameof(sideState));
            }

            var survivorCount =
                0;

            var totalSurvivorRankContribution =
                0;

            foreach (var slot in
                     sideState.Board.Slots)
            {
                if (!slot.OccupantInstanceId
                        .HasValue)
                {
                    continue;
                }

                var card =
                    sideState.Cards.GetCard(
                        slot.OccupantInstanceId
                            .Value);

                if (card.IsAtDeathThreshold)
                {
                    continue;
                }

                var finalRankContribution =
                    _finalRankContributionResolver
                        .Resolve(
                            card);

                survivorCount =
                    checked(
                        survivorCount + 1);

                totalSurvivorRankContribution =
                    checked(
                        totalSurvivorRankContribution +
                        finalRankContribution);
            }

            var finalAttackMultiplier =
                _warBannerAttackMultiplierResolver
                    .Resolve(
                        sideState);

            return new CombatSideResultContribution(
                sideState.Side,
                survivorCount,
                totalSurvivorRankContribution,
                finalAttackMultiplier);
        }
    }
}