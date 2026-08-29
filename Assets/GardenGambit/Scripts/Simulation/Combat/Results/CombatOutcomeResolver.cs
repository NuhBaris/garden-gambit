using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatOutcomeResolver
    {
        public CombatOutcomeCalculation Resolve(
            CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            var playerBattleHealth =
                state.GetSide(
                    CombatSide.Player)
                    .BattleHealth;

            var enemyBattleHealth =
                state.GetSide(
                    CombatSide.Enemy)
                    .BattleHealth;

            return new CombatOutcomeCalculation(
                playerBattleHealth,
                enemyBattleHealth);
        }
    }
}