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

        private readonly CombatEventResolutionEngine
            _resolutionEngine;

        private CombatEvent
            _activeAltarEvent;

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
        }

        public bool HasActiveChain =>
            _activeAltarEvent != null;

        public CombatEvent ActiveAltarEvent =>
            _activeAltarEvent;

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

            if (_activeAltarEvent != null)
            {
                throw new InvalidOperationException(
                    "The active Altar death chain must " +
                    "be completed before another Altar " +
                    "can activate.");
            }

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

        public CombatEvent ResumeActiveChain(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeAltarEvent == null)
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
            var altarEvent =
                _activeAltarEvent;

            _resolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            _activeAltarEvent = null;

            return altarEvent;
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