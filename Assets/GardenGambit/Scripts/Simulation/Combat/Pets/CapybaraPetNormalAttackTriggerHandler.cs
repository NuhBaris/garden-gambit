using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CapybaraPetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int DamageBonus = 1;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackSourceDamageModifierRegistry
            _sourceDamageModifierRegistry;

        public CapybaraPetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
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
                    nameof(sourceDamageModifierRegistry));
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

        protected override bool CanTriggerOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            return IsEligibleVegetableAttack(
                context,
                pet);
        }

        protected override void ResolveOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            if (!IsEligibleVegetableAttack(
                    context,
                    pet))
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                context.SourceEvent.AttackerInstanceId,
                () =>
                    _sourceDamageModifierRegistry
                        .AddModifier(
                            context.SourceEvent
                                .Metadata.EventId,
                            DamageBonus));
        }

        private bool IsEligibleVegetableAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            var attackEvent =
                context.SourceEvent;

            if (attackEvent.AttackerSide !=
                context.Side)
            {
                return false;
            }

            if (attackEvent.AttackerPosition.Row !=
                context.GetAffectedRow(
                    pet))
            {
                return false;
            }

            if (_usageCommitter.HasTriggered(
                    pet.InstanceId,
                    attackEvent.AttackerInstanceId))
            {
                return false;
            }

            var attackerOccupiesEventPosition =
                false;

            foreach (var slot in
                     context.SideState.Board.Slots)
            {
                if (slot.Position !=
                    attackEvent.AttackerPosition)
                {
                    continue;
                }

                attackerOccupiesEventPosition =
                    slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value ==
                    attackEvent.AttackerInstanceId;

                break;
            }

            if (!attackerOccupiesEventPosition)
            {
                return false;
            }

            foreach (var card in
                     context.SideState.Cards.Cards)
            {
                if (card.InstanceId ==
                    attackEvent.AttackerInstanceId)
                {
                    return card.IsVegetable;
                }
            }

            return false;
        }
    }
}
