using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        CombatBattleStartCardSnapshot
    {
        public CombatBattleStartCardSnapshot(
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
                    "Battle-start card snapshot requires " +
                    "a valid board position.",
                    nameof(position));
            }

            DefinitionId =
                card.DefinitionId;

            InstanceId =
                card.InstanceId;

            Position =
                position;

            Rank =
                card.Rank;

            Season =
                card.Season;

            HpCapacity =
                card.HpCapacity;

            CurrentHp =
                card.CurrentHp;

            Armor =
                card.Armor;

            Attack =
                card.Attack;
        }

        public DefinitionId DefinitionId
        {
            get;
        }

        public InstanceId InstanceId
        {
            get;
        }

        public BoardPosition Position
        {
            get;
        }

        public CombatSide Side =>
            Position.Side;

        public BoardRow Row =>
            Position.Row;

        public BoardColumn Column =>
            Position.Column;

        public CardRank Rank
        {
            get;
        }

        public CombatCardSeason Season
        {
            get;
        }

        public bool HasSpecifiedSeason =>
            Season != CombatCardSeason.Unspecified;

        public bool IsSpring =>
            Season == CombatCardSeason.Spring;

        public bool IsSummer =>
            Season == CombatCardSeason.Summer;

        public bool IsAutumn =>
            Season == CombatCardSeason.Autumn;

        public bool IsWinter =>
            Season == CombatCardSeason.Winter;

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

        public bool WasAtDeathThreshold =>
            CurrentHp <= 0;

        public bool WasAlive =>
            CurrentHp > 0;
    }
}