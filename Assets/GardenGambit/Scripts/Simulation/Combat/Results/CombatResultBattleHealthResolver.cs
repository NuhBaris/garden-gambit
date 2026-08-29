using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatResultBattleHealthResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatResultBattleHealthResolver(
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
        }

        public IReadOnlyList<
            BattleHealthChangedCombatEvent> Apply(
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

            EnsureDamageNotAlreadyApplied(
                resultEvent);

            var playerSide =
                state.GetSide(
                    CombatSide.Player);

            var enemySide =
                state.GetSide(
                    CombatSide.Enemy);

            var playerDamage =
                resultEvent
                    .ResolvedIncomingDamageToPlayer;

            var enemyDamage =
                resultEvent
                    .ResolvedIncomingDamageToEnemy;

            var previousPlayerBattleHealth =
                playerSide.BattleHealth;

            var previousEnemyBattleHealth =
                enemySide.BattleHealth;

            var currentPlayerBattleHealth =
                previousPlayerBattleHealth
                    .ApplyDamage(
                        playerDamage);

            var currentEnemyBattleHealth =
                previousEnemyBattleHealth
                    .ApplyDamage(
                        enemyDamage);

            var pendingEvents =
                new List<
                    BattleHealthChangedCombatEvent>();

            if (playerDamage > 0)
            {
                var playerMetadata =
                    _metadataFactory.CreateChild(
                        resultEvent.Metadata);

                pendingEvents.Add(
                    new BattleHealthChangedCombatEvent(
                        playerMetadata,
                        CombatSide.Player,
                        previousPlayerBattleHealth,
                        currentPlayerBattleHealth));
            }

            if (enemyDamage > 0)
            {
                var enemyMetadata =
                    _metadataFactory.CreateChild(
                        resultEvent.Metadata);

                pendingEvents.Add(
                    new BattleHealthChangedCombatEvent(
                        enemyMetadata,
                        CombatSide.Enemy,
                        previousEnemyBattleHealth,
                        currentEnemyBattleHealth));
            }

            ValidatePendingEvents(
                pendingEvents);

            if (playerDamage > 0)
            {
                playerSide.ApplyBattleHealthDamage(
                    playerDamage);
            }

            if (enemyDamage > 0)
            {
                enemySide.ApplyBattleHealthDamage(
                    enemyDamage);
            }

            foreach (var pendingEvent in
                     pendingEvents)
            {
                _eventLog.Append(
                    pendingEvent);
            }

            return pendingEvents.AsReadOnly();
        }

        private void ValidateLoggedResultEvent(
            CombatResultCalculatedCombatEvent
                resultEvent)
        {
            if (!_eventLog.ContainsEvent(
                    resultEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Result Calculated event " +
                    "must already exist in the combat " +
                    "event log.",
                    nameof(resultEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    resultEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    resultEvent))
            {
                throw new ArgumentException(
                    "Combat Result Calculated event " +
                    "must be the exact event stored in " +
                    "the combat event log.",
                    nameof(resultEvent));
            }
        }

        private void EnsureDamageNotAlreadyApplied(
            CombatResultCalculatedCombatEvent
                resultEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var changeEvent =
                    _eventLog.Events[index]
                        as
                        BattleHealthChangedCombatEvent;

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

                throw new InvalidOperationException(
                    "Result damage has already been " +
                    "applied for this Combat Result " +
                    "Calculated event.");
            }
        }

        private void ValidatePendingEvents(
            IReadOnlyList<
                BattleHealthChangedCombatEvent>
                pendingEvents)
        {
            var allocatedEventIds =
                new HashSet<CombatEventId>();

            var hasPreviousSequence =
                _eventLog.Count > 0;

            var previousSequence =
                hasPreviousSequence
                    ? _eventLog.Events[
                        _eventLog.Count - 1]
                        .Metadata.SequenceNo
                    : default(
                        CombatSequenceNumber);

            foreach (var pendingEvent in
                     pendingEvents)
            {
                var metadata =
                    pendingEvent.Metadata;

                if (_eventLog.ContainsEvent(
                        metadata.EventId))
                {
                    throw new InvalidOperationException(
                        $"Allocated EventId already exists " +
                        $"in the log: {metadata.EventId}.");
                }

                if (!allocatedEventIds.Add(
                        metadata.EventId))
                {
                    throw new InvalidOperationException(
                        $"Allocated EventId was repeated " +
                        $"inside the pending result " +
                        $"events: {metadata.EventId}.");
                }

                if (hasPreviousSequence &&
                    metadata.SequenceNo <=
                    previousSequence)
                {
                    throw new InvalidOperationException(
                        "Allocated SequenceNo is not " +
                        "strictly greater than the " +
                        "previous event sequence.");
                }

                previousSequence =
                    metadata.SequenceNo;

                hasPreviousSequence =
                    true;
            }
        }
    }
}