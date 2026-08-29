using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatCardTombstone
    {
        public CombatCardTombstone(
            CombatCardState card,
            BoardPosition lastPosition,
            CombatCardRemovalReason removalReason,
            CombatEventMetadata removalMetadata)
        {
            if (card == null)
            {
                throw new ArgumentNullException(
                    nameof(card));
            }

            if (!lastPosition.IsValid)
            {
                throw new ArgumentException(
                    "A tombstone requires a valid " +
                    "last board position.",
                    nameof(lastPosition));
            }

            if (removalReason !=
                    CombatCardRemovalReason.DeathRemoval &&
                removalReason !=
                    CombatCardRemovalReason.DirectDelete)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(removalReason),
                    removalReason,
                    "A valid permanent card removal " +
                    "reason is required.");
            }

            if (!removalMetadata.IsValid)
            {
                throw new ArgumentException(
                    "A tombstone requires valid removal " +
                    "event metadata.",
                    nameof(removalMetadata));
            }

            if (!removalMetadata.HasParent)
            {
                throw new ArgumentException(
                    "Card removal metadata must reference " +
                    "a parent event.",
                    nameof(removalMetadata));
            }

            if (removalReason ==
                    CombatCardRemovalReason.DeathRemoval &&
                !card.IsAtDeathThreshold)
            {
                throw new ArgumentException(
                    "A Death Removal tombstone requires " +
                    "a card at the death threshold.",
                    nameof(card));
            }

            DefinitionId = card.DefinitionId;
            InstanceId = card.InstanceId;
            Rank = card.Rank;
            HpCapacity = card.HpCapacity;
            CurrentHp = card.CurrentHp;
            Armor = card.Armor;
            Attack = card.Attack;
            LastPosition = lastPosition;
            RemovalReason = removalReason;
            RemovalMetadata = removalMetadata;
        }

        public DefinitionId DefinitionId
        {
            get;
        }

        public InstanceId InstanceId
        {
            get;
        }

        public CardRank Rank
        {
            get;
        }

        public int HpCapacity
        {
            get;
        }

        public int CurrentHp
        {
            get;
        }

        public int Armor
        {
            get;
        }

        public int Attack
        {
            get;
        }

        public BoardPosition LastPosition
        {
            get;
        }

        public CombatCardRemovalReason RemovalReason
        {
            get;
        }

        public CombatEventMetadata RemovalMetadata
        {
            get;
        }

        public bool WasAtDeathThreshold =>
            CurrentHp <= 0;
    }
}