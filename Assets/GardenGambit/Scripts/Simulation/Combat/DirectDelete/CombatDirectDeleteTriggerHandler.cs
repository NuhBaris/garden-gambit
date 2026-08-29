using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatDirectDeleteTriggerHandler<TEvent> :
        CombatEventTriggerHandler<TEvent>
        where TEvent : CombatEvent
    {
        private readonly CombatDirectDeleteResolver
            _directDeleteResolver;

        protected CombatDirectDeleteTriggerHandler(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            BoardPosition targetPosition)
        {
            if (metadataFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(metadataFactory));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "A valid Direct Delete target " +
                    "position is required.",
                    nameof(targetPosition));
            }

            _directDeleteResolver =
                new CombatDirectDeleteResolver(
                    metadataFactory,
                    eventLog);

            TargetPosition = targetPosition;
        }

        protected BoardPosition TargetPosition
        {
            get;
        }

        protected sealed override bool CanTriggerTyped(
            CombatState state,
            TEvent sourceEvent)
        {
            return CanDirectDelete(
                state,
                sourceEvent);
        }

        protected sealed override void ResolveTyped(
            CombatState state,
            TEvent sourceEvent)
        {
            var targetSlot =
                state.GetSide(TargetPosition.Side)
                    .Board.GetSlot(TargetPosition);

            if (!targetSlot.IsOccupied)
            {
                return;
            }

            _directDeleteResolver.ApplyDirectDelete(
                state,
                sourceEvent,
                TargetPosition);
        }

        protected abstract bool CanDirectDelete(
            CombatState state,
            TEvent sourceEvent);
    }
}