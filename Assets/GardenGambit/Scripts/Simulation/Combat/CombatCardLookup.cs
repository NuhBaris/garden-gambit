using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatCardLookup
    {
        private readonly CombatEventLog
            _eventLog;

        public CombatCardLookup(
            CombatEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _eventLog = eventLog;
        }

        public CombatCardLookupResult Get(
            CombatState state,
            InstanceId instanceId)
        {
            CombatCardLookupResult result;

            if (TryGet(
                    state,
                    instanceId,
                    out result))
            {
                return result;
            }

            throw new KeyNotFoundException(
                $"Active card or tombstone was not " +
                $"found: {instanceId}.");
        }

        public bool TryGet(
            CombatState state,
            InstanceId instanceId,
            out CombatCardLookupResult result)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid card InstanceId is required.",
                    nameof(instanceId));
            }

            result = null;

            CombatCardState activeCard;
            BoardPosition activePosition;

            var hasActiveCard =
                TryFindActiveCard(
                    state,
                    instanceId,
                    out activeCard,
                    out activePosition);

            var hasTombstone =
                _eventLog.CardTombstones.Contains(
                    instanceId);

            if (hasActiveCard &&
                hasTombstone)
            {
                throw new InvalidOperationException(
                    $"Card {instanceId} cannot be both " +
                    "active and permanently removed.");
            }

            if (hasActiveCard)
            {
                result =
                    CombatCardLookupResult
                        .FromActiveCard(
                            activeCard,
                            activePosition);

                return true;
            }

            if (hasTombstone)
            {
                result =
                    CombatCardLookupResult
                        .FromTombstone(
                            _eventLog.CardTombstones.Get(
                                instanceId));

                return true;
            }

            return false;
        }

        private static bool TryFindActiveCard(
            CombatState state,
            InstanceId instanceId,
            out CombatCardState activeCard,
            out BoardPosition activePosition)
        {
            activeCard = null;
            activePosition =
                default(BoardPosition);

            CombatCardState playerCard;
            BoardPosition playerPosition;

            var foundOnPlayerSide =
                TryFindOnSide(
                    state.GetSide(
                        CombatSide.Player),
                    instanceId,
                    out playerCard,
                    out playerPosition);

            CombatCardState enemyCard;
            BoardPosition enemyPosition;

            var foundOnEnemySide =
                TryFindOnSide(
                    state.GetSide(
                        CombatSide.Enemy),
                    instanceId,
                    out enemyCard,
                    out enemyPosition);

            if (foundOnPlayerSide &&
                foundOnEnemySide)
            {
                throw new InvalidOperationException(
                    $"Card {instanceId} cannot occupy " +
                    "both combat sides.");
            }

            if (foundOnPlayerSide)
            {
                activeCard = playerCard;
                activePosition = playerPosition;

                return true;
            }

            if (foundOnEnemySide)
            {
                activeCard = enemyCard;
                activePosition = enemyPosition;

                return true;
            }

            return false;
        }

        private static bool TryFindOnSide(
            CombatSideState side,
            InstanceId instanceId,
            out CombatCardState card,
            out BoardPosition position)
        {
            card = null;
            position =
                default(BoardPosition);

            foreach (var slot in side.Board.Slots)
            {
                if (!slot.OccupantInstanceId.HasValue)
                {
                    continue;
                }

                if (slot.OccupantInstanceId.Value !=
                    instanceId)
                {
                    continue;
                }

                card =
                    side.Cards.GetCard(
                        instanceId);

                position =
                    slot.Position;

                return true;
            }

            return false;
        }
    }
}