using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct
        CombatResultDamageResolution
    {
        public CombatResultDamageResolution(
            CombatResultDamageCalculation calculation,
            int resolvedIncomingDamageToPlayer,
            int resolvedIncomingDamageToEnemy)
        {
            if (!calculation.IsValid)
            {
                throw new ArgumentException(
                    "Result damage resolution requires " +
                    "a valid damage calculation.",
                    nameof(calculation));
            }

            if (resolvedIncomingDamageToPlayer < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(
                        resolvedIncomingDamageToPlayer),
                    resolvedIncomingDamageToPlayer,
                    "Resolved incoming damage to Player " +
                    "cannot be negative.");
            }

            if (resolvedIncomingDamageToEnemy < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(
                        resolvedIncomingDamageToEnemy),
                    resolvedIncomingDamageToEnemy,
                    "Resolved incoming damage to Enemy " +
                    "cannot be negative.");
            }

            Calculation =
                calculation;

            ResolvedIncomingDamageToPlayer =
                resolvedIncomingDamageToPlayer;

            ResolvedIncomingDamageToEnemy =
                resolvedIncomingDamageToEnemy;
        }

        public CombatResultDamageCalculation
            Calculation
        {
            get;
        }

        public int BaseIncomingDamageToPlayer =>
            Calculation.BaseIncomingDamageToPlayer;

        public int BaseIncomingDamageToEnemy =>
            Calculation.BaseIncomingDamageToEnemy;

        public int ResolvedIncomingDamageToPlayer
        {
            get;
        }

        public int ResolvedIncomingDamageToEnemy
        {
            get;
        }

        public long PlayerDamageDelta =>
            (long)ResolvedIncomingDamageToPlayer -
            BaseIncomingDamageToPlayer;

        public long EnemyDamageDelta =>
            (long)ResolvedIncomingDamageToEnemy -
            BaseIncomingDamageToEnemy;

        public long PreventedDamageForPlayer =>
            Math.Max(
                0L,
                -PlayerDamageDelta);

        public long PreventedDamageForEnemy =>
            Math.Max(
                0L,
                -EnemyDamageDelta);

        public long AddedDamageToPlayer =>
            Math.Max(
                0L,
                PlayerDamageDelta);

        public long AddedDamageToEnemy =>
            Math.Max(
                0L,
                EnemyDamageDelta);

        public bool IsPlayerDamageReduced =>
            PlayerDamageDelta < 0;

        public bool IsEnemyDamageReduced =>
            EnemyDamageDelta < 0;

        public bool IsPlayerDamageIncreased =>
            PlayerDamageDelta > 0;

        public bool IsEnemyDamageIncreased =>
            EnemyDamageDelta > 0;

        public bool HasAnyDamageModification =>
            PlayerDamageDelta != 0 ||
            EnemyDamageDelta != 0;

        public bool IsValid =>
            Calculation.IsValid &&
            ResolvedIncomingDamageToPlayer >= 0 &&
            ResolvedIncomingDamageToEnemy >= 0;
    }
}