using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatResolutionRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatNormalColumnsRunner
            _normalColumnsRunner;

        private readonly CombatResultResolutionResolver
            _resultResolutionResolver;

        private CombatStartedCombatEvent
            _activeCombatStartedEvent;

        private CombatCompletedCombatEvent
            _activeCompletedEvent;

        private int _resolvedExchangeCount;

        public CombatResolutionRunner(
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

            _state =
                state;

            _eventLog =
                eventLog;

            _normalColumnsRunner =
                new CombatNormalColumnsRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            _resultResolutionResolver =
                new CombatResultResolutionResolver(
                    metadataFactory,
                    eventLog);
        }

        public bool HasActiveCombat =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public CombatCompletedCombatEvent
            ActiveCompletedEvent =>
                _activeCompletedEvent;

        public bool HasActiveColumn =>
            _normalColumnsRunner.HasActiveColumn;

        public bool HasPendingColumnResolution =>
            _normalColumnsRunner
                .HasPendingResolution;

        public int NextColumnValue =>
            _normalColumnsRunner.NextColumnValue;

        public int ResolvedExchangeCount
        {
            get
            {
                if (_normalColumnsRunner
                    .HasActiveCombat)
                {
                    return _normalColumnsRunner
                        .ResolvedExchangeCount;
                }

                return _resolvedExchangeCount;
            }
        }

        public CombatCompletedCombatEvent
            StartAndResolveCombat(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent != null)
            {
                throw new InvalidOperationException(
                    "The active combat must be completed " +
                    "before another combat can start.");
            }

            _resolvedExchangeCount = 0;
            _activeCompletedEvent = null;

            try
            {
                _resolvedExchangeCount =
                    _normalColumnsRunner
                        .StartAndResolveAllColumns(
                            maximumExchangeCountPerColumn,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }
            catch
            {
                CaptureActiveCombatFromColumnsRunner();

                throw;
            }

            _activeCombatStartedEvent =
                GetLoggedCombatStartedEvent();

            return CompleteActiveCombat(
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public CombatCompletedCombatEvent
            ResumeActiveCombat(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active combat to resume.");
            }

            if (_normalColumnsRunner.HasActiveCombat)
            {
                _resolvedExchangeCount =
                    _normalColumnsRunner
                        .ResumeActiveCombat(
                            maximumExchangeCountPerColumn,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }

            return CompleteActiveCombat(
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private CombatCompletedCombatEvent
            CompleteActiveCombat(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (_activeCompletedEvent == null)
            {
                _activeCompletedEvent =
                    _resultResolutionResolver.Resolve(
                        _state,
                        _activeCombatStartedEvent);
            }

            _normalColumnsRunner
                .CompletePendingResolution(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

            var completedEvent =
                _activeCompletedEvent;

            _activeCombatStartedEvent = null;
            _activeCompletedEvent = null;

            return completedEvent;
        }

        private void
            CaptureActiveCombatFromColumnsRunner()
        {
            if (!_normalColumnsRunner.HasActiveCombat)
            {
                return;
            }

            _activeCombatStartedEvent =
                _normalColumnsRunner
                    .ActiveCombatStartedEvent;
        }

        private CombatStartedCombatEvent
            GetLoggedCombatStartedEvent()
        {
            if (_eventLog.Count == 0)
            {
                throw new InvalidOperationException(
                    "Combat Started event was not logged.");
            }

            var combatStartedEvent =
                _eventLog.Events[0]
                    as CombatStartedCombatEvent;

            if (combatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "The first combat event must be the " +
                    "Combat Started event.");
            }

            return combatStartedEvent;
        }

        private static void ValidateBudgets(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumExchangeCountPerColumn <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumExchangeCountPerColumn),
                    maximumExchangeCountPerColumn,
                    "Maximum exchange count per column " +
                    "must be greater than zero.");
            }

            if (maximumPassCountPerExchange <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCountPerExchange),
                    maximumPassCountPerExchange,
                    "Maximum pass count must be " +
                    "greater than zero.");
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