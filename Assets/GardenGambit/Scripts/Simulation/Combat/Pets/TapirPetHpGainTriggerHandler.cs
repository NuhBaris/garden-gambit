using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class TapirPetHpGainTriggerHandler : CombatPetHpGainTriggerHandler
    {
        public const int AttackBonus = 2;

        private readonly CombatPetCardTriggerUsageCommitter _usageCommitter;
        private readonly CombatAttackGainResolver _attackGainResolver;

        public TapirPetHpGainTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(nameof(attackGainResolver));
            }

            _usageCommitter = usageCommitter;
            _attackGainResolver = attackGainResolver;
        }

        public CombatPetCardTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatAttackGainResolver AttackGainResolver => _attackGainResolver;

        protected override bool CanTriggerOnHpGain(CombatPetHpGainContext context, CombatPetState pet)
        {
            BoardPosition targetPosition;
            return TryGetEligibleTargetPosition(context, pet, out targetPosition);
        }

        protected override void ResolveOnHpGain(CombatPetHpGainContext context, CombatPetState pet)
        {
            BoardPosition targetPosition;
            if (!TryGetEligibleTargetPosition(context, pet, out targetPosition))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, context.SourceEvent.TargetInstanceId, () =>
                _attackGainResolver.TryApplyAttackGain(
                    context.State, context.SourceEvent, targetPosition, AttackBonus));
        }

        private bool TryGetEligibleTargetPosition(
            CombatPetHpGainContext context,
            CombatPetState pet,
            out BoardPosition targetPosition)
        {
            targetPosition = default(BoardPosition);
            var sourceEvent = context.SourceEvent;
            if (!sourceEvent.RestoredAboveDeathThreshold || sourceEvent.TargetSide != context.Side ||
                _usageCommitter.HasTriggered(pet.InstanceId, sourceEvent.TargetInstanceId))
            {
                return false;
            }

            var row = context.GetAffectedRow(pet);
            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position.Row == row && slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value == sourceEvent.TargetInstanceId)
                {
                    // Resolve by identity at its current position. The old event
                    // position may now be empty or occupied by a different card.
                    // No living-only condition: a threshold card remains targetable
                    // while it still occupies the board during its death chain.
                    targetPosition = slot.Position;
                    return true;
                }
            }

            return false;
        }
    }
}
