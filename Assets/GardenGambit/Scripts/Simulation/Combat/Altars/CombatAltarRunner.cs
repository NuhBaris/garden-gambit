using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatAltarRunner
    {
        private const int SideCount = 2;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatSideAltarRunner
            _sideRunner;

        private CombatStartedCombatEvent
            _activeCombatStartedEvent;

        private int
            _nextSideIndex;

        private int
            _resolvedActivationCount;

        public CombatAltarRunner(
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

            _eventLog =
                eventLog;

            _sideRunner =
                new CombatSideAltarRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    resolutionEngine);
        }

        public bool HasActiveResolution =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public bool HasActiveSide =>
            _sideRunner.HasActiveSide;

        public CombatSide? ActiveSide =>
            _sideRunner.ActiveSide;

        public bool HasActiveChain =>
            _sideRunner.HasActiveChain;

        public CombatEvent ActiveAltarEvent =>
            _sideRunner.ActiveAltarEvent;

        public bool HasPendingEventResolution =>
            _sideRunner.HasPendingResolution;

        public int NextSideIndex =>
            _nextSideIndex;

        public CombatSide? NextSide
        {
            get
            {
                if (_activeCombatStartedEvent == null ||
                    _nextSideIndex >= SideCount)
                {
                    return null;
                }

                return GetSideAt(
                    _nextSideIndex);
            }
        }

        public int ResolvedActivationCount =>
            _activeCombatStartedEvent == null
                ? 0
                : _resolvedActivationCount;

        public int StartAndResolveAllAltars(
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

            if (_activeCombatStartedEvent != null)
            {
                throw new InvalidOperationException(
                    "The active battle-start Altar " +
                    "resolution must be completed before " +
                    "another resolution can start.");
            }

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            _activeCombatStartedEvent =
                combatStartedEvent;

            _nextSideIndex =
                0;

            _resolvedActivationCount =
                0;

            return ContinueActiveResolution(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public int ResumeActiveResolution(
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
                    "There is no active battle-start " +
                    "Altar resolution to resume.");
            }

            return ContinueActiveResolution(
                maximumPassCountPerAltar,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private int ContinueActiveResolution(
            int maximumPassCountPerAltar,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_sideRunner.HasActiveSide)
            {
                var resolvedOnActiveSide =
                    _sideRunner.ResumeActiveSide(
                        maximumPassCountPerAltar,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _resolvedActivationCount =
                    checked(
                        _resolvedActivationCount +
                        resolvedOnActiveSide);

                _nextSideIndex =
                    checked(
                        _nextSideIndex + 1);
            }

            while (_nextSideIndex < SideCount)
            {
                var side =
                    GetSideAt(
                        _nextSideIndex);

                var resolvedOnSide =
                    _sideRunner.StartAndResolveSide(
                        _activeCombatStartedEvent,
                        side,
                        maximumPassCountPerAltar,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);

                _resolvedActivationCount =
                    checked(
                        _resolvedActivationCount +
                        resolvedOnSide);

                _nextSideIndex =
                    checked(
                        _nextSideIndex + 1);
            }

            var resolvedActivationCount =
                _resolvedActivationCount;

            ClearActiveResolution();

            return resolvedActivationCount;
        }

        private void ValidateLoggedCombatStartedEvent(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            if (!combatStartedEvent.Metadata
                    .IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started event must be a " +
                    "trigger-root event.",
                    nameof(combatStartedEvent));
            }

            if (!_eventLog.ContainsEvent(
                    combatStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Combat Started event must already " +
                    "exist in the combat event log.",
                    nameof(combatStartedEvent));
            }

            var loggedEvent =
                _eventLog.GetEvent(
                    combatStartedEvent.Metadata.EventId);

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

            _nextSideIndex =
                0;

            _resolvedActivationCount =
                0;
        }

        private static CombatSide GetSideAt(
            int sideIndex)
        {
            if (sideIndex == 0)
            {
                return CombatSide.Player;
            }

            if (sideIndex == 1)
            {
                return CombatSide.Enemy;
            }

            throw new ArgumentOutOfRangeException(
                nameof(sideIndex),
                sideIndex,
                "Altar side index must identify " +
                "Player or Enemy.");
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