using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatCardBattleStartRunner
    {
        private readonly CombatBattleStartStageResolver
            _stageResolver;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private BattleStartStageStartedCombatEvent
            _activeCardStageEvent;

        public CombatCardBattleStartRunner(
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

        public bool HasActiveCardStage =>
            _activeCardStageEvent != null;

        public BattleStartStageStartedCombatEvent
            ActiveCardStageEvent =>
                _activeCardStageEvent;

        public bool HasPendingResolution =>
            _eventResolutionEngine.HasPendingWork;

        public BattleStartStageStartedCombatEvent
            StartAndResolveCardStage(
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

            if (_activeCardStageEvent != null)
            {
                throw new InvalidOperationException(
                    "The active Card battle-start stage " +
                    "must be completed before another " +
                    "Card stage can start.");
            }

            CompletePreviousStageResolution(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var cardStageEvent =
                _stageResolver.StartStage(
                    combatStartedEvent,
                    CombatBattleStartStage.Card);

            _activeCardStageEvent =
                cardStageEvent;

            return CompleteActiveCardStage(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public BattleStartStageStartedCombatEvent
            ResumeActiveCardStage(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (_activeCardStageEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active Card battle-start " +
                    "stage to resume.");
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            return CompleteActiveCardStage(
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
            CompleteActiveCardStage(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var completedCardStageEvent =
                _activeCardStageEvent;

            _activeCardStageEvent = null;

            return completedCardStageEvent;
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