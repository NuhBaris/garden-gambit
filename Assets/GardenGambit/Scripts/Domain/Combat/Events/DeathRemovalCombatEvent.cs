using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class DeathRemovalCombatEvent :
        CombatEvent
    {
        public DeathRemovalCombatEvent(
            CombatEventMetadata metadata,
            InstanceId instanceId,
            BoardPosition position,
            int hpAtRemoval)
            : base(
                metadata,
                CombatEventKind.DeathRemoval)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Death Removal event requires a valid " +
                    "InstanceId.",
                    nameof(instanceId));
            }

            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "Death Removal event requires a valid " +
                    "board position.",
                    nameof(position));
            }

            if (hpAtRemoval > 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hpAtRemoval),
                    hpAtRemoval,
                    "HP at Death Removal must be " +
                    "zero or below.");
            }

            InstanceId = instanceId;
            Position = position;
            HpAtRemoval = hpAtRemoval;
        }

        public InstanceId InstanceId { get; }

        public BoardPosition Position { get; }

        public int HpAtRemoval { get; }
    }
}