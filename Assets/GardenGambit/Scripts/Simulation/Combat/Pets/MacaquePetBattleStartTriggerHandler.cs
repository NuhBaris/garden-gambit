using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class MacaquePetBattleStartTriggerHandler
        : CombatPetBattleStartTriggerHandler
    {
        public const int AttackBonus = 2;
        public const int MinimumMatchingRankCount = 3;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatAttackGainResolver _attackGainResolver;

        public MacaquePetBattleStartTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
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

        public CombatPetTriggerUsageCommitter UsageCommitter =>
            _usageCommitter;

        public CombatAttackGainResolver AttackGainResolver =>
            _attackGainResolver;

        protected override bool CanTriggerAtBattleStart(
            CombatState state,
            BattleStartStageStartedCombatEvent sourceEvent,
            CombatPetState pet)
        {
            return CanTriggerAtBattleStart(
                CreateBattleStartContext(state, sourceEvent),
                pet);
        }

        protected override void ResolveAtBattleStart(
            CombatState state,
            BattleStartStageStartedCombatEvent sourceEvent,
            CombatPetState pet)
        {
            ResolveAtBattleStart(
                CreateBattleStartContext(state, sourceEvent),
                pet);
        }

        protected override bool CanTriggerAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            return !_usageCommitter.HasTriggered(pet.InstanceId)
                && GetEligibleSnapshotIds(context, pet).Count > 0;
        }

        protected override void ResolveAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(pet.InstanceId))
            {
                return;
            }

            var eligibleIds = GetEligibleSnapshotIds(context, pet);

            if (eligibleIds.Count == 0)
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
            {
                var row = context.State.GetPets(context.Side)
                    .GetAffectedRow(pet.InstanceId);

                var positions = new List<BoardPosition>();

                foreach (var slot in context.SideState.Board.Slots)
                {
                    if (slot.Position.Row != row ||
                        !slot.OccupantInstanceId.HasValue)
                    {
                        continue;
                    }

                    if (eligibleIds.Contains(slot.OccupantInstanceId.Value))
                    {
                        positions.Add(slot.Position);
                    }
                }

                positions.Sort(
                    (left, right) => left.Column.CompareTo(right.Column));

                _attackGainResolver.TryApplyAttackGainBatch(
                    context.State,
                    context.SourceEvent,
                    positions,
                    AttackBonus);
            });
        }

        private static HashSet<InstanceId> GetEligibleSnapshotIds(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            var row = context.State.GetPets(context.Side)
                .GetAffectedRow(pet.InstanceId);

            var rankCounts = new Dictionary<int, int>();

            foreach (var card in context.SideSnapshot.Cards)
            {
                if (card.Row != row)
                {
                    continue;
                }

                var rank = card.Rank.Value;
                int count;
                rankCounts.TryGetValue(rank, out count);
                rankCounts[rank] = count + 1;
            }

            var result = new HashSet<InstanceId>();

            foreach (var card in context.SideSnapshot.Cards)
            {
                if (card.Row == row &&
                    rankCounts[card.Rank.Value] >= MinimumMatchingRankCount)
                {
                    result.Add(card.InstanceId);
                }
            }

            return result;
        }
    }
}