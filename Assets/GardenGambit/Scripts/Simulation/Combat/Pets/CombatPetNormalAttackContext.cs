using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetNormalAttackContext
    {
        public CombatPetNormalAttackContext(
            CombatState state,
            CombatSide side,
            NormalAttackCombatEvent sourceEvent)
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
                    "Pet Normal Attack context requires " +
                    "Player or Enemy side.");
            }

            if (sourceEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceEvent));
            }

            State =
                state;

            Side =
                side;

            SourceEvent =
                sourceEvent;

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

        public NormalAttackCombatEvent SourceEvent
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