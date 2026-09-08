using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatBattleEndResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly
            CombatNormalColumnsCompletionValidator
            _normalColumnsCompletionValidator;

        public CombatBattleEndResolver(
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

            _normalColumnsCompletionValidator =
                new
                    CombatNormalColumnsCompletionValidator(
                        eventLog);
        }

        public BattleEndStartedCombatEvent
            StartBattleEnd(
                CombatState state,
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (combatStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(combatStartedEvent));
            }

            _normalColumnsCompletionValidator
                .Validate(
                    state,
                    combatStartedEvent);

            EnsureBattleEndNotAlreadyStarted(
                combatStartedEvent);

            EnsureResultNotAlreadyLogged(
                combatStartedEvent);

            var metadata =
                _metadataFactory.CreateChild(
                    combatStartedEvent.Metadata);

            EnsureMetadataCanBeAppended(
                metadata);

            BattleEndStartedCombatEvent
                battleEndEvent;

            if (combatStartedEvent
                    .HasBattleStartSnapshot)
            {
                battleEndEvent =
                    new BattleEndStartedCombatEvent(
                        metadata,
                        combatStartedEvent
                            .BattleStartSnapshot);
            }
            else
            {
                battleEndEvent =
                    new BattleEndStartedCombatEvent(
                        metadata);
            }

            _eventLog.Append(
                battleEndEvent);

            return battleEndEvent;
        }

        private void
            EnsureBattleEndNotAlreadyStarted(
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var battleEndEvent =
                    _eventLog.Events[index]
                        as
                            BattleEndStartedCombatEvent;

                if (battleEndEvent == null)
                {
                    continue;
                }

                if (battleEndEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "Battle End has already been started " +
                    "for this combat.");
            }
        }

        private void EnsureResultNotAlreadyLogged(
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            var triggerRootId =
                combatStartedEvent.Metadata
                    .TriggerRootId;

            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var resultEvent =
                    _eventLog.Events[index]
                        as
                            CombatResultCalculatedCombatEvent;

                if (resultEvent == null)
                {
                    continue;
                }

                if (resultEvent.Metadata
                        .TriggerRootId !=
                    triggerRootId)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "Battle End cannot start after combat " +
                    "result calculation has already begun.");
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
    }
}