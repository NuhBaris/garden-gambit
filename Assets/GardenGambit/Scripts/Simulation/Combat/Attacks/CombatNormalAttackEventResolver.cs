using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackEventResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatNormalAttackEventResolver(
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

        public CombatNormalAttackEventBatch
            AppendExchangeAttacks(
                NormalAttackExchangeCombatEvent
                    exchangeEvent)
        {
            return AppendExchangeAttacks(
                exchangeEvent,
                CombatCardSeason.Unspecified,
                CombatCardSeason.Unspecified);
        }

        public CombatNormalAttackEventBatch
            AppendExchangeAttacks(
                NormalAttackExchangeCombatEvent
                    exchangeEvent,
                CombatCardSeason playerAttackerSeason,
                CombatCardSeason enemyAttackerSeason)
        {
            if (exchangeEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(exchangeEvent));
            }

            ValidateSeason(
                playerAttackerSeason,
                nameof(playerAttackerSeason));

            ValidateSeason(
                enemyAttackerSeason,
                nameof(enemyAttackerSeason));

            ValidateLoggedExchangeEvent(
                exchangeEvent);

            EnsureAttackNotAlreadyLogged(
                exchangeEvent,
                CombatSide.Player);

            EnsureAttackNotAlreadyLogged(
                exchangeEvent,
                CombatSide.Enemy);

            var playerAttackMetadata =
                _metadataFactory.CreateChild(
                    exchangeEvent.Metadata);

            var enemyAttackMetadata =
                _metadataFactory.CreateChild(
                    exchangeEvent.Metadata);

            var playerAttackEvent =
                CreateAttackEvent(
                    exchangeEvent,
                    CombatSide.Player,
                    playerAttackerSeason,
                    enemyAttackerSeason,
                    playerAttackMetadata);

            var enemyAttackEvent =
                CreateAttackEvent(
                    exchangeEvent,
                    CombatSide.Enemy,
                    enemyAttackerSeason,
                    playerAttackerSeason,
                    enemyAttackMetadata);

            var batch =
                new CombatNormalAttackEventBatch(
                    exchangeEvent,
                    playerAttackEvent,
                    enemyAttackEvent);

            _eventLog.EnsureCanAppend(
                playerAttackEvent);

            _eventLog.EnsureCanAppend(
                enemyAttackEvent);

            _eventLog.Append(
                playerAttackEvent);

            _eventLog.Append(
                enemyAttackEvent);

            return batch;
        }

        public NormalAttackCombatEvent AppendAttack(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            CombatSide attackerSide)
        {
            return AppendAttack(
                exchangeEvent,
                attackerSide,
                CombatCardSeason.Unspecified,
                CombatCardSeason.Unspecified);
        }

        public NormalAttackCombatEvent AppendAttack(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            CombatSide attackerSide,
            CombatCardSeason attackerSeason)
        {
            return AppendAttack(
                exchangeEvent,
                attackerSide,
                attackerSeason,
                CombatCardSeason.Unspecified);
        }

        public NormalAttackCombatEvent AppendAttack(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            CombatSide attackerSide,
            CombatCardSeason attackerSeason,
            CombatCardSeason targetSeason)
        {
            if (exchangeEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(exchangeEvent));
            }

            if (attackerSide != CombatSide.Player &&
                attackerSide != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackerSide),
                    attackerSide,
                    "Normal attack event resolver " +
                    "requires Player or Enemy side.");
            }

            ValidateSeason(
                attackerSeason,
                nameof(attackerSeason));

            ValidateSeason(
                targetSeason,
                nameof(targetSeason));

            ValidateLoggedExchangeEvent(
                exchangeEvent);

            EnsureAttackNotAlreadyLogged(
                exchangeEvent,
                attackerSide);

            var metadata =
                _metadataFactory.CreateChild(
                    exchangeEvent.Metadata);

            var attackEvent =
                CreateAttackEvent(
                    exchangeEvent,
                    attackerSide,
                    attackerSeason,
                    targetSeason,
                    metadata);

            _eventLog.EnsureCanAppend(
                attackEvent);

            _eventLog.Append(
                attackEvent);

            return attackEvent;
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                NormalAttackExchangeCombatEvent
                    exchangeEvent,
                CombatSide attackerSide,
                CombatCardSeason attackerSeason,
                CombatCardSeason targetSeason,
                CombatEventMetadata metadata)
        {
            if (attackerSide == CombatSide.Player)
            {
                return new NormalAttackCombatEvent(
                    metadata,
                    exchangeEvent
                        .PlayerInstanceId,
                    exchangeEvent
                        .PlayerPosition,
                    attackerSeason,
                    exchangeEvent
                        .EnemyInstanceId,
                    exchangeEvent
                        .EnemyPosition,
                    targetSeason,
                    exchangeEvent
                        .PlayerAttack);
            }

            return new NormalAttackCombatEvent(
                metadata,
                exchangeEvent
                    .EnemyInstanceId,
                exchangeEvent
                    .EnemyPosition,
                attackerSeason,
                exchangeEvent
                    .PlayerInstanceId,
                exchangeEvent
                    .PlayerPosition,
                targetSeason,
                exchangeEvent
                    .EnemyAttack);
        }

        private void ValidateLoggedExchangeEvent(
            NormalAttackExchangeCombatEvent
                exchangeEvent)
        {
            if (!_eventLog.ContainsEvent(
                    exchangeEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Normal Attack Exchange event must " +
                    "already exist in the combat event log.",
                    nameof(exchangeEvent));
            }

            var loggedExchangeEvent =
                _eventLog.GetEvent(
                    exchangeEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedExchangeEvent,
                    exchangeEvent))
            {
                throw new ArgumentException(
                    "Normal Attack Exchange event must " +
                    "be the exact event stored in the " +
                    "combat event log.",
                    nameof(exchangeEvent));
            }
        }

        private void EnsureAttackNotAlreadyLogged(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            CombatSide attackerSide)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var existingAttackEvent =
                    _eventLog.Events[index]
                        as NormalAttackCombatEvent;

                if (existingAttackEvent == null)
                {
                    continue;
                }

                if (!existingAttackEvent
                        .Metadata.HasParent)
                {
                    continue;
                }

                if (existingAttackEvent.Metadata
                        .ParentEventId.Value !=
                    exchangeEvent.Metadata.EventId)
                {
                    continue;
                }

                if (existingAttackEvent.AttackerSide !=
                    attackerSide)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"A {attackerSide} Normal Attack event " +
                    $"has already been logged for this " +
                    $"exchange.");
            }
        }

        private static void ValidateSeason(
            CombatCardSeason season,
            string parameterName)
        {
            if (season <
                    CombatCardSeason.Unspecified ||
                season >
                    CombatCardSeason.Winter)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    season,
                    "A valid Combat Card Season is " +
                    "required.");
            }
        }
    }
}