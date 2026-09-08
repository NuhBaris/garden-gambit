using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatBattleEndRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatBattleEndResolver
            _battleEndResolver;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private BattleEndStartedCombatEvent
            _activeBattleEndEvent;

        public CombatBattleEndRunner(
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

            _state =
                state;

            _battleEndResolver =
                new CombatBattleEndResolver(
                    metadataFactory,
                    eventLog);

            _eventResolutionEngine =
                eventResolutionEngine;
        }

        public bool HasActiveResolution =>
            _activeBattleEndEvent != null;

        public BattleEndStartedCombatEvent
            ActiveBattleEndEvent =>
                _activeBattleEndEvent;

        public bool HasPendingResolution =>
            _eventResolutionEngine.HasPendingWork;

        public BattleEndStartedCombatEvent
            StartAndResolveBattleEnd(
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

            if (_activeBattleEndEvent != null)
            {
                throw new InvalidOperationException(
                    "The active Battle End resolution " +
                    "must be completed before another " +
                    "Battle End can start.");
            }

            CompletePreviousResolution(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var battleEndEvent =
                _battleEndResolver.StartBattleEnd(
                    _state,
                    combatStartedEvent);

            _activeBattleEndEvent =
                battleEndEvent;

            return CompleteActiveBattleEnd(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public BattleEndStartedCombatEvent
            ResumeActiveBattleEnd(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (_activeBattleEndEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active Battle End " +
                    "resolution to resume.");
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            return CompleteActiveBattleEnd(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private void CompletePreviousResolution(
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

        private BattleEndStartedCombatEvent
            CompleteActiveBattleEnd(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var completedEvent =
                _activeBattleEndEvent;

            _activeBattleEndEvent =
                null;

            return completedEvent;
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