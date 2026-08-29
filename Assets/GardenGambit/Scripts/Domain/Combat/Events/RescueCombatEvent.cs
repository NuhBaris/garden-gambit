using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class RescueCombatEvent :
        CombatEvent
    {
        public RescueCombatEvent(
            CombatEventMetadata metadata,
            InstanceId instanceId,
            BoardPosition position,
            int previousHp,
            int currentHp)
            : base(
                metadata,
                CombatEventKind.Rescue)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Rescue event requires a valid InstanceId.",
                    nameof(instanceId));
            }

            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "Rescue event requires a valid " +
                    "board position.",
                    nameof(position));
            }

            if (previousHp > 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousHp),
                    previousHp,
                    "Previous HP must be zero or below.");
            }

            if (currentHp != 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHp),
                    currentHp,
                    "Rescue must set current HP exactly to one.");
            }

            InstanceId = instanceId;
            Position = position;
            PreviousHp = previousHp;
            CurrentHp = currentHp;
        }

        public InstanceId InstanceId { get; }

        public BoardPosition Position { get; }

        public int PreviousHp { get; }

        public int CurrentHp { get; }
    }
}