using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatHpGainResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatHpGainResolver(
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

            _metadataFactory = metadataFactory;
            _eventLog = eventLog;
        }

        public HpGainCombatEvent TryApplyHpStatGain(
            CombatState state,
            CombatEvent parentEvent,
            BoardPosition targetPosition,
            int requestedAmount)
        {
            var targetCard = ValidateRequest(
                state,
                parentEvent,
                targetPosition,
                requestedAmount);

            return TryApplyHpStatGain(
                state,
                parentEvent,
                targetCard.InstanceId,
                targetPosition,
                requestedAmount);
        }

        public HpGainCombatEvent TryApplyHpStatGain(
            CombatState state,
            CombatEvent parentEvent,
            InstanceId sourceInstanceId,
            BoardPosition targetPosition,
            int requestedAmount)
        {
            ValidateSourceInstanceId(
                sourceInstanceId);

            var targetCard = ValidateRequest(
                state,
                parentEvent,
                targetPosition,
                requestedAmount);

            if (requestedAmount == 0)
            {
                return null;
            }

            var previousHpCapacity =
                targetCard.HpCapacity;

            var previousHp =
                targetCard.CurrentHp;

            var currentHpCapacityValue =
                (long)previousHpCapacity + requestedAmount;

            var currentHpValue =
                (long)previousHp + requestedAmount;

            if (currentHpCapacityValue > int.MaxValue ||
                currentHpValue > int.MaxValue)
            {
                throw new OverflowException(
                    "HP stat gain would overflow the " +
                    "target card's HP values.");
            }

            var currentHpCapacity =
                (int)currentHpCapacityValue;

            var currentHp =
                (int)currentHpValue;

            var metadata = _metadataFactory.CreateChild(
                parentEvent.Metadata);

            var hpGainEvent = new HpGainCombatEvent(
                metadata,
                sourceInstanceId,
                targetCard.InstanceId,
                targetPosition,
                previousHpCapacity,
                currentHpCapacity,
                previousHp,
                currentHp);

            _eventLog.EnsureCanAppend(
                hpGainEvent);

            targetCard.ApplyHpStatGain(
                requestedAmount);

            _eventLog.Append(
                hpGainEvent);

            return hpGainEvent;
        }

        public HpGainCombatEvent TryApplyHeal(
            CombatState state,
            CombatEvent parentEvent,
            BoardPosition targetPosition,
            int requestedAmount)
        {
            var targetCard = ValidateRequest(
                state,
                parentEvent,
                targetPosition,
                requestedAmount);

            return TryApplyHeal(
                state,
                parentEvent,
                targetCard.InstanceId,
                targetPosition,
                requestedAmount);
        }

        public HpGainCombatEvent TryApplyHeal(
            CombatState state,
            CombatEvent parentEvent,
            InstanceId sourceInstanceId,
            BoardPosition targetPosition,
            int requestedAmount)
        {
            ValidateSourceInstanceId(
                sourceInstanceId);

            var targetCard = ValidateRequest(
                state,
                parentEvent,
                targetPosition,
                requestedAmount);

            if (requestedAmount == 0)
            {
                return null;
            }

            var previousHpCapacity =
                targetCard.HpCapacity;

            var previousHp =
                targetCard.CurrentHp;

            var missingHp =
                (long)previousHpCapacity - previousHp;

            var actualGainedAmount = (int)Math.Min(
                (long)requestedAmount,
                missingHp);

            if (actualGainedAmount <= 0)
            {
                return null;
            }

            var currentHp = checked(
                previousHp + actualGainedAmount);

            var metadata = _metadataFactory.CreateChild(
                parentEvent.Metadata);

            var hpGainEvent = new HpGainCombatEvent(
                metadata,
                sourceInstanceId,
                targetCard.InstanceId,
                targetPosition,
                previousHpCapacity,
                previousHpCapacity,
                previousHp,
                currentHp);

            _eventLog.EnsureCanAppend(
                hpGainEvent);

            targetCard.Heal(
                requestedAmount);

            _eventLog.Append(
                hpGainEvent);

            return hpGainEvent;
        }

        private static void ValidateSourceInstanceId(
            InstanceId sourceInstanceId)
        {
            if (!sourceInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "HP gain requires a valid source InstanceId.",
                    nameof(sourceInstanceId));
            }
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
                    "HP gain requires a valid target " +
                    "board position.",
                    nameof(targetPosition));
            }

            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Requested HP gain amount cannot " +
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
                    "HP gain parent event must already " +
                    "exist in the combat event log.",
                    nameof(parentEvent));
            }

            var loggedParentEvent = _eventLog.GetEvent(
                parentEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedParentEvent,
                    parentEvent))
            {
                throw new ArgumentException(
                    "HP gain parent event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(parentEvent));
            }
        }
    }
}