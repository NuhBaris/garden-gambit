using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatArmorGainResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatArmorGainResolver(
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

        public ArmorGainCombatEvent
            TryApplyArmorGain(
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

            var previousArmor =
                targetCard.Armor;

            var currentArmorValue =
                (long)previousArmor +
                requestedAmount;

            if (currentArmorValue >
                int.MaxValue)
            {
                throw new OverflowException(
                    "Armor gain would overflow the " +
                    "target card's Armor value.");
            }

            var currentArmor =
                (int)currentArmorValue;

            var metadata =
                _metadataFactory.CreateChild(
                    parentEvent.Metadata);

            var armorGainEvent =
                new ArmorGainCombatEvent(
                    metadata,
                    targetCard.InstanceId,
                    targetPosition,
                    previousArmor,
                    currentArmor);

            _eventLog.EnsureCanAppend(
                armorGainEvent);

            targetCard.ApplyArmorGain(
                requestedAmount);

            _eventLog.Append(
                armorGainEvent);

            return armorGainEvent;
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
                    "Armor gain requires a valid target " +
                    "board position.",
                    nameof(targetPosition));
            }

            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Requested Armor gain amount cannot " +
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
                    "Armor gain parent event must already " +
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
                    "Armor gain parent event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(parentEvent));
            }
        }
    }
}