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

        private bool
            _activeCombatUsesStagedNormalAttack;

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

            _state =
                state;

            _eventLog =
                eventLog;

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

        public CombatNormalColumnsRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine)
            : this(
                state,
                metadataFactory,
                eventLog,
                eventResolutionEngine,
                new
                    CombatNormalAttackSourceDamageModifierRegistry(),
                CreateDefaultTargetReductionResolver())
        {
        }

        public CombatNormalColumnsRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
            : this(
                state,
                metadataFactory,
                eventLog,
                eventResolutionEngine,
                sourceDamageModifierRegistry,
                CreateDefaultTargetReductionResolver())
        {
        }

        public CombatNormalColumnsRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionResolver
                targetDamageReductionResolver)
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

            if (eventResolutionEngine == null)
            {
                throw new ArgumentNullException(
                    nameof(eventResolutionEngine));
            }

            if (sourceDamageModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceDamageModifierRegistry));
            }

            if (targetDamageReductionResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        targetDamageReductionResolver));
            }

            _state =
                state;

            _eventLog =
                eventLog;

            _combatStartResolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            _columnRunner =
                new CombatColumnRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    sourceDamageModifierRegistry,
                    targetDamageReductionResolver);
        }

        public bool HasActiveCombat =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public bool ActiveCombatUsesStagedNormalAttack =>
            _activeCombatStartedEvent != null &&
            _activeCombatUsesStagedNormalAttack;

        public bool HasActiveColumn =>
            _columnRunner.HasActiveColumn;

        public ColumnStartedCombatEvent
            ActiveColumnEvent =>
                _columnRunner.ActiveColumnEvent;

        public bool ActiveColumnUsesStagedNormalAttack =>
            _columnRunner
                .ActiveColumnUsesStagedNormalAttack;

        public bool HasPendingResolution =>
            _columnRunner.HasPendingResolution;

        public bool HasActiveNormalAttackExecution =>
            _columnRunner
                .HasActiveNormalAttackExecution;

        public CombatNormalAttackExecutionStage
            ActiveNormalAttackStage =>
                _columnRunner
                    .ActiveNormalAttackStage;

        public CombatNormalAttackEventBatch
            ActiveNormalAttackBatch =>
                _columnRunner
                    .ActiveNormalAttackBatch;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _columnRunner
                    .SourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _columnRunner
                    .TargetDamageReductionResolver;

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
            return StartAndResolveAllColumnsCore(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: false);
        }

        public int StartAndResolveAllColumnsStaged(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return StartAndResolveAllColumnsCore(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: true);
        }

        public int ResolveAllColumnsForStartedCombat(
            CombatStartedCombatEvent
                combatStartedEvent,
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResolveAllColumnsForStartedCombatCore(
                combatStartedEvent,
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: false);
        }

        public int
            ResolveAllColumnsForStartedCombatStaged(
                CombatStartedCombatEvent
                    combatStartedEvent,
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return ResolveAllColumnsForStartedCombatCore(
                combatStartedEvent,
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: true);
        }

        public int ResumeActiveCombat(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResumeActiveCombatCore(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: false);
        }

        public int ResumeActiveCombatStaged(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResumeActiveCombatCore(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: true);
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

        private int StartAndResolveAllColumnsCore(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent,
            bool useStagedNormalAttack)
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

            BeginActiveCombat(
                combatStartedEvent,
                initialExchangeEventCount,
                useStagedNormalAttack);

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int
            ResolveAllColumnsForStartedCombatCore(
                CombatStartedCombatEvent
                    combatStartedEvent,
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                bool useStagedNormalAttack)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

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

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            EnsureColumnsNotAlreadyStarted(
                combatStartedEvent);

            var initialExchangeEventCount =
                CountExchangeEvents();

            BeginActiveCombat(
                combatStartedEvent,
                initialExchangeEventCount,
                useStagedNormalAttack);

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int ResumeActiveCombatCore(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent,
            bool useStagedNormalAttack)
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

            if (_activeCombatUsesStagedNormalAttack !=
                useStagedNormalAttack)
            {
                var activeMode =
                    _activeCombatUsesStagedNormalAttack
                        ? "staged"
                        : "legacy";

                var requestedMode =
                    useStagedNormalAttack
                        ? "staged"
                        : "legacy";

                throw new InvalidOperationException(
                    $"The active combat uses {activeMode} " +
                    $"Normal Attack resolution and cannot " +
                    $"be resumed through the " +
                    $"{requestedMode} path.");
            }

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private void BeginActiveCombat(
            CombatStartedCombatEvent
                combatStartedEvent,
            int initialExchangeEventCount,
            bool useStagedNormalAttack)
        {
            _activeCombatStartedEvent =
                combatStartedEvent;

            _activeCombatUsesStagedNormalAttack =
                useStagedNormalAttack;

            _nextColumnValue =
                BoardColumn.MinimumValue;

            _initialExchangeEventCount =
                initialExchangeEventCount;
        }

        private int ContinueActiveCombat(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_columnRunner.HasActiveColumn)
            {
                if (_activeCombatUsesStagedNormalAttack)
                {
                    _columnRunner
                        .ResumeActiveColumnStaged(
                            maximumExchangeCountPerColumn,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
                }
                else
                {
                    _columnRunner.ResumeActiveColumn(
                        maximumExchangeCountPerColumn,
                        maximumPassCountPerExchange,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);
                }

                _nextColumnValue =
                    checked(
                        _nextColumnValue + 1);
            }

            while (_nextColumnValue <=
                   BoardColumn.MaximumValue)
            {
                var column =
                    new BoardColumn(
                        _nextColumnValue);

                if (_activeCombatUsesStagedNormalAttack)
                {
                    _columnRunner
                        .StartAndResolveColumnStaged(
                            _activeCombatStartedEvent,
                            column,
                            maximumExchangeCountPerColumn,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
                }
                else
                {
                    _columnRunner.StartAndResolveColumn(
                        _activeCombatStartedEvent,
                        column,
                        maximumExchangeCountPerColumn,
                        maximumPassCountPerExchange,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);
                }

                _nextColumnValue =
                    checked(
                        _nextColumnValue + 1);
            }

            var totalResolvedExchangeCount =
                CountExchangeEvents() -
                _initialExchangeEventCount;

            ClearActiveCombat();

            return totalResolvedExchangeCount;
        }

        private void ClearActiveCombat()
        {
            _activeCombatStartedEvent =
                null;

            _activeCombatUsesStagedNormalAttack =
                false;

            _nextColumnValue =
                0;

            _initialExchangeEventCount =
                0;
        }

        private void ValidateLoggedCombatStartedEvent(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            if (!combatStartedEvent.Metadata
                    .IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            if (!_eventLog.ContainsEvent(
                    combatStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatStartedEvent))
            {
                throw new ArgumentException(
                    "Combat Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(combatStartedEvent));
            }
        }

        private void EnsureColumnsNotAlreadyStarted(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var columnEvent =
                    _eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (columnEvent == null)
                {
                    continue;
                }

                if (columnEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "Normal columns have already started " +
                    "for this combat.");
            }
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
                    count =
                        checked(
                            count + 1);
                }
            }

            return count;
        }

        private static
            CombatNormalAttackTargetDamageReductionResolver
            CreateDefaultTargetReductionResolver()
        {
            var usageRegistry =
                new CombatPetCardTriggerUsageRegistry();

            var usageCommitter =
                new CombatPetCardTriggerUsageCommitter(
                    usageRegistry);

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            return new CombatNormalAttackTargetDamageReductionResolver(
                reductionRegistry,
                usageCommitter);
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