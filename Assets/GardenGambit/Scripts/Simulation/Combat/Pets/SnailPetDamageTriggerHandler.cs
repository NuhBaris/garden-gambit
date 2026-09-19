using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class SnailPetDamageTriggerHandler : CombatPetDamageTriggerHandler
    {
        public const int ArmorBonus = 2;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatArmorGainResolver _armorGainResolver;

        public SnailPetDamageTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(nameof(armorGainResolver));
            }

            _usageCommitter = usageCommitter;
            _armorGainResolver = armorGainResolver;
        }

        public CombatPetTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatArmorGainResolver ArmorGainResolver => _armorGainResolver;

        protected override bool CanTriggerOnDamage(CombatPetDamageContext context, CombatPetState pet)
        {
            BoardPosition position;
            return TryGetEligibleTargetPosition(context, pet, out position);
        }

        protected override void ResolveOnDamage(CombatPetDamageContext context, CombatPetState pet)
        {
            BoardPosition position;
            if (!TryGetEligibleTargetPosition(context, pet, out position))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
                _armorGainResolver.TryApplyArmorGain(
                    context.State, context.SourceEvent, position, ArmorBonus));
        }

        private bool TryGetEligibleTargetPosition(
            CombatPetDamageContext context,
            CombatPetState pet,
            out BoardPosition position)
        {
            position = default(BoardPosition);
            var damage = context.SourceEvent;
            if (damage.TargetPosition.Side != context.Side ||
                damage.Result.PreviousArmor <= 0 || damage.Result.CurrentArmor != 0 ||
                _usageCommitter.HasTriggered(pet.InstanceId))
            {
                return false;
            }

            var row = context.GetAffectedRow(pet);
            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position.Row == row && slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value == damage.TargetInstanceId)
                {
                    // The recorded transition qualifies the event. Resolve the
                    // same card at its current position, even if another effect
                    // has since restored Armor. Threshold cards remain targetable.
                    position = slot.Position;
                    return true;
                }
            }

            return false;
        }
    }
}
