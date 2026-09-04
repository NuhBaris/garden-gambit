using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        CombatBattleStartSideSnapshot
    {
        private readonly List<
            CombatBattleStartCardSnapshot>
            _cards;

        private readonly ReadOnlyCollection<
            CombatBattleStartCardSnapshot>
            _readOnlyCards;

        public CombatBattleStartSideSnapshot(
            CombatSide side,
            IEnumerable<
                CombatBattleStartCardSnapshot>
                cards)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Battle-start side snapshot requires " +
                    "Player or Enemy side.");
            }

            if (cards == null)
            {
                throw new ArgumentNullException(
                    nameof(cards));
            }

            var instanceIds =
                new HashSet<InstanceId>();

            var positions =
                new HashSet<BoardPosition>();

            _cards =
                new List<
                    CombatBattleStartCardSnapshot>();

            foreach (var card in cards)
            {
                if (card == null)
                {
                    throw new ArgumentException(
                        "Battle-start side snapshot cannot " +
                        "contain a null card snapshot.",
                        nameof(cards));
                }

                if (card.Side != side)
                {
                    throw new ArgumentException(
                        "Every card snapshot must belong " +
                        "to the snapshot side.",
                        nameof(cards));
                }

                if (!instanceIds.Add(
                        card.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate battle-start card " +
                        $"InstanceId detected: " +
                        $"{card.InstanceId}.",
                        nameof(cards));
                }

                if (!positions.Add(
                        card.Position))
                {
                    throw new ArgumentException(
                        $"Duplicate battle-start board " +
                        $"position detected: " +
                        $"{card.Position}.",
                        nameof(cards));
                }

                _cards.Add(
                    card);
            }

            Side = side;

            _readOnlyCards =
                _cards.AsReadOnly();
        }

        public CombatSide Side
        {
            get;
        }

        public int Count =>
            _cards.Count;

        public IReadOnlyList<
            CombatBattleStartCardSnapshot>
            Cards =>
                _readOnlyCards;

        public CombatBattleStartCardSnapshot
            GetCard(
                InstanceId instanceId)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(instanceId));
            }

            for (var index = 0;
                 index < _cards.Count;
                 index++)
            {
                if (_cards[index].InstanceId ==
                    instanceId)
                {
                    return _cards[index];
                }
            }

            throw new KeyNotFoundException(
                $"Battle-start card snapshot was not " +
                $"found: {instanceId}.");
        }

        public CombatBattleStartCardSnapshot
            GetCardAt(
                BoardPosition position)
        {
            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "A valid board position is required.",
                    nameof(position));
            }

            if (position.Side != Side)
            {
                throw new ArgumentException(
                    "Board position must belong to the " +
                    "snapshot side.",
                    nameof(position));
            }

            for (var index = 0;
                 index < _cards.Count;
                 index++)
            {
                if (_cards[index].Position ==
                    position)
                {
                    return _cards[index];
                }
            }

            throw new KeyNotFoundException(
                $"Battle-start card snapshot was not " +
                $"found at: {position}.");
        }

        public int CountInRow(
            BoardRow row)
        {
            ValidateRow(
                row);

            var count = 0;

            for (var index = 0;
                 index < _cards.Count;
                 index++)
            {
                if (_cards[index].Row == row)
                {
                    count++;
                }
            }

            return count;
        }

        public int CountLivingInRow(
            BoardRow row)
        {
            ValidateRow(
                row);

            var count = 0;

            for (var index = 0;
                 index < _cards.Count;
                 index++)
            {
                var card =
                    _cards[index];

                if (card.Row == row &&
                    card.WasAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ValidateRow(
            BoardRow row)
        {
            if (row != BoardRow.Front &&
                row != BoardRow.Back)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(row),
                    row,
                    "Battle-start snapshot row requires " +
                    "Front or Back.");
            }
        }
    }
}