using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct CombatEventMetadata
    {
        public CombatEventMetadata(
            CombatEventId eventId,
            CombatSequenceNumber sequenceNo,
            CombatEventId? parentEventId,
            CombatEventId triggerRootId)
        {
            if (!eventId.IsValid)
            {
                throw new ArgumentException(
                    "A valid CombatEventId is required.",
                    nameof(eventId));
            }

            if (!sequenceNo.IsValid)
            {
                throw new ArgumentException(
                    "A valid CombatSequenceNumber is required.",
                    nameof(sequenceNo));
            }

            if (!triggerRootId.IsValid)
            {
                throw new ArgumentException(
                    "A valid trigger root event ID is required.",
                    nameof(triggerRootId));
            }

            if (parentEventId.HasValue &&
                !parentEventId.Value.IsValid)
            {
                throw new ArgumentException(
                    "Parent event ID must be valid when provided.",
                    nameof(parentEventId));
            }

            if (parentEventId.HasValue &&
                parentEventId.Value == eventId)
            {
                throw new ArgumentException(
                    "An event cannot be its own parent.",
                    nameof(parentEventId));
            }

            var isRoot =
                eventId == triggerRootId;

            if (isRoot && parentEventId.HasValue)
            {
                throw new ArgumentException(
                    "A trigger-root event cannot have a parent.",
                    nameof(parentEventId));
            }

            if (!isRoot && !parentEventId.HasValue)
            {
                throw new ArgumentException(
                    "A non-root event requires a parent event ID.",
                    nameof(parentEventId));
            }

            EventId = eventId;
            SequenceNo = sequenceNo;
            ParentEventId = parentEventId;
            TriggerRootId = triggerRootId;
        }

        public CombatEventId EventId { get; }

        public CombatSequenceNumber SequenceNo { get; }

        public CombatEventId? ParentEventId { get; }

        public CombatEventId TriggerRootId { get; }

        public bool HasParent =>
            ParentEventId.HasValue;

        public bool IsTriggerRoot =>
            EventId.IsValid &&
            EventId == TriggerRootId;

        public bool IsValid
        {
            get
            {
                if (!EventId.IsValid ||
                    !SequenceNo.IsValid ||
                    !TriggerRootId.IsValid)
                {
                    return false;
                }

                if (ParentEventId.HasValue &&
                    !ParentEventId.Value.IsValid)
                {
                    return false;
                }

                if (ParentEventId.HasValue &&
                    ParentEventId.Value == EventId)
                {
                    return false;
                }

                return IsTriggerRoot
                    ? !ParentEventId.HasValue
                    : ParentEventId.HasValue;
            }
        }
    }
}