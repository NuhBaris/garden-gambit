using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatSideAltarSlotOrderResolver
    {
        public IReadOnlyList<BoardPosition> Resolve(
            CombatSideState sideState)
        {
            if (sideState == null)
            {
                throw new ArgumentNullException(
                    nameof(sideState));
            }

            var altarPositions =
                new List<BoardPosition>();

            foreach (var slot in
                     sideState.Board.Slots)
            {
                if (!slot.HasSacrificialAltar &&
                    !slot.HasWarAltar)
                {
                    continue;
                }

                altarPositions.Add(
                    slot.Position);
            }

            altarPositions.Sort(
                ComparePositions);

            return altarPositions.AsReadOnly();
        }

        private static int ComparePositions(
            BoardPosition left,
            BoardPosition right)
        {
            var columnComparison =
                left.Column.Value.CompareTo(
                    right.Column.Value);

            if (columnComparison != 0)
            {
                return columnComparison;
            }

            return GetRowOrder(
                    left.Row)
                .CompareTo(
                    GetRowOrder(
                        right.Row));
        }

        private static int GetRowOrder(
            BoardRow row)
        {
            if (row == BoardRow.Front)
            {
                return 0;
            }

            if (row == BoardRow.Back)
            {
                return 1;
            }

            throw new ArgumentOutOfRangeException(
                nameof(row),
                row,
                "Altar Slot ordering requires " +
                "Front or Back row.");
        }
    }
}