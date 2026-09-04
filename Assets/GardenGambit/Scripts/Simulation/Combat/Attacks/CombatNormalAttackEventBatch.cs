using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackEventBatch
    {
        public CombatNormalAttackEventBatch(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            NormalAttackCombatEvent
                playerAttackEvent,
            NormalAttackCombatEvent
                enemyAttackEvent)
        {
            if (exchangeEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(exchangeEvent));
            }

            if (playerAttackEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(playerAttackEvent));
            }

            if (enemyAttackEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(enemyAttackEvent));
            }

            ValidatePlayerAttack(
                exchangeEvent,
                playerAttackEvent);

            ValidateEnemyAttack(
                exchangeEvent,
                enemyAttackEvent);

            ExchangeEvent =
                exchangeEvent;

            PlayerAttackEvent =
                playerAttackEvent;

            EnemyAttackEvent =
                enemyAttackEvent;
        }

        public NormalAttackExchangeCombatEvent
            ExchangeEvent
        {
            get;
        }

        public NormalAttackCombatEvent
            PlayerAttackEvent
        {
            get;
        }

        public NormalAttackCombatEvent
            EnemyAttackEvent
        {
            get;
        }

        private static void ValidatePlayerAttack(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            NormalAttackCombatEvent
                playerAttackEvent)
        {
            ValidateParent(
                exchangeEvent,
                playerAttackEvent,
                nameof(playerAttackEvent));

            if (!playerAttackEvent.IsPlayerAttack)
            {
                throw new ArgumentException(
                    "Player attack event must use the " +
                    "Player side as its attacker.",
                    nameof(playerAttackEvent));
            }

            if (playerAttackEvent.AttackerInstanceId !=
                exchangeEvent.PlayerInstanceId)
            {
                throw new ArgumentException(
                    "Player attack event attacker does " +
                    "not match the exchange Player card.",
                    nameof(playerAttackEvent));
            }

            if (playerAttackEvent.AttackerPosition !=
                exchangeEvent.PlayerPosition)
            {
                throw new ArgumentException(
                    "Player attack event position does " +
                    "not match the exchange Player " +
                    "position.",
                    nameof(playerAttackEvent));
            }

            if (playerAttackEvent.TargetInstanceId !=
                exchangeEvent.EnemyInstanceId)
            {
                throw new ArgumentException(
                    "Player attack event target does not " +
                    "match the exchange Enemy card.",
                    nameof(playerAttackEvent));
            }

            if (playerAttackEvent.TargetPosition !=
                exchangeEvent.EnemyPosition)
            {
                throw new ArgumentException(
                    "Player attack event target position " +
                    "does not match the exchange Enemy " +
                    "position.",
                    nameof(playerAttackEvent));
            }

            if (playerAttackEvent.BaseDamage !=
                exchangeEvent.PlayerAttack)
            {
                throw new ArgumentException(
                    "Player attack event base damage does " +
                    "not match the exchange Player Attack.",
                    nameof(playerAttackEvent));
            }
        }

        private static void ValidateEnemyAttack(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            NormalAttackCombatEvent
                enemyAttackEvent)
        {
            ValidateParent(
                exchangeEvent,
                enemyAttackEvent,
                nameof(enemyAttackEvent));

            if (!enemyAttackEvent.IsEnemyAttack)
            {
                throw new ArgumentException(
                    "Enemy attack event must use the " +
                    "Enemy side as its attacker.",
                    nameof(enemyAttackEvent));
            }

            if (enemyAttackEvent.AttackerInstanceId !=
                exchangeEvent.EnemyInstanceId)
            {
                throw new ArgumentException(
                    "Enemy attack event attacker does " +
                    "not match the exchange Enemy card.",
                    nameof(enemyAttackEvent));
            }

            if (enemyAttackEvent.AttackerPosition !=
                exchangeEvent.EnemyPosition)
            {
                throw new ArgumentException(
                    "Enemy attack event position does " +
                    "not match the exchange Enemy " +
                    "position.",
                    nameof(enemyAttackEvent));
            }

            if (enemyAttackEvent.TargetInstanceId !=
                exchangeEvent.PlayerInstanceId)
            {
                throw new ArgumentException(
                    "Enemy attack event target does not " +
                    "match the exchange Player card.",
                    nameof(enemyAttackEvent));
            }

            if (enemyAttackEvent.TargetPosition !=
                exchangeEvent.PlayerPosition)
            {
                throw new ArgumentException(
                    "Enemy attack event target position " +
                    "does not match the exchange Player " +
                    "position.",
                    nameof(enemyAttackEvent));
            }

            if (enemyAttackEvent.BaseDamage !=
                exchangeEvent.EnemyAttack)
            {
                throw new ArgumentException(
                    "Enemy attack event base damage does " +
                    "not match the exchange Enemy Attack.",
                    nameof(enemyAttackEvent));
            }
        }

        private static void ValidateParent(
            NormalAttackExchangeCombatEvent
                exchangeEvent,
            NormalAttackCombatEvent attackEvent,
            string parameterName)
        {
            if (!attackEvent.Metadata.HasParent)
            {
                throw new ArgumentException(
                    "Normal attack event must reference " +
                    "its exchange parent.",
                    parameterName);
            }

            if (attackEvent.Metadata
                    .ParentEventId.Value !=
                exchangeEvent.Metadata.EventId)
            {
                throw new ArgumentException(
                    "Normal attack event must be a child " +
                    "of the supplied exchange event.",
                    parameterName);
            }

            if (attackEvent.Metadata.TriggerRootId !=
                exchangeEvent.Metadata.TriggerRootId)
            {
                throw new ArgumentException(
                    "Normal attack event and exchange " +
                    "must share the same TriggerRootId.",
                    parameterName);
            }
        }
    }
}