using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ToucanPetBattleStartTriggerHandler :
        CombatPetBattleStartTriggerHandler
    {
        public const int HpBonus = 1;

        public const int MinimumDistinctSuitCount = 3;

        public const int MaximumTargetCount = 2;

        private readonly CombatPetTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        public ToucanPetBattleStartTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver)
            : base(
                side,
                petInstanceId)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (hpGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(hpGainResolver));
            }

            _usageCommitter = usageCommitter;
            _hpGainResolver = hpGainResolver;
        }

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        protected override bool CanTriggerAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            return !_usageCommitter.HasTriggered(
                       pet.InstanceId) &&
                   HasRequiredSnapshotSuitDiversity(
                       context,
                       pet);
        }

        protected override void ResolveAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(
                    pet.InstanceId) ||
                !HasRequiredSnapshotSuitDiversity(
                    context,
                    pet))
            {
                return;
            }

            var positions = GetTargetPositions(
                context,
                pet);

            if (positions.Count == 0)
            {
                return;
            }

            ValidateGains(
                context.SideState,
                positions);

            _usageCommitter.TryCommit(
                pet.InstanceId,
                () =>
                {
                    foreach (var position in positions)
                    {
                        _hpGainResolver.TryApplyHpStatGain(
                            context.State,
                            context.SourceEvent,
                            pet.InstanceId,
                            position,
                            HpBonus);
                    }
                });
        }

        protected override bool CanTriggerAtBattleStart(
            CombatState state,
            BattleStartStageStartedCombatEvent sourceEvent,
            CombatPetState pet)
        {
            return false;
        }

        protected override void ResolveAtBattleStart(
            CombatState state,
            BattleStartStageStartedCombatEvent sourceEvent,
            CombatPetState pet)
        {
        }

        private static bool HasRequiredSnapshotSuitDiversity(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            var row = context.State.GetPets(
                    context.Side)
                .GetAffectedRow(
                    pet.InstanceId);
            var suits = new HashSet<CombatCardSuit>();

            foreach (var card in context.SideSnapshot.Cards)
            {
                if (card.Row == row &&
                    card.HasSpecifiedSuit)
                {
                    suits.Add(card.Suit);
                }
            }

            return suits.Count >=
                   MinimumDistinctSuitCount;
        }

        private static List<BoardPosition>
            GetTargetPositions(
                CombatPetBattleStartContext context,
                CombatPetState pet)
        {
            var row = context.State.GetPets(
                    context.Side)
                .GetAffectedRow(
                    pet.InstanceId);
            var positions = new List<BoardPosition>();

            foreach (var slot in context.SideState.Board.Slots)
            {
                if (slot.Position.Row == row &&
                    slot.IsOccupied)
                {
                    positions.Add(slot.Position);
                }
            }

            positions.Sort(
                (left, right) =>
                {
                    var comparison =
                        context.SideState.GetCardAt(left)
                            .CurrentHp.CompareTo(
                                context.SideState
                                    .GetCardAt(right)
                                    .CurrentHp);

                    return comparison != 0
                        ? comparison
                        : left.Column.CompareTo(
                            right.Column);
                });

            if (positions.Count > MaximumTargetCount)
            {
                positions.RemoveRange(
                    MaximumTargetCount,
                    positions.Count -
                    MaximumTargetCount);
            }

            return positions;
        }

        private static void ValidateGains(
            CombatSideState sideState,
            IEnumerable<BoardPosition> positions)
        {
            foreach (var position in positions)
            {
                var card = sideState.GetCardAt(position);

                if ((long)card.HpCapacity +
                        HpBonus > int.MaxValue ||
                    (long)card.CurrentHp +
                        HpBonus > int.MaxValue)
                {
                    throw new OverflowException(
                        "Toucan HP stat gain would " +
                        "overflow a target card value.");
                }
            }
        }
    }
}
