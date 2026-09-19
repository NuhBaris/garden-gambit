using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class CombatPetDeathTriggerHandler :
        CombatPetEventTriggerHandler<DeathCombatEvent>
    {
        protected CombatPetDeathTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId)
            : base(
                side,
                petInstanceId)
        {
        }

        protected sealed override bool CanPetTrigger(
            CombatState state,
            DeathCombatEvent sourceEvent,
            CombatPetState pet)
        {
            var context = new CombatPetDeathContext(
                state,
                Side,
                sourceEvent);

            return CanTriggerOnDeath(
                context,
                pet);
        }

        protected sealed override void ResolvePetTrigger(
            CombatState state,
            DeathCombatEvent sourceEvent,
            CombatPetState pet)
        {
            var context = new CombatPetDeathContext(
                state,
                Side,
                sourceEvent);

            ResolveOnDeath(
                context,
                pet);
        }

        protected abstract bool CanTriggerOnDeath(
            CombatPetDeathContext context,
            CombatPetState pet);

        protected abstract void ResolveOnDeath(
            CombatPetDeathContext context,
            CombatPetState pet);
    }
}