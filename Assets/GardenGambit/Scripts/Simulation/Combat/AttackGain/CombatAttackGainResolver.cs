using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAttackGainResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatAttackGainResolver(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog)
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

            _metadataFactory =
                metadataFactory;

            _eventLog =
                eventLog;
        }

        public AttackGainCombatEvent
            TryApplyAttackGain(
                CombatState state,
                CombatEvent parentEvent,
                BoardPosition targetPosition,
                int requestedAmount)
        {
            var targetCard =
                ValidateRequest(
                    state,
                    parentEvent,
                    targetPosition,
                    requestedAmount);

            if (requestedAmount == 0)
            {
                return null;
            }

            var previousAttack =
                targetCard.Attack;

            var currentAttackValue =
                (long)previousAttack +
                requestedAmount;

            if (currentAttackValue >
                int.MaxValue)
            {
                throw new OverflowException(
                    "Attack gain would overflow the " +
                    "target card's Attack value.");
            }

            var currentAttack =
                (int)currentAttackValue;

            var metadata =
                _metadataFactory.CreateChild(
                    parentEvent.Metadata);

            var attackGainEvent =
                new AttackGainCombatEvent(
                    metadata,
                    targetCard.InstanceId,
                    targetPosition,
                    previousAttack,
                    currentAttack);

            _eventLog.EnsureCanAppend(
                attackGainEvent);

            targetCard.ApplyAttackGain(
                requestedAmount);

            _eventLog.Append(
                attackGainEvent);

            return attackGainEvent;
        }

        public IReadOnlyList<AttackGainCombatEvent> TryApplyAttackGainBatch(
            CombatState state,
            CombatEvent parentEvent,
            IEnumerable<BoardPosition> targetPositions,
            int requestedAmount)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (parentEvent == null)
            {
                throw new ArgumentNullException(nameof(parentEvent));
            }

            if (targetPositions == null)
            {
                throw new ArgumentNullException(nameof(targetPositions));
            }

            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedAmount));
            }

            ValidateLoggedParentEvent(parentEvent);

            var positions = new List<BoardPosition>();
            var cards = new List<CombatCardState>();
            var targetIds = new HashSet<InstanceId>();

            // Validate the entire request before allocating or changing any target.
            // The caller supplies effect order; enumeration order is preserved.
            foreach (var position in targetPositions)
            {
                var card = ValidateRequest(state, parentEvent, position, requestedAmount);

                if (!targetIds.Add(card.InstanceId))
                {
                    throw new ArgumentException(
                        "An Attack gain batch cannot contain the same target more than once.",
                        nameof(targetPositions));
                }

                if ((long)card.Attack + requestedAmount > int.MaxValue)
                {
                    throw new OverflowException(
                        "Attack gain would overflow a target card's Attack value.");
                }

                positions.Add(position);
                cards.Add(card);
            }

            if (requestedAmount == 0 || cards.Count == 0)
            {
                return Array.Empty<AttackGainCombatEvent>();
            }

            var events = new List<AttackGainCombatEvent>(cards.Count);

            // Every child has the same logged parent. The shared factory supplies
            // unique IDs and increasing sequences within this synchronous batch.
            // A metadata/log rejection must leave all card stats and the log intact.
            // Allocations already made during this preflight are not rolled back.
            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                var gain = new AttackGainCombatEvent(
                    _metadataFactory.CreateChild(parentEvent.Metadata),
                    card.InstanceId,
                    positions[i],
                    card.Attack,
                    checked(card.Attack + requestedAmount));

                _eventLog.EnsureCanAppend(gain);
                events.Add(gain);
            }

            for (var i = 0; i < cards.Count; i++)
            {
                cards[i].ApplyAttackGain(requestedAmount);
                _eventLog.Append(events[i]);
            }

            return events.AsReadOnly();
        }

        private CombatCardState ValidateRequest(
            CombatState state,
            CombatEvent parentEvent,
            BoardPosition targetPosition,
            int requestedAmount)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (parentEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(parentEvent));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "Attack gain requires a valid target " +
                    "board position.",
                    nameof(targetPosition));
            }

            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Requested Attack gain amount cannot " +
                    "be negative.");
            }

            ValidateLoggedParentEvent(
                parentEvent);

            return state.GetSide(
                    targetPosition.Side)
                .GetCardAt(
                    targetPosition);
        }

        private void ValidateLoggedParentEvent(
            CombatEvent parentEvent)
        {
            if (!_eventLog.ContainsEvent(
                    parentEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Attack gain parent event must already " +
                    "exist in the combat event log.",
                    nameof(parentEvent));
            }

            var loggedParentEvent =
                _eventLog.GetEvent(
                    parentEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedParentEvent,
                    parentEvent))
            {
                throw new ArgumentException(
                    "Attack gain parent event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(parentEvent));
            }
        }
    }
}
