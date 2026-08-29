using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class DamageAppliedCombatEvent :
        CombatEvent
    {
        public DamageAppliedCombatEvent(
            CombatEventMetadata metadata,
            InstanceId sourceInstanceId,
            BoardPosition sourcePosition,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            DamageApplicationResult result)
            : base(
                metadata,
                CombatEventKind.DamageApplied)
        {
            if (!sourceInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Damage event requires a valid " +
                    "source InstanceId.",
                    nameof(sourceInstanceId));
            }

            if (!sourcePosition.IsValid)
            {
                throw new ArgumentException(
                    "Damage event requires a valid " +
                    "source board position.",
                    nameof(sourcePosition));
            }

            if (!targetInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Damage event requires a valid " +
                    "target InstanceId.",
                    nameof(targetInstanceId));
            }

            if (!targetPosition.IsValid)
            {
                throw new ArgumentException(
                    "Damage event requires a valid " +
                    "target board position.",
                    nameof(targetPosition));
            }

            if (!result.IsValid)
            {
                throw new ArgumentException(
                    "Damage event requires a valid " +
                    "DamageApplicationResult.",
                    nameof(result));
            }

            SourceInstanceId = sourceInstanceId;
            SourcePosition = sourcePosition;
            TargetInstanceId = targetInstanceId;
            TargetPosition = targetPosition;
            Result = result;
        }

        public InstanceId SourceInstanceId { get; }

        public BoardPosition SourcePosition { get; }

        public InstanceId TargetInstanceId { get; }

        public BoardPosition TargetPosition { get; }

        public DamageApplicationResult Result { get; }
    }
}