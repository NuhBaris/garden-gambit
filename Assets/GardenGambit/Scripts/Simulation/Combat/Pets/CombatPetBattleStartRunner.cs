using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetBattleStartRunner
    {
        private readonly CombatBattleStartStageResolver
            _stageResolver;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private BattleStartStageStartedCombatEvent
            _activePetStageEvent;

        public CombatPetBattleStartRunner(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine)
        {
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

            _eventResolutionEngine =
                eventResolutionEngine;
        }

        public bool HasActivePetStage =>
            _activePetStageEvent != null;

        public BattleStartStageStartedCombatEvent
            ActivePetStageEvent =>
                _activePetStageEvent;

        public bool HasPendingResolution =>
            _eventResolutionEngine.HasPendingWork;

        public BattleStartStageStartedCombatEvent
            StartAndResolvePetStage(
                CombatStartedCombatEvent
                    combatStartedEvent,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activePetStageEvent != null)
            {
                throw new InvalidOperationException(
                    "The active Pet battle-start stage " +
                    "must be completed before another " +
                    "Pet stage can start.");
            }

            CompletePreviousStageResolution(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var petStageEvent =
                _stageResolver.StartStage(
                    combatStartedEvent,
                    CombatBattleStartStage.Pet);

            _activePetStageEvent =
                petStageEvent;

            return CompleteActivePetStage(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public BattleStartStageStartedCombatEvent
            ResumeActivePetStage(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (_activePetStageEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active Pet battle-start " +
                    "stage to resume.");
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            return CompleteActivePetStage(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private void CompletePreviousStageResolution(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (!_eventResolutionEngine.HasPendingWork)
            {
                return;
            }

            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private BattleStartStageStartedCombatEvent
            CompleteActivePetStage(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var completedPetStageEvent =
                _activePetStageEvent;

            _activePetStageEvent = null;

            return completedPetStageEvent;
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