using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct
        CombatResultDamageCalculation
    {
        private readonly bool
            _isInitialized;

        public CombatResultDamageCalculation(
            CombatSideResultContribution
                playerContribution,
            CombatSideResultContribution
                enemyContribution)
        {
            if (!playerContribution.IsValid)
            {
                throw new ArgumentException(
                    "A valid Player result contribution " +
                    "is required.",
                    nameof(playerContribution));
            }

            if (playerContribution.Side !=
                CombatSide.Player)
            {
                throw new ArgumentException(
                    "Player contribution must belong to " +
                    "the Player side.",
                    nameof(playerContribution));
            }

            if (!enemyContribution.IsValid)
            {
                throw new ArgumentException(
                    "A valid Enemy result contribution " +
                    "is required.",
                    nameof(enemyContribution));
            }

            if (enemyContribution.Side !=
                CombatSide.Enemy)
            {
                throw new ArgumentException(
                    "Enemy contribution must belong to " +
                    "the Enemy side.",
                    nameof(enemyContribution));
            }

            PlayerContribution =
                playerContribution;

            EnemyContribution =
                enemyContribution;

            _isInitialized = true;
        }

        public CombatSideResultContribution
            PlayerContribution
        {
            get;
        }

        public CombatSideResultContribution
            EnemyContribution
        {
            get;
        }

        public int BaseIncomingDamageToPlayer =>
            EnemyContribution
                .FinalResultContribution;

        public int BaseIncomingDamageToEnemy =>
            PlayerContribution
                .FinalResultContribution;

        public bool HasIncomingDamageToPlayer =>
            BaseIncomingDamageToPlayer > 0;

        public bool HasIncomingDamageToEnemy =>
            BaseIncomingDamageToEnemy > 0;

        public bool HasMutualIncomingDamage =>
            HasIncomingDamageToPlayer &&
            HasIncomingDamageToEnemy;

        public bool IsValid =>
            _isInitialized &&
            PlayerContribution.IsValid &&
            PlayerContribution.Side ==
            CombatSide.Player &&
            EnemyContribution.IsValid &&
            EnemyContribution.Side ==
            CombatSide.Enemy;
    }
}