using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatBattleEndResultPrerequisiteValidator
    {
        private readonly CombatEventLog
            _eventLog;

        public
            CombatBattleEndResultPrerequisiteValidator(
                CombatEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _eventLog =
                eventLog;
        }

        public BattleEndStartedCombatEvent Validate(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            BattleEndStartedCombatEvent
                matchingBattleEndEvent = null;

            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var battleEndEvent =
                    _eventLog.Events[index]
                        as
                            BattleEndStartedCombatEvent;

                if (battleEndEvent == null)
                {
                    continue;
                }

                if (battleEndEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                ValidateDirectChild(
                    combatStartedEvent,
                    battleEndEvent);

                if (matchingBattleEndEvent != null)
                {
                    throw new InvalidOperationException(
                        "Combat result requires exactly " +
                        "one Battle End Started event.");
                }

                matchingBattleEndEvent =
                    battleEndEvent;
            }

            if (matchingBattleEndEvent == null)
            {
                throw new InvalidOperationException(
                    "Combat result cannot be calculated " +
                    "before Battle End has started.");
            }

            EnsureNoLaterColumnStartedEvent(
                matchingBattleEndEvent,
                triggerRootId);

            return matchingBattleEndEvent;
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

        private static void ValidateDirectChild(
            CombatStartedCombatEvent
                combatStartedEvent,
            BattleEndStartedCombatEvent
                battleEndEvent)
        {
            if (!battleEndEvent.Metadata.HasParent)
            {
                throw new InvalidOperationException(
                    "Battle End Started event must " +
                    "reference its Combat Started event.");
            }

            if (battleEndEvent.Metadata
                    .ParentEventId.Value !=
                combatStartedEvent.Metadata.EventId)
            {
                throw new InvalidOperationException(
                    "Battle End Started event must be a " +
                    "direct child of its Combat Started " +
                    "event.");
            }
        }

        private void
            EnsureNoLaterColumnStartedEvent(
                BattleEndStartedCombatEvent
                    battleEndEvent,
                CombatEventId triggerRootId)
        {
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

                if (columnEvent.Metadata.SequenceNo <=
                    battleEndEvent.Metadata.SequenceNo)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "A normal column cannot start after " +
                    "Battle End has started.");
            }
        }
    }
}