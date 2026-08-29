using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatCardRegistry
    {
        private readonly List<CombatCardState> _cards;

        private readonly ReadOnlyCollection<CombatCardState>
            _readOnlyCards;

        public CombatCardRegistry(
            IEnumerable<CombatCardState> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(
                    nameof(cards));
            }

            var instanceIds =
                new HashSet<InstanceId>();

            _cards = new List<CombatCardState>();

            foreach (var card in cards)
            {
                if (card == null)
                {
                    throw new ArgumentException(
                        "Combat card registry cannot " +
                        "contain a null card.",
                        nameof(cards));
                }

                if (!instanceIds.Add(card.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate card InstanceId detected: " +
                        $"{card.InstanceId}.",
                        nameof(cards));
                }

                _cards.Add(card);
            }

            _readOnlyCards = _cards.AsReadOnly();
        }

        public int Count => _cards.Count;

        public IReadOnlyList<CombatCardState> Cards =>
            _readOnlyCards;

        public CombatCardState GetCard(
            InstanceId instanceId)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(instanceId));
            }

            foreach (var card in _cards)
            {
                if (card.InstanceId == instanceId)
                {
                    return card;
                }
            }

            throw new KeyNotFoundException(
                $"Combat card was not found: {instanceId}.");
        }

        public CombatCardState RemoveCard(
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
                var card =
                    _cards[index];

                if (card.InstanceId != instanceId)
                {
                    continue;
                }

                _cards.RemoveAt(index);

                return card;
            }

            throw new KeyNotFoundException(
                $"Combat card was not found: {instanceId}.");
        }


    }
}