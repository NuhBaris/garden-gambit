using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatNormalColumnsRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatStartResolver
            _combatStartResolver;

        private readonly CombatColumnRunner
            _columnRunner;

        private CombatStartedCombatEvent
            _activeCombatStartedEvent;

        private int _nextColumnValue;

        private int _initialExchangeEventCount;

        public CombatNormalColumnsRunner(
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
            _eventLog = eventLog;

            _combatStartResolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            _columnRunner =
                new CombatColumnRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);
        }

        public bool HasActiveCombat =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public bool HasActiveColumn =>
            _columnRunner.HasActiveColumn;

        public ColumnStartedCombatEvent
            ActiveColumnEvent =>
                _columnRunner.ActiveColumnEvent;

        public bool HasPendingResolution =>
            _columnRunner.HasPendingResolution;

        public int NextColumnValue =>
            _nextColumnValue;

        public int ResolvedExchangeCount
        {
            get
            {
                if (_activeCombatStartedEvent == null)
                {
                    return 0;
                }

                return CountExchangeEvents() -
                       _initialExchangeEventCount;
            }
        }

        public int StartAndResolveAllColumns(
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

            var initialExchangeEventCount =
                CountExchangeEvents();

            var combatStartedEvent =
                _combatStartResolver.Start(
                    _state);

            _activeCombatStartedEvent =
                combatStartedEvent;

            _nextColumnValue =
                BoardColumn.MinimumValue;

            _initialExchangeEventCount =
                initialExchangeEventCount;

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public int ResumeActiveCombat(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_activeCombatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active combat to resume.");
            }

            ValidateBudgets(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public int CompletePendingResolution(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return _columnRunner
                .CompletePendingResolution(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        private int ContinueActiveCombat(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_columnRunner.HasActiveColumn)
            {
                _columnRunner.ResumeActiveColumn(
                    maximumExchangeCountPerColumn,
                    maximumPassCountPerExchange,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _nextColumnValue++;
            }

            while (_nextColumnValue <=
                   BoardColumn.MaximumValue)
            {
                var column =
                    new BoardColumn(
                        _nextColumnValue);

                _columnRunner.StartAndResolveColumn(
                    _activeCombatStartedEvent,
                    column,
                    maximumExchangeCountPerColumn,
                    maximumPassCountPerExchange,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _nextColumnValue++;
            }

            var totalResolvedExchangeCount =
                CountExchangeEvents() -
                _initialExchangeEventCount;

            _activeCombatStartedEvent = null;
            _nextColumnValue = 0;
            _initialExchangeEventCount = 0;

            return totalResolvedExchangeCount;
        }

        private int CountExchangeEvents()
        {
            var count = 0;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                if (_eventLog.Events[index].Kind ==
                    CombatEventKind
                        .NormalAttackExchange)
                {
                    count++;
                }
            }

            return count;
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