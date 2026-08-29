using System;

namespace GardenGambit.Domain.Combat
{
    public abstract class CombatEvent
    {
        protected CombatEvent(
            CombatEventMetadata metadata,
            CombatEventKind kind)
        {
            if (!metadata.IsValid)
            {
                throw new ArgumentException(
                    "Combat event requires valid metadata.",
                    nameof(metadata));
            }

            if (kind == CombatEventKind.Unspecified ||
                !Enum.IsDefined(
                    typeof(CombatEventKind),
                    kind))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    kind,
                    "Combat event requires a defined kind.");
            }

            Metadata = metadata;
            Kind = kind;
        }

        public CombatEventMetadata Metadata { get; }

        public CombatEventKind Kind { get; }
    }
}