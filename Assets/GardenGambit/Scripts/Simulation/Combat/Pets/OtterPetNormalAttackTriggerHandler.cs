using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        OtterPetNormalAttackTriggerHandler :
        CombatPetNormalAttackTriggerHandler
    {
        public const int AttackBonus = 1;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatAttackGainResolver
            _attackGainResolver;

        public OtterPetNormalAttackTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver)
            : base(
                side,
                petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(attackGainResolver));
            }

            _usageCommitter =
                usageCommitter;

            _attackGainResolver =
                attackGainResolver;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatAttackGainResolver
            AttackGainResolver =>
                _attackGainResolver;

        protected override bool CanTriggerOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            return IsEligibleDrinkAttack(
                context,
                pet);
        }

        protected override void ResolveOnNormalAttack(
            CombatPetNormalAttackContext context,
            CombatPetState pet)
        {
            if (!IsEligibleDrinkAttack(
                    context,
                    pet))
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                context.SourceEvent.AttackerInstanceId,
                () =>
                    _attackGainResolver.TryApplyAttackGain(
                        context.State,
                        context.SourceEvent,
                        context.SourceEvent.AttackerPosition,
                        AttackBonus));
        }

        private bool IsEligibleDrinkAttack(
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
                    return card.IsDrink;
                }
            }

            return false;
        }
    }
}
