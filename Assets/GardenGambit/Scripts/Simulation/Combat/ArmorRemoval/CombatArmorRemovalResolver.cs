using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatArmorRemovalResolver
    {
        private readonly CombatEventMetadataFactory _metadataFactory;
        private readonly CombatEventLog _eventLog;

        public CombatArmorRemovalResolver(CombatEventMetadataFactory metadataFactory, CombatEventLog eventLog)
        {
            if (metadataFactory == null)
            {
                throw new ArgumentNullException(nameof(metadataFactory));
            }
            if (eventLog == null)
            {
                throw new ArgumentNullException(nameof(eventLog));
            }
            _metadataFactory = metadataFactory;
            _eventLog = eventLog;
        }

        public ArmorRemovedCombatEvent TryRemoveArmor(
            CombatState state,
            CombatEvent parentEvent,
            InstanceId sourceInstanceId,
            BoardPosition targetPosition,
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
            if (!sourceInstanceId.IsValid)
            {
                throw new ArgumentException("A valid source InstanceId is required.", nameof(sourceInstanceId));
            }
            if (!targetPosition.IsValid)
            {
                throw new ArgumentException("A valid target board position is required.", nameof(targetPosition));
            }
            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedAmount));
            }
            ValidateLoggedParentEvent(parentEvent);
            var card = state.GetSide(targetPosition.Side).GetCardAt(targetPosition);
            var previousArmor = card.Armor;
            var actualRemovedAmount = Math.Min(previousArmor, requestedAmount);
            if (actualRemovedAmount == 0)
            {
                return null;
            }

            var result = new ArmorRemovedCombatEvent(
                _metadataFactory.CreateChild(parentEvent.Metadata), sourceInstanceId,
                card.InstanceId, targetPosition, previousArmor, previousArmor - actualRemovedAmount);
            // Validate metadata/log acceptance before changing the card, as in
            // the existing stat-gain resolvers. No HP or damage event is involved.
            _eventLog.EnsureCanAppend(result);
            card.RemoveArmor(actualRemovedAmount);
            _eventLog.Append(result);
            return result;
        }

        private void ValidateLoggedParentEvent(CombatEvent parentEvent)
        {
            if (!_eventLog.ContainsEvent(parentEvent.Metadata.EventId))
            {
                throw new ArgumentException("Armor removal parent event must already exist in the combat event log.", nameof(parentEvent));
            }
            if (!ReferenceEquals(_eventLog.GetEvent(parentEvent.Metadata.EventId), parentEvent))
            {
                throw new ArgumentException("Armor removal parent must be the exact event stored in the combat event log.", nameof(parentEvent));
            }
        }
    }
}
