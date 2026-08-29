using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatCardAdvancementResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatCardAdvancementResolver(
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

        public CardAdvancedCombatEvent TryAdvance(
            CombatState state,
            CombatEvent parentEvent,
            CombatSide side,
            BoardColumn column)
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

            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Card advancement requires " +
                    "Player or Enemy side.");
            }

            if (!column.IsValid)
            {
                throw new ArgumentException(
                    "Card advancement requires a valid column.",
                    nameof(column));
            }

            ValidateLoggedParentEvent(
                parentEvent);

            EnsureColumnNotAlreadyAdvanced(
                parentEvent,
                side,
                column);

            var frontPosition =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    column);

            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    column);

            var combatSide =
                state.GetSide(side);

            var frontSlot =
                combatSide.Board.GetSlot(
                    frontPosition);

            var backSlot =
                combatSide.Board.GetSlot(
                    backPosition);

            if (frontSlot.IsOccupied)
            {
                return null;
            }

            if (!backSlot.IsOccupied)
            {
                return null;
            }

            var card =
                combatSide.GetCardAt(
                    backPosition);

            if (card.IsAtDeathThreshold)
            {
                throw new InvalidOperationException(
                    "A card at the death threshold " +
                    "cannot advance.");
            }

            var metadata =
                _metadataFactory.CreateChild(
                    parentEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            var advancedEvent =
                new CardAdvancedCombatEvent(
                    metadata,
                    card.InstanceId,
                    backPosition,
                    frontPosition);

            var movedCard =
                combatSide.MoveCard(
                    backPosition,
                    frontPosition);

            if (!ReferenceEquals(
                    movedCard,
                    card))
            {
                throw new InvalidOperationException(
                    "Moved card does not match the " +
                    "validated advancement card.");
            }

            _eventLog.Append(
                advancedEvent);

            return advancedEvent;
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

        private void EnsureColumnNotAlreadyAdvanced(
            CombatEvent parentEvent,
            CombatSide side,
            BoardColumn column)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var advancedEvent =
                    _eventLog.Events[index]
                        as CardAdvancedCombatEvent;

                if (advancedEvent == null)
                {
                    continue;
                }

                if (!advancedEvent.Metadata.HasParent)
                {
                    continue;
                }

                if (advancedEvent.Metadata
                        .ParentEventId.Value !=
                    parentEvent.Metadata.EventId)
                {
                    continue;
                }

                if (advancedEvent.SourcePosition.Side ==
                        side &&
                    advancedEvent.SourcePosition.Column ==
                        column)
                {
                    throw new InvalidOperationException(
                        "This side and column have already " +
                        "advanced for the parent event.");
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