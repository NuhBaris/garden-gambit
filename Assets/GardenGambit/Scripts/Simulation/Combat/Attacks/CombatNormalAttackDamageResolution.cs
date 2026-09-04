using System;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackDamageResolution
    {
        public CombatNormalAttackDamageResolution(
            CombatNormalAttackEventBatch batch,
            int resolvedDamageToEnemy,
            int resolvedDamageToPlayer)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(
                    nameof(batch));
            }

            if (resolvedDamageToEnemy < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolvedDamageToEnemy),
                    resolvedDamageToEnemy,
                    "Resolved normal attack damage to " +
                    "Enemy cannot be negative.");
            }

            if (resolvedDamageToPlayer < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolvedDamageToPlayer),
                    resolvedDamageToPlayer,
                    "Resolved normal attack damage to " +
                    "Player cannot be negative.");
            }

            Batch =
                batch;

            ResolvedDamageToEnemy =
                resolvedDamageToEnemy;

            ResolvedDamageToPlayer =
                resolvedDamageToPlayer;
        }

        public CombatNormalAttackEventBatch Batch
        {
            get;
        }

        public int BaseDamageToEnemy =>
            Batch.PlayerAttackEvent.BaseDamage;

        public int BaseDamageToPlayer =>
            Batch.EnemyAttackEvent.BaseDamage;

        public int ResolvedDamageToEnemy
        {
            get;
        }

        public int ResolvedDamageToPlayer
        {
            get;
        }

        public long PlayerAttackDamageDelta =>
            (long)ResolvedDamageToEnemy -
            BaseDamageToEnemy;

        public long EnemyAttackDamageDelta =>
            (long)ResolvedDamageToPlayer -
            BaseDamageToPlayer;

        public bool HasDamageToEnemy =>
            ResolvedDamageToEnemy > 0;

        public bool HasDamageToPlayer =>
            ResolvedDamageToPlayer > 0;

        public bool HasMutualDamage =>
            HasDamageToEnemy &&
            HasDamageToPlayer;
    }
}