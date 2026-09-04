using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackDamageApplication
    {
        public CombatNormalAttackDamageApplication(
            CombatNormalAttackDamageResolution
                resolution,
            DamageAppliedCombatEvent
                damageToEnemyEvent,
            DamageAppliedCombatEvent
                damageToPlayerEvent,
            DeathCombatEvent playerDeathEvent,
            DeathCombatEvent enemyDeathEvent)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(
                    nameof(resolution));
            }

            if (damageToEnemyEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(damageToEnemyEvent));
            }

            if (damageToPlayerEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(damageToPlayerEvent));
            }

            ValidateDamageToEnemy(
                resolution,
                damageToEnemyEvent);

            ValidateDamageToPlayer(
                resolution,
                damageToPlayerEvent);

            if (playerDeathEvent != null)
            {
                ValidatePlayerDeath(
                    resolution,
                    damageToPlayerEvent,
                    playerDeathEvent);
            }

            if (enemyDeathEvent != null)
            {
                ValidateEnemyDeath(
                    resolution,
                    damageToEnemyEvent,
                    enemyDeathEvent);
            }

            Resolution =
                resolution;

            DamageToEnemyEvent =
                damageToEnemyEvent;

            DamageToPlayerEvent =
                damageToPlayerEvent;

            PlayerDeathEvent =
                playerDeathEvent;

            EnemyDeathEvent =
                enemyDeathEvent;
        }

        public CombatNormalAttackDamageResolution
            Resolution
        {
            get;
        }

        public CombatNormalAttackEventBatch Batch =>
            Resolution.Batch;

        public DamageAppliedCombatEvent
            DamageToEnemyEvent
        {
            get;
        }

        public DamageAppliedCombatEvent
            DamageToPlayerEvent
        {
            get;
        }

        public DeathCombatEvent PlayerDeathEvent
        {
            get;
        }

        public DeathCombatEvent EnemyDeathEvent
        {
            get;
        }

        public bool DidPlayerDie =>
            PlayerDeathEvent != null;

        public bool DidEnemyDie =>
            EnemyDeathEvent != null;

        public bool DidBothDie =>
            DidPlayerDie &&
            DidEnemyDie;

        private static void ValidateDamageToEnemy(
            CombatNormalAttackDamageResolution
                resolution,
            DamageAppliedCombatEvent damageEvent)
        {
            var attackEvent =
                resolution.Batch.PlayerAttackEvent;

            if (!damageEvent.Metadata.HasParent ||
                damageEvent.Metadata
                    .ParentEventId.Value !=
                attackEvent.Metadata.EventId)
            {
                throw new ArgumentException(
                    "Damage to Enemy event must be a " +
                    "child of the Player Normal Attack.",
                    nameof(damageEvent));
            }

            if (damageEvent.TargetInstanceId !=
                attackEvent.TargetInstanceId)
            {
                throw new ArgumentException(
                    "Damage to Enemy target card does " +
                    "not match the Player Normal Attack.",
                    nameof(damageEvent));
            }

            if (damageEvent.TargetPosition !=
                attackEvent.TargetPosition)
            {
                throw new ArgumentException(
                    "Damage to Enemy target position does " +
                    "not match the Player Normal Attack.",
                    nameof(damageEvent));
            }
        }

        private static void ValidateDamageToPlayer(
            CombatNormalAttackDamageResolution
                resolution,
            DamageAppliedCombatEvent damageEvent)
        {
            var attackEvent =
                resolution.Batch.EnemyAttackEvent;

            if (!damageEvent.Metadata.HasParent ||
                damageEvent.Metadata
                    .ParentEventId.Value !=
                attackEvent.Metadata.EventId)
            {
                throw new ArgumentException(
                    "Damage to Player event must be a " +
                    "child of the Enemy Normal Attack.",
                    nameof(damageEvent));
            }

            if (damageEvent.TargetInstanceId !=
                attackEvent.TargetInstanceId)
            {
                throw new ArgumentException(
                    "Damage to Player target card does " +
                    "not match the Enemy Normal Attack.",
                    nameof(damageEvent));
            }

            if (damageEvent.TargetPosition !=
                attackEvent.TargetPosition)
            {
                throw new ArgumentException(
                    "Damage to Player target position does " +
                    "not match the Enemy Normal Attack.",
                    nameof(damageEvent));
            }
        }

        private static void ValidatePlayerDeath(
            CombatNormalAttackDamageResolution
                resolution,
            DamageAppliedCombatEvent damageEvent,
            DeathCombatEvent deathEvent)
        {
            var playerAttackTarget =
                resolution.Batch.EnemyAttackEvent;

            if (!deathEvent.Metadata.HasParent ||
                deathEvent.Metadata
                    .ParentEventId.Value !=
                damageEvent.Metadata.EventId)
            {
                throw new ArgumentException(
                    "Player Death event must be a child " +
                    "of the damage applied to Player.",
                    nameof(deathEvent));
            }

            if (deathEvent.InstanceId !=
                playerAttackTarget.TargetInstanceId ||
                deathEvent.Position !=
                playerAttackTarget.TargetPosition)
            {
                throw new ArgumentException(
                    "Player Death event does not match " +
                    "the damaged Player card.",
                    nameof(deathEvent));
            }
        }

        private static void ValidateEnemyDeath(
            CombatNormalAttackDamageResolution
                resolution,
            DamageAppliedCombatEvent damageEvent,
            DeathCombatEvent deathEvent)
        {
            var enemyAttackTarget =
                resolution.Batch.PlayerAttackEvent;

            if (!deathEvent.Metadata.HasParent ||
                deathEvent.Metadata
                    .ParentEventId.Value !=
                damageEvent.Metadata.EventId)
            {
                throw new ArgumentException(
                    "Enemy Death event must be a child " +
                    "of the damage applied to Enemy.",
                    nameof(deathEvent));
            }

            if (deathEvent.InstanceId !=
                enemyAttackTarget.TargetInstanceId ||
                deathEvent.Position !=
                enemyAttackTarget.TargetPosition)
            {
                throw new ArgumentException(
                    "Enemy Death event does not match " +
                    "the damaged Enemy card.",
                    nameof(deathEvent));
            }
        }
    }
}