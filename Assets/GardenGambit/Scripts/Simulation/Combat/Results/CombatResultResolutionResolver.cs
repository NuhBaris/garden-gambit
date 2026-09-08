using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatResultResolutionResolver
    {
        private readonly
            CombatResultCalculationResolver
            _calculationResolver;

        private readonly
            CombatResultBattleHealthResolver
            _battleHealthResolver;

        private readonly
            CombatCompletionResolver
            _completionResolver;

        private readonly
            CombatNormalColumnsCompletionValidator
            _normalColumnsCompletionValidator;

        private readonly
            CombatBattleEndResultPrerequisiteValidator
            _battleEndResultPrerequisiteValidator;

        public CombatResultResolutionResolver(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog)
            : this(
                metadataFactory,
                eventLog,
                new
                    CombatFinalRankModifierRegistry())
        {
        }

        public CombatResultResolutionResolver(
            CombatEventMetadataFactory metadataFactory,
            CombatEventLog eventLog,
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
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

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(finalRankModifierRegistry));
            }

            _calculationResolver =
                new CombatResultCalculationResolver(
                    metadataFactory,
                    eventLog,
                    finalRankModifierRegistry);

            _battleHealthResolver =
                new CombatResultBattleHealthResolver(
                    metadataFactory,
                    eventLog);

            _completionResolver =
                new CombatCompletionResolver(
                    metadataFactory,
                    eventLog);

            _normalColumnsCompletionValidator =
                new
                    CombatNormalColumnsCompletionValidator(
                        eventLog);

            _battleEndResultPrerequisiteValidator =
                new
                    CombatBattleEndResultPrerequisiteValidator(
                        eventLog);
        }

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _calculationResolver
                    .FinalRankModifierRegistry;

        public CombatCompletedCombatEvent Resolve(
            CombatState state,
            CombatStartedCombatEvent
                combatStartedEvent)
        {
            return ResolveCore(
                state,
                combatStartedEvent);
        }

        public CombatCompletedCombatEvent
            ResolveAfterNormalColumns(
                CombatState state,
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            return ResolveAfterBattleEnd(
                state,
                combatStartedEvent);
        }

        public CombatCompletedCombatEvent
            ResolveAfterBattleEnd(
                CombatState state,
                CombatStartedCombatEvent
                    combatStartedEvent)
        {
            _normalColumnsCompletionValidator
                .Validate(
                    state,
                    combatStartedEvent);

            _battleEndResultPrerequisiteValidator
                .Validate(
                    combatStartedEvent);

            return ResolveCore(
                state,
                combatStartedEvent);
        }

        private CombatCompletedCombatEvent
            ResolveCore(
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

            var resultEvent =
                _calculationResolver.Resolve(
                    state,
                    combatStartedEvent);

            _battleHealthResolver.Apply(
                state,
                resultEvent);

            return _completionResolver.Resolve(
                state,
                resultEvent);
        }
    }
}