using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetBattleStartContext
    {
        public CombatPetBattleStartContext(
            CombatState state,
            CombatSide side,
            BattleStartStageStartedCombatEvent
                sourceEvent)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Pet battle-start context requires " +
                    "Player or Enemy side.");
            }

            if (sourceEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceEvent));
            }

            if (!sourceEvent.IsPetStage)
            {
                throw new ArgumentException(
                    "Pet battle-start context requires " +
                    "a Pet stage event.",
                    nameof(sourceEvent));
            }

            if (!sourceEvent.HasBattleStartSnapshot)
            {
                throw new InvalidOperationException(
                    "Pet battle-start context requires " +
                    "a battle-start snapshot.");
            }

            State = state;
            Side = side;
            SourceEvent = sourceEvent;

            BattleStartSnapshot =
                sourceEvent.BattleStartSnapshot;

            SideSnapshot =
                BattleStartSnapshot.GetSide(
                    side);

            OpposingSideSnapshot =
                BattleStartSnapshot.GetOpposingSide(
                    side);

            SideState =
                state.GetSide(
                    side);

            OpposingSideState =
                state.GetOpposingSide(
                    side);
        }

        public CombatState State
        {
            get;
        }

        public CombatSide Side
        {
            get;
        }

        public BattleStartStageStartedCombatEvent
            SourceEvent
        {
            get;
        }

        public CombatBattleStartSnapshot
            BattleStartSnapshot
        {
            get;
        }

        public CombatBattleStartSideSnapshot
            SideSnapshot
        {
            get;
        }

        public CombatBattleStartSideSnapshot
            OpposingSideSnapshot
        {
            get;
        }

        public CombatSideState SideState
        {
            get;
        }

        public CombatSideState OpposingSideState
        {
            get;
        }
    }
}