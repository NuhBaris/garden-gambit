using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class AlpacaPetBattleStartTriggerHandler
        : CombatPetBattleStartTriggerHandler
    {
        public const int AttackBonus = 1;
        public const int RequiredCardCount = 5;
        public const int MaximumTargetCount = 2;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatAttackGainResolver _attackGainResolver;

        public AlpacaPetBattleStartTriggerHandler(
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
                && HasRequiredSnapshotRanks(context, pet);
        }

        protected override void ResolveAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(pet.InstanceId) ||
                !HasRequiredSnapshotRanks(context, pet))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
            {
                var sideState = context.SideState;
                var row = context.State.GetPets(context.Side)
                    .GetAffectedRow(pet.InstanceId);

                var positions = new List<BoardPosition>();

                foreach (var slot in sideState.Board.Slots)
                {
                    if (slot.Position.Row == row && slot.IsOccupied)
                    {
                        positions.Add(slot.Position);
                    }
                }

                // Select all targets before applying any bonus.
                positions.Sort((left, right) =>
                {
                    var comparison = sideState.GetCardAt(left).Attack.CompareTo(
                        sideState.GetCardAt(right).Attack);

                    return comparison != 0
                        ? comparison
                        : left.Column.CompareTo(right.Column);
                });

                if (positions.Count > MaximumTargetCount)
                {
                    positions.RemoveRange(
                        MaximumTargetCount,
                        positions.Count - MaximumTargetCount);
                }

                _attackGainResolver.TryApplyAttackGainBatch(
                    context.State,
                    context.SourceEvent,
                    positions,
                    AttackBonus);
            });
        }

        private static bool HasRequiredSnapshotRanks(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            var row = context.State.GetPets(context.Side)
                .GetAffectedRow(pet.InstanceId);

            var cardCount = 0;
            var ranks = new HashSet<int>();

            foreach (var card in context.SideSnapshot.Cards)
            {
                if (card.Row != row)
                {
                    continue;
                }

                cardCount++;
                ranks.Add(card.Rank.Value);
            }

            return cardCount == RequiredCardCount
                && ranks.Count == RequiredCardCount;
        }
    }
}