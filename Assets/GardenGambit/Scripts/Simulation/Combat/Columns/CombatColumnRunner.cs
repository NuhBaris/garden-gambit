using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatColumnRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatColumnStartResolver
            _columnStartResolver;

        private readonly CombatColumnResolutionResolver
            _columnResolutionResolver;

        private ColumnStartedCombatEvent
            _activeColumnEvent;

        private bool
            _activeColumnUsesStagedNormalAttack;

        public CombatColumnRunner(
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

            _columnStartResolver =
                new CombatColumnStartResolver(
                    metadataFactory,
                    eventLog);

            _columnResolutionResolver =
                new CombatColumnResolutionResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);
        }

        public CombatColumnRunner(
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

        public CombatColumnRunner(
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

        public CombatColumnRunner(
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

            _columnStartResolver =
                new CombatColumnStartResolver(
                    metadataFactory,
                    eventLog);

            _columnResolutionResolver =
                new CombatColumnResolutionResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    sourceDamageModifierRegistry,
                    targetDamageReductionResolver);
        }

        public bool HasActiveColumn =>
            _activeColumnEvent != null;

        public ColumnStartedCombatEvent
            ActiveColumnEvent =>
                _activeColumnEvent;

        public bool ActiveColumnUsesStagedNormalAttack =>
            _activeColumnEvent != null &&
            _activeColumnUsesStagedNormalAttack;

        public bool HasPendingResolution =>
            _columnResolutionResolver
                .HasPendingResolution;

        public bool HasActiveNormalAttackExecution =>
            _columnResolutionResolver
                .HasActiveNormalAttackExecution;

        public CombatNormalAttackExecutionStage
            ActiveNormalAttackStage =>
                _columnResolutionResolver
                    .ActiveNormalAttackStage;

        public CombatNormalAttackEventBatch
            ActiveNormalAttackBatch =>
                _columnResolutionResolver
                    .ActiveNormalAttackBatch;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _columnResolutionResolver
                    .SourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _columnResolutionResolver
                    .TargetDamageReductionResolver;

        public int StartAndResolveColumn(
            CombatStartedCombatEvent
                combatStartedEvent,
            BoardColumn column,
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return StartAndResolveColumnCore(
                combatStartedEvent,
                column,
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: false);
        }

        public int StartAndResolveColumnStaged(
            CombatStartedCombatEvent
                combatStartedEvent,
            BoardColumn column,
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return StartAndResolveColumnCore(
                combatStartedEvent,
                column,
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: true);
        }

        public int ResumeActiveColumn(
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResumeActiveColumnCore(
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: false);
        }

        public int ResumeActiveColumnStaged(
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResumeActiveColumnCore(
                maximumExchangeCount,
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
            return _columnResolutionResolver
                .CompletePendingResolution(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        private int StartAndResolveColumnCore(
            CombatStartedCombatEvent
                combatStartedEvent,
            BoardColumn column,
            int maximumExchangeCount,
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

            if (!column.IsValid)
            {
                throw new ArgumentException(
                    "A valid board column is required.",
                    nameof(column));
            }

            ValidateBudgets(
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            if (_activeColumnEvent != null)
            {
                throw new InvalidOperationException(
                    "The active column must be completed " +
                    "before another column can start.");
            }

            if (_columnResolutionResolver
                    .HasPendingResolution)
            {
                _columnResolutionResolver
                    .CompletePendingResolution(
                        maximumPassCountPerExchange,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);
            }

            if (_columnResolutionResolver
                    .HasPendingResolution)
            {
                throw new InvalidOperationException(
                    "Pending combat event resolution " +
                    "remains before the column start.");
            }

            var columnStartedEvent =
                _columnStartResolver.StartColumn(
                    _state,
                    combatStartedEvent,
                    column);

            _activeColumnEvent =
                columnStartedEvent;

            _activeColumnUsesStagedNormalAttack =
                useStagedNormalAttack;

            int exchangeCount;

            if (useStagedNormalAttack)
            {
                exchangeCount =
                    _columnResolutionResolver
                        .ResolveStartedColumnStaged(
                            columnStartedEvent,
                            maximumExchangeCount,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }
            else
            {
                exchangeCount =
                    _columnResolutionResolver
                        .ResolveStartedColumn(
                            columnStartedEvent,
                            maximumExchangeCount,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }

            ClearActiveColumn();

            return exchangeCount;
        }

        private int ResumeActiveColumnCore(
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent,
            bool useStagedNormalAttack)
        {
            if (_activeColumnEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active column to resume.");
            }

            ValidateBudgets(
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeColumnUsesStagedNormalAttack !=
                useStagedNormalAttack)
            {
                var activeMode =
                    _activeColumnUsesStagedNormalAttack
                        ? "staged"
                        : "legacy";

                var requestedMode =
                    useStagedNormalAttack
                        ? "staged"
                        : "legacy";

                throw new InvalidOperationException(
                    $"The active column uses {activeMode} " +
                    $"Normal Attack resolution and cannot " +
                    $"be resumed through the " +
                    $"{requestedMode} path.");
            }

            int exchangeCount;

            if (useStagedNormalAttack)
            {
                exchangeCount =
                    _columnResolutionResolver
                        .ResolveStartedColumnStaged(
                            _activeColumnEvent,
                            maximumExchangeCount,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }
            else
            {
                exchangeCount =
                    _columnResolutionResolver
                        .ResolveStartedColumn(
                            _activeColumnEvent,
                            maximumExchangeCount,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }

            ClearActiveColumn();

            return exchangeCount;
        }

        private void ClearActiveColumn()
        {
            _activeColumnEvent =
                null;

            _activeColumnUsesStagedNormalAttack =
                false;
        }

        private void ValidateLoggedCombatStartedEvent(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            if (!_eventLog.ContainsEvent(
                    combatStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedCombatStartedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedCombatStartedEvent,
                    combatStartedEvent))
            {
                throw new ArgumentException(
                    "Combat Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(combatStartedEvent));
            }

            if (!combatStartedEvent.Metadata
                    .IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }
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