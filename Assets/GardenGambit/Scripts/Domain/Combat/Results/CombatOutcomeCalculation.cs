using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct
        CombatOutcomeCalculation
    {
        private readonly bool
            _isInitialized;

        public CombatOutcomeCalculation(
            BattleHealth playerBattleHealth,
            BattleHealth enemyBattleHealth)
        {
            PlayerBattleHealth =
                playerBattleHealth;

            EnemyBattleHealth =
                enemyBattleHealth;

            Outcome =
                DetermineOutcome(
                    playerBattleHealth,
                    enemyBattleHealth);

            _isInitialized = true;
        }

        public BattleHealth PlayerBattleHealth
        {
            get;
        }

        public BattleHealth EnemyBattleHealth
        {
            get;
        }

        public CombatOutcome Outcome { get; }

        public long BattleHealthDifference =>
            (long)PlayerBattleHealth.Value -
            EnemyBattleHealth.Value;

        public long WinningMargin =>
            Math.Abs(
                BattleHealthDifference);

        public bool IsPlayerVictory =>
            Outcome ==
            CombatOutcome.PlayerVictory;

        public bool IsEnemyVictory =>
            Outcome ==
            CombatOutcome.EnemyVictory;

        public bool IsDraw =>
            Outcome ==
            CombatOutcome.Draw;

        public bool IsValid
        {
            get
            {
                if (!_isInitialized)
                {
                    return false;
                }

                return Outcome ==
                       DetermineOutcome(
                           PlayerBattleHealth,
                           EnemyBattleHealth);
            }
        }

        private static CombatOutcome
            DetermineOutcome(
                BattleHealth playerBattleHealth,
                BattleHealth enemyBattleHealth)
        {
            if (playerBattleHealth.Value >
                enemyBattleHealth.Value)
            {
                return CombatOutcome.PlayerVictory;
            }

            if (enemyBattleHealth.Value >
                playerBattleHealth.Value)
            {
                return CombatOutcome.EnemyVictory;
            }

            return CombatOutcome.Draw;
        }
    }
}