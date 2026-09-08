using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalColumnsCompletionValidator
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly CombatColumnFrontlineResolver
            _frontlineResolver;

        public CombatNormalColumnsCompletionValidator(
            CombatEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _eventLog =
                eventLog;

            _frontlineResolver =
                new CombatColumnFrontlineResolver();
        }

        public void Validate(
            CombatState state,
            CombatStartedCombatEvent
                combatStartedEvent)
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

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            ValidateStartedColumnSequence(
                combatStartedEvent);

            ValidateResolvedColumnStates(
                state);
        }

        private void
            ValidateLoggedCombatStartedEvent(
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            if (!combatStartedEvent.Metadata
                    .IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            if (!_eventLog.ContainsEvent(
                    combatStartedEvent.Metadata
                        .EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent.Metadata
                        .EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatStartedEvent))
            {
                throw new ArgumentException(
                    "Combat Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(combatStartedEvent));
            }
        }

        private void ValidateStartedColumnSequence(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            var expectedColumnValue =
                BoardColumn.MinimumValue;

            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var columnEvent =
                    _eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (columnEvent == null)
                {
                    continue;
                }

                if (columnEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                if (!columnEvent.Metadata.HasParent ||
                    columnEvent.Metadata
                        .ParentEventId.Value !=
                    combatStartedEvent.Metadata
                        .EventId)
                {
                    throw new InvalidOperationException(
                        "Every normal Column Started event " +
                        "must be a direct child of its " +
                        "Combat Started event.");
                }

                if (expectedColumnValue >
                    BoardColumn.MaximumValue)
                {
                    throw new InvalidOperationException(
                        "More than five normal columns " +
                        "were started for this combat.");
                }

                if (columnEvent.Column.Value !=
                    expectedColumnValue)
                {
                    throw new InvalidOperationException(
                        $"Expected completed column " +
                        $"{expectedColumnValue}, but logged " +
                        $"column {columnEvent.Column.Value}.");
                }

                expectedColumnValue =
                    checked(
                        expectedColumnValue + 1);
            }

            var completedColumnCount =
                expectedColumnValue -
                BoardColumn.MinimumValue;

            var requiredColumnCount =
                BoardColumn.MaximumValue -
                BoardColumn.MinimumValue +
                1;

            if (completedColumnCount !=
                requiredColumnCount)
            {
                throw new InvalidOperationException(
                    "Combat result requires all five " +
                    "normal columns to be started in " +
                    "1, 2, 3, 4, 5 order.");
            }
        }

        private void ValidateResolvedColumnStates(
            CombatState state)
        {
            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                     BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

                BoardPosition playerPosition;
                BoardPosition enemyPosition;

                var hasAvailableExchange =
                    _frontlineResolver
                        .TryGetExchangePositions(
                            state,
                            column,
                            out playerPosition,
                            out enemyPosition);

                if (!hasAvailableExchange)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Column {columnValue} still has " +
                    "opposing living Front cards and " +
                    "cannot be considered complete.");
            }
        }
    }
}