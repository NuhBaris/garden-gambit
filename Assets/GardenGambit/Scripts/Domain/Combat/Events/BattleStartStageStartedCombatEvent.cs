using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        BattleStartStageStartedCombatEvent :
        CombatEvent
    {
        public BattleStartStageStartedCombatEvent(
            CombatEventMetadata metadata,
            CombatBattleStartStage stage)
            : base(
                metadata,
                CombatEventKind
                    .BattleStartStageStarted)
        {
            ValidateMetadata(
                metadata);

            ValidateStage(
                stage);

            Stage = stage;
        }

        public BattleStartStageStartedCombatEvent(
            CombatEventMetadata metadata,
            CombatBattleStartStage stage,
            CombatBattleStartSnapshot
                battleStartSnapshot)
            : this(
                metadata,
                stage)
        {
            if (battleStartSnapshot == null)
            {
                throw new ArgumentNullException(
                    nameof(battleStartSnapshot));
            }

            BattleStartSnapshot =
                battleStartSnapshot;
        }

        public CombatBattleStartStage Stage
        {
            get;
        }

        public bool IsSlotStage =>
            Stage == CombatBattleStartStage.Slot;

        public bool IsPetStage =>
            Stage == CombatBattleStartStage.Pet;

        public bool IsCardStage =>
            Stage == CombatBattleStartStage.Card;

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
                    "Battle Start Stage Started event " +
                    "must have a parent event.",
                    nameof(metadata));
            }

            if (metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Battle Start Stage Started event " +
                    "cannot be a trigger-root event.",
                    nameof(metadata));
            }

            if (metadata.ParentEventId.Value !=
                metadata.TriggerRootId)
            {
                throw new ArgumentException(
                    "Battle Start Stage Started event " +
                    "must be a direct child of its " +
                    "trigger-root event.",
                    nameof(metadata));
            }
        }

        private static void ValidateStage(
            CombatBattleStartStage stage)
        {
            if (stage < CombatBattleStartStage.Slot ||
                stage > CombatBattleStartStage.Card)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stage),
                    stage,
                    "Battle-start stage event requires " +
                    "Slot, Pet or Card stage.");
            }
        }
    }
}