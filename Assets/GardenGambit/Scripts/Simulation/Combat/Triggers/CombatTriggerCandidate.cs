using GardenGambit.Domain.Combat;
using System;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerCandidate<TTrigger>
        where TTrigger : class
    {
        public CombatTriggerCandidate(
            CombatTriggerOrderKey orderKey,
            TTrigger trigger)
        {
            if (!orderKey.IsValid)
            {
                throw new ArgumentException(
                    "Combat trigger candidate requires a valid " +
                    "CombatTriggerOrderKey.",
                    nameof(orderKey));
            }

            if (trigger == null)
            {
                throw new ArgumentNullException(
                    nameof(trigger));
            }

            OrderKey = orderKey;
            Trigger = trigger;
        }

        public CombatTriggerOrderKey OrderKey { get; }

        public TTrigger Trigger { get; }
    }
}