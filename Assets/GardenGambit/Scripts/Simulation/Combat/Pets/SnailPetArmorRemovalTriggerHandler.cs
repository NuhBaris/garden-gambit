using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class SnailPetArmorRemovalTriggerHandler : CombatPetEventTriggerHandler<ArmorRemovedCombatEvent>
    {
        public const int ArmorBonus = 2;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatArmorGainResolver _armorGainResolver;

        public SnailPetArmorRemovalTriggerHandler(
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

        protected override bool CanPetTrigger(CombatState state, ArmorRemovedCombatEvent sourceEvent, CombatPetState pet)
        {
            BoardPosition position;
            return TryGetEligibleTargetPosition(state, sourceEvent, pet, out position);
        }

        protected override void ResolvePetTrigger(CombatState state, ArmorRemovedCombatEvent sourceEvent, CombatPetState pet)
        {
            BoardPosition position;
            if (!TryGetEligibleTargetPosition(state, sourceEvent, pet, out position))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
                _armorGainResolver.TryApplyArmorGain(
                    state, sourceEvent, position, ArmorBonus));
        }

        private bool TryGetEligibleTargetPosition(
            CombatState state,
            ArmorRemovedCombatEvent sourceEvent,
            CombatPetState pet,
            out BoardPosition position)
        {
            position = default(BoardPosition);
            if (sourceEvent.TargetSide != Side || !sourceEvent.DepletedArmor ||
                _usageCommitter.HasTriggered(pet.InstanceId))
            {
                return false;
            }

            var row = state.GetPets(Side).GetAffectedRow(pet.InstanceId);
            foreach (var slot in state.GetSide(Side).Board.Slots)
            {
                if (slot.Position.Row == row && slot.OccupantInstanceId.HasValue &&
                    slot.OccupantInstanceId.Value == sourceEvent.TargetInstanceId)
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
