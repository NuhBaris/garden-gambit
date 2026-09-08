using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatAltarActivationChainResolver
    {
        private readonly CombatState
            _state;

        private readonly CombatAltarActivationResolver
            _activationResolver;

        private readonly
            CombatAltarActivationExecutionResolver
            _executionResolver;

        private readonly CombatEventResolutionEngine
            _resolutionEngine;

        private CombatEvent
            _activeAltarEvent;

        private CombatAltarActivationExecutionState
            _activeExecutionState;

        public CombatAltarActivationChainResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine resolutionEngine)
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

            if (resolutionEngine == null)
            {
                throw new ArgumentNullException(
                    nameof(resolutionEngine));
            }

            _state =
                state;

            _activationResolver =
                new CombatAltarActivationResolver(
                    metadataFactory,
                    eventLog);

            _resolutionEngine =
                resolutionEngine;

            _executionResolver =
                new
                    CombatAltarActivationExecutionResolver(
                        _activationResolver,
                        resolutionEngine);
        }

        public bool HasActiveChain =>
            _activeAltarEvent != null ||
            _activeExecutionState != null;

        public CombatEvent ActiveAltarEvent =>
            _activeExecutionState != null
                ? _activeExecutionState.AltarEvent
                : _activeAltarEvent;

        public bool HasStagedExecution =>
            _activeExecutionState != null;

        public CombatAltarActivationExecutionState
            ActiveExecutionState =>
                _activeExecutionState;

        public CombatAltarActivationExecutionStage
            ActiveStage =>
                _activeExecutionState == null
                    ? CombatAltarActivationExecutionStage
                        .Unspecified
                    : _activeExecutionState.Stage;

        public bool HasPendingResolution =>
            _resolutionEngine.HasPendingWork;

        public CombatEvent
            TryActivateAndCompleteChain(
                CombatStartedCombatEvent
                    combatStartedEvent,
                BoardPosition donorPosition,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            ValidateActivationRequest(
                combatStartedEvent,
                donorPosition,
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            EnsureNoActiveChain();

            var altarEvent =
                _activationResolver.TryActivate(
                    _state,
                    combatStartedEvent,
                    donorPosition);

            if (altarEvent == null)
            {
                return null;
            }

            _activeAltarEvent =
                altarEvent;

            return CompleteActiveChain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public CombatEvent
            TryActivateAndCompleteStagedChain(
                CombatStartedCombatEvent
                    combatStartedEvent,
                BoardPosition donorPosition,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            ValidateActivationRequest(
                combatStartedEvent,
                donorPosition,
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            EnsureNoActiveChain();

            var executionState =
                _activationResolver
                    .TryStartActivation(
                        _state,
                        combatStartedEvent,
                        donorPosition);

            if (executionState == null)
            {
                return null;
            }

            _activeExecutionState =
                executionState;

            return CompleteActiveChain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public CombatEvent ResumeActiveChain(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (!HasActiveChain)
            {
                throw new InvalidOperationException(
                    "There is no active Altar death " +
                    "chain to resume.");
            }

            return CompleteActiveChain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private CombatEvent CompleteActiveChain(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_activeExecutionState != null)
            {
                var altarEvent =
                    _executionResolver.Continue(
                        _activeExecutionState,
                        maximumPassCount,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _activeExecutionState =
                    null;

                return altarEvent;
            }

            var legacyAltarEvent =
                _activeAltarEvent;

            _resolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            _activeAltarEvent =
                null;

            return legacyAltarEvent;
        }

        private void EnsureNoActiveChain()
        {
            if (HasActiveChain)
            {
                throw new InvalidOperationException(
                    "The active Altar death chain must " +
                    "be completed before another Altar " +
                    "can activate.");
            }
        }

        private static void ValidateActivationRequest(
            CombatStartedCombatEvent
                combatStartedEvent,
            BoardPosition donorPosition,
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            if (!donorPosition.IsValid)
            {
                throw new ArgumentException(
                    "A valid Altar donor position " +
                    "is required.",
                    nameof(donorPosition));
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
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