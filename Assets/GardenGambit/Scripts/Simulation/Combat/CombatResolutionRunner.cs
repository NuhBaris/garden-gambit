using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatResolutionRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatStartResolver
            _combatStartResolver;

        private readonly CombatEventResolutionEngine
            _eventResolutionEngine;

        private readonly CombatBattleStartRunner
            _battleStartRunner;

        private readonly CombatNormalColumnsRunner
            _normalColumnsRunner;

        private readonly CombatResultResolutionResolver
            _resultResolutionResolver;

        private readonly
            CombatNormalAttackSourceDamageModifierRegistry
            _sourceDamageModifierRegistry;

        private readonly
            CombatNormalAttackTargetDamageReductionResolver
            _targetDamageReductionResolver;

        private CombatStartedCombatEvent
            _activeCombatStartedEvent;

        private CombatCompletedCombatEvent
            _activeCompletedEvent;

        private ResolutionPhase
            _activePhase;

        private bool
            _activeCombatUsesStagedNormalAttack;

        private int
            _resolvedAltarActivationCount;

        private int
            _resolvedExchangeCount;

        public CombatResolutionRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry)
            : this(
                state,
                metadataFactory,
                eventLog,
                eventQueue,
                sourceRegistry,
                new
                    CombatNormalAttackSourceDamageModifierRegistry())
        {
        }

        public CombatResolutionRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
            : this(
                state,
                metadataFactory,
                eventLog,
                eventQueue,
                sourceRegistry,
                sourceDamageModifierRegistry,
                CreateDefaultTargetReductionResolver())
        {
        }

        public CombatResolutionRunner(
            CombatState state,
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatEventQueue eventQueue,
            CombatTriggerSourceRegistry sourceRegistry,
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

            if (sourceDamageModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        sourceDamageModifierRegistry));
            }

            if (targetDamageReductionResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        targetDamageReductionResolver));
            }

            _state =
                state;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;

            _targetDamageReductionResolver =
                targetDamageReductionResolver;

            _combatStartResolver =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog);

            _eventResolutionEngine =
                new CombatEventResolutionEngine(
                    state,
                    metadataFactory,
                    eventLog,
                    eventQueue,
                    sourceRegistry);

            _battleStartRunner =
                new CombatBattleStartRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    _eventResolutionEngine);

            _normalColumnsRunner =
                new CombatNormalColumnsRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    _eventResolutionEngine,
                    sourceDamageModifierRegistry,
                    targetDamageReductionResolver);

            _resultResolutionResolver =
                new CombatResultResolutionResolver(
                    metadataFactory,
                    eventLog);
        }

        public bool HasActiveCombat =>
            _activeCombatStartedEvent != null;

        public CombatStartedCombatEvent
            ActiveCombatStartedEvent =>
                _activeCombatStartedEvent;

        public CombatCompletedCombatEvent
            ActiveCompletedEvent =>
                _activeCompletedEvent;

        public bool ActiveCombatUsesStagedNormalAttack =>
            _activeCombatStartedEvent != null &&
            _activeCombatUsesStagedNormalAttack;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _sourceDamageModifierRegistry;

        public
            CombatNormalAttackTargetDamageReductionResolver
            TargetDamageReductionResolver =>
                _targetDamageReductionResolver;

        public bool HasActiveBattleStartResolution =>
            _battleStartRunner.HasActiveResolution;

        public CombatBattleStartStage
            NextBattleStartStage =>
                _battleStartRunner.NextStage;

        public bool HasActiveBattleStartStage =>
            _battleStartRunner.HasActiveStage;

        public BattleStartStageStartedCombatEvent
            ActiveBattleStartStageEvent =>
                _battleStartRunner.ActiveStageEvent;

        public bool HasActiveSlotStage =>
            _battleStartRunner.HasActiveSlotStage;

        public bool HasActivePetStage =>
            _battleStartRunner.HasActivePetStage;

        public bool HasActiveCardStage =>
            _battleStartRunner.HasActiveCardStage;

        public bool HasPendingBattleStartResolution =>
            _battleStartRunner.HasPendingResolution;

        public bool HasActiveAltarResolution =>
            _battleStartRunner
                .HasActiveAltarResolution;

        public CombatSide? ActiveAltarSide =>
            _battleStartRunner.ActiveAltarSide;

        public bool HasActiveAltarChain =>
            _battleStartRunner
                .HasActiveAltarChain;

        public CombatEvent ActiveAltarEvent =>
            _battleStartRunner.ActiveAltarEvent;

        public bool HasActiveColumn =>
            _normalColumnsRunner.HasActiveColumn;

        public bool HasPendingColumnResolution =>
            _normalColumnsRunner
                .HasPendingResolution;

        public int NextColumnValue =>
            _normalColumnsRunner.NextColumnValue;

        public int ResolvedAltarActivationCount
        {
            get
            {
                if (_battleStartRunner
                        .HasActiveResolution)
                {
                    return _battleStartRunner
                        .ResolvedAltarActivationCount;
                }

                return
                    _resolvedAltarActivationCount;
            }
        }

        public int ResolvedExchangeCount
        {
            get
            {
                if (_normalColumnsRunner
                        .HasActiveCombat)
                {
                    return _normalColumnsRunner
                        .ResolvedExchangeCount;
                }

                return _resolvedExchangeCount;
            }
        }

        public CombatCompletedCombatEvent
            StartAndResolveCombat(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return StartAndResolveCombatCore(
                false,
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public CombatCompletedCombatEvent
            StartAndResolveCombatStaged(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return StartAndResolveCombatCore(
                true,
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public CombatCompletedCombatEvent
            ResumeActiveCombat(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return ResumeActiveCombatCore(
                false,
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        public CombatCompletedCombatEvent
            ResumeActiveCombatStaged(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            return ResumeActiveCombatCore(
                true,
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private CombatCompletedCombatEvent
            StartAndResolveCombatCore(
                bool useStagedNormalAttack,
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent != null)
            {
                throw new InvalidOperationException(
                    "The active combat must be completed " +
                    "before another combat can start.");
            }

            _resolvedAltarActivationCount =
                0;

            _resolvedExchangeCount =
                0;

            _activeCompletedEvent =
                null;

            var combatStartedEvent =
                _combatStartResolver.Start(
                    _state);

            _activeCombatStartedEvent =
                combatStartedEvent;

            _activeCombatUsesStagedNormalAttack =
                useStagedNormalAttack;

            _activePhase =
                ResolutionPhase.BattleStart;

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private CombatCompletedCombatEvent
            ResumeActiveCombatCore(
                bool useStagedNormalAttack,
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            ValidateBudgets(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_activeCombatStartedEvent == null)
            {
                throw new InvalidOperationException(
                    "There is no active combat to resume.");
            }

            if (_activeCombatUsesStagedNormalAttack !=
                useStagedNormalAttack)
            {
                if (_activeCombatUsesStagedNormalAttack)
                {
                    throw new InvalidOperationException(
                        "The active combat uses staged " +
                        "Normal Attack resolution and must " +
                        "be resumed with " +
                        "ResumeActiveCombatStaged.");
                }

                throw new InvalidOperationException(
                    "The active combat uses legacy " +
                    "Normal Attack resolution and must " +
                    "be resumed with ResumeActiveCombat.");
            }

            return ContinueActiveCombat(
                maximumExchangeCountPerColumn,
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private CombatCompletedCombatEvent
            ContinueActiveCombat(
                int maximumExchangeCountPerColumn,
                int maximumPassCountPerExchange,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (_activePhase ==
                ResolutionPhase.BattleStart)
            {
                if (_battleStartRunner
                        .HasActiveResolution)
                {
                    _resolvedAltarActivationCount =
                        _battleStartRunner
                            .ResumeActiveBattleStart(
                                maximumPassCountPerExchange,
                                maximumEventCountPerPass,
                                maximumTriggerCountPerEvent);
                }
                else
                {
                    _resolvedAltarActivationCount =
                        _battleStartRunner
                            .StartAndResolveBattleStart(
                                _activeCombatStartedEvent,
                                maximumPassCountPerExchange,
                                maximumEventCountPerPass,
                                maximumTriggerCountPerEvent);
                }

                _activePhase =
                    ResolutionPhase.NormalColumns;
            }

            if (_activePhase ==
                ResolutionPhase.NormalColumns)
            {
                ResolveNormalColumns(
                    maximumExchangeCountPerColumn,
                    maximumPassCountPerExchange,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);

                _activePhase =
                    ResolutionPhase.Result;
            }

            return CompleteActiveCombat(
                maximumPassCountPerExchange,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);
        }

        private void ResolveNormalColumns(
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (_activeCombatUsesStagedNormalAttack)
            {
                if (_normalColumnsRunner
                        .HasActiveCombat)
                {
                    _resolvedExchangeCount =
                        _normalColumnsRunner
                            .ResumeActiveCombatStaged(
                                maximumExchangeCountPerColumn,
                                maximumPassCountPerExchange,
                                maximumEventCountPerPass,
                                maximumTriggerCountPerEvent);
                }
                else
                {
                    _resolvedExchangeCount =
                        _normalColumnsRunner
                            .ResolveAllColumnsForStartedCombatStaged(
                                _activeCombatStartedEvent,
                                maximumExchangeCountPerColumn,
                                maximumPassCountPerExchange,
                                maximumEventCountPerPass,
                                maximumTriggerCountPerEvent);
                }

                return;
            }

            if (_normalColumnsRunner
                    .HasActiveCombat)
            {
                _resolvedExchangeCount =
                    _normalColumnsRunner
                        .ResumeActiveCombat(
                            maximumExchangeCountPerColumn,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }
            else
            {
                _resolvedExchangeCount =
                    _normalColumnsRunner
                        .ResolveAllColumnsForStartedCombat(
                            _activeCombatStartedEvent,
                            maximumExchangeCountPerColumn,
                            maximumPassCountPerExchange,
                            maximumEventCountPerPass,
                            maximumTriggerCountPerEvent);
            }
        }

        private CombatCompletedCombatEvent
            CompleteActiveCombat(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (_activePhase !=
                ResolutionPhase.Result)
            {
                throw new InvalidOperationException(
                    "Combat result cannot be resolved " +
                    "before battle-start and normal-column " +
                    "resolution are complete.");
            }

            if (_activeCompletedEvent == null)
            {
                _activeCompletedEvent =
                    _resultResolutionResolver.Resolve(
                        _state,
                        _activeCombatStartedEvent);
            }

            _eventResolutionEngine.Drain(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            var completedEvent =
                _activeCompletedEvent;

            _activeCombatStartedEvent =
                null;

            _activeCompletedEvent =
                null;

            _activePhase =
                ResolutionPhase.None;

            _activeCombatUsesStagedNormalAttack =
                false;

            return completedEvent;
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
            int maximumExchangeCountPerColumn,
            int maximumPassCountPerExchange,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumExchangeCountPerColumn <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumExchangeCountPerColumn),
                    maximumExchangeCountPerColumn,
                    "Maximum exchange count per column " +
                    "must be greater than zero.");
            }

            if (maximumPassCountPerExchange <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCountPerExchange),
                    maximumPassCountPerExchange,
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

        private enum ResolutionPhase
        {
            None = 0,

            BattleStart = 1,

            NormalColumns = 2,

            Result = 3
        }
    }
}