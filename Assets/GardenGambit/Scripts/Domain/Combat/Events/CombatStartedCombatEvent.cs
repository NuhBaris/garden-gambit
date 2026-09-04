using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        CombatStartedCombatEvent :
        CombatEvent
    {
        public CombatStartedCombatEvent(
            CombatEventMetadata metadata)
            : base(
                metadata,
                CombatEventKind.CombatStarted)
        {
            ValidateRootMetadata(
                metadata);
        }

        public CombatStartedCombatEvent(
            CombatEventMetadata metadata,
            CombatBattleStartSnapshot
                battleStartSnapshot)
            : this(metadata)
        {
            if (battleStartSnapshot == null)
            {
                throw new ArgumentNullException(
                    nameof(battleStartSnapshot));
            }

            BattleStartSnapshot =
                battleStartSnapshot;
        }

        public CombatBattleStartSnapshot
            BattleStartSnapshot
        {
            get;
        }

        public bool HasBattleStartSnapshot =>
            BattleStartSnapshot != null;

        private static void ValidateRootMetadata(
            CombatEventMetadata metadata)
        {
            if (!metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started must be a root event.",
                    nameof(metadata));
            }
        }
    }
}