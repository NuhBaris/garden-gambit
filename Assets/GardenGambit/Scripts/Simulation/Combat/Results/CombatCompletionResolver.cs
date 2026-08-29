using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatCompletionResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatOutcomeResolver
            _outcomeResolver;

        public CombatCompletionResolver(
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

            _outcomeResolver =
                new CombatOutcomeResolver();
        }

        public CombatCompletedCombatEvent Resolve(
            CombatState state,
            CombatResultCalculatedCombatEvent
                resultEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (resultEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(resultEvent));
            }

            ValidateLoggedResultEvent(
                resultEvent);

            EnsureCompletionNotAlreadyLogged(
                resultEvent);

            EnsureBattleHealthChangeCompleted(
                state,
                resultEvent,
                CombatSide.Player,
                resultEvent
                    .ResolvedIncomingDamageToPlayer);

            EnsureBattleHealthChangeCompleted(
                state,
                resultEvent,
                CombatSide.Enemy,
                resultEvent
                    .ResolvedIncomingDamageToEnemy);

            var calculation =
                _outcomeResolver.Resolve(
                    state);

            var metadata =
                _metadataFactory.CreateChild(
                    resultEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            var completedEvent =
                new CombatCompletedCombatEvent(
                    metadata,
                    calculation);

            _eventLog.Append(
                completedEvent);

            return completedEvent;
        }

        private void ValidateLoggedResultEvent(
            CombatResultCalculatedCombatEvent
                resultEvent)
        {
            if (!_eventLog.ContainsEvent(
                    resultEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Result event must already exist " +
                    "in the combat event log.",
                    nameof(resultEvent));
            }

            var loggedResultEvent =
                _eventLog.GetEvent(
                    resultEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedResultEvent,
                    resultEvent))
            {
                throw new ArgumentException(
                    "Result event must be the exact event " +
                    "stored in the combat event log.",
                    nameof(resultEvent));
            }
        }

        private void EnsureCompletionNotAlreadyLogged(
            CombatResultCalculatedCombatEvent
                resultEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var completedEvent =
                    _eventLog.Events[index]
                        as CombatCompletedCombatEvent;

                if (completedEvent == null)
                {
                    continue;
                }

                if (!completedEvent.Metadata.HasParent)
                {
                    continue;
                }

                if (completedEvent.Metadata
                        .ParentEventId.Value ==
                    resultEvent.Metadata.EventId)
                {
                    throw new InvalidOperationException(
                        "A Combat Completed event has " +
                        "already been logged for this " +
                        "result event.");
                }
            }
        }

        private void EnsureBattleHealthChangeCompleted(
            CombatState state,
            CombatResultCalculatedCombatEvent
                resultEvent,
            CombatSide side,
            int resolvedIncomingDamage)
        {
            BattleHealthChangedCombatEvent
                matchingChangeEvent = null;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var changeEvent =
                    _eventLog.Events[index]
                        as BattleHealthChangedCombatEvent;

                if (changeEvent == null)
                {
                    continue;
                }

                if (!changeEvent.Metadata.HasParent)
                {
                    continue;
                }

                if (changeEvent.Metadata
                        .ParentEventId.Value !=
                    resultEvent.Metadata.EventId)
                {
                    continue;
                }

                if (changeEvent.Side != side)
                {
                    continue;
                }

                if (matchingChangeEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Battle Health change " +
                        "events were logged for the same " +
                        "side and result event.");
                }

                matchingChangeEvent =
                    changeEvent;
            }

            if (resolvedIncomingDamage == 0)
            {
                if (matchingChangeEvent != null)
                {
                    throw new InvalidOperationException(
                        "A zero-damage result cannot have " +
                        "a Battle Health change event.");
                }

                return;
            }

            if (matchingChangeEvent == null)
            {
                throw new InvalidOperationException(
                    "Result damage must be applied before " +
                    "combat can be completed.");
            }

            if (!matchingChangeEvent.IsDamage ||
                matchingChangeEvent.ChangedAmount !=
                resolvedIncomingDamage)
            {
                throw new InvalidOperationException(
                    "Battle Health change does not match " +
                    "the resolved incoming result damage.");
            }

            var currentBattleHealth =
                state.GetSide(side)
                    .BattleHealth;

            if (matchingChangeEvent
                    .CurrentBattleHealth.Value !=
                currentBattleHealth.Value)
            {
                throw new InvalidOperationException(
                    "Battle Health change event does not " +
                    "match the current combat state.");
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