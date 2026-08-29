using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ComputedCombatTriggerOrderKeyProvider :
        ICombatTriggerOrderKeyProvider
    {
        private readonly Func<
            CombatState,
            CombatEvent,
            CombatTriggerOrderKey>
            _computeOrderKey;

        public ComputedCombatTriggerOrderKeyProvider(
            Func<
                CombatState,
                CombatEvent,
                CombatTriggerOrderKey>
                computeOrderKey)
        {
            if (computeOrderKey == null)
            {
                throw new ArgumentNullException(
                    nameof(computeOrderKey));
            }

            _computeOrderKey = computeOrderKey;
        }

        public CombatTriggerOrderKey GetOrderKey(
            CombatState state,
            CombatEvent sourceEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (sourceEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceEvent));
            }

            var orderKey =
                _computeOrderKey(
                    state,
                    sourceEvent);

            if (!orderKey.IsValid)
            {
                throw new InvalidOperationException(
                    "Computed combat trigger order-key " +
                    "provider returned an invalid key.");
            }

            return orderKey;
        }
    }
}