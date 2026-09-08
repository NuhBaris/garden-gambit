using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class ArmorGainCombatEvent :
        CombatEvent
    {
        public ArmorGainCombatEvent(
            CombatEventMetadata metadata,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int previousArmor,
            int currentArmor)
            : base(
                metadata,
                CombatEventKind.ArmorGain)
        {
            ValidateMetadata(
                metadata);

            if (!targetInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Armor Gain event requires a valid " +
                    "target InstanceId.",
                    nameof(targetInstanceId));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "Armor Gain event requires a valid " +
                    "target board position.",
                    nameof(targetPosition));
            }

            if (previousArmor < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousArmor),
                    previousArmor,
                    "Previous Armor cannot be negative.");
            }

            if (currentArmor < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentArmor),
                    currentArmor,
                    "Current Armor cannot be negative.");
            }

            if (currentArmor <= previousArmor)
            {
                throw new ArgumentException(
                    "Armor Gain event requires an actual " +
                    "positive Armor increase.",
                    nameof(currentArmor));
            }

            TargetInstanceId =
                targetInstanceId;

            TargetPosition =
                targetPosition;

            PreviousArmor =
                previousArmor;

            CurrentArmor =
                currentArmor;
        }

        public InstanceId TargetInstanceId
        {
            get;
        }

        public BoardPosition TargetPosition
        {
            get;
        }

        public CombatSide TargetSide =>
            TargetPosition.Side;

        public int PreviousArmor
        {
            get;
        }

        public int CurrentArmor
        {
            get;
        }

        public int ActualGainedAmount =>
            CurrentArmor -
            PreviousArmor;

        public bool WasUnarmored =>
            PreviousArmor == 0;

        public bool IsArmored =>
            CurrentArmor > 0;

        private static void ValidateMetadata(
            CombatEventMetadata metadata)
        {
            if (!metadata.HasParent)
            {
                throw new ArgumentException(
                    "Armor Gain event must have a parent " +
                    "source event.",
                    nameof(metadata));
            }

            if (metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Armor Gain event cannot be a " +
                    "trigger-root event.",
                    nameof(metadata));
            }
        }
    }
}