using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetNormalAttackTriggerHandler :
        CombatPetEventTriggerHandler<
            NormalAttackCombatEvent>
    {
        protected CombatPetNormalAttackTriggerHandler(
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
                NormalAttackCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetNormalAttackContext(
                    state,
                    Side,
                    sourceEvent);

            return CanTriggerOnNormalAttack(
                context,
                pet);
        }

        protected sealed override void
            ResolvePetTrigger(
                CombatState state,
                NormalAttackCombatEvent sourceEvent,
                CombatPetState pet)
        {
            var context =
                new CombatPetNormalAttackContext(
                    state,
                    Side,
                    sourceEvent);

            ResolveOnNormalAttack(
                context,
                pet);
        }

        protected virtual bool
            CanTriggerOnNormalAttack(
                CombatPetNormalAttackContext context,
                CombatPetState pet)
        {
            return CanTriggerOnNormalAttack(
                context.State,
                context.SourceEvent,
                pet);
        }

        protected virtual void
            ResolveOnNormalAttack(
                CombatPetNormalAttackContext context,
                CombatPetState pet)
        {
            ResolveOnNormalAttack(
                context.State,
                context.SourceEvent,
                pet);
        }

        protected virtual bool
            CanTriggerOnNormalAttack(
                CombatState state,
                NormalAttackCombatEvent sourceEvent,
                CombatPetState pet)
        {
            return false;
        }

        protected virtual void
            ResolveOnNormalAttack(
                CombatState state,
                NormalAttackCombatEvent sourceEvent,
                CombatPetState pet)
        {
            throw new InvalidOperationException(
                "Pet Normal Attack handler accepted the " +
                "trigger but did not implement a resolve " +
                "path.");
        }
    }
}