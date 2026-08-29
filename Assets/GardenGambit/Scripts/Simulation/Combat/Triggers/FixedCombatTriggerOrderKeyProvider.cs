using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        FixedCombatTriggerOrderKeyProvider :
        ICombatTriggerOrderKeyProvider
    {
        public FixedCombatTriggerOrderKeyProvider(
            CombatTriggerOrderKey orderKey)
        {
            if (!orderKey.IsValid)
            {
                throw new ArgumentException(
                    "Fixed combat trigger order-key " +
                    "provider requires a valid key.",
                    nameof(orderKey));
            }

            OrderKey = orderKey;
        }

        public CombatTriggerOrderKey OrderKey
        {
            get;
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

            return OrderKey;
        }
    }
}