using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetRescueTriggerHandler :
        CombatPetEventTriggerHandler<
            RescueCombatEvent>
    {
        protected CombatPetRescueTriggerHandler(
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
                RescueCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetRescueContext(
                    state,
                    Side,
                    sourceEvent);

            return CanTriggerOnRescue(
                context,
                pet);
        }

        protected sealed override void
            ResolvePetTrigger(
                CombatState state,
                RescueCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetRescueContext(
                    state,
                    Side,
                    sourceEvent);

            ResolveOnRescue(
                context,
                pet);
        }

        protected abstract bool
            CanTriggerOnRescue(
                CombatPetRescueContext context,
                CombatPetState pet);

        protected abstract void
            ResolveOnRescue(
                CombatPetRescueContext context,
                CombatPetState pet);
    }
}
