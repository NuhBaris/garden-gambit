using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        BattleHealthChangedCombatEvent :
        CombatEvent
    {
        public BattleHealthChangedCombatEvent(
            CombatEventMetadata metadata,
            CombatSide side,
            BattleHealth previousBattleHealth,
            BattleHealth currentBattleHealth)
            : base(
                metadata,
                CombatEventKind
                    .BattleHealthChanged)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Battle Health change requires " +
                    "Player or Enemy side.");
            }

            if (previousBattleHealth ==
                currentBattleHealth)
            {
                throw new ArgumentException(
                    "Battle Health change event requires " +
                    "an actual value change.",
                    nameof(currentBattleHealth));
            }

            Side =
                side;

            PreviousBattleHealth =
                previousBattleHealth;

            CurrentBattleHealth =
                currentBattleHealth;
        }

        public CombatSide Side { get; }

        public BattleHealth PreviousBattleHealth
        {
            get;
        }

        public BattleHealth CurrentBattleHealth
        {
            get;
        }

        public long Delta =>
            (long)CurrentBattleHealth.Value -
            PreviousBattleHealth.Value;

        public long ChangedAmount =>
            Math.Abs(Delta);

        public bool IsGain =>
            Delta > 0;

        public bool IsDamage =>
            Delta < 0;
    }
}