using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatBattleStartSnapshotResolver
    {
        public CombatBattleStartSnapshot Resolve(
            CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            var playerSnapshot =
                CreateSideSnapshot(
                    state.Player);

            var enemySnapshot =
                CreateSideSnapshot(
                    state.Enemy);

            return new CombatBattleStartSnapshot(
                playerSnapshot,
                enemySnapshot);
        }

        private static
            CombatBattleStartSideSnapshot
            CreateSideSnapshot(
                CombatSideState sideState)
        {
            var occupiedSlots =
                new List<CombatSlotState>();

            foreach (var slot in
                     sideState.Board.Slots)
            {
                if (!slot.OccupantInstanceId
                        .HasValue)
                {
                    continue;
                }

                occupiedSlots.Add(
                    slot);
            }

            occupiedSlots.Sort(
                CompareSlots);

            var cardSnapshots =
                new List<
                    CombatBattleStartCardSnapshot>();

            foreach (var slot in
                     occupiedSlots)
            {
                var card =
                    sideState.Cards.GetCard(
                        slot.OccupantInstanceId
                            .Value);

                cardSnapshots.Add(
                    new CombatBattleStartCardSnapshot(
                        card,
                        slot.Position));
            }

            return new CombatBattleStartSideSnapshot(
                sideState.Side,
                cardSnapshots);
        }

        private static int CompareSlots(
            CombatSlotState left,
            CombatSlotState right)
        {
            var columnComparison =
                left.Position.Column.Value
                    .CompareTo(
                        right.Position.Column.Value);

            if (columnComparison != 0)
            {
                return columnComparison;
            }

            return GetRowOrder(
                    left.Position.Row)
                .CompareTo(
                    GetRowOrder(
                        right.Position.Row));
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
                "Battle-start snapshot requires " +
                "Front or Back row.");
        }
    }
}