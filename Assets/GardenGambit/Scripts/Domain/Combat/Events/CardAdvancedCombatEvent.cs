using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CardAdvancedCombatEvent :
        CombatEvent
    {
        public CardAdvancedCombatEvent(
            CombatEventMetadata metadata,
            InstanceId instanceId,
            BoardPosition sourcePosition,
            BoardPosition destinationPosition)
            : base(
                metadata,
                CombatEventKind.CardAdvanced)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "Card Advanced event requires a valid " +
                    "InstanceId.",
                    nameof(instanceId));
            }

            if (!sourcePosition.IsValid)
            {
                throw new ArgumentException(
                    "Card Advanced event requires a valid " +
                    "source position.",
                    nameof(sourcePosition));
            }

            if (!destinationPosition.IsValid)
            {
                throw new ArgumentException(
                    "Card Advanced event requires a valid " +
                    "destination position.",
                    nameof(destinationPosition));
            }

            if (sourcePosition.Side !=
                destinationPosition.Side)
            {
                throw new ArgumentException(
                    "Card advancement must remain on the " +
                    "same combat side.",
                    nameof(destinationPosition));
            }

            if (sourcePosition.Column !=
                destinationPosition.Column)
            {
                throw new ArgumentException(
                    "Card advancement must remain in the " +
                    "same board column.",
                    nameof(destinationPosition));
            }

            if (sourcePosition.Row !=
                BoardRow.Back)
            {
                throw new ArgumentException(
                    "Card advancement must start from " +
                    "the Back row.",
                    nameof(sourcePosition));
            }

            if (destinationPosition.Row !=
                BoardRow.Front)
            {
                throw new ArgumentException(
                    "Card advancement must end in " +
                    "the Front row.",
                    nameof(destinationPosition));
            }

            InstanceId = instanceId;
            SourcePosition = sourcePosition;
            DestinationPosition = destinationPosition;
        }

        public InstanceId InstanceId { get; }

        public BoardPosition SourcePosition { get; }

        public BoardPosition DestinationPosition { get; }
    }
}