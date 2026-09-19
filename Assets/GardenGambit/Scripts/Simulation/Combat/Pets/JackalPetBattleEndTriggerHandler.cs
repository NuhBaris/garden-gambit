using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class JackalPetBattleEndTriggerHandler : CombatPetBattleEndTriggerHandler
    {
        public const int FinalRankBonus = 2;

        private readonly CombatPetCardTriggerUsageCommitter _usageCommitter;
        private readonly CombatFinalRankModifierRegistry _finalRankModifierRegistry;

        public JackalPetBattleEndTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatFinalRankModifierRegistry finalRankModifierRegistry)
            : base(side, petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(nameof(finalRankModifierRegistry));
            }

            _usageCommitter = usageCommitter;
            _finalRankModifierRegistry = finalRankModifierRegistry;
        }

        public CombatPetCardTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatFinalRankModifierRegistry FinalRankModifierRegistry => _finalRankModifierRegistry;

        protected override bool CanTriggerAtBattleEnd(CombatPetBattleEndContext context, CombatPetState pet)
        {
            var eligibleSlots = GetEligibleSlots(context, pet);
            foreach (var slot in eligibleSlots)
            {
                if (!_usageCommitter.HasTriggered(pet.InstanceId, slot.OccupantInstanceId.Value))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ResolveAtBattleEnd(CombatPetBattleEndContext context, CombatPetState pet)
        {
            var eligibleSlots = GetEligibleSlots(context, pet);
            var pendingTargets = new List<InstanceId>();
            foreach (var slot in eligibleSlots)
            {
                var cardId = slot.OccupantInstanceId.Value;
                if (_usageCommitter.HasTriggered(pet.InstanceId, cardId))
                {
                    continue;
                }

                // Validate every pending addition before changing modifiers or usage.
                if ((long)_finalRankModifierRegistry.GetTotalModifier(cardId) + FinalRankBonus > int.MaxValue)
                {
                    throw new OverflowException("Jackal final Rank bonus would overflow a target's modifier.");
                }

                pendingTargets.Add(cardId);
            }

            foreach (var cardId in pendingTargets)
            {
                _usageCommitter.TryCommit(pet.InstanceId, cardId,
                    () => _finalRankModifierRegistry.AddModifier(cardId, FinalRankBonus));
            }
        }

        private static List<CombatSlotState> GetEligibleSlots(CombatPetBattleEndContext context, CombatPetState pet)
        {
            var row = context.GetAffectedRow(pet);
            var result = new List<CombatSlotState>();
            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position.Row != row || !slot.OccupantInstanceId.HasValue)
                {
                    continue;
                }

                var card = context.SideState.Cards.GetCard(slot.OccupantInstanceId.Value);
                if (!card.IsAtDeathThreshold && card.Rank.Value % 2 != 0)
                {
                    result.Add(slot);
                }
            }

            result.Sort((left, right) => left.Position.Column.CompareTo(right.Position.Column));
            return result;
        }
    }
}
