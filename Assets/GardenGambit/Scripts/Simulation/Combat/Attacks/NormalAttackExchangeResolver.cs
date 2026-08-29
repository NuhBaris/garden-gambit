using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class NormalAttackExchangeResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatDamageResolver
            _damageResolver;

        private readonly CombatDeathEventResolver
            _deathEventResolver;

        public NormalAttackExchangeResolver(
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

            _damageResolver =
                new CombatDamageResolver(
                    metadataFactory,
                    eventLog);

            _deathEventResolver =
                new CombatDeathEventResolver(
                    metadataFactory,
                    eventLog);
        }

        public NormalAttackExchangeCombatEvent Resolve(
            CombatState state,
            BoardPosition playerPosition,
            BoardPosition enemyPosition)
        {
            ValidateRequest(
                state,
                playerPosition,
                enemyPosition);

            var exchangeMetadata =
                _metadataFactory.CreateRoot();

            return ResolveWithMetadata(
                state,
                playerPosition,
                enemyPosition,
                exchangeMetadata);
        }

        public NormalAttackExchangeCombatEvent
            ResolveInColumn(
                CombatState state,
                ColumnStartedCombatEvent
                    columnStartedEvent,
                BoardPosition playerPosition,
                BoardPosition enemyPosition)
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

            ValidatePositions(
                playerPosition,
                enemyPosition);

            ValidateLoggedColumnStartedEvent(
                columnStartedEvent);

            EnsurePositionsMatchColumn(
                columnStartedEvent,
                playerPosition,
                enemyPosition);

            var exchangeMetadata =
                _metadataFactory.CreateChild(
                    columnStartedEvent.Metadata);

            return ResolveWithMetadata(
                state,
                playerPosition,
                enemyPosition,
                exchangeMetadata);
        }

        private NormalAttackExchangeCombatEvent
            ResolveWithMetadata(
                CombatState state,
                BoardPosition playerPosition,
                BoardPosition enemyPosition,
                CombatEventMetadata exchangeMetadata)
        {
            var playerCard =
                state.GetSide(CombatSide.Player)
                    .GetCardAt(playerPosition);

            var enemyCard =
                state.GetSide(CombatSide.Enemy)
                    .GetCardAt(enemyPosition);

            var playerAttack =
                playerCard.Attack;

            var enemyAttack =
                enemyCard.Attack;

            enemyCard.PreviewIncomingDamage(
                playerAttack);

            playerCard.PreviewIncomingDamage(
                enemyAttack);

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    exchangeMetadata,
                    playerCard.InstanceId,
                    playerPosition,
                    playerAttack,
                    enemyCard.InstanceId,
                    enemyPosition,
                    enemyAttack);

            var damageToEnemyMetadata =
                _metadataFactory.CreateChild(
                    exchangeMetadata);

            var damageToPlayerMetadata =
                _metadataFactory.CreateChild(
                    exchangeMetadata);

            _eventLog.EnsureCanAppend(
                exchangeEvent);

            _eventLog.Append(
                exchangeEvent);

            var damageToEnemyEvent =
                _damageResolver.ApplyPreparedCardDamage(
                    state,
                    exchangeEvent,
                    playerPosition,
                    enemyPosition,
                    playerAttack,
                    damageToEnemyMetadata);

            var damageToPlayerEvent =
                _damageResolver.ApplyPreparedCardDamage(
                    state,
                    exchangeEvent,
                    enemyPosition,
                    playerPosition,
                    enemyAttack,
                    damageToPlayerMetadata);

            _deathEventResolver.AppendFromDamage(
                damageToPlayerEvent);

            _deathEventResolver.AppendFromDamage(
                damageToEnemyEvent);

            return exchangeEvent;
        }

        private static void ValidateRequest(
            CombatState state,
            BoardPosition playerPosition,
            BoardPosition enemyPosition)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            ValidatePositions(
                playerPosition,
                enemyPosition);
        }

        private static void ValidatePositions(
            BoardPosition playerPosition,
            BoardPosition enemyPosition)
        {
            if (!playerPosition.IsValid ||
                playerPosition.Side !=
                    CombatSide.Player)
            {
                throw new ArgumentException(
                    "A valid Player-side position " +
                    "is required.",
                    nameof(playerPosition));
            }

            if (!enemyPosition.IsValid ||
                enemyPosition.Side !=
                    CombatSide.Enemy)
            {
                throw new ArgumentException(
                    "A valid Enemy-side position " +
                    "is required.",
                    nameof(enemyPosition));
            }
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
                    "exact event stored in the log.",
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

        private static void EnsurePositionsMatchColumn(
            ColumnStartedCombatEvent
                columnStartedEvent,
            BoardPosition playerPosition,
            BoardPosition enemyPosition)
        {
            if (playerPosition.Column !=
                columnStartedEvent.Column)
            {
                throw new ArgumentException(
                    "Player position must belong to " +
                    "the started column.",
                    nameof(playerPosition));
            }

            if (enemyPosition.Column !=
                columnStartedEvent.Column)
            {
                throw new ArgumentException(
                    "Enemy position must belong to " +
                    "the started column.",
                    nameof(enemyPosition));
            }
        }
    }
}