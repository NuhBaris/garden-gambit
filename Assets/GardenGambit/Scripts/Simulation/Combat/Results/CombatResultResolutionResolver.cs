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

        public CombatResultResolutionResolver(
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

            _calculationResolver =
                new CombatResultCalculationResolver(
                    metadataFactory,
                    eventLog);

            _battleHealthResolver =
                new CombatResultBattleHealthResolver(
                    metadataFactory,
                    eventLog);

            _completionResolver =
                new CombatCompletionResolver(
                    metadataFactory,
                    eventLog);
        }

        public CombatCompletedCombatEvent Resolve(
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