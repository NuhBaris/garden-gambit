using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatEventTriggerHandler<TEvent> :
        ICombatTriggerHandler
        where TEvent : CombatEvent
    {
        public bool CanTrigger(
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

            var typedEvent =
                sourceEvent as TEvent;

            if (typedEvent == null)
            {
                return false;
            }

            return CanTriggerTyped(
                state,
                typedEvent);
        }

        public void Resolve(
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

            var typedEvent =
                sourceEvent as TEvent;

            if (typedEvent == null)
            {
                throw new ArgumentException(
                    $"Combat trigger handler requires " +
                    $"{typeof(TEvent).Name}.",
                    nameof(sourceEvent));
            }

            ResolveTyped(
                state,
                typedEvent);
        }

        protected abstract bool CanTriggerTyped(
            CombatState state,
            TEvent sourceEvent);

        protected abstract void ResolveTyped(
            CombatState state,
            TEvent sourceEvent);
    }
}