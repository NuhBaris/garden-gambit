using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class BadgerPetNormalAttackTriggerHandler : CombatPetNormalAttackTriggerHandler
    {
        public const int DamageReduction = 1;
        public const int MinimumEligibleRank = 7;
        public const int MaximumEligibleRank = 10;

        private readonly CombatPetCardTriggerUsageCommitter _usageCommitter;
        private readonly CombatNormalAttackTargetDamageReductionRegistry _targetDamageReductionRegistry;

        public BadgerPetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry targetDamageReductionRegistry)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }
            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(nameof(targetDamageReductionRegistry));
            }
            _usageCommitter = usageCommitter;
            _targetDamageReductionRegistry = targetDamageReductionRegistry;
        }

        public CombatPetCardTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatNormalAttackTargetDamageReductionRegistry TargetDamageReductionRegistry =>
            _targetDamageReductionRegistry;

        protected override bool CanTriggerOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            return IsEligible(context, pet);
        }

        protected override void ResolveOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            if (!IsEligible(context, pet))
            {
                return;
            }

            _targetDamageReductionRegistry.TryRegister(
                new CombatNormalAttackTargetDamageReductionRequest(
                    context.SourceEvent.Metadata.EventId,
                    pet.InstanceId,
                    context.SourceEvent.TargetInstanceId,
                    DamageReduction));
        }

        private bool IsEligible(CombatPetNormalAttackContext context, CombatPetState pet)
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
                if (slot.Position != attack.TargetPosition)
                {
                    continue;
                }
                if (!slot.OccupantInstanceId.HasValue ||
                    slot.OccupantInstanceId.Value != attack.TargetInstanceId)
                {
                    return false;
                }

                var rank = context.SideState.GetCardAt(slot.Position).Rank.Value;
                return rank >= MinimumEligibleRank && rank <= MaximumEligibleRank;
            }

            return false;
        }
    }
}
