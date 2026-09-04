using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatBattleStartRunner
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly CombatSlotBattleStartRunner
            _slotRunner;

        private readonly CombatPetBattleStartRunner
            _petRunner;

        private readonly CombatCardBattleStartRunner
            _cardRunner;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private CombatStartedCombatEvent
            _activeCombatStartedEvent;

        private CombatBattleStartStage
            _nextStage;

        private int
            _resolvedAltarActivationCount;

        public CombatBattleStartRunner(
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

            _eventLog =
                eventLog;

            _eventResolutionEngine =
                eventResolutionEngine;

            _slotRunner =
                new CombatSlotBattleStartRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);

            _petRunner =
                new CombatPetBattleStartRunner(
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);

            _cardRunner =
                new CombatCardBattleStartRunner(
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine);
        }

        public bool HasActiveResolution =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public CombatBattleStartStage NextStage =>
            _activeCombatStartedEvent == null
                ? CombatBattleStartStage.Unspecified
                : _nextStage;

        public bool HasActiveStage =>
            _slotRunner.HasActiveSlotStage ||
            _petRunner.HasActivePetStage ||
            _cardRunner.HasActiveCardStage;

        public BattleStartStageStartedCombatEvent
            ActiveStageEvent
        {
            get
            {
                if (_slotRunner.HasActiveSlotStage)
                {
                    return _slotRunner
                        .ActiveSlotStageEvent;
                }

                if (_petRunner.HasActivePetStage)
                {
                    return _petRunner
                        .ActivePetStageEvent;
                }

                if (_cardRunner.HasActiveCardStage)
                {
                    return _cardRunner
                        .ActiveCardStageEvent;
                }

                return null;
            }
        }

        public bool HasPendingResolution =>
            _eventResolutionEngine.HasPendingWork;

        public bool HasActiveAltarResolution =>
            _slotRunner.HasActiveAltarResolution;

        public CombatSide? ActiveAltarSide =>
            _slotRunner.ActiveSide;

        public bool HasActiveAltarChain =>
            _slotRunner.HasActiveChain;

        public CombatEvent ActiveAltarEvent =>
            _slotRunner.ActiveAltarEvent;

        public bool HasActiveSlotStage =>
            _slotRunner.HasActiveSlotStage;

        public bool HasActivePetStage =>
            _petRunner.HasActivePetStage;

        public bool HasActiveCardStage =>
            _cardRunner.HasActiveCardStage;

        public int ResolvedAltarActivationCount
        {
            get
            {
                if (_activeCombatStartedEvent == null)
                {
                    return 0;
                }

                if (_slotRunner.HasActiveSlotStage)
                {
                    return _slotRunner
                        .ResolvedActivationCount;
                }

                return
                    _resolvedAltarActivationCount;
            }
        }

        public int StartAndResolveBattleStart(
            CombatStartedCombatEvent
                combatStartedEvent,
            int maximumPassCountPerStage,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateBudgets(
                maximumPassCountPerStage,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent != null)
            {
                throw new InvalidOperationException(
                    "The active battle-start resolution " +
                    "must be completed before another " +
                    "resolution can start.");
            }

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            _activeCombatStartedEvent =
                combatStartedEvent;

            _nextStage =
                CombatBattleStartStage.Slot;

            _resolvedAltarActivationCount =
                0;

            return ContinueActiveBattleStart(
                maximumPassCountPerStage,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public int ResumeActiveBattleStart(
            int maximumPassCountPerStage,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumPassCountPerStage,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active battle-start " +
                    "resolution to resume.");
            }

            return ContinueActiveBattleStart(
                maximumPassCountPerStage,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int ContinueActiveBattleStart(
            int maximumPassCountPerStage,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_slotRunner.HasActiveSlotStage)
            {
                _resolvedAltarActivationCount =
                    _slotRunner.ResumeActiveSlotStage(
                        maximumPassCountPerStage,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _nextStage =
                    CombatBattleStartStage.Pet;
            }

            if (_petRunner.HasActivePetStage)
            {
                _petRunner.ResumeActivePetStage(
                    maximumPassCountPerStage,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _nextStage =
                    CombatBattleStartStage.Card;
            }

            if (_cardRunner.HasActiveCardStage)
            {
                _cardRunner.ResumeActiveCardStage(
                    maximumPassCountPerStage,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _nextStage =
                    CombatBattleStartStage.Completed;
            }

            if (_nextStage ==
                CombatBattleStartStage.Slot)
            {
                _resolvedAltarActivationCount =
                    _slotRunner.StartAndResolveSlotStage(
                        _activeCombatStartedEvent,
                        maximumPassCountPerStage,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _nextStage =
                    CombatBattleStartStage.Pet;
            }

            if (_nextStage ==
                CombatBattleStartStage.Pet)
            {
                _petRunner.StartAndResolvePetStage(
                    _activeCombatStartedEvent,
                    maximumPassCountPerStage,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _nextStage =
                    CombatBattleStartStage.Card;
            }

            if (_nextStage ==
                CombatBattleStartStage.Card)
            {
                _cardRunner.StartAndResolveCardStage(
                    _activeCombatStartedEvent,
                    maximumPassCountPerStage,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _nextStage =
                    CombatBattleStartStage.Completed;
            }

            if (_nextStage !=
                CombatBattleStartStage.Completed)
            {
                throw new InvalidOperationException(
                    "Battle-start resolution did not " +
                    "reach the Completed stage.");
            }

            var resolvedAltarActivationCount =
                _resolvedAltarActivationCount;

            ClearActiveResolution();

            return resolvedAltarActivationCount;
        }

        private void
            ValidateLoggedCombatStartedEvent(
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            if (!combatStartedEvent
                    .Metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            if (!_eventLog.ContainsEvent(
                    combatStartedEvent
                        .Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent
                        .Metadata.EventId);

            if (!ReferenceEquals(
                    loggedEvent,
                    combatStartedEvent))
            {
                throw new ArgumentException(
                    "Combat Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(combatStartedEvent));
            }
        }

        private void ClearActiveResolution()
        {
            _activeCombatStartedEvent =
                null;

            _nextStage =
                CombatBattleStartStage.Unspecified;

            _resolvedAltarActivationCount =
                0;
        }

        private static void ValidateBudgets(
            int maximumPassCountPerStage,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumPassCountPerStage <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCountPerStage),
                    maximumPassCountPerStage,
                    "Maximum pass count per stage must " +
                    "be greater than zero.");
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