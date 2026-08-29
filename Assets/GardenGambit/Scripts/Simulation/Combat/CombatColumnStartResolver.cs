using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatColumnStartResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatColumnStartResolver(
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

        public ColumnStartedCombatEvent StartColumn(
            CombatState state,
            CombatStartedCombatEvent combatStartedEvent,
            BoardColumn column)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            if (!column.IsValid)
            {
                throw new ArgumentException(
                    "A valid board column is required.",
                    nameof(column));
            }

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            EnsureColumnNotAlreadyStarted(
                combatStartedEvent,
                column);

            EnsureColumnStartsInOrder(
                combatStartedEvent,
                column);

            var metadata =
                _metadataFactory.CreateChild(
                    combatStartedEvent.Metadata);

            var columnStartedEvent =
                new ColumnStartedCombatEvent(
                    metadata,
                    column);

            _eventLog.EnsureCanAppend(
                columnStartedEvent);

            _eventLog.Append(
                columnStartedEvent);

            return columnStartedEvent;
        }

        private void ValidateLoggedCombatStartedEvent(
            CombatStartedCombatEvent combatStartedEvent)
        {
            if (!_eventLog.ContainsEvent(
                    combatStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatStartedEvent))
            {
                throw new ArgumentException(
                    "Combat Started event must be the " +
                    "exact event stored in the log.",
                    nameof(combatStartedEvent));
            }
        }

        private void EnsureColumnNotAlreadyStarted(
            CombatStartedCombatEvent combatStartedEvent,
            BoardColumn column)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingColumnEvent =
                    _eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (existingColumnEvent == null)
                {
                    continue;
                }

                if (!existingColumnEvent
                        .Metadata.HasParent)
                {
                    continue;
                }

                if (existingColumnEvent.Metadata
                        .ParentEventId.Value !=
                    combatStartedEvent.Metadata.EventId)
                {
                    continue;
                }

                if (existingColumnEvent.Column ==
                    column)
                {
                    throw new InvalidOperationException(
                        $"Column {column} has already " +
                        "started for this combat.");
                }
            }
        }

        private void EnsureColumnStartsInOrder(
            CombatStartedCombatEvent combatStartedEvent,
            BoardColumn requestedColumn)
        {
            var expectedColumnValue =
                BoardColumn.MinimumValue;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingColumnEvent =
                    _eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (existingColumnEvent == null)
                {
                    continue;
                }

                if (!existingColumnEvent
                        .Metadata.HasParent)
                {
                    continue;
                }

                if (existingColumnEvent.Metadata
                        .ParentEventId.Value !=
                    combatStartedEvent.Metadata.EventId)
                {
                    continue;
                }

                if (expectedColumnValue >
                    BoardColumn.MaximumValue)
                {
                    throw new InvalidOperationException(
                        "Combat column history contains " +
                        "too many started columns.");
                }

                var expectedExistingColumn =
                    new BoardColumn(
                        expectedColumnValue);

                if (existingColumnEvent.Column !=
                    expectedExistingColumn)
                {
                    throw new InvalidOperationException(
                        "Existing combat columns are not " +
                        "in strict left-to-right order.");
                }

                expectedColumnValue++;
            }

            if (expectedColumnValue >
                BoardColumn.MaximumValue)
            {
                throw new InvalidOperationException(
                    "All combat columns have already started.");
            }

            var expectedRequestedColumn =
                new BoardColumn(
                    expectedColumnValue);

            if (requestedColumn !=
                expectedRequestedColumn)
            {
                throw new InvalidOperationException(
                    $"Expected column " +
                    $"{expectedRequestedColumn}, but " +
                    $"{requestedColumn} was requested.");
            }
        }
    }
}