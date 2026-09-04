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
            : this(
                player,
                enemy,
                CreateEmptyPetSide(
                    CombatSide.Player),
                CreateEmptyPetSide(
                    CombatSide.Enemy))
        {
        }

        public CombatState(
            CombatSideState player,
            CombatSideState enemy,
            CombatSidePetState playerPets,
            CombatSidePetState enemyPets)
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

            if (playerPets == null)
            {
                throw new ArgumentNullException(
                    nameof(playerPets));
            }

            if (enemyPets == null)
            {
                throw new ArgumentNullException(
                    nameof(enemyPets));
            }

            if (playerPets.Side !=
                CombatSide.Player)
            {
                throw new ArgumentException(
                    "Player Pet state must use the " +
                    "Player side.",
                    nameof(playerPets));
            }

            if (enemyPets.Side !=
                CombatSide.Enemy)
            {
                throw new ArgumentException(
                    "Enemy Pet state must use the " +
                    "Enemy side.",
                    nameof(enemyPets));
            }

            ValidateUniqueInstanceIds(
                player,
                enemy,
                playerPets,
                enemyPets);

            Player =
                player;

            Enemy =
                enemy;

            PlayerPets =
                playerPets;

            EnemyPets =
                enemyPets;
        }

        public CombatSideState Player
        {
            get;
        }

        public CombatSideState Enemy
        {
            get;
        }

        public CombatSidePetState PlayerPets
        {
            get;
        }

        public CombatSidePetState EnemyPets
        {
            get;
        }

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

        public CombatSidePetState GetPets(
            CombatSide side)
        {
            if (side == CombatSide.Player)
            {
                return PlayerPets;
            }

            if (side == CombatSide.Enemy)
            {
                return EnemyPets;
            }

            throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                "Combat Pet side must be Player " +
                "or Enemy.");
        }

        private static void ValidateUniqueInstanceIds(
            CombatSideState player,
            CombatSideState enemy,
            CombatSidePetState playerPets,
            CombatSidePetState enemyPets)
        {
            var instanceIds =
                new HashSet<InstanceId>();

            foreach (var card in
                     player.Cards.Cards)
            {
                instanceIds.Add(
                    card.InstanceId);
            }

            foreach (var card in
                     enemy.Cards.Cards)
            {
                if (!instanceIds.Add(
                        card.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate cross-side InstanceId " +
                        $"detected: {card.InstanceId}.",
                        nameof(enemy));
                }
            }

            foreach (var pet in
                     playerPets.Pets.Pets)
            {
                if (!instanceIds.Add(
                        pet.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate card/Pet InstanceId " +
                        $"detected: {pet.InstanceId}.",
                        nameof(playerPets));
                }
            }

            foreach (var pet in
                     enemyPets.Pets.Pets)
            {
                if (!instanceIds.Add(
                        pet.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate card/Pet InstanceId " +
                        $"detected: {pet.InstanceId}.",
                        nameof(enemyPets));
                }
            }
        }

        private static CombatSidePetState
            CreateEmptyPetSide(
                CombatSide side)
        {
            return new CombatSidePetState(
                side,
                new CombatPetRegistry(
                    new CombatPetState[0]));
        }
    }
}