using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        HazelDormousePetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int DamageReduction = 1;

        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        public HazelDormousePetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
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

            _usageCommitter =
                usageCommitter;

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
            return IsEligibleNutTarget(
                context,
                pet);
        }

        protected override void ResolveOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            if (!IsEligibleNutTarget(
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

        private bool IsEligibleNutTarget(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            var attackEvent =
                context.SourceEvent;

            if (attackEvent.TargetSide !=
                context.Side)
            {
                return false;
            }

            if (attackEvent.TargetPosition.Row !=
                context.GetAffectedRow(
                    pet))
            {
                return false;
            }

            if (_usageCommitter.HasTriggered(
                    pet.InstanceId,
                    attackEvent.TargetInstanceId))
            {
                return false;
            }

            var targetOccupiesEventPosition =
                false;

            foreach (var slot in
                     context.SideState.Board.Slots)
            {
                if (slot.Position !=
                    attackEvent.TargetPosition)
                {
                    continue;
                }

                targetOccupiesEventPosition =
                    slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value ==
                    attackEvent.TargetInstanceId;

                break;
            }

            if (!targetOccupiesEventPosition)
            {
                return false;
            }

            foreach (var card in
                     context.SideState.Cards.Cards)
            {
                if (card.InstanceId ==
                    attackEvent.TargetInstanceId)
                {
                    return card.IsNut;
                }
            }

            return false;
        }
    }
}
