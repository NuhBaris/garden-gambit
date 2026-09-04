using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatNormalAttackRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private readonly
            CombatNormalAttackPreparationResolver
            _preparationResolver;

        private readonly
            CombatNormalAttackExecutionResolver
            _executionResolver;

        private readonly
            CombatNormalAttackSourceDamageModifierRegistry
            _sourceDamageModifierRegistry;

        private readonly
            CombatNormalAttackTargetDamageReductionResolver
            _targetDamageReductionResolver;

        private CombatNormalAttackExecutionState
            _activeExecutionState;

        public CombatNormalAttackRunner(
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

        public CombatNormalAttackRunner(
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

        public CombatNormalAttackRunner(
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

            _eventResolutionEngine =
                eventResolutionEngine;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            _targetDamageReductionResolver =
                targetDamageReductionResolver;

            _preparationResolver =
                new
                    CombatNormalAttackPreparationResolver(
                        metadataFactory,
                        eventLog);

            _executionResolver =
                new CombatNormalAttackExecutionResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    targetDamageReductionResolver);
        }

        public bool HasActiveExecution =>
            _activeExecutionState != null;

        public CombatNormalAttackExecutionState
            ActiveExecutionState =>
                _activeExecutionState;

        public CombatNormalAttackEventBatch
            ActiveBatch =>
                _activeExecutionState == null
                    ? null
                    : _activeExecutionState.Batch;

        public CombatNormalAttackExecutionStage
            ActiveStage =>
                _activeExecutionState == null
                    ? CombatNormalAttackExecutionStage
                        .Unspecified
                    : _activeExecutionState.Stage;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _sourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _targetDamageReductionResolver;

        public CombatNormalAttackDamageApplication
            StartAndResolve(
                BoardPosition playerPosition,
                BoardPosition enemyPosition,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return StartAndResolve(
                playerPosition,
                enemyPosition,
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                _sourceDamageModifierRegistry
                    .ResolveDamage);
        }

        public CombatNormalAttackDamageApplication
            StartAndResolve(
                BoardPosition playerPosition,
                BoardPosition enemyPosition,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            ValidateExecutionRequest(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);

            EnsureCanStart();

            var batch =
                _preparationResolver.Prepare(
                    _state,
                    playerPosition,
                    enemyPosition);

            return StartPreparedBatchAndContinue(
                batch,
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);
        }

        public CombatNormalAttackDamageApplication
            StartAndResolveInColumn(
                ColumnStartedCombatEvent
                    columnStartedEvent,
                BoardPosition playerPosition,
                BoardPosition enemyPosition,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return StartAndResolveInColumn(
                columnStartedEvent,
                playerPosition,
                enemyPosition,
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                _sourceDamageModifierRegistry
                    .ResolveDamage);
        }

        public CombatNormalAttackDamageApplication
            StartAndResolveInColumn(
                ColumnStartedCombatEvent
                    columnStartedEvent,
                BoardPosition playerPosition,
                BoardPosition enemyPosition,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            ValidateExecutionRequest(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);

            EnsureCanStart();

            var batch =
                _preparationResolver.PrepareInColumn(
                    _state,
                    columnStartedEvent,
                    playerPosition,
                    enemyPosition);

            return StartPreparedBatchAndContinue(
                batch,
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);
        }

        public CombatNormalAttackDamageApplication
            ResumeActiveExecution(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return ResumeActiveExecution(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                _sourceDamageModifierRegistry
                    .ResolveDamage);
        }

        public CombatNormalAttackDamageApplication
            ResumeActiveExecution(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            if (_activeExecutionState == null)
            {
                throw new InvalidOperationException(
                    "There is no active Normal Attack " +
                    "execution to resume.");
            }

            ValidateExecutionRequest(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);

            return ContinueActiveExecution(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);
        }

        private CombatNormalAttackDamageApplication
            StartPreparedBatchAndContinue(
                CombatNormalAttackEventBatch batch,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            _activeExecutionState =
                new CombatNormalAttackExecutionState(
                    batch);

            return ContinueActiveExecution(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                resolveDamage);
        }

        private CombatNormalAttackDamageApplication
            ContinueActiveExecution(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            var application =
                _executionResolver.Continue(
                    _activeExecutionState,
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent,
                    resolveDamage);

            _activeExecutionState =
                null;

            return application;
        }

        private void EnsureCanStart()
        {
            if (_activeExecutionState != null)
            {
                throw new InvalidOperationException(
                    "The active Normal Attack execution " +
                    "must be completed before another " +
                    "Normal Attack can start.");
            }

            if (_eventResolutionEngine.HasPendingWork)
            {
                throw new InvalidOperationException(
                    "Pending combat event resolution must " +
                    "be completed before starting a " +
                    "Normal Attack.");
            }
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

        private static void ValidateExecutionRequest(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent,
            Func<NormalAttackCombatEvent, int>
                resolveDamage)
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

            if (resolveDamage == null)
            {
                throw new ArgumentNullException(
                    nameof(resolveDamage));
            }
        }
    }
}