using System;
using System.Collections.Generic;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatState
    {
        public CombatState(
            CombatSideState player,
            CombatSideState enemy)
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
                    "Player state must use the Player side.",
                    nameof(player));
            }

            if (enemy.Side != CombatSide.Enemy)
            {
                throw new ArgumentException(
                    "Enemy state must use the Enemy side.",
                    nameof(enemy));
            }

            var instanceIds =
                new HashSet<InstanceId>();

            foreach (var card in player.Cards.Cards)
            {
                instanceIds.Add(card.InstanceId);
            }

            foreach (var card in enemy.Cards.Cards)
            {
                if (!instanceIds.Add(card.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate cross-side InstanceId " +
                        $"detected: {card.InstanceId}.",
                        nameof(enemy));
                }
            }

            Player = player;
            Enemy = enemy;
        }

        public CombatSideState Player { get; }

        public CombatSideState Enemy { get; }

        public CombatSideState GetSide(
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
                "Combat side must be Player or Enemy.");
        }

        public CombatSideState GetOpposingSide(
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
                "Combat side must be Player or Enemy.");
        }
    }
}