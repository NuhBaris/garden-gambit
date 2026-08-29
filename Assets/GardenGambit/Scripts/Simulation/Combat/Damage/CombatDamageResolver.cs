using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatDamageResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatDamageResolver(
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

        public DamageAppliedCombatEvent
            ApplyResolvedCardDamage(
                CombatState state,
                CombatEvent parentEvent,
                BoardPosition sourcePosition,
                BoardPosition targetPosition,
                int incomingDamage)
        {
            CombatCardState sourceCard;
            CombatCardState targetCard;

            ValidateRequest(
                state,
                parentEvent,
                sourcePosition,
                targetPosition,
                incomingDamage,
                out sourceCard,
                out targetCard);

            var metadata =
                _metadataFactory.CreateChild(
                    parentEvent.Metadata);

            return ApplyValidatedDamage(
                parentEvent,
                sourcePosition,
                targetPosition,
                incomingDamage,
                metadata,
                sourceCard,
                targetCard);
        }

        public DamageAppliedCombatEvent
            ApplyPreparedCardDamage(
                CombatState state,
                CombatEvent parentEvent,
                BoardPosition sourcePosition,
                BoardPosition targetPosition,
                int incomingDamage,
                CombatEventMetadata metadata)
        {
            CombatCardState sourceCard;
            CombatCardState targetCard;

            ValidateRequest(
                state,
                parentEvent,
                sourcePosition,
                targetPosition,
                incomingDamage,
                out sourceCard,
                out targetCard);

            ValidatePreparedMetadata(
                metadata,
                parentEvent);

            return ApplyValidatedDamage(
                parentEvent,
                sourcePosition,
                targetPosition,
                incomingDamage,
                metadata,
                sourceCard,
                targetCard);
        }

        private DamageAppliedCombatEvent
            ApplyValidatedDamage(
                CombatEvent parentEvent,
                BoardPosition sourcePosition,
                BoardPosition targetPosition,
                int incomingDamage,
                CombatEventMetadata metadata,
                CombatCardState sourceCard,
                CombatCardState targetCard)
        {
            EnsureMetadataCanBeAppended(metadata);

            var result =
                targetCard.ApplyIncomingDamage(
                    incomingDamage);

            var damageEvent =
                new DamageAppliedCombatEvent(
                    metadata,
                    sourceCard.InstanceId,
                    sourcePosition,
                    targetCard.InstanceId,
                    targetPosition,
                    result);

            _eventLog.Append(damageEvent);

            return damageEvent;
        }

        private void ValidateRequest(
            CombatState state,
            CombatEvent parentEvent,
            BoardPosition sourcePosition,
            BoardPosition targetPosition,
            int incomingDamage,
            out CombatCardState sourceCard,
            out CombatCardState targetCard)
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

            if (!sourcePosition.IsValid)
            {
                throw new ArgumentException(
                    "A valid source position is required.",
                    nameof(sourcePosition));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "A valid target position is required.",
                    nameof(targetPosition));
            }

            if (incomingDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incomingDamage),
                    incomingDamage,
                    "Incoming damage cannot be negative.");
            }

            var loggedParent = _eventLog.GetEvent(
                parentEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedParent,
                    parentEvent))
            {
                throw new ArgumentException(
                    "Parent event must be the exact event " +
                    "stored in the combat event log.",
                    nameof(parentEvent));
            }

            sourceCard = state
                .GetSide(sourcePosition.Side)
                .GetCardAt(sourcePosition);

            targetCard = state
                .GetSide(targetPosition.Side)
                .GetCardAt(targetPosition);

            EnsureDamageWillNotOverflow(
                targetCard,
                incomingDamage);
        }

        private static void ValidatePreparedMetadata(
            CombatEventMetadata metadata,
            CombatEvent parentEvent)
        {
            if (!metadata.IsValid)
            {
                throw new ArgumentException(
                    "Prepared damage metadata must be valid.",
                    nameof(metadata));
            }

            if (!metadata.HasParent ||
                metadata.ParentEventId.Value !=
                parentEvent.Metadata.EventId)
            {
                throw new ArgumentException(
                    "Prepared damage metadata must reference " +
                    "the supplied parent event.",
                    nameof(metadata));
            }

            if (metadata.TriggerRootId !=
                parentEvent.Metadata.TriggerRootId)
            {
                throw new ArgumentException(
                    "Prepared damage metadata must share " +
                    "the parent trigger root.",
                    nameof(metadata));
            }

            if (metadata.SequenceNo <=
                parentEvent.Metadata.SequenceNo)
            {
                throw new ArgumentException(
                    "Prepared damage sequence must follow " +
                    "the parent sequence.",
                    nameof(metadata));
            }
        }

        private void EnsureMetadataCanBeAppended(
            CombatEventMetadata metadata)
        {
            if (_eventLog.ContainsEvent(
                    metadata.EventId))
            {
                throw new InvalidOperationException(
                    $"Allocated EventId already exists " +
                    $"in the log: {metadata.EventId}.");
            }

            if (_eventLog.Count == 0)
            {
                return;
            }

            var previousSequence =
                _eventLog.Events[
                    _eventLog.Count - 1]
                    .Metadata.SequenceNo;

            if (metadata.SequenceNo <=
                previousSequence)
            {
                throw new InvalidOperationException(
                    "Allocated SequenceNo is not greater " +
                    "than the latest logged sequence.");
            }
        }

        private static void EnsureDamageWillNotOverflow(
            CombatCardState targetCard,
            int incomingDamage)
        {
            var armorAbsorbed =
                Math.Min(
                    targetCard.Armor,
                    incomingDamage);

            var hpDamage =
                incomingDamage - armorAbsorbed;

            var resultingHp =
                (long)targetCard.CurrentHp -
                hpDamage;

            if (resultingHp < int.MinValue ||
                resultingHp > int.MaxValue)
            {
                throw new OverflowException(
                    "Damage would overflow target HP.");
            }
        }
    }
}