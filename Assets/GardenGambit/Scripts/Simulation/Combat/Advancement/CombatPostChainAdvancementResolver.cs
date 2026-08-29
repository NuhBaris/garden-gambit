using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPostChainAdvancementResolver
    {
        private readonly CombatCardAdvancementResolver
            _advancementResolver;

        public CombatPostChainAdvancementResolver(
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

            _advancementResolver =
                new CombatCardAdvancementResolver(
                    metadataFactory,
                    eventLog);
        }

        public CardAdvancedCombatEvent
            TryAdvanceAfterChain(
                CombatState state,
                CombatEvent removalEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (removalEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(removalEvent));
            }

            var vacatedPosition =
                GetVacatedPosition(
                    removalEvent);

            if (vacatedPosition.Row !=
                BoardRow.Front)
            {
                return null;
            }

            return _advancementResolver.TryAdvance(
                state,
                removalEvent,
                vacatedPosition.Side,
                vacatedPosition.Column);
        }

        private static BoardPosition
            GetVacatedPosition(
                CombatEvent removalEvent)
        {
            var deathRemovalEvent =
                removalEvent
                    as DeathRemovalCombatEvent;

            if (deathRemovalEvent != null)
            {
                return deathRemovalEvent.Position;
            }

            var directDeleteEvent =
                removalEvent
                    as DirectDeleteCombatEvent;

            if (directDeleteEvent != null)
            {
                return directDeleteEvent.Position;
            }

            throw new ArgumentException(
                "Post-chain advancement requires a " +
                "Death Removal or Direct Delete event.",
                nameof(removalEvent));
        }
    }
}