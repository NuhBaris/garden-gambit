using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatEventQueue
    {
        private readonly CombatEventLog
            _eventLog;

        private int _nextEventIndex;

        public CombatEventQueue(
            CombatEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _eventLog = eventLog;
            _nextEventIndex = 0;
        }

        public int ProcessedCount =>
            _nextEventIndex;

        public int PendingCount =>
            _eventLog.Count -
            _nextEventIndex;

        public bool HasPending =>
            PendingCount > 0;

        public CombatEvent PeekNext()
        {
            if (!HasPending)
            {
                throw new InvalidOperationException(
                    "The combat event queue is empty.");
            }

            return _eventLog.Events[
                _nextEventIndex];
        }

        public CombatEvent DequeueNext()
        {
            var combatEvent =
                PeekNext();

            _nextEventIndex++;

            return combatEvent;
        }
    }
}