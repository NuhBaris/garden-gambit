using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatColumnExchangeCycleResolver
    {
        private readonly CombatState
            _state;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatColumnNormalAttackResolver
            _normalAttackResolver;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private readonly CombatNormalAttackRunner
            _normalAttackRunner;

        private readonly CombatColumnNormalAttackRunner
            _columnNormalAttackRunner;

        public CombatColumnExchangeCycleResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry)
            : this(
                state,
                metadataFactory,
                eventLog,
                CreateEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry))
        {
        }

        public CombatColumnExchangeCycleResolver(
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

        public CombatColumnExchangeCycleResolver(
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

        public CombatColumnExchangeCycleResolver(
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

            _eventResolutionEngine =
                eventResolutionEngine;

            _normalAttackResolver =
                new CombatColumnNormalAttackResolver(
                    metadataFactory,
                    eventLog);

            _normalAttackRunner =
                new CombatNormalAttackRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    sourceDamageModifierRegistry,
                    targetDamageReductionResolver);

            _columnNormalAttackRunner =
                new CombatColumnNormalAttackRunner(
                    state,
                    _normalAttackRunner);
        }

        public bool HasPendingResolution =>
            _columnNormalAttackRunner
                .HasActiveExecution ||
            _eventResolutionEngine.HasPendingWork;

        public bool HasActiveNormalAttackExecution =>
            _columnNormalAttackRunner
                .HasActiveExecution;

        public CombatNormalAttackExecutionState
            ActiveNormalAttackExecutionState =>
                _columnNormalAttackRunner
                    .ActiveExecutionState;

        public CombatNormalAttackEventBatch
            ActiveNormalAttackBatch =>
                _columnNormalAttackRunner
                    .ActiveBatch;

        public CombatNormalAttackExecutionStage
            ActiveNormalAttackStage =>
                _columnNormalAttackRunner
                    .ActiveStage;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _normalAttackRunner
                    .SourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _normalAttackRunner
                    .TargetDamageReductionResolver;

        public NormalAttackExchangeCombatEvent
            TryResolveExchangeAndCompleteChain(
                ColumnStartedCombatEvent
                    columnStartedEvent,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (columnStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(columnStartedEvent));
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_columnNormalAttackRunner
                    .HasActiveExecution)
            {
                throw new InvalidOperationException(
                    "The active staged Normal Attack " +
                    "execution must be completed before " +
                    "using the legacy exchange path.");
            }

            if (_eventResolutionEngine.HasPendingWork)
            {
                throw new InvalidOperationException(
                    "Pending combat event resolution must " +
                    "be completed before starting another " +
                    "normal attack exchange.");
            }

            var exchangeEvent =
                _normalAttackResolver
                    .TryResolveExchange(
                        _state,
                        columnStartedEvent);

            if (exchangeEvent == null)
            {
                return null;
            }

            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            return exchangeEvent;
        }

        public NormalAttackExchangeCombatEvent
            TryResolveStagedExchangeAndCompleteChain(
                ColumnStartedCombatEvent
                    columnStartedEvent,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (columnStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(columnStartedEvent));
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_columnNormalAttackRunner
                    .HasActiveExecution)
            {
                throw new InvalidOperationException(
                    "The active staged Normal Attack " +
                    "execution must be resumed before " +
                    "another exchange can start.");
            }

            if (_eventResolutionEngine.HasPendingWork)
            {
                throw new InvalidOperationException(
                    "Pending combat event resolution must " +
                    "be completed before starting another " +
                    "staged normal attack exchange.");
            }

            var application =
                _columnNormalAttackRunner
                    .TryStartAndResolve(
                        columnStartedEvent,
                        maximumPassCount,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

            if (application == null)
            {
                return null;
            }

            return application.Batch.ExchangeEvent;
        }

        public int CompletePendingResolution(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_columnNormalAttackRunner
                    .HasActiveExecution)
            {
                var pendingEventCountBeforeResume =
                    _eventResolutionEngine
                        .PendingEventCount;

                var eventCountBeforeResume =
                    _eventLog.Count;

                _columnNormalAttackRunner
                    .ResumeActiveExecution(
                        maximumPassCount,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                var appendedEventCount =
                    checked(
                        _eventLog.Count -
                        eventCountBeforeResume);

                return checked(
                    pendingEventCountBeforeResume +
                    appendedEventCount);
            }

            return _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private static
            CombatNormalAttackTargetDamageReductionResolver
            CreateDefaultTargetReductionResolver()
        {
            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        usageRegistry);

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            return new
                CombatNormalAttackTargetDamageReductionResolver(
                    reductionRegistry,
                    usageCommitter);
        }

        private static CombatEventResolutionEngine
            CreateEventResolutionEngine(
                CombatState state,
                CombatEventMetadataFactory
                    metadataFactory,
                CombatEventLog eventLog,
                CombatEventQueue eventQueue,
                CombatTriggerSourceRegistry
                    sourceRegistry)
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

            return new CombatEventResolutionEngine(
                state,
                metadataFactory,
                eventLog,
                eventQueue,
                sourceRegistry);
        }

        private static void ValidateBudgets(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumPassCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCount),
                    maximumPassCount,
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