using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class DirectDeleteCombatEvent :
        CombatEvent
    {
        public DirectDeleteCombatEvent(
            CombatEventMetadata metadata,
            InstanceId instanceId,
            BoardPosition position,
            int hpAtDeletion)
            : base(
                metadata,
                CombatEventKind.DirectDelete)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Direct Delete event requires a valid " +
                    "InstanceId.",
                    nameof(instanceId));
            }

            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "Direct Delete event requires a valid " +
                    "board position.",
                    nameof(position));
            }

            InstanceId = instanceId;
            Position = position;
            HpAtDeletion = hpAtDeletion;
        }

        public InstanceId InstanceId { get; }

        public BoardPosition Position { get; }

        public int HpAtDeletion { get; }
    }
}