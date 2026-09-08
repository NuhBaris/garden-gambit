using System;
using System.Collections.Generic;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatFinalRankModifierRegistry
    {
        private readonly Dictionary<
            InstanceId,
            int> _modifiers;

        public CombatFinalRankModifierRegistry()
        {
            _modifiers =
                new Dictionary<
                    InstanceId,
                    int>();
        }

        public int Count =>
            _modifiers.Count;

        public void AddModifier(
            InstanceId cardInstanceId,
            int rankDelta)
        {
            ValidateInstanceId(
                cardInstanceId);

            if (rankDelta == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rankDelta),
                    rankDelta,
                    "Final Rank modifier cannot be zero.");
            }

            int existingModifier;

            if (!_modifiers.TryGetValue(
                    cardInstanceId,
                    out existingModifier))
            {
                existingModifier =
                    0;
            }

            var combinedModifier =
                checked(
                    existingModifier +
                    rankDelta);

            _modifiers[cardInstanceId] =
                combinedModifier;
        }

        public bool HasModifier(
            InstanceId cardInstanceId)
        {
            ValidateInstanceId(
                cardInstanceId);

            return _modifiers.ContainsKey(
                cardInstanceId);
        }

        public int GetTotalModifier(
            InstanceId cardInstanceId)
        {
            ValidateInstanceId(
                cardInstanceId);

            int modifier;

            if (_modifiers.TryGetValue(
                    cardInstanceId,
                    out modifier))
            {
                return modifier;
            }

            return 0;
        }

        private static void ValidateInstanceId(
            InstanceId cardInstanceId)
        {
            if (!cardInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(cardInstanceId));
            }
        }
    }
}