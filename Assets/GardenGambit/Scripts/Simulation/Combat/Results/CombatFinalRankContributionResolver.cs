using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatFinalRankContributionResolver
    {
        private readonly
            CombatFinalRankModifierRegistry
            _modifierRegistry;

        public CombatFinalRankContributionResolver(
            CombatFinalRankModifierRegistry
                modifierRegistry)
        {
            if (modifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(modifierRegistry));
            }

            _modifierRegistry =
                modifierRegistry;
        }

        public CombatFinalRankModifierRegistry
            ModifierRegistry =>
                _modifierRegistry;

        public int Resolve(
            CombatCardState card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(
                    nameof(card));
            }

            return Resolve(
                card.InstanceId,
                card.Rank.Value);
        }

        public int Resolve(
            InstanceId cardInstanceId,
            int structuralRankContribution)
        {
            if (!cardInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(cardInstanceId));
            }

            if (structuralRankContribution <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(structuralRankContribution),
                    structuralRankContribution,
                    "Structural Rank contribution must " +
                    "be greater than zero.");
            }

            var modifier =
                _modifierRegistry.GetTotalModifier(
                    cardInstanceId);

            var resolvedContribution =
                (long)structuralRankContribution +
                modifier;

            if (resolvedContribution <= 0L)
            {
                throw new InvalidOperationException(
                    "Resolved final Rank contribution " +
                    "must be greater than zero.");
            }

            if (resolvedContribution > int.MaxValue)
            {
                throw new OverflowException(
                    "Resolved final Rank contribution " +
                    "exceeds Int32 maximum value.");
            }

            return (int)resolvedContribution;
        }
    }
}