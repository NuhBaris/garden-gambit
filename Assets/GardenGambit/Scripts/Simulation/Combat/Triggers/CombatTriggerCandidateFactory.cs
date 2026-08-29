using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerCandidateFactory
    {
        public bool TryCreate(
            CombatState state,
            CombatEvent sourceEvent,
            ICombatTriggerOrderKeyProvider
                orderKeyProvider,
            ICombatTriggerHandler handler,
            out CombatTriggerCandidate<
                ICombatTriggerHandler> candidate)
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

            if (orderKeyProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(orderKeyProvider));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(
                    nameof(handler));
            }

            candidate = null;

            if (!handler.CanTrigger(
                    state,
                    sourceEvent))
            {
                return false;
            }

            var orderKey =
                orderKeyProvider.GetOrderKey(
                    state,
                    sourceEvent);

            if (!orderKey.IsValid)
            {
                throw new InvalidOperationException(
                    "Combat trigger order-key provider " +
                    "returned an invalid order key.");
            }

            candidate =
                new CombatTriggerCandidate<
                    ICombatTriggerHandler>(
                    orderKey,
                    handler);

            return true;
        }

        public bool TryCreate(
            CombatState state,
            CombatEvent sourceEvent,
            CombatTriggerOrderKey orderKey,
            ICombatTriggerHandler handler,
            out CombatTriggerCandidate<
                ICombatTriggerHandler> candidate)
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

            if (!orderKey.IsValid)
            {
                throw new ArgumentException(
                    "Combat trigger candidate creation " +
                    "requires a valid order key.",
                    nameof(orderKey));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(
                    nameof(handler));
            }

            candidate = null;

            if (!handler.CanTrigger(
                    state,
                    sourceEvent))
            {
                return false;
            }

            candidate =
                new CombatTriggerCandidate<
                    ICombatTriggerHandler>(
                    orderKey,
                    handler);

            return true;
        }
    }
}