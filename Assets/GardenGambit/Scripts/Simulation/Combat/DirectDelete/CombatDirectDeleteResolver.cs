using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatDirectDeleteResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatCardRemovalCommitter
            _removalCommitter;

        public CombatDirectDeleteResolver(
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

            _removalCommitter =
                new CombatCardRemovalCommitter(
                    eventLog);
        }

        public DirectDeleteCombatEvent ApplyDirectDelete(
            CombatState state,
            CombatEvent parentEvent,
            BoardPosition targetPosition)
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
                    "A valid target position is required.",
                    nameof(targetPosition));
            }

            ValidateLoggedParentEvent(
                parentEvent);

            EnsureTargetNotAlreadyDeleted(
                parentEvent,
                targetPosition);

            var side =
                state.GetSide(
                    targetPosition.Side);

            var card =
                side.GetCardAt(
                    targetPosition);

            var metadata =
                _metadataFactory.CreateChild(
                    parentEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            var deleteEvent =
                new DirectDeleteCombatEvent(
                    metadata,
                    card.InstanceId,
                    targetPosition,
                    card.CurrentHp);

            var tombstone =
                new CombatCardTombstone(
                    card,
                    targetPosition,
                    CombatCardRemovalReason.DirectDelete,
                    metadata);

            _removalCommitter.EnsureCanCommit(
                deleteEvent,
                tombstone);

            var removedCard =
                side.RemoveCardFromCombat(
                    targetPosition);

            if (!ReferenceEquals(
                    removedCard,
                    card))
            {
                throw new InvalidOperationException(
                    "Removed card does not match the " +
                    "validated Direct Delete target.");
            }

            _removalCommitter.Commit(
                deleteEvent,
                tombstone);

            return deleteEvent;
        }

        private void ValidateLoggedParentEvent(
            CombatEvent parentEvent)
        {
            if (!_eventLog.ContainsEvent(
                    parentEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Parent event must already exist " +
                    "in the combat event log.",
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
                    "Parent event must be the exact event " +
                    "stored in the combat event log.",
                    nameof(parentEvent));
            }
        }

        private void EnsureTargetNotAlreadyDeleted(
            CombatEvent parentEvent,
            BoardPosition targetPosition)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingDeleteEvent =
                    _eventLog.Events[index]
                        as DirectDeleteCombatEvent;

                if (existingDeleteEvent == null)
                {
                    continue;
                }

                if (!existingDeleteEvent
                        .Metadata.HasParent)
                {
                    continue;
                }

                if (existingDeleteEvent.Metadata
                        .ParentEventId.Value !=
                    parentEvent.Metadata.EventId)
                {
                    continue;
                }

                if (existingDeleteEvent.Position ==
                    targetPosition)
                {
                    throw new InvalidOperationException(
                        "This target position has already " +
                        "been Direct Deleted by the parent event.");
                }
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
    }
}