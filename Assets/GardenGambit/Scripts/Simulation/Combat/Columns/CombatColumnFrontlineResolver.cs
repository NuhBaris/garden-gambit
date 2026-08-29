using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatColumnFrontlineResolver
    {
        public bool TryGetExchangePositions(
            CombatState state,
            BoardColumn column,
            out BoardPosition playerPosition,
            out BoardPosition enemyPosition)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (!column.IsValid)
            {
                throw new ArgumentException(
                    "A valid board column is required.",
                    nameof(column));
            }

            playerPosition =
                default(BoardPosition);

            enemyPosition =
                default(BoardPosition);

            var candidatePlayerPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    column);

            var candidateEnemyPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    column);

            var playerSide =
                state.GetSide(
                    CombatSide.Player);

            var enemySide =
                state.GetSide(
                    CombatSide.Enemy);

            if (!IsOccupiedAt(
                    playerSide.Board,
                    candidatePlayerPosition))
            {
                return false;
            }

            if (!IsOccupiedAt(
                    enemySide.Board,
                    candidateEnemyPosition))
            {
                return false;
            }

            var playerCard =
                playerSide.GetCardAt(
                    candidatePlayerPosition);

            var enemyCard =
                enemySide.GetCardAt(
                    candidateEnemyPosition);

            if (playerCard.IsAtDeathThreshold)
            {
                throw new InvalidOperationException(
                    "A Player front card at the death " +
                    "threshold cannot begin another " +
                    "normal attack exchange.");
            }

            if (enemyCard.IsAtDeathThreshold)
            {
                throw new InvalidOperationException(
                    "An Enemy front card at the death " +
                    "threshold cannot begin another " +
                    "normal attack exchange.");
            }

            playerPosition =
                candidatePlayerPosition;

            enemyPosition =
                candidateEnemyPosition;

            return true;
        }

        private static bool IsOccupiedAt(
            CombatBoardState board,
            BoardPosition position)
        {
            for (var index = 0;
                 index < board.SlotCount;
                 index++)
            {
                var slot =
                    board.Slots[index];

                if (slot.Position != position)
                {
                    continue;
                }

                return slot.IsOccupied;
            }

            return false;
        }
    }
}