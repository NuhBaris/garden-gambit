using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatCardLookupResult
    {
        private CombatCardLookupResult(
            CombatCardState activeCard,
            CombatCardTombstone tombstone,
            BoardPosition position)
        {
            ActiveCard = activeCard;
            Tombstone = tombstone;
            Position = position;
        }

        public static CombatCardLookupResult
            FromActiveCard(
                CombatCardState card,
                BoardPosition position)
        {
            if (card == null)
            {
                throw new ArgumentNullException(
                    nameof(card));
            }

            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "An active card lookup requires " +
                    "a valid board position.",
                    nameof(position));
            }

            return new CombatCardLookupResult(
                card,
                null,
                position);
        }

        public static CombatCardLookupResult
            FromTombstone(
                CombatCardTombstone tombstone)
        {
            if (tombstone == null)
            {
                throw new ArgumentNullException(
                    nameof(tombstone));
            }

            return new CombatCardLookupResult(
                null,
                tombstone,
                tombstone.LastPosition);
        }

        public CombatCardState ActiveCard
        {
            get;
        }

        public CombatCardTombstone Tombstone
        {
            get;
        }

        public BoardPosition Position
        {
            get;
        }

        public bool IsActive =>
            ActiveCard != null;

        public bool IsRemoved =>
            Tombstone != null;

        public DefinitionId DefinitionId =>
            IsActive
                ? ActiveCard.DefinitionId
                : Tombstone.DefinitionId;

        public InstanceId InstanceId =>
            IsActive
                ? ActiveCard.InstanceId
                : Tombstone.InstanceId;

        public CardRank Rank =>
            IsActive
                ? ActiveCard.Rank
                : Tombstone.Rank;

        public int HpCapacity =>
            IsActive
                ? ActiveCard.HpCapacity
                : Tombstone.HpCapacity;

        public int CurrentHp =>
            IsActive
                ? ActiveCard.CurrentHp
                : Tombstone.CurrentHp;

        public int Armor =>
            IsActive
                ? ActiveCard.Armor
                : Tombstone.Armor;

        public int Attack =>
            IsActive
                ? ActiveCard.Attack
                : Tombstone.Attack;

        public CombatCardRemovalReason RemovalReason =>
            IsRemoved
                ? Tombstone.RemovalReason
                : CombatCardRemovalReason.Unspecified;
    }
}