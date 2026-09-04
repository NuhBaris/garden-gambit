using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public abstract class
        CombatPetBattleStartTriggerHandler :
        CombatPetEventTriggerHandler<
            BattleStartStageStartedCombatEvent>
    {
        private readonly CombatSide
            _side;

        protected CombatPetBattleStartTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId)
            : base(
                side,
                petInstanceId)
        {
            _side = side;
        }

        protected CombatSide PetSide =>
            _side;

        protected CombatPetBattleStartContext
            CreateBattleStartContext(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent)
        {
            return new CombatPetBattleStartContext(
                state,
                _side,
                sourceEvent);
        }

        protected sealed override bool
            CanPetTrigger(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent,
                CombatPetState pet)
        {
            if (!sourceEvent.IsPetStage)
            {
                return false;
            }

            if (sourceEvent.HasBattleStartSnapshot)
            {
                var context =
                    CreateBattleStartContext(
                        state,
                        sourceEvent);

                return CanTriggerAtBattleStart(
                    context,
                    pet);
            }

            return CanTriggerAtBattleStart(
                state,
                sourceEvent,
                pet);
        }

        protected sealed override void
            ResolvePetTrigger(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent,
                CombatPetState pet)
        {
            if (!sourceEvent.IsPetStage)
            {
                throw new InvalidOperationException(
                    "A Pet battle-start trigger can only " +
                    "resolve from the Pet stage event.");
            }

            if (sourceEvent.HasBattleStartSnapshot)
            {
                var context =
                    CreateBattleStartContext(
                        state,
                        sourceEvent);

                ResolveAtBattleStart(
                    context,
                    pet);

                return;
            }

            ResolveAtBattleStart(
                state,
                sourceEvent,
                pet);
        }

        protected virtual bool
            CanTriggerAtBattleStart(
                CombatPetBattleStartContext context,
                CombatPetState pet)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            return CanTriggerAtBattleStart(
                context.State,
                context.SourceEvent,
                pet);
        }

        protected virtual void
            ResolveAtBattleStart(
                CombatPetBattleStartContext context,
                CombatPetState pet)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            ResolveAtBattleStart(
                context.State,
                context.SourceEvent,
                pet);
        }

        protected abstract bool
            CanTriggerAtBattleStart(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent,
                CombatPetState pet);

        protected abstract void
            ResolveAtBattleStart(
                CombatState state,
                BattleStartStageStartedCombatEvent
                    sourceEvent,
                CombatPetState pet);
    }
}