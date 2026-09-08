using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        BattleEndStartedCombatEvent :
        CombatEvent
    {
        public BattleEndStartedCombatEvent(
            CombatEventMetadata metadata)
            : base(
                metadata,
                CombatEventKind.BattleEndStarted)
        {
            ValidateMetadata(
                metadata);
        }

        public BattleEndStartedCombatEvent(
            CombatEventMetadata metadata,
            CombatBattleStartSnapshot
                battleStartSnapshot)
            : this(
                metadata)
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

        private static void ValidateMetadata(
            CombatEventMetadata metadata)
        {
            if (!metadata.HasParent)
            {
                throw new ArgumentException(
                    "Battle End Started event must have " +
                    "a parent event.",
                    nameof(metadata));
            }

            if (metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Battle End Started event cannot be " +
                    "a trigger-root event.",
                    nameof(metadata));
            }

            if (metadata.ParentEventId.Value !=
                metadata.TriggerRootId)
            {
                throw new ArgumentException(
                    "Battle End Started event must be a " +
                    "direct child of its trigger-root " +
                    "event.",
                    nameof(metadata));
            }
        }
    }
}