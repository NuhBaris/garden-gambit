using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class PhoenixPetDeathTriggerHandler : CombatPetDeathTriggerHandler
    {
        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatRescueResolver _rescueResolver;

        public PhoenixPetDeathTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatRescueResolver rescueResolver)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }
            if (rescueResolver == null)
            {
                throw new ArgumentNullException(nameof(rescueResolver));
            }
            _usageCommitter = usageCommitter;
            _rescueResolver = rescueResolver;
        }

        public CombatPetTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatRescueResolver RescueResolver => _rescueResolver;

        protected override bool CanTriggerOnDeath(
            CombatPetDeathContext context,
            CombatPetState pet)
        {
            return !_usageCommitter.HasTriggered(pet.InstanceId) &&
                   IsRescuableDeath(context, pet);
        }

        protected override void ResolveOnDeath(
            CombatPetDeathContext context,
            CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(pet.InstanceId) ||
                !IsRescuableDeath(context, pet))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
                _rescueResolver.ApplyRescue(context.State, context.SourceEvent));
        }

        private bool IsRescuableDeath(
            CombatPetDeathContext context,
            CombatPetState pet)
        {
            var death = context.SourceEvent;
            if (death.Position.Side != context.Side ||
                death.Position.Row != context.GetAffectedRow(pet))
            {
                return false;
            }

            var sideState = context.State.GetSide(context.Side);
            foreach (var slot in sideState.Board.Slots)
            {
                if (slot.Position != death.Position)
                {
                    continue;
                }
                if (!slot.OccupantInstanceId.HasValue ||
                    slot.OccupantInstanceId.Value != death.InstanceId)
                {
                    return false;
                }

                return sideState.GetCardAt(slot.Position).IsAtDeathThreshold;
            }

            return false;
        }
    }
}
