using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatTriggerEngine
    {
        private readonly CombatTriggerSourceRegistry
            _sourceRegistry;

        private readonly CombatTriggerHandlerRunner
            _handlerRunner;

        public CombatTriggerEngine(
            CombatState state,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry)
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

            if (sourceRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceRegistry));
            }

            _sourceRegistry = sourceRegistry;

            _handlerRunner =
                new CombatTriggerHandlerRunner(
                    state,
                    eventQueue);
        }

        public bool HasActiveBatch =>
            _handlerRunner.HasActiveBatch;

        public CombatEventTriggerBatch<
            ICombatTriggerHandler> ActiveBatch =>
                _handlerRunner.ActiveBatch;

        public int PendingTriggerCount =>
            _handlerRunner.PendingTriggerCount;

        public int Drain(
            int maximumEventCount,
            int maximumTriggerCountPerEvent)
        {
            return _handlerRunner.Drain(
                maximumEventCount,
                maximumTriggerCountPerEvent,
                _sourceRegistry.DiscoverTriggers);
        }
    }
}