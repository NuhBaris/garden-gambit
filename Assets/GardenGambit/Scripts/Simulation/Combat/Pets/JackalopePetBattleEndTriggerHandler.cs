using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        JackalopePetBattleEndTriggerHandler :
        CombatPetBattleEndTriggerHandler
    {
        public const int HighCardFinalRankBonus = 3;
        public const int PairFinalRankBonus = 2;
        public const int TwoPairFinalRankBonus = 1;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatFinalRankModifierRegistry
            _finalRankModifierRegistry;

        public JackalopePetBattleEndTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
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
                    nameof(finalRankModifierRegistry));
            }

            _usageCommitter = usageCommitter;
            _finalRankModifierRegistry =
                finalRankModifierRegistry;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _finalRankModifierRegistry;

        protected override bool CanTriggerAtBattleEnd(
            CombatPetBattleEndContext context,
            CombatPetState pet)
        {
            var finalRankBonus =
                GetFinalRankBonus(
                    context,
                    pet);

            if (finalRankBonus <= 0)
            {
                return false;
            }

            var livingSlots =
                GetLivingSlots(
                    context,
                    pet);

            for (var index = 0;
                 index < livingSlots.Count;
                 index++)
            {
                var cardInstanceId =
                    livingSlots[index]
                        .OccupantInstanceId.Value;

                if (!_usageCommitter.HasTriggered(
                        pet.InstanceId,
                        cardInstanceId))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ResolveAtBattleEnd(
            CombatPetBattleEndContext context,
            CombatPetState pet)
        {
            var finalRankBonus =
                GetFinalRankBonus(
                    context,
                    pet);

            if (finalRankBonus <= 0)
            {
                return;
            }

            var livingSlots =
                GetLivingSlots(
                    context,
                    pet);

            var pendingTargets =
                new List<InstanceId>();

            for (var index = 0;
                 index < livingSlots.Count;
                 index++)
            {
                var cardInstanceId =
                    livingSlots[index]
                        .OccupantInstanceId.Value;

                if (_usageCommitter.HasTriggered(
                        pet.InstanceId,
                        cardInstanceId))
                {
                    continue;
                }

                if ((long)_finalRankModifierRegistry
                        .GetTotalModifier(
                            cardInstanceId) +
                    finalRankBonus > int.MaxValue)
                {
                    throw new OverflowException(
                        "Jackalope final Rank bonus would " +
                        "overflow a target's modifier.");
                }

                pendingTargets.Add(
                    cardInstanceId);
            }

            for (var index = 0;
                 index < pendingTargets.Count;
                 index++)
            {
                var cardInstanceId =
                    pendingTargets[index];

                _usageCommitter.TryCommit(
                    pet.InstanceId,
                    cardInstanceId,
                    () => _finalRankModifierRegistry
                        .AddModifier(
                            cardInstanceId,
                            finalRankBonus));
            }
        }

        private static int GetFinalRankBonus(
            CombatPetBattleEndContext context,
            CombatPetState pet)
        {
            if (!context.HasBattleStartSnapshot)
            {
                return 0;
            }

            var row =
                context.GetAffectedRow(
                    pet);

            var pokerHand =
                context.SideBattleStartSnapshot
                    .GetPokerHand(
                        row);

            switch (pokerHand)
            {
                case CombatPokerHand.HighCard:
                    return HighCardFinalRankBonus;

                case CombatPokerHand.Pair:
                    return PairFinalRankBonus;

                case CombatPokerHand.TwoPair:
                    return TwoPairFinalRankBonus;

                default:
                    return 0;
            }
        }

        private static List<CombatSlotState>
            GetLivingSlots(
                CombatPetBattleEndContext context,
                CombatPetState pet)
        {
            var row =
                context.GetAffectedRow(
                    pet);

            var result =
                new List<CombatSlotState>();

            foreach (var slot in
                     context.SideState.Board.Slots)
            {
                if (slot.Position.Row != row ||
                    !slot.OccupantInstanceId.HasValue)
                {
                    continue;
                }

                var card =
                    context.SideState.Cards.GetCard(
                        slot.OccupantInstanceId.Value);

                if (!card.IsAtDeathThreshold)
                {
                    result.Add(
                        slot);
                }
            }

            result.Sort(
                (left, right) =>
                    left.Position.Column.CompareTo(
                        right.Position.Column));

            return result;
        }
    }
}
