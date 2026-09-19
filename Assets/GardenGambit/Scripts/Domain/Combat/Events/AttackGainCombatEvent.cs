using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class AttackGainCombatEvent :
        CombatEvent
    {
        public AttackGainCombatEvent(
            CombatEventMetadata metadata,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int previousAttack,
            int currentAttack)
            : base(
                metadata,
                CombatEventKind.AttackGain)
        {
            ValidateMetadata(metadata);

            if (!targetInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Attack Gain event requires a valid " +
                    "target InstanceId.",
                    nameof(targetInstanceId));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "Attack Gain event requires a valid " +
                    "target board position.",
                    nameof(targetPosition));
            }

            if (previousAttack < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousAttack),
                    previousAttack,
                    "Previous Attack cannot be negative.");
            }

            if (currentAttack < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentAttack),
                    currentAttack,
                    "Current Attack cannot be negative.");
            }

            if (currentAttack <= previousAttack)
            {
                throw new ArgumentException(
                    "Attack Gain event requires an actual " +
                    "positive Attack increase.",
                    nameof(currentAttack));
            }

            TargetInstanceId = targetInstanceId;
            TargetPosition = targetPosition;
            PreviousAttack = previousAttack;
            CurrentAttack = currentAttack;
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

        public int PreviousAttack
        {
            get;
        }

        public int CurrentAttack
        {
            get;
        }

        public int ActualGainedAmount =>
            CurrentAttack - PreviousAttack;

        private static void ValidateMetadata(
            CombatEventMetadata metadata)
        {
            if (!metadata.HasParent)
            {
                throw new ArgumentException(
                    "Attack Gain event must have a parent " +
                    "source event.",
                    nameof(metadata));
            }

            if (metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Attack Gain event cannot be a " +
                    "trigger-root event.",
                    nameof(metadata));
            }
        }
    }
}