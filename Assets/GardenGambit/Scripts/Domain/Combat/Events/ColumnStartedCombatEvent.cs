using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class ColumnStartedCombatEvent :
        CombatEvent
    {
        public ColumnStartedCombatEvent(
            CombatEventMetadata metadata,
            BoardColumn column)
            : base(
                metadata,
                CombatEventKind.ColumnStarted)
        {
            if (!metadata.HasParent)
            {
                throw new ArgumentException(
                    "Column Started must reference " +
                    "a parent Combat Started event.",
                    nameof(metadata));
            }

            if (!column.IsValid)
            {
                throw new ArgumentException(
                    "Column Started requires a valid " +
                    "board column.",
                    nameof(column));
            }

            Column = column;
        }

        public BoardColumn Column
        {
            get;
        }
    }
}