using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class HpGainCombatEvent :
        CombatEvent
    {
        public HpGainCombatEvent(
            CombatEventMetadata metadata,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int previousHpCapacity,
            int currentHpCapacity,
            int previousHp,
            int currentHp)
            : this(
                metadata,
                targetInstanceId,
                targetInstanceId,
                targetPosition,
                previousHpCapacity,
                currentHpCapacity,
                previousHp,
                currentHp)
        {
        }

        public HpGainCombatEvent(
            CombatEventMetadata metadata,
            InstanceId sourceInstanceId,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int previousHpCapacity,
            int currentHpCapacity,
            int previousHp,
            int currentHp)
            : base(
                metadata,
                CombatEventKind.HpGain)
        {
            ValidateMetadata(
                metadata);

            if (!sourceInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "HP Gain event requires a valid " +
                    "source InstanceId.",
                    nameof(sourceInstanceId));
            }

            if (!targetInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "HP Gain event requires a valid " +
                    "target InstanceId.",
                    nameof(targetInstanceId));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "HP Gain event requires a valid " +
                    "target board position.",
                    nameof(targetPosition));
            }

            if (previousHpCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousHpCapacity),
                    previousHpCapacity,
                    "Previous HP Capacity must be " +
                    "greater than zero.");
            }

            if (currentHpCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHpCapacity),
                    currentHpCapacity,
                    "Current HP Capacity must be " +
                    "greater than zero.");
            }

            if (previousHp >
                previousHpCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousHp),
                    previousHp,
                    "Previous HP cannot exceed previous " +
                    "HP Capacity.");
            }

            if (currentHp >
                currentHpCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHp),
                    currentHp,
                    "Current HP cannot exceed current " +
                    "HP Capacity.");
            }

            var hpGain =
                (long)currentHp -
                previousHp;

            if (hpGain <= 0L)
            {
                throw new ArgumentException(
                    "HP Gain event requires an actual " +
                    "positive HP increase.",
                    nameof(currentHp));
            }

            if (hpGain > int.MaxValue)
            {
                throw new OverflowException(
                    "Actual HP Gain exceeds Int32 " +
                    "maximum value.");
            }

            var capacityGain =
                (long)currentHpCapacity -
                previousHpCapacity;

            if (capacityGain < 0L)
            {
                throw new ArgumentException(
                    "HP Gain event cannot reduce HP " +
                    "Capacity.",
                    nameof(currentHpCapacity));
            }

            if (capacityGain != 0L &&
                capacityGain != hpGain)
            {
                throw new ArgumentException(
                    "HP stat gain must increase HP " +
                    "Capacity and current HP by the " +
                    "same amount.",
                    nameof(currentHpCapacity));
            }

            SourceInstanceId =
                sourceInstanceId;

            TargetInstanceId =
                targetInstanceId;

            TargetPosition =
                targetPosition;

            PreviousHpCapacity =
                previousHpCapacity;

            CurrentHpCapacity =
                currentHpCapacity;

            PreviousHp =
                previousHp;

            CurrentHp =
                currentHp;
        }

        public InstanceId SourceInstanceId
        {
            get;
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

        public int PreviousHpCapacity
        {
            get;
        }

        public int CurrentHpCapacity
        {
            get;
        }

        public int PreviousHp
        {
            get;
        }

        public int CurrentHp
        {
            get;
        }

        public int ActualGainedAmount =>
            CurrentHp -
            PreviousHp;

        public int CapacityGainedAmount =>
            CurrentHpCapacity -
            PreviousHpCapacity;

        public bool IsHpStatGain =>
            CapacityGainedAmount > 0;

        public bool IsHeal =>
            CapacityGainedAmount == 0;

        public bool IsSelfSource =>
            SourceInstanceId ==
            TargetInstanceId;

        public bool IsFromAnotherSource =>
            SourceInstanceId !=
            TargetInstanceId;

        public bool WasAtDeathThreshold =>
            PreviousHp <= 0;

        public bool IsAtDeathThreshold =>
            CurrentHp <= 0;

        public bool RestoredAboveDeathThreshold =>
            WasAtDeathThreshold &&
            !IsAtDeathThreshold;

        private static void ValidateMetadata(
            CombatEventMetadata metadata)
        {
            if (!metadata.HasParent)
            {
                throw new ArgumentException(
                    "HP Gain event must have a parent " +
                    "source event.",
                    nameof(metadata));
            }

            if (metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "HP Gain event cannot be a " +
                    "trigger-root event.",
                    nameof(metadata));
            }
        }
    }
}