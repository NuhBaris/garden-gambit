using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetBattleEndTriggerHandler :
        CombatPetEventTriggerHandler<
            BattleEndStartedCombatEvent>
    {
        protected CombatPetBattleEndTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId)
            : base(
                side,
                petInstanceId)
        {
        }

        protected sealed override bool
            CanPetTrigger(
                CombatState state,
                BattleEndStartedCombatEvent
                    sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetBattleEndContext(
                    state,
                    Side,
                    sourceEvent);

            return CanTriggerAtBattleEnd(
                context,
                pet);
        }

        protected sealed override void
            ResolvePetTrigger(
                CombatState state,
                BattleEndStartedCombatEvent
                    sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetBattleEndContext(
                    state,
                    Side,
                    sourceEvent);

            ResolveAtBattleEnd(
                context,
                pet);
        }

        protected abstract bool
            CanTriggerAtBattleEnd(
                CombatPetBattleEndContext context,
                CombatPetState pet);

        protected abstract void
            ResolveAtBattleEnd(
                CombatPetBattleEndContext context,
                CombatPetState pet);
    }
}