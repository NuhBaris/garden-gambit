using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class LadybugPetDeathTriggerHandler : CombatPetDeathTriggerHandler
    {
        public const int ArmorBonus = 1;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatArmorGainResolver _armorGainResolver;

        public LadybugPetDeathTriggerHandler(
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

        protected override bool CanTriggerOnDeath(CombatPetDeathContext context, CombatPetState pet)
        {
            return !_usageCommitter.HasTriggered(pet.InstanceId) && IsEligibleDeath(context, pet);
        }

        protected override void ResolveOnDeath(CombatPetDeathContext context, CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(pet.InstanceId) || !IsEligibleDeath(context, pet))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
            {
                var positions = new List<BoardPosition>();
                var sideState = context.State.GetSide(context.Side);
                var row = context.GetAffectedRow(pet);

                foreach (var slot in sideState.Board.Slots)
                {
                    if (slot.Position.Row != row || !slot.OccupantInstanceId.HasValue)
                    {
                        continue;
                    }

                    var card = sideState.Cards.GetCard(slot.OccupantInstanceId.Value);
                    if (!card.IsAtDeathThreshold)
                    {
                        positions.Add(slot.Position);
                    }
                }

                positions.Sort((left, right) => left.Column.CompareTo(right.Column));
                _armorGainResolver.TryApplyArmorGainBatch(
                    context.State, context.SourceEvent, positions, ArmorBonus);
                // The first eligible death consumes the use even with no living targets.
            });
        }

        private static bool IsEligibleDeath(CombatPetDeathContext context, CombatPetState pet)
        {
            // Death keeps its event-time side/row; rescue or removal does not
            // cancel an already queued trigger from this independent Pet.
            return context.SourceEvent.Position.Side == context.Side &&
                context.SourceEvent.Position.Row == context.GetAffectedRow(pet);
        }
    }
}
