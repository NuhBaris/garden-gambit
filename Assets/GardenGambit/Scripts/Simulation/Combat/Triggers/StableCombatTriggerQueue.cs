using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class StableCombatTriggerQueue<T>
        where T : class
    {
        private readonly List<Entry>
            _entries;

        private long _nextEnqueueOrder;

        public StableCombatTriggerQueue()
        {
            _entries =
                new List<Entry>();

            _nextEnqueueOrder = 0;
        }

        public int Count =>
            _entries.Count;

        public bool HasPending =>
            _entries.Count > 0;

        public void Enqueue(
            T item,
            CombatTriggerOrderKey orderKey)
        {
            if (item == null)
            {
                throw new ArgumentNullException(
                    nameof(item));
            }

            if (!orderKey.IsValid)
            {
                throw new ArgumentException(
                    "A valid trigger order key is required.",
                    nameof(orderKey));
            }

            if (_nextEnqueueOrder ==
                long.MaxValue)
            {
                throw new OverflowException(
                    "Trigger enqueue order is exhausted.");
            }

            var entry =
                new Entry(
                    item,
                    orderKey,
                    _nextEnqueueOrder);

            _entries.Add(
                entry);

            _nextEnqueueOrder++;
        }

        public T PeekNext()
        {
            var nextIndex =
                FindNextIndex();

            return _entries[
                nextIndex].Item;
        }

        public T DequeueNext()
        {
            var nextIndex =
                FindNextIndex();

            var item =
                _entries[
                    nextIndex].Item;

            _entries.RemoveAt(
                nextIndex);

            return item;
        }

        private int FindNextIndex()
        {
            if (_entries.Count == 0)
            {
                throw new InvalidOperationException(
                    "The trigger priority queue is empty.");
            }

            var nextIndex = 0;

            for (var index = 1;
                 index < _entries.Count;
                 index++)
            {
                var candidate =
                    _entries[index];

                var current =
                    _entries[nextIndex];

                var priorityComparison =
                    candidate.OrderKey.CompareTo(
                        current.OrderKey);

                if (priorityComparison < 0)
                {
                    nextIndex = index;
                    continue;
                }

                if (priorityComparison == 0 &&
                    candidate.EnqueueOrder <
                    current.EnqueueOrder)
                {
                    nextIndex = index;
                }
            }

            return nextIndex;
        }

        private sealed class Entry
        {
            public Entry(
                T item,
                CombatTriggerOrderKey orderKey,
                long enqueueOrder)
            {
                Item = item;
                OrderKey = orderKey;
                EnqueueOrder = enqueueOrder;
            }

            public T Item { get; }

            public CombatTriggerOrderKey
                OrderKey
            {
                get;
            }

            public long EnqueueOrder { get; }
        }
    }
}