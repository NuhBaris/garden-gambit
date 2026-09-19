using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetDamageTriggerHandler :
        CombatPetEventTriggerHandler<
            DamageAppliedCombatEvent>
    {
        protected CombatPetDamageTriggerHandler(
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
                DamageAppliedCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetDamageContext(
                    state,
                    Side,
                    sourceEvent);

            return CanTriggerOnDamage(
                context,
                pet);
        }

        protected sealed override void
            ResolvePetTrigger(
                CombatState state,
                DamageAppliedCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetDamageContext(
                    state,
                    Side,
                    sourceEvent);

            ResolveOnDamage(
                context,
                pet);
        }

        protected abstract bool
            CanTriggerOnDamage(
                CombatPetDamageContext context,
                CombatPetState pet);

        protected abstract void
            ResolveOnDamage(
                CombatPetDamageContext context,
                CombatPetState pet);
    }
}
