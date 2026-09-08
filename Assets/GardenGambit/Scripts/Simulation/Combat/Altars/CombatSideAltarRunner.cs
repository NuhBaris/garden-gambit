using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatSideAltarRunner
    {
        private readonly CombatState
            _state;

        private readonly
            CombatSideAltarSlotOrderResolver
            _slotOrderResolver;

        private readonly
            CombatAltarActivationChainResolver
            _activationChainResolver;

        private CombatStartedCombatEvent
            _activeCombatStartedEvent;

        private CombatSide?
            _activeSide;

        private IReadOnlyList<BoardPosition>
            _altarPositions;

        private int
            _nextAltarPositionIndex;

        private int
            _resolvedActivationCount;

        private bool
            _useStagedActivationChain;

        public CombatSideAltarRunner(
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

            _slotOrderResolver =
                new CombatSideAltarSlotOrderResolver();

            _activationChainResolver =
                new CombatAltarActivationChainResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    resolutionEngine);
        }

        public bool HasActiveSide =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public CombatSide? ActiveSide =>
            _activeSide;

        public bool UsesStagedActivationChain =>
            HasActiveSide &&
            _useStagedActivationChain;

        public bool HasActiveChain =>
            _activationChainResolver
                .HasActiveChain;

        public CombatEvent ActiveAltarEvent =>
            _activationChainResolver
                .ActiveAltarEvent;

        public bool HasStagedExecution =>
            _activationChainResolver
                .HasStagedExecution;

        public CombatAltarActivationExecutionState
            ActiveExecutionState =>
                _activationChainResolver
                    .ActiveExecutionState;

        public CombatAltarActivationExecutionStage
            ActiveExecutionStage =>
                _activationChainResolver
                    .ActiveStage;

        public bool HasPendingResolution =>
            _activationChainResolver
                .HasPendingResolution;

        public int NextAltarPositionIndex =>
            _nextAltarPositionIndex;

        public int AltarPositionCount =>
            _altarPositions == null
                ? 0
                : _altarPositions.Count;

        public int ResolvedActivationCount =>
            _activeCombatStartedEvent == null
                ? 0
                : _resolvedActivationCount;

        public int StartAndResolveSide(
            CombatStartedCombatEvent
                combatStartedEvent,
            CombatSide side,
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return StartAndResolveSideCore(
                combatStartedEvent,
                side,
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedActivationChain: false);
        }

        public int StartAndResolveSideStaged(
            CombatStartedCombatEvent
                combatStartedEvent,
            CombatSide side,
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return StartAndResolveSideCore(
                combatStartedEvent,
                side,
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedActivationChain: true);
        }

        public int ResumeActiveSide(
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active side Altar " +
                    "sequence to resume.");
            }

            return ContinueActiveSide(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int StartAndResolveSideCore(
            CombatStartedCombatEvent
                combatStartedEvent,
            CombatSide side,
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent,
            bool useStagedActivationChain)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateSide(
                side);

            ValidateBudgets(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent != null)
            {
                throw new InvalidOperationException(
                    "The active side Altar sequence " +
                    "must be completed before another " +
                    "side can start.");
            }

            var sideState =
                _state.GetSide(
                    side);

            _activeCombatStartedEvent =
                combatStartedEvent;

            _activeSide =
                side;

            _altarPositions =
                _slotOrderResolver.Resolve(
                    sideState);

            _nextAltarPositionIndex =
                0;

            _resolvedActivationCount =
                0;

            _useStagedActivationChain =
                useStagedActivationChain;

            return ContinueActiveSide(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int ContinueActiveSide(
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_activationChainResolver
                    .HasActiveChain)
            {
                _activationChainResolver
                    .ResumeActiveChain(
                        maximumPassCountPerAltar,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _resolvedActivationCount =
                    checked(
                        _resolvedActivationCount + 1);

                _nextAltarPositionIndex =
                    checked(
                        _nextAltarPositionIndex + 1);
            }

            while (_nextAltarPositionIndex <
                   _altarPositions.Count)
            {
                var donorPosition =
                    _altarPositions[
                        _nextAltarPositionIndex];

                CombatEvent altarEvent;

                if (_useStagedActivationChain)
                {
                    altarEvent =
                        _activationChainResolver
                            .TryActivateAndCompleteStagedChain(
                                _activeCombatStartedEvent,
                                donorPosition,
                                maximumPassCountPerAltar,
                                maximumEventCountPerPass,
                                maximumTriggerCountPerEvent);
                }
                else
                {
                    altarEvent =
                        _activationChainResolver
                            .TryActivateAndCompleteChain(
                                _activeCombatStartedEvent,
                                donorPosition,
                                maximumPassCountPerAltar,
                                maximumEventCountPerPass,
                                maximumTriggerCountPerEvent);
                }

                if (altarEvent != null)
                {
                    _resolvedActivationCount =
                        checked(
                            _resolvedActivationCount + 1);
                }

                _nextAltarPositionIndex =
                    checked(
                        _nextAltarPositionIndex + 1);
            }

            var resolvedActivationCount =
                _resolvedActivationCount;

            ClearActiveSide();

            return resolvedActivationCount;
        }

        private void ClearActiveSide()
        {
            _activeCombatStartedEvent =
                null;

            _activeSide =
                null;

            _altarPositions =
                null;

            _nextAltarPositionIndex =
                0;

            _resolvedActivationCount =
                0;

            _useStagedActivationChain =
                false;
        }

        private static void ValidateSide(
            CombatSide side)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Altar resolution requires " +
                    "Player or Enemy side.");
            }
        }

        private static void ValidateBudgets(
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumPassCountPerAltar <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCountPerAltar),
                    maximumPassCountPerAltar,
                    "Maximum pass count per Altar " +
                    "must be greater than zero.");
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