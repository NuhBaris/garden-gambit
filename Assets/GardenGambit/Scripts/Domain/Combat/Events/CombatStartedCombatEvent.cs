using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatStartedCombatEvent :
        CombatEvent
    {
        public CombatStartedCombatEvent(
            CombatEventMetadata metadata)
            : base(
                metadata,
                CombatEventKind.CombatStarted)
        {
            if (!metadata.IsTriggerRoot)
            {
                throw new ArgumentException(
                    "Combat Started must be a root event.",
                    nameof(metadata));
            }
        }
    }
}