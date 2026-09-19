using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class KoalaPetNormalAttackTriggerHandler : CombatPetNormalAttackTriggerHandler
    {
        public const int DamageReduction = 1;
        private readonly CombatPetCardTriggerUsageCommitter _usageCommitter;
        private readonly CombatNormalAttackTargetDamageReductionRegistry _targetDamageReductionRegistry;

        public KoalaPetNormalAttackTriggerHandler(
            CombatSide side, InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry targetDamageReductionRegistry)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null) { throw new ArgumentNullException(nameof(usageCommitter)); }
            if (targetDamageReductionRegistry == null) { throw new ArgumentNullException(nameof(targetDamageReductionRegistry)); }
            _usageCommitter = usageCommitter;
            _targetDamageReductionRegistry = targetDamageReductionRegistry;
        }

        public CombatPetCardTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatNormalAttackTargetDamageReductionRegistry TargetDamageReductionRegistry => _targetDamageReductionRegistry;

        protected override bool CanTriggerOnNormalAttack(CombatPetNormalAttackContext context, CombatPetState pet)
        {
            var attack = context.SourceEvent;
            if (attack.TargetSide != context.Side ||
                attack.TargetPosition.Row != context.GetAffectedRow(pet) ||
                _usageCommitter.HasTriggered(pet.InstanceId, attack.TargetInstanceId))
            {
                return false;
            }

            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position == attack.TargetPosition)
                {
                    return slot.HasEnhance && slot.OccupantInstanceId.HasValue &&
                        slot.OccupantInstanceId.Value == attack.TargetInstanceId;
                }
            }
            return false;
        }

        protected override void ResolveOnNormalAttack(CombatPetNormalAttackContext context, CombatPetState pet)
        {
            if (!CanTriggerOnNormalAttack(context, pet)) { return; }
            // As with Polar Ferret, discovery only registers a request.
            // The existing damage-reduction resolver commits usage when an
            // actual positive reduction is applied, after earlier modifiers.
            _targetDamageReductionRegistry.TryRegister(new CombatNormalAttackTargetDamageReductionRequest(
                context.SourceEvent.Metadata.EventId, pet.InstanceId,
                context.SourceEvent.TargetInstanceId, DamageReduction));
        }
    }
}
