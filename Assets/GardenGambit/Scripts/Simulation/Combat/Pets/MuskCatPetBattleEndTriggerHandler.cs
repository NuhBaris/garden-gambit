using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        MuskCatPetBattleEndTriggerHandler :
        CombatPetBattleEndTriggerHandler
    {
        public const int FinalRankBonus = 4;

        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatFinalRankModifierRegistry
            _finalRankModifierRegistry;

        public MuskCatPetBattleEndTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
            : base(
                side,
                petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        finalRankModifierRegistry));
            }

            _usageCommitter =
                usageCommitter;

            _finalRankModifierRegistry =
                finalRankModifierRegistry;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _finalRankModifierRegistry;

        protected override bool
            CanTriggerAtBattleEnd(
                CombatPetBattleEndContext context,
                CombatPetState pet)
        {
            CombatCardState livingCard;

            if (!TryGetOnlyLivingCard(
                    context,
                    pet,
                    out livingCard))
            {
                return false;
            }

            return !_usageCommitter.HasTriggered(
                pet.InstanceId,
                livingCard.InstanceId);
        }

        protected override void
            ResolveAtBattleEnd(
                CombatPetBattleEndContext context,
                CombatPetState pet)
        {
            CombatCardState livingCard;

            if (!TryGetOnlyLivingCard(
                    context,
                    pet,
                    out livingCard))
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                livingCard.InstanceId,
                () =>
                    _finalRankModifierRegistry
                        .AddModifier(
                            livingCard.InstanceId,
                            FinalRankBonus));
        }

        private static bool
            TryGetOnlyLivingCard(
                CombatPetBattleEndContext context,
                CombatPetState pet,
                out CombatCardState livingCard)
        {
            livingCard =
                null;

            var affectedRow =
                context.GetAffectedRow(
                    pet);

            foreach (var slot in
                     context.SideState.Board.Slots)
            {
                if (slot.Position.Row !=
                    affectedRow)
                {
                    continue;
                }

                if (!slot.OccupantInstanceId
                        .HasValue)
                {
                    continue;
                }

                var candidate =
                    context.SideState.Cards.GetCard(
                        slot.OccupantInstanceId
                            .Value);

                if (candidate.IsAtDeathThreshold)
                {
                    continue;
                }

                if (livingCard != null)
                {
                    livingCard =
                        null;

                    return false;
                }

                livingCard =
                    candidate;
            }

            return livingCard != null;
        }
    }
}