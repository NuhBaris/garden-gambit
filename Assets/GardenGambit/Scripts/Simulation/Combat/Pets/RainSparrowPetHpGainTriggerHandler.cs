using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class RainSparrowPetHpGainTriggerHandler :
        CombatPetHpGainTriggerHandler
    {
        public const int ArmorBonus = 1;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatArmorGainResolver
            _armorGainResolver;

        public RainSparrowPetHpGainTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            _usageCommitter = usageCommitter;
            _armorGainResolver = armorGainResolver;
        }

        public CombatPetCardTriggerUsageCommitter UsageCommitter =>
            _usageCommitter;

        public CombatArmorGainResolver ArmorGainResolver =>
            _armorGainResolver;

        protected override bool CanTriggerOnHpGain(
            CombatPetHpGainContext context,
            CombatPetState pet)
        {
            BoardPosition targetPosition;

            if (!TryGetEligibleTargetPosition(
                    context,
                    pet,
                    out targetPosition))
            {
                return false;
            }

            return !_usageCommitter.HasTriggered(
                pet.InstanceId,
                context.SourceEvent.TargetInstanceId);
        }

        protected override void ResolveOnHpGain(
            CombatPetHpGainContext context,
            CombatPetState pet)
        {
            BoardPosition targetPosition;

            if (!TryGetEligibleTargetPosition(
                    context,
                    pet,
                    out targetPosition))
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                context.SourceEvent.TargetInstanceId,
                () => _armorGainResolver.TryApplyArmorGain(
                    context.State,
                    context.SourceEvent,
                    targetPosition,
                    ArmorBonus));
        }

        private static bool TryGetEligibleTargetPosition(
            CombatPetHpGainContext context,
            CombatPetState pet,
            out BoardPosition targetPosition)
        {
            targetPosition = default(BoardPosition);

            var sourceEvent = context.SourceEvent;

            if (sourceEvent.TargetSide != context.Side)
            {
                return false;
            }

            if (!sourceEvent.IsFromAnotherSource)
            {
                return false;
            }

            var affectedRow = context.GetAffectedRow(pet);

            foreach (var slot in context.SideState.Board.Slots)
            {
                if (!slot.OccupantInstanceId.HasValue)
                {
                    continue;
                }

                if (slot.OccupantInstanceId.Value !=
                    sourceEvent.TargetInstanceId)
                {
                    continue;
                }

                if (slot.Position.Row != affectedRow)
                {
                    return false;
                }

                var targetCard = context.SideState.Cards.GetCard(
                    sourceEvent.TargetInstanceId);

                if (!targetCard.IsSpring)
                {
                    return false;
                }

                targetPosition = slot.Position;
                return true;
            }

            return false;
        }
    }
}