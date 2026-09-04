using System;
using System.Collections.Generic;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        CombatBattleStartSnapshot
    {
        public CombatBattleStartSnapshot(
            CombatBattleStartSideSnapshot player,
            CombatBattleStartSideSnapshot enemy)
        {
            if (player == null)
            {
                throw new ArgumentNullException(
                    nameof(player));
            }

            if (enemy == null)
            {
                throw new ArgumentNullException(
                    nameof(enemy));
            }

            if (player.Side != CombatSide.Player)
            {
                throw new ArgumentException(
                    "Player battle-start snapshot must " +
                    "use the Player side.",
                    nameof(player));
            }

            if (enemy.Side != CombatSide.Enemy)
            {
                throw new ArgumentException(
                    "Enemy battle-start snapshot must " +
                    "use the Enemy side.",
                    nameof(enemy));
            }

            var instanceIds =
                new HashSet<InstanceId>();

            for (var index = 0;
                 index < player.Count;
                 index++)
            {
                instanceIds.Add(
                    player.Cards[index].InstanceId);
            }

            for (var index = 0;
                 index < enemy.Count;
                 index++)
            {
                var instanceId =
                    enemy.Cards[index].InstanceId;

                if (!instanceIds.Add(
                        instanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate cross-side battle-start " +
                        $"card InstanceId detected: " +
                        $"{instanceId}.",
                        nameof(enemy));
                }
            }

            Player = player;
            Enemy = enemy;
        }

        public CombatBattleStartSideSnapshot Player
        {
            get;
        }

        public CombatBattleStartSideSnapshot Enemy
        {
            get;
        }

        public int TotalCardCount =>
            checked(
                Player.Count +
                Enemy.Count);

        public CombatBattleStartSideSnapshot GetSide(
            CombatSide side)
        {
            if (side == CombatSide.Player)
            {
                return Player;
            }

            if (side == CombatSide.Enemy)
            {
                return Enemy;
            }

            throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                "Battle-start snapshot side must be " +
                "Player or Enemy.");
        }

        public CombatBattleStartSideSnapshot
            GetOpposingSide(
                CombatSide side)
        {
            if (side == CombatSide.Player)
            {
                return Enemy;
            }

            if (side == CombatSide.Enemy)
            {
                return Player;
            }

            throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                "Battle-start snapshot side must be " +
                "Player or Enemy.");
        }
    }
}