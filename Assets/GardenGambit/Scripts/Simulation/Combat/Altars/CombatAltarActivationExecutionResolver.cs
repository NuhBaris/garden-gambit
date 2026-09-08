using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarActivationExecutionResolver
    {
        private readonly CombatAltarActivationResolver
            _activationResolver;

        private readonly CombatEventResolutionEngine
            _resolutionEngine;

        public CombatAltarActivationExecutionResolver(
            CombatAltarActivationResolver
                activationResolver,
            CombatEventResolutionEngine
                resolutionEngine)
        {
            if (activationResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(activationResolver));
            }

            if (resolutionEngine == null)
            {
                throw new ArgumentNullException(
                    nameof(resolutionEngine));
            }

            _activationResolver =
                activationResolver;

            _resolutionEngine =
                resolutionEngine;
        }

        public CombatEvent Continue(
            CombatAltarActivationExecutionState
                executionState,
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
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

            if (executionState.Stage ==
                CombatAltarActivationExecutionStage
                    .TransferApplied)
            {
                _resolutionEngine.Drain(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                executionState
                    .MarkTransferTriggersResolved();
            }

            if (executionState.Stage ==
                CombatAltarActivationExecutionStage
                    .TransferTriggersResolved)
            {
                _activationResolver.StartDonorDeath(
                    executionState);
            }

            if (executionState.Stage ==
                CombatAltarActivationExecutionStage
                    .DonorDeathStarted)
            {
                _resolutionEngine.Drain(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                executionState.MarkCompleted();
            }

            if (!executionState.IsCompleted)
            {
                throw new InvalidOperationException(
                    "Altar activation execution stopped " +
                    "at an unsupported stage: " +
                    $"{executionState.Stage}.");
            }

            return executionState.AltarEvent;
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
                    "Maximum trigger count per event " +
                    "must be greater than zero.");
            }
        }
    }
}