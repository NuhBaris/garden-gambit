using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class ArmorRemovedCombatEvent : CombatEvent
    {
        public ArmorRemovedCombatEvent(
            CombatEventMetadata metadata,
            InstanceId sourceInstanceId,
            InstanceId targetInstanceId,
            BoardPosition targetPosition,
            int previousArmor,
            int currentArmor)
            : base(metadata, CombatEventKind.ArmorRemoved)
        {
            if (!metadata.HasParent || metadata.IsTriggerRoot)
            {
                throw new ArgumentException("Armor removal event requires a parent source event.", nameof(metadata));
            }
            if (!sourceInstanceId.IsValid)
            {
                throw new ArgumentException("A valid source InstanceId is required.", nameof(sourceInstanceId));
            }
            if (!targetInstanceId.IsValid)
            {
                throw new ArgumentException("A valid target InstanceId is required.", nameof(targetInstanceId));
            }
            if (!targetPosition.IsValid)
            {
                throw new ArgumentException("A valid target board position is required.", nameof(targetPosition));
            }
            if (previousArmor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(previousArmor));
            }
            if (currentArmor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentArmor));
            }
            if (currentArmor >= previousArmor)
            {
                throw new ArgumentException("Armor removal event requires an actual Armor decrease.", nameof(currentArmor));
            }

            SourceInstanceId = sourceInstanceId;
            TargetInstanceId = targetInstanceId;
            TargetPosition = targetPosition;
            PreviousArmor = previousArmor;
            CurrentArmor = currentArmor;
        }

        public InstanceId SourceInstanceId { get; }
        public InstanceId TargetInstanceId { get; }
        public BoardPosition TargetPosition { get; }
        public CombatSide TargetSide => TargetPosition.Side;
        public int PreviousArmor { get; }
        public int CurrentArmor { get; }
        public int ActualRemovedAmount => PreviousArmor - CurrentArmor;
        public bool DepletedArmor => CurrentArmor == 0;
    }
}
