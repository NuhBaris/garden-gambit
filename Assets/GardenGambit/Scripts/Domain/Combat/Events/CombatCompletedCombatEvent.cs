using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        CombatCompletedCombatEvent :
        CombatEvent
    {
        public CombatCompletedCombatEvent(
            CombatEventMetadata metadata,
            CombatOutcomeCalculation calculation)
            : base(
                metadata,
                CombatEventKind.CombatCompleted)
        {
            if (!calculation.IsValid)
            {
                throw new ArgumentException(
                    "Combat Completed event requires " +
                    "a valid outcome calculation.",
                    nameof(calculation));
            }

            Calculation =
                calculation;
        }

        public CombatOutcomeCalculation Calculation
        {
            get;
        }

        public BattleHealth PlayerBattleHealth =>
            Calculation.PlayerBattleHealth;

        public BattleHealth EnemyBattleHealth =>
            Calculation.EnemyBattleHealth;

        public CombatOutcome Outcome =>
            Calculation.Outcome;

        public long BattleHealthDifference =>
            Calculation.BattleHealthDifference;

        public long WinningMargin =>
            Calculation.WinningMargin;

        public bool IsPlayerVictory =>
            Calculation.IsPlayerVictory;

        public bool IsEnemyVictory =>
            Calculation.IsEnemyVictory;

        public bool IsDraw =>
            Calculation.IsDraw;
    }
}