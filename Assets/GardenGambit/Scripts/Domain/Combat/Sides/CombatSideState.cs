using System;
using System.Collections.Generic;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatSideState
    {
        public CombatSideState(
            CombatBoardState board,
            CombatCardRegistry cards,
            BattleHealth battleHealth,
            AttackMultiplier attackMultiplier)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board));
            }

            if (cards == null)
            {
                throw new ArgumentNullException(
                    nameof(cards));
            }

            if (!attackMultiplier.IsValid)
            {
                throw new ArgumentException(
                    "Combat side requires a valid " +
                    "AttackMultiplier.",
                    nameof(attackMultiplier));
            }

            var registeredInstanceIds =
                new HashSet<InstanceId>();

            foreach (var card in cards.Cards)
            {
                registeredInstanceIds.Add(
                    card.InstanceId);
            }

            foreach (var slot in board.Slots)
            {
                if (!slot.OccupantInstanceId.HasValue)
                {
                    continue;
                }

                var occupantInstanceId =
                    slot.OccupantInstanceId.Value;

                if (!registeredInstanceIds.Contains(
                        occupantInstanceId))
                {
                    throw new ArgumentException(
                        $"Board occupant {occupantInstanceId} " +
                        $"does not exist in the card registry.",
                        nameof(cards));
                }
            }

            Board = board;
            Cards = cards;
            BattleHealth = battleHealth;
            AttackMultiplier = attackMultiplier;
        }

        public CombatSide Side => Board.Side;

        public CombatBoardState Board { get; }

        public CombatCardRegistry Cards { get; }

        public BattleHealth BattleHealth
        {
            get;
            private set;
        }

        public AttackMultiplier AttackMultiplier
        {
            get;
            private set;
        }

        public BattleHealth ApplyBattleHealthDamage(
        int damage)
        {
            var updatedBattleHealth =
                BattleHealth.ApplyDamage(damage);

            BattleHealth = updatedBattleHealth;

            return updatedBattleHealth;
        }

        public BattleHealth ApplyBattleHealthGain(
            int amount)
        {
            var updatedBattleHealth =
                BattleHealth.ApplyGain(amount);

            BattleHealth = updatedBattleHealth;

            return updatedBattleHealth;
        }

        public AttackMultiplier ApplyAttackMultiplierGain(
            int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Attack Multiplier gain cannot be negative.");
            }

            var value = checked(
                AttackMultiplier.Value + amount);

            var updatedAttackMultiplier =
                new AttackMultiplier(value);

            AttackMultiplier =
                updatedAttackMultiplier;

            return updatedAttackMultiplier;
        }

        public CombatCardState GetCardAt(
            BoardPosition position)
        {
            var slot = Board.GetSlot(position);

            if (!slot.OccupantInstanceId.HasValue)
            {
                throw new InvalidOperationException(
                    $"Combat slot {slot.SlotId} is empty.");
            }

            return Cards.GetCard(
                slot.OccupantInstanceId.Value);
        }

        public CombatCardState PlaceCard(
            BoardPosition position,
            InstanceId cardInstanceId)
        {
            var card =
                Cards.GetCard(cardInstanceId);

            Board.PlaceOccupant(
                position,
                cardInstanceId);

            return card;
        }

        public CombatCardState MoveCard(
            BoardPosition sourcePosition,
            BoardPosition destinationPosition)
        {
            var card =
                GetCardAt(sourcePosition);

            Board.MoveOccupant(
                sourcePosition,
                destinationPosition);

            return card;
        }

        public CombatCardState RemoveCardFromCombat(
            BoardPosition position)
        {
            var card =
                GetCardAt(position);

            var instanceId =
                card.InstanceId;

            var removedOccupantInstanceId =
                Board.RemoveOccupant(position);

            if (removedOccupantInstanceId !=
                instanceId)
            {
                throw new InvalidOperationException(
                    "Removed board occupant does not match " +
                    "the registered combat card.");
            }

            var removedCard =
                Cards.RemoveCard(instanceId);

            if (!ReferenceEquals(
                    removedCard,
                    card))
            {
                throw new InvalidOperationException(
                    "Removed registry card does not match " +
                    "the card removed from the board.");
            }

            return removedCard;
        }

        public CombatCardState RemoveCard(
            BoardPosition position)
        {
            var card =
                GetCardAt(position);

            var removedInstanceId =
                Board.RemoveOccupant(position);

            if (removedInstanceId != card.InstanceId)
            {
                throw new InvalidOperationException(
                    "Removed board occupant does not match " +
                    "the registered combat card.");
            }

            return card;
        }

    }
}