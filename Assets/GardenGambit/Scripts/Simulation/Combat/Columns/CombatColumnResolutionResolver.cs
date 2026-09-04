using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatColumnResolutionResolver
    {
        private readonly CombatEventLog
            _eventLog;

        private readonly CombatColumnExchangeLoopResolver
            _exchangeLoopResolver;

        public CombatColumnResolutionResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry)
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

            if (eventQueue == null)
            {
                throw new ArgumentNullException(
                    nameof(eventQueue));
            }

            if (sourceRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceRegistry));
            }

            _eventLog =
                eventLog;

            _exchangeLoopResolver =
                new CombatColumnExchangeLoopResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);
        }

        public CombatColumnResolutionResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine)
            : this(
                state,
                metadataFactory,
                eventLog,
                eventResolutionEngine,
                new
                    CombatNormalAttackSourceDamageModifierRegistry(),
                CreateDefaultTargetReductionResolver())
        {
        }

        public CombatColumnResolutionResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
            : this(
                state,
                metadataFactory,
                eventLog,
                eventResolutionEngine,
                sourceDamageModifierRegistry,
                CreateDefaultTargetReductionResolver())
        {
        }

        public CombatColumnResolutionResolver(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventResolutionEngine
                eventResolutionEngine,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry,
            CombatNormalAttackTargetDamageReductionResolver
                targetDamageReductionResolver)
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

            if (sourceDamageModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceDamageModifierRegistry));
            }

            if (targetDamageReductionResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        targetDamageReductionResolver));
            }

            _eventLog =
                eventLog;

            _exchangeLoopResolver =
                new CombatColumnExchangeLoopResolver(
                    state,
                    metadataFactory,
                    eventLog,
                    eventResolutionEngine,
                    sourceDamageModifierRegistry,
                    targetDamageReductionResolver);
        }

        public bool HasPendingResolution =>
            _exchangeLoopResolver
                .HasPendingResolution;

        public bool HasActiveNormalAttackExecution =>
            _exchangeLoopResolver
                .HasActiveNormalAttackExecution;

        public CombatNormalAttackExecutionStage
            ActiveNormalAttackStage =>
                _exchangeLoopResolver
                    .ActiveNormalAttackStage;

        public CombatNormalAttackEventBatch
            ActiveNormalAttackBatch =>
                _exchangeLoopResolver
                    .ActiveNormalAttackBatch;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _exchangeLoopResolver
                    .SourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _exchangeLoopResolver
                    .TargetDamageReductionResolver;

        public int ResolveStartedColumn(
            ColumnStartedCombatEvent
                columnStartedEvent,
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResolveStartedColumnCore(
                columnStartedEvent,
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: false);
        }

        public int ResolveStartedColumnStaged(
            ColumnStartedCombatEvent
                columnStartedEvent,
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return ResolveStartedColumnCore(
                columnStartedEvent,
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent,
                useStagedNormalAttack: true);
        }

        public int CompletePendingResolution(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            return _exchangeLoopResolver
                .CompletePendingResolution(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        private int ResolveStartedColumnCore(
            ColumnStartedCombatEvent
                columnStartedEvent,
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent,
            bool useStagedNormalAttack)
        {
            if (columnStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(columnStartedEvent));
            }

            ValidateBudgets(
                maximumExchangeCount,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            ValidateLoggedColumnStartedEvent(
                columnStartedEvent);

            EnsureNoLaterColumnStartedEvent(
                columnStartedEvent);

            if (_exchangeLoopResolver
                    .HasPendingResolution)
            {
                _exchangeLoopResolver
                    .CompletePendingResolution(
                        maximumPassCountPerExchange,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);
            }

            if (_exchangeLoopResolver
                    .HasPendingResolution)
            {
                throw new InvalidOperationException(
                    "Pending combat event resolution " +
                    "remains after the drain completed.");
            }

            if (useStagedNormalAttack)
            {
                return _exchangeLoopResolver
                    .ResolveAvailableStagedExchanges(
                        columnStartedEvent,
                        maximumExchangeCount,
                        maximumPassCountPerExchange,
                        maximumEventCountPerPass,
                        maximumTriggerCountPerEvent);
            }

            return _exchangeLoopResolver
                .ResolveAvailableExchanges(
                    columnStartedEvent,
                    maximumExchangeCount,
                    maximumPassCountPerExchange,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        private void ValidateLoggedColumnStartedEvent(
            ColumnStartedCombatEvent
                columnStartedEvent)
        {
            if (!_eventLog.ContainsEvent(
                    columnStartedEvent.Metadata.EventId))
            {
                throw new ArgumentException(
                    "Column Started event must already " +
                    "exist in the combat event log.",
                    nameof(columnStartedEvent));
            }

            var loggedColumnEvent =
                _eventLog.GetEvent(
                    columnStartedEvent.Metadata.EventId);

            if (!ReferenceEquals(
                    loggedColumnEvent,
                    columnStartedEvent))
            {
                throw new ArgumentException(
                    "Column Started event must be the " +
                    "exact event stored in the combat " +
                    "event log.",
                    nameof(columnStartedEvent));
            }

            if (!columnStartedEvent.Metadata.HasParent)
            {
                throw new ArgumentException(
                    "Column Started event must reference " +
                    "a Combat Started parent.",
                    nameof(columnStartedEvent));
            }

            var parentEvent =
                _eventLog.GetEvent(
                    columnStartedEvent.Metadata
                        .ParentEventId.Value);

            if (!(parentEvent is
                    CombatStartedCombatEvent))
            {
                throw new ArgumentException(
                    "Column Started parent must be a " +
                    "Combat Started event.",
                    nameof(columnStartedEvent));
            }
        }

        private void EnsureNoLaterColumnStartedEvent(
            ColumnStartedCombatEvent
                columnStartedEvent)
        {
            for (var index = 0;
                 index < _eventLog.Count;
                 index++)
            {
                var laterColumnEvent =
                    _eventLog.Events[index]
                        as ColumnStartedCombatEvent;

                if (laterColumnEvent == null)
                {
                    continue;
                }

                if (laterColumnEvent.Metadata.SequenceNo <=
                    columnStartedEvent.Metadata.SequenceNo)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "An older column cannot be resolved " +
                    "after a later Column Started event " +
                    "has already been logged.");
            }
        }

        private static
            CombatNormalAttackTargetDamageReductionResolver
            CreateDefaultTargetReductionResolver()
        {
            var usageRegistry =
                new CombatPetCardTriggerUsageRegistry();

            var usageCommitter =
                new CombatPetCardTriggerUsageCommitter(
                    usageRegistry);

            var reductionRegistry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            return new CombatNormalAttackTargetDamageReductionResolver(
                reductionRegistry,
                usageCommitter);
        }

        private static void ValidateBudgets(
            int maximumExchangeCount,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumExchangeCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumExchangeCount),
                    maximumExchangeCount,
                    "Maximum exchange count must be " +
                    "greater than zero.");
            }

            if (maximumPassCountPerExchange <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCountPerExchange),
                    maximumPassCountPerExchange,
                    "Maximum pass count per exchange must " +
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