using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        PolarFerretPetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int DamageReduction = 1;

        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        public PolarFerretPetNormalAttackTriggerHandler(
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
                    nameof(
                        targetDamageReductionRegistry));
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

        protected override bool
            CanTriggerOnNormalAttack(
                CombatPetNormalAttackContext context,
                CombatPetState pet)
        {
            if (context.SourceEvent.TargetSide !=
                context.Side)
            {
                return false;
            }

            if (!context.SourceEvent.IsWinterTarget)
            {
                return false;
            }

            return !_usageCommitter.HasTriggered(
                pet.InstanceId,
                context.SourceEvent
                    .TargetInstanceId);
        }

        protected override void
            ResolveOnNormalAttack(
                CombatPetNormalAttackContext context,
                CombatPetState pet)
        {
            if (context.SourceEvent.TargetSide !=
                context.Side)
            {
                return;
            }

            if (!context.SourceEvent.IsWinterTarget)
            {
                return;
            }

            if (_usageCommitter.HasTriggered(
                    pet.InstanceId,
                    context.SourceEvent
                        .TargetInstanceId))
            {
                return;
            }

            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        context.SourceEvent
                            .Metadata.EventId,
                        pet.InstanceId,
                        context.SourceEvent
                            .TargetInstanceId,
                        DamageReduction);

            _targetDamageReductionRegistry
                .TryRegister(
                    request);
        }
    }
}