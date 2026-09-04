using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackDamageApplicationResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatDamageResolver
            _damageResolver;

        private readonly CombatDeathEventResolver
            _deathEventResolver;

        public CombatNormalAttackDamageApplicationResolver(
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

            _damageResolver =
                new CombatDamageResolver(
                    metadataFactory,
                    eventLog);

            _deathEventResolver =
                new CombatDeathEventResolver(
                    metadataFactory,
                    eventLog);
        }

        public CombatNormalAttackDamageApplication
            Apply(
                CombatState state,
                CombatNormalAttackDamageResolution
                    resolution)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (resolution == null)
            {
                throw new ArgumentNullException(
                    nameof(resolution));
            }

            ValidateLoggedBatch(
                resolution.Batch);

            EnsureDamageNotAlreadyApplied(
                resolution.Batch);

            var playerAttackEvent =
                resolution.Batch.PlayerAttackEvent;

            var enemyAttackEvent =
                resolution.Batch.EnemyAttackEvent;

            var playerCard =
                state.GetSide(
                        CombatSide.Player)
                    .GetCardAt(
                        enemyAttackEvent
                            .TargetPosition);

            var enemyCard =
                state.GetSide(
                        CombatSide.Enemy)
                    .GetCardAt(
                        playerAttackEvent
                            .TargetPosition);

            if (playerCard.InstanceId !=
                enemyAttackEvent.TargetInstanceId)
            {
                throw new InvalidOperationException(
                    "Player card does not match the " +
                    "prepared Enemy Normal Attack target.");
            }

            if (enemyCard.InstanceId !=
                playerAttackEvent.TargetInstanceId)
            {
                throw new InvalidOperationException(
                    "Enemy card does not match the " +
                    "prepared Player Normal Attack target.");
            }

            enemyCard.PreviewIncomingDamage(
                resolution.ResolvedDamageToEnemy);

            playerCard.PreviewIncomingDamage(
                resolution.ResolvedDamageToPlayer);

            var damageToEnemyMetadata =
                _metadataFactory.CreateChild(
                    playerAttackEvent.Metadata);

            var damageToPlayerMetadata =
                _metadataFactory.CreateChild(
                    enemyAttackEvent.Metadata);

            var damageToEnemyEvent =
                _damageResolver
                    .ApplyPreparedCardDamage(
                        state,
                        playerAttackEvent,
                        playerAttackEvent
                            .AttackerPosition,
                        playerAttackEvent
                            .TargetPosition,
                        resolution
                            .ResolvedDamageToEnemy,
                        damageToEnemyMetadata);

            var damageToPlayerEvent =
                _damageResolver
                    .ApplyPreparedCardDamage(
                        state,
                        enemyAttackEvent,
                        enemyAttackEvent
                            .AttackerPosition,
                        enemyAttackEvent
                            .TargetPosition,
                        resolution
                            .ResolvedDamageToPlayer,
                        damageToPlayerMetadata);

            var playerDeathEvent =
                _deathEventResolver
                    .AppendFromDamage(
                        damageToPlayerEvent);

            var enemyDeathEvent =
                _deathEventResolver
                    .AppendFromDamage(
                        damageToEnemyEvent);

            return new
                CombatNormalAttackDamageApplication(
                    resolution,
                    damageToEnemyEvent,
                    damageToPlayerEvent,
                    playerDeathEvent,
                    enemyDeathEvent);
        }

        private void ValidateLoggedBatch(
            CombatNormalAttackEventBatch batch)
        {
            ValidateExactLoggedEvent(
                batch.ExchangeEvent,
                "Normal Attack Exchange");

            ValidateExactLoggedEvent(
                batch.PlayerAttackEvent,
                "Player Normal Attack");

            ValidateExactLoggedEvent(
                batch.EnemyAttackEvent,
                "Enemy Normal Attack");
        }

        private void ValidateExactLoggedEvent(
            CombatEvent combatEvent,
            string eventName)
        {
            if (!_eventLog.ContainsEvent(
                    combatEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    $"{eventName} event must already " +
                    $"exist in the combat event log.",
                    nameof(combatEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatEvent))
            {
                throw new ArgumentException(
                    $"{eventName} event must be the " +
                    $"exact event stored in the combat " +
                    $"event log.",
                    nameof(combatEvent));
            }
        }

        private void EnsureDamageNotAlreadyApplied(
            CombatNormalAttackEventBatch batch)
        {
            var playerAttackEventId =
                batch.PlayerAttackEvent
                    .Metadata.EventId;

            var enemyAttackEventId =
                batch.EnemyAttackEvent
                    .Metadata.EventId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var damageEvent =
                    _eventLog.Events[index]
                        as DamageAppliedCombatEvent;

                if (damageEvent == null ||
                    !damageEvent.Metadata.HasParent)
                {
                    continue;
                }

                var parentEventId =
                    damageEvent.Metadata
                        .ParentEventId.Value;

                if (parentEventId ==
                        playerAttackEventId ||
                    parentEventId ==
                        enemyAttackEventId)
                {
                    throw new InvalidOperationException(
                        "Normal attack batch damage has " +
                        "already been applied.");
                }
            }
        }
    }
}