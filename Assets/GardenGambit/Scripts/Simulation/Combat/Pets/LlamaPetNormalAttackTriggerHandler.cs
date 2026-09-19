using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        LlamaPetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int DamageReduction = 1;

        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        public LlamaPetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
            : base(
                side,
                petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageReductionRegistry));
            }

            _usageCommitter = usageCommitter;
            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                _targetDamageReductionRegistry;

        protected override bool CanTriggerOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            return IsEligibleRepeatedRankTarget(
                context,
                pet);
        }

        protected override void ResolveOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            if (!IsEligibleRepeatedRankTarget(
                    context,
                    pet))
            {
                return;
            }

            _targetDamageReductionRegistry.TryRegister(
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        context.SourceEvent
                            .Metadata.EventId,
                        pet.InstanceId,
                        context.SourceEvent
                            .TargetInstanceId,
                        DamageReduction));
        }

        private bool IsEligibleRepeatedRankTarget(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            var attack = context.SourceEvent;
            var affectedRow = context.GetAffectedRow(pet);

            if (attack.TargetSide != context.Side ||
                attack.TargetPosition.Row != affectedRow ||
                _usageCommitter.HasTriggered(
                    pet.InstanceId,
                    attack.TargetInstanceId))
            {
                return false;
            }

            CombatCardState target = null;

            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position != attack.TargetPosition)
                {
                    continue;
                }

                if (!slot.OccupantInstanceId.HasValue ||
                    slot.OccupantInstanceId.Value !=
                    attack.TargetInstanceId)
                {
                    return false;
                }

                target = context.SideState.GetCardAt(
                    slot.Position);

                break;
            }

            if (target == null ||
                target.IsAtDeathThreshold)
            {
                return false;
            }

            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position.Row != affectedRow ||
                    !slot.OccupantInstanceId.HasValue ||
                    slot.OccupantInstanceId.Value ==
                    target.InstanceId)
                {
                    continue;
                }

                var card = context.SideState.GetCardAt(
                    slot.Position);

                if (!card.IsAtDeathThreshold &&
                    card.Rank == target.Rank)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
