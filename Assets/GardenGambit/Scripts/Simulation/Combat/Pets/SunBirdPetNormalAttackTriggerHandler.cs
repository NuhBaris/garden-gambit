using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        SunBirdPetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int DamageBonus = 1;

        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackSourceDamageModifierRegistry
            _sourceDamageModifierRegistry;

        public SunBirdPetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
            : base(
                side,
                petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (sourceDamageModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        sourceDamageModifierRegistry));
            }

            _usageCommitter =
                usageCommitter;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _sourceDamageModifierRegistry;

        protected override bool
            CanTriggerOnNormalAttack(
                CombatPetNormalAttackContext context,
                CombatPetState pet)
        {
            if (context.SourceEvent.AttackerSide !=
                context.Side)
            {
                return false;
            }

            if (!context.SourceEvent.IsSummerAttack)
            {
                return false;
            }

            return !_usageCommitter.HasTriggered(
                pet.InstanceId,
                context.SourceEvent
                    .AttackerInstanceId);
        }

        protected override void
            ResolveOnNormalAttack(
                CombatPetNormalAttackContext context,
                CombatPetState pet)
        {
            if (context.SourceEvent.AttackerSide !=
                context.Side)
            {
                return;
            }

            if (!context.SourceEvent.IsSummerAttack)
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                context.SourceEvent
                    .AttackerInstanceId,
                () =>
                    _sourceDamageModifierRegistry
                        .AddModifier(
                            context.SourceEvent
                                .Metadata.EventId,
                            DamageBonus));
        }
    }
}