using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class DeathCombatEvent :
        CombatEvent
    {
        public DeathCombatEvent(
            CombatEventMetadata metadata,
            InstanceId instanceId,
            BoardPosition position,
            int previousHp,
            int currentHp)
            : base(
                metadata,
                CombatEventKind.Death)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Death event requires a valid InstanceId.",
                    nameof(instanceId));
            }

            if (!position.IsValid)
            {
                throw new ArgumentException(
                    "Death event requires a valid board position.",
                    nameof(position));
            }

            if (previousHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousHp),
                    previousHp,
                    "Previous HP must be greater than zero.");
            }

            if (currentHp > 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHp),
                    currentHp,
                    "Current HP must be zero or below.");
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