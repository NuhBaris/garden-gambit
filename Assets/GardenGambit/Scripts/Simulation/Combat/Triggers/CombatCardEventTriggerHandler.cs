using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatCardEventTriggerHandler<TEvent> :
        CombatEventTriggerHandler<TEvent>
        where TEvent : CombatEvent
    {
        protected CombatCardEventTriggerHandler(
            CombatSide side,
            InstanceId cardInstanceId)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Card trigger handler requires " +
                    "Player or Enemy side.");
            }

            if (!cardInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(cardInstanceId));
            }

            Side =
                side;

            CardInstanceId =
                cardInstanceId;
        }

        public CombatSide Side
        {
            get;
        }

        public InstanceId CardInstanceId
        {
            get;
        }

        protected sealed override bool
            CanTriggerTyped(
                CombatState state,
                TEvent sourceEvent)
        {
            CombatCardState card;

            if (!TryGetCard(
                    state,
                    out card))
            {
                return false;
            }

            return CanCardTrigger(
                state,
                sourceEvent,
                card);
        }

        protected sealed override void
            ResolveTyped(
                CombatState state,
                TEvent sourceEvent)
        {
            CombatCardState card;

            if (!TryGetCard(
                    state,
                    out card))
            {
                return;
            }

            ResolveCardTrigger(
                state,
                sourceEvent,
                card);
        }

        protected abstract bool
            CanCardTrigger(
                CombatState state,
                TEvent sourceEvent,
                CombatCardState card);

        protected abstract void
            ResolveCardTrigger(
                CombatState state,
                TEvent sourceEvent,
                CombatCardState card);

        private bool TryGetCard(
            CombatState state,
            out CombatCardState card)
        {
            var cards =
                state.GetSide(
                        Side)
                    .Cards.Cards;

            for (var index = 0;
                 index < cards.Count;
                 index++)
            {
                var candidate =
                    cards[index];

                if (candidate.InstanceId !=
                    CardInstanceId)
                {
                    continue;
                }

                card =
                    candidate;

                return true;
            }

            card =
                null;

            return false;
        }
    }
}