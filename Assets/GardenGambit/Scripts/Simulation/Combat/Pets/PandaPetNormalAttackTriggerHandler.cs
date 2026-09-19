using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        PandaPetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int DamageReduction = 1;

        public const int MaximumReductionCount = 2;

        private readonly CombatPetTriggerUsageCommitter
            _activationUsageCommitter;

        private readonly CombatPetLimitedTriggerUsageCommitter
            _limitedUsageCommitter;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        public PandaPetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter
                activationUsageCommitter,
            CombatPetLimitedTriggerUsageCommitter
                limitedUsageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
            : base(
                side,
                petInstanceId)
        {
            if (activationUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(activationUsageCommitter));
            }

            if (limitedUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(limitedUsageCommitter));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageReductionRegistry));
            }

            _activationUsageCommitter =
                activationUsageCommitter;
            _limitedUsageCommitter =
                limitedUsageCommitter;
            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;
        }

        public CombatPetTriggerUsageCommitter
            ActivationUsageCommitter =>
                _activationUsageCommitter;

        public CombatPetLimitedTriggerUsageCommitter
            LimitedUsageCommitter =>
                _limitedUsageCommitter;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                _targetDamageReductionRegistry;

        protected override bool CanTriggerOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            return IsEligible(
                context,
                pet);
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
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        context.SourceEvent.Metadata.EventId,
                        pet.InstanceId,
                        context.SourceEvent.TargetInstanceId,
                        DamageReduction,
                        MaximumReductionCount));
        }

        private bool IsEligible(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            var sourceEvent = context.SourceEvent;

            return sourceEvent.TargetSide == context.Side &&
                   sourceEvent.TargetPosition.Row ==
                   context.GetAffectedRow(pet) &&
                   _activationUsageCommitter.HasTriggered(
                       pet.InstanceId) &&
                   !_limitedUsageCommitter.HasReachedLimit(
                       pet.InstanceId,
                       MaximumReductionCount);
        }
    }
}
