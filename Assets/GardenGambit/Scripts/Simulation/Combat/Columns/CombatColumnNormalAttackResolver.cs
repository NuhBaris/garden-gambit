using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatColumnNormalAttackResolver
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly CombatColumnFrontlineResolver
            _frontlineResolver;

        private readonly NormalAttackExchangeResolver
            _exchangeResolver;

        public CombatColumnNormalAttackResolver(
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

            _eventLog = eventLog;

            _frontlineResolver =
                new CombatColumnFrontlineResolver();

            _exchangeResolver =
                new NormalAttackExchangeResolver(
                    metadataFactory,
                    eventLog);
        }

        public NormalAttackExchangeCombatEvent
            TryResolveExchange(
                CombatState state,
                ColumnStartedCombatEvent
                    columnStartedEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (columnStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(columnStartedEvent));
            }

            ValidateLoggedColumnStartedEvent(
                columnStartedEvent);

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var hasExchange =
                _frontlineResolver
                    .TryGetExchangePositions(
                        state,
                        columnStartedEvent.Column,
                        out playerPosition,
                        out enemyPosition);

            if (!hasExchange)
            {
                return null;
            }

            return _exchangeResolver.ResolveInColumn(
                state,
                columnStartedEvent,
                playerPosition,
                enemyPosition);
        }

        private void ValidateLoggedColumnStartedEvent(
            ColumnStartedCombatEvent
                columnStartedEvent)
        {
            if (!_eventLog.ContainsEvent(
                    columnStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Column Started event must already " +
                    "exist in the combat event log.",
                    nameof(columnStartedEvent));
            }

            var loggedColumnEvent =
                _eventLog.GetEvent(
                    columnStartedEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedColumnEvent,
                    columnStartedEvent))
            {
                throw new ArgumentException(
                    "Column Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(columnStartedEvent));
            }

            if (!columnStartedEvent.Metadata.HasParent)
            {
                throw new ArgumentException(
                    "Column Started event must reference " +
                    "a Combat Started parent.",
                    nameof(columnStartedEvent));
            }

            var parentEvent =
                _eventLog.GetEvent(
                    columnStartedEvent.Metadata
                        .ParentEventId.Value);

            if (!(parentEvent is
                    CombatStartedCombatEvent))
            {
                throw new ArgumentException(
                    "Column Started parent must be a " +
                    "Combat Started event.",
                    nameof(columnStartedEvent));
            }
        }
    }
}