using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetHpGainTriggerHandler :
        CombatPetEventTriggerHandler<
            HpGainCombatEvent>
    {
        protected CombatPetHpGainTriggerHandler(
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
                HpGainCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetHpGainContext(
                    state,
                    Side,
                    sourceEvent);

            return CanTriggerOnHpGain(
                context,
                pet);
        }

        protected sealed override void
            ResolvePetTrigger(
                CombatState state,
                HpGainCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetHpGainContext(
                    state,
                    Side,
                    sourceEvent);

            ResolveOnHpGain(
                context,
                pet);
        }

        protected abstract bool
            CanTriggerOnHpGain(
                CombatPetHpGainContext context,
                CombatPetState pet);

        protected abstract void
            ResolveOnHpGain(
                CombatPetHpGainContext context,
                CombatPetState pet);
    }
}