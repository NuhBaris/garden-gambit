using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatStartResolver
    {
        private readonly CombatEventMetadataFactory
            _metadataFactory;

        private readonly CombatEventLog
            _eventLog;

        private readonly CombatBattleStartSnapshotResolver
            _snapshotResolver;

        public CombatStartResolver(
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

            _snapshotResolver =
                new CombatBattleStartSnapshotResolver();
        }

        public CombatStartedCombatEvent Start(
            CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (_eventLog.Count != 0)
            {
                throw new InvalidOperationException(
                    "Combat can only start with an " +
                    "empty combat event log.");
            }

            if (_eventLog.CardTombstones.Count != 0)
            {
                throw new InvalidOperationException(
                    "Combat can only start with an " +
                    "empty tombstone registry.");
            }

            var battleStartSnapshot =
                _snapshotResolver.Resolve(
                    state);

            var metadata =
                _metadataFactory.CreateRoot();

            var combatStartedEvent =
                new CombatStartedCombatEvent(
                    metadata,
                    battleStartSnapshot);

            _eventLog.Append(
                combatStartedEvent);

            return combatStartedEvent;
        }
    }
}