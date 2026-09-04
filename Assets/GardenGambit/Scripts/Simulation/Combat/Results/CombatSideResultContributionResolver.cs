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

        public CombatSideResultContributionResolver()
        {
            _warBannerAttackMultiplierResolver =
                new WarBannerAttackMultiplierResolver();
        }

        public CombatSideResultContribution Resolve(
            CombatSideState sideState)
        {
            if (sideState == null)
            {
                throw new ArgumentNullException(
                    nameof(sideState));
            }

            var survivorCount = 0;

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

                survivorCount =
                    checked(
                        survivorCount + 1);

                totalSurvivorRankContribution =
                    checked(
                        totalSurvivorRankContribution +
                        card.Rank.Value);
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