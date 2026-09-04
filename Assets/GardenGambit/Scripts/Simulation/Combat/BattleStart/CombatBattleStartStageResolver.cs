using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatBattleStartStageResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        public CombatBattleStartStageResolver(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog)
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

            _metadataFactory =
                metadataFactory;

            _eventLog =
                eventLog;
        }

        public BattleStartStageStartedCombatEvent
            StartStage(
                CombatStartedCombatEvent
                    combatStartedEvent,
                CombatBattleStartStage stage)
        {
            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            ValidateStage(
                stage);

            ValidateLoggedCombatStartedEvent(
                combatStartedEvent);

            EnsureStageCanStart(
                combatStartedEvent,
                stage);

            var metadata =
                _metadataFactory.CreateChild(
                    combatStartedEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            BattleStartStageStartedCombatEvent
                 stageEvent;

            if (combatStartedEvent
                    .HasBattleStartSnapshot)
            {
                stageEvent =
                    new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage,
                        combatStartedEvent
                            .BattleStartSnapshot);
            }
            else
            {
                stageEvent =
                    new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage);
            }

            _eventLog.Append(
                stageEvent);

            return stageEvent;
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

        private void EnsureStageCanStart(
            CombatStartedCombatEvent
                combatStartedEvent,
            CombatBattleStartStage requestedStage)
        {
            var expectedStage =
                CombatBattleStartStage.Slot;

            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var stageEvent =
                    _eventLog.Events[index]
                        as
                        BattleStartStageStartedCombatEvent;

                if (stageEvent == null)
                {
                    continue;
                }

                if (stageEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                if (stageEvent.Stage !=
                    expectedStage)
                {
                    throw new InvalidOperationException(
                        "Logged Battle Start Stage events " +
                        "are not in Slot, Pet, Card order.");
                }

                expectedStage =
                    GetNextStage(
                        expectedStage);
            }

            if (requestedStage != expectedStage)
            {
                throw new InvalidOperationException(
                    $"Cannot start {requestedStage} stage. " +
                    $"The next expected stage is " +
                    $"{expectedStage}.");
            }
        }

        private void EnsureMetadataCanBeAppended(
            CombatEventMetadata metadata)
        {
            if (_eventLog.ContainsEvent(
                    metadata.EventId))
            {
                throw new InvalidOperationException(
                    $"Allocated EventId already exists " +
                    $"in the log: {metadata.EventId}.");
            }

            if (_eventLog.Count == 0)
            {
                return;
            }

            var previousSequence =
                _eventLog.Events[
                    _eventLog.Count - 1]
                    .Metadata.SequenceNo;

            if (metadata.SequenceNo <=
                previousSequence)
            {
                throw new InvalidOperationException(
                    "Allocated SequenceNo is not greater " +
                    "than the latest logged sequence.");
            }
        }

        private static void ValidateStage(
            CombatBattleStartStage stage)
        {
            if (stage != CombatBattleStartStage.Slot &&
                stage != CombatBattleStartStage.Pet &&
                stage != CombatBattleStartStage.Card)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stage),
                    stage,
                    "Battle Start Stage resolver requires " +
                    "Slot, Pet or Card stage.");
            }
        }

        private static CombatBattleStartStage
            GetNextStage(
                CombatBattleStartStage stage)
        {
            if (stage ==
                CombatBattleStartStage.Slot)
            {
                return CombatBattleStartStage.Pet;
            }

            if (stage ==
                CombatBattleStartStage.Pet)
            {
                return CombatBattleStartStage.Card;
            }

            if (stage ==
                CombatBattleStartStage.Card)
            {
                return CombatBattleStartStage.Completed;
            }

            throw new ArgumentOutOfRangeException(
                nameof(stage),
                stage,
                "Cannot determine the next Battle " +
                "Start Stage.");
        }
    }
}