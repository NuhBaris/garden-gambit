using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatColumnExchangeLoopResolver
    {
        private readonly CombatState
            _state;

        private readonly CombatColumnFrontlineResolver
            _frontlineResolver;

        private readonly CombatColumnExchangeCycleResolver
            _exchangeCycleResolver;

        public CombatColumnExchangeLoopResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (metadataFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(metadataFactory));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
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

            _state = state;

            _frontlineResolver =
                new CombatColumnFrontlineResolver();

            _exchangeCycleResolver =
                new CombatColumnExchangeCycleResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);
        }

        public bool HasPendingResolution =>
            _exchangeCycleResolver
                .HasPendingResolution;

        public int ResolveAvailableExchanges(
            ColumnStartedCombatEvent
                columnStartedEvent,
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (columnStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(columnStartedEvent));
            }

            ValidateBudgets(
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_exchangeCycleResolver
                    .HasPendingResolution)
            {
                throw new InvalidOperationException(
                    "Pending combat event resolution must " +
                    "be completed before continuing the " +
                    "column exchange loop.");
            }

            var resolvedExchangeCount = 0;

            while (resolvedExchangeCount <
                   maximumExchangeCount)
            {
                var exchangeEvent =
                    _exchangeCycleResolver
                        .TryResolveExchangeAndCompleteChain(
                            columnStartedEvent,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);

                if (exchangeEvent == null)
                {
                    return resolvedExchangeCount;
                }

                resolvedExchangeCount++;
            }

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var canContinue =
                _frontlineResolver
                    .TryGetExchangePositions(
                        _state,
                        columnStartedEvent.Column,
                        out playerPosition,
                        out enemyPosition);

            if (canContinue)
            {
                throw new InvalidOperationException(
                    "Maximum column exchange count was " +
                    "exhausted while both sides still had " +
                    "living Front cards.");
            }

            return resolvedExchangeCount;
        }

        public int CompletePendingResolution(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return _exchangeCycleResolver
                .CompletePendingResolution(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        private static void ValidateBudgets(
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumExchangeCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumExchangeCount),
                    maximumExchangeCount,
                    "Maximum exchange count must be " +
                    "greater than zero.");
            }

            if (maximumPassCountPerExchange <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCountPerExchange),
                    maximumPassCountPerExchange,
                    "Maximum pass count per exchange must " +
                    "be greater than zero.");
            }

            if (maximumEventCountPerPass <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumEventCountPerPass),
                    maximumEventCountPerPass,
                    "Maximum event count per pass must " +
                    "be greater than zero.");
            }

            if (maximumTriggerCountPerEvent <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTriggerCountPerEvent),
                    maximumTriggerCountPerEvent,
                    "Maximum trigger count per event must " +
                    "be greater than zero.");
            }
        }
    }
}