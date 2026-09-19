using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatPetDeathContext
    {
        public CombatPetDeathContext(
            CombatState state,
            CombatSide side,
            DeathCombatEvent sourceEvent)
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
                    "Pet Death context requires " +
                    "Player or Enemy side.");
            }

            if (sourceEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceEvent));
            }

            State = state;
            Side = side;
            SourceEvent = sourceEvent;

            SideState = state.GetSide(side);
            OpposingSideState = state.GetOpposingSide(side);
            SidePetState = state.GetPets(side);
        }

        public CombatState State
        {
            get;
        }

        public CombatSide Side
        {
            get;
        }

        public DeathCombatEvent SourceEvent
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

        public CombatSidePetState SidePetState
        {
            get;
        }

        public BoardRow GetAffectedRow(
            CombatPetState pet)
        {
            if (pet == null)
            {
                throw new ArgumentNullException(
                    nameof(pet));
            }

            return SidePetState.GetAffectedRow(
                pet.InstanceId);
        }
    }
}