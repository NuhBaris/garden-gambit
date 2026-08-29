using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class NormalAttackExchangeCombatEvent :
        CombatEvent
    {
        public NormalAttackExchangeCombatEvent(
            CombatEventMetadata metadata,
            InstanceId playerInstanceId,
            BoardPosition playerPosition,
            int playerAttack,
            InstanceId enemyInstanceId,
            BoardPosition enemyPosition,
            int enemyAttack)
            : base(
                metadata,
                CombatEventKind.NormalAttackExchange)
        {
            if (!playerInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Normal attack exchange requires a valid " +
                    "Player InstanceId.",
                    nameof(playerInstanceId));
            }

            if (!playerPosition.IsValid ||
                playerPosition.Side != CombatSide.Player)
            {
                throw new ArgumentException(
                    "Player position must be a valid " +
                    "Player-side board position.",
                    nameof(playerPosition));
            }

            if (playerAttack < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(playerAttack),
                    playerAttack,
                    "Player Attack cannot be negative.");
            }

            if (!enemyInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Normal attack exchange requires a valid " +
                    "Enemy InstanceId.",
                    nameof(enemyInstanceId));
            }

            if (!enemyPosition.IsValid ||
                enemyPosition.Side != CombatSide.Enemy)
            {
                throw new ArgumentException(
                    "Enemy position must be a valid " +
                    "Enemy-side board position.",
                    nameof(enemyPosition));
            }

            if (enemyAttack < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(enemyAttack),
                    enemyAttack,
                    "Enemy Attack cannot be negative.");
            }

            PlayerInstanceId = playerInstanceId;
            PlayerPosition = playerPosition;
            PlayerAttack = playerAttack;
            EnemyInstanceId = enemyInstanceId;
            EnemyPosition = enemyPosition;
            EnemyAttack = enemyAttack;
        }

        public InstanceId PlayerInstanceId { get; }

        public BoardPosition PlayerPosition { get; }

        public int PlayerAttack { get; }

        public InstanceId EnemyInstanceId { get; }

        public BoardPosition EnemyPosition { get; }

        public int EnemyAttack { get; }
    }
}