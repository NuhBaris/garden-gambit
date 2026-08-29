using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerHandlerRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatEventTriggerRunner<
            ICombatTriggerHandler> _runner;

        public CombatTriggerHandlerRunner(
            CombatState state,
            CombatEventQueue eventQueue)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            _state = state;

            _runner =
                new CombatEventTriggerRunner<
                    ICombatTriggerHandler>(
                    eventQueue);
        }

        public bool HasActiveBatch =>
            _runner.HasActiveBatch;

        public CombatEventTriggerBatch<
            ICombatTriggerHandler> ActiveBatch =>
                _runner.ActiveBatch;

        public int PendingTriggerCount =>
            _runner.PendingTriggerCount;

        public int Drain(
            int maximumEventCount,
            int maximumTriggerCountPerEvent,
            Func<
                CombatState,
                CombatEvent,
                IEnumerable<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>>
                discoverTriggers)
        {
            if (discoverTriggers == null)
            {
                throw new ArgumentNullException(
                    nameof(discoverTriggers));
            }

            return _runner.DrainWithSource(
                maximumEventCount,
                maximumTriggerCountPerEvent,
                sourceEvent =>
                    discoverTriggers(
                        _state,
                        sourceEvent),
                (sourceEvent, handler) =>
                    handler.Resolve(
                        _state,
                        sourceEvent));
        }
    }
}