using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatSlotBattleStartRunner
    {
        private readonly CombatBattleStartStageResolver
            _stageResolver;

        private readonly CombatAltarRunner
            _altarRunner;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private BattleStartStageStartedCombatEvent
            _activeSlotStageEvent;

        private bool
            _altarResolutionCompleted;

        private int
            _resolvedActivationCount;

        public CombatSlotBattleStartRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine)
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

            _stageResolver =
                new CombatBattleStartStageResolver(
                    metadataFactory,
                    eventLog);

            _altarRunner =
                new CombatAltarRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);

            _eventResolutionEngine =
                eventResolutionEngine;
        }

        public bool HasActiveSlotStage =>
            _activeSlotStageEvent != null;

        public BattleStartStageStartedCombatEvent
            ActiveSlotStageEvent =>
                _activeSlotStageEvent;

        public bool HasActiveAltarResolution =>
            _altarRunner.HasActiveResolution;

        public bool HasActiveSide =>
            _altarRunner.HasActiveSide;

        public CombatSide? ActiveSide =>
            _altarRunner.ActiveSide;

        public bool HasActiveChain =>
            _altarRunner.HasActiveChain;

        public CombatEvent ActiveAltarEvent =>
            _altarRunner.ActiveAltarEvent;

        public CombatSide? NextSide =>
            _altarRunner.NextSide;

        public bool HasPendingResolution =>
            _eventResolutionEngine.HasPendingWork;

        public int ResolvedActivationCount
        {
            get
            {
                if (_activeSlotStageEvent == null)
                {
                    return 0;
                }

                if (_altarRunner.HasActiveResolution)
                {
                    return _altarRunner
                        .ResolvedActivationCount;
                }

                return _resolvedActivationCount;
            }
        }

        public int StartAndResolveSlotStage(
            CombatStartedCombatEvent
                combatStartedEvent,
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateBudgets(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeSlotStageEvent != null)
            {
                throw new InvalidOperationException(
                    "The active Slot battle-start stage " +
                    "must be completed before another " +
                    "Slot stage can start.");
            }

            var slotStageEvent =
                _stageResolver.StartStage(
                    combatStartedEvent,
                    CombatBattleStartStage.Slot);

            _activeSlotStageEvent =
                slotStageEvent;

            _altarResolutionCompleted =
                false;

            _resolvedActivationCount =
                0;

            return StartAltarResolution(
                combatStartedEvent,
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public int ResumeActiveSlotStage(
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeSlotStageEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active Slot battle-start " +
                    "stage to resume.");
            }

            if (!_altarResolutionCompleted)
            {
                if (!_altarRunner.HasActiveResolution)
                {
                    throw new InvalidOperationException(
                        "Active Slot stage is missing its " +
                        "Altar resolution state.");
                }

                _resolvedActivationCount =
                    _altarRunner.ResumeActiveResolution(
                        maximumPassCountPerAltar,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _altarResolutionCompleted =
                    true;
            }

            return CompleteActiveSlotStage(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int StartAltarResolution(
            CombatStartedCombatEvent
                combatStartedEvent,
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            _resolvedActivationCount =
                _altarRunner.StartAndResolveAllAltars(
                    combatStartedEvent,
                    maximumPassCountPerAltar,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

            _altarResolutionCompleted =
                true;

            return CompleteActiveSlotStage(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int CompleteActiveSlotStage(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var resolvedActivationCount =
                _resolvedActivationCount;

            ClearActiveSlotStage();

            return resolvedActivationCount;
        }

        private void ClearActiveSlotStage()
        {
            _activeSlotStageEvent =
                null;

            _altarResolutionCompleted =
                false;

            _resolvedActivationCount =
                0;
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