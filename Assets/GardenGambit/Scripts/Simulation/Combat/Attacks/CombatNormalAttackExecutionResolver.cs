using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackExecutionResolver
    {
        private readonly CombatState
            _state;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private readonly
            CombatNormalAttackDamageResolutionResolver
            _damageResolutionResolver;

        private readonly
            CombatNormalAttackDamageApplicationResolver
            _damageApplicationResolver;

        private readonly
            CombatNormalAttackTargetDamageReductionResolver
            _targetDamageReductionResolver;

        public CombatNormalAttackExecutionResolver(
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
                CreateDefaultTargetReductionResolver())
        {
        }

        public CombatNormalAttackExecutionResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine,
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

            _targetDamageReductionResolver =
                targetDamageReductionResolver;

            _damageResolutionResolver =
                new
                    CombatNormalAttackDamageResolutionResolver();

            _damageApplicationResolver =
                new
                    CombatNormalAttackDamageApplicationResolver(
                        metadataFactory,
                        eventLog);
        }

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _targetDamageReductionResolver;

        public CombatNormalAttackDamageApplication
            Continue(
                CombatNormalAttackExecutionState
                    executionState,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            if (executionState == null)
            {
                throw new ArgumentNullException(
                    nameof(executionState));
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (resolveDamage == null)
            {
                throw new ArgumentNullException(
                    nameof(resolveDamage));
            }

            if (executionState.Stage ==
                CombatNormalAttackExecutionStage
                    .Prepared)
            {
                _eventResolutionEngine.Drain(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                executionState
                    .MarkAttackTriggersResolved();
            }

            if (executionState.Stage ==
                CombatNormalAttackExecutionStage
                    .AttackTriggersResolved)
            {
                var damageResolution =
                    _damageResolutionResolver.Resolve(
                        executionState.Batch,
                        resolveDamage,
                        _targetDamageReductionResolver);

                var damageApplication =
                    _damageApplicationResolver.Apply(
                        _state,
                        damageResolution);

                executionState.SetDamageApplication(
                    damageApplication);
            }

            if (executionState.Stage ==
                CombatNormalAttackExecutionStage
                    .DamageApplied)
            {
                _eventResolutionEngine.Drain(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                executionState.MarkCompleted();
            }

            if (!executionState.IsCompleted)
            {
                throw new InvalidOperationException(
                    "Normal Attack execution stopped at " +
                    $"an unsupported stage: " +
                    $"{executionState.Stage}.");
            }

            return executionState
                .DamageApplication;
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