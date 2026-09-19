using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class HawkPetBattleStartTriggerHandler
        : CombatPetBattleStartTriggerHandler
    {
        public const int AttackBonus = 3;
        public const int MaximumTargetCount = 2;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatAttackGainResolver _attackGainResolver;

        public HawkPetBattleStartTriggerHandler(
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
            return !_usageCommitter.HasTriggered(pet.InstanceId);
        }

        protected override void ResolveAtBattleStart(
            CombatState state,
            BattleStartStageStartedCombatEvent sourceEvent,
            CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(pet.InstanceId))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
            {
                var sideState = state.GetSide(Side);
                var row = state.GetPets(Side).GetAffectedRow(pet.InstanceId);
                var targets = new List<BoardPosition>();

                foreach (var slot in sideState.Board.Slots)
                {
                    // The ability has no living-only restriction.
                    // Threshold cards remain targetable while on the board.
                    if (slot.Position.Row == row && slot.IsOccupied)
                    {
                        targets.Add(slot.Position);
                    }
                }

                // Select using current Attack before applying any bonus.
                // Equal Attack values use the global left-to-right tie-break.
                targets.Sort((left, right) =>
                {
                    var comparison = sideState.GetCardAt(right).Attack.CompareTo(
                        sideState.GetCardAt(left).Attack);

                    return comparison != 0
                        ? comparison
                        : left.Column.CompareTo(right.Column);
                });

                if (targets.Count > MaximumTargetCount)
                {
                    targets.RemoveRange(
                        MaximumTargetCount,
                        targets.Count - MaximumTargetCount);
                }

                _attackGainResolver.TryApplyAttackGainBatch(
                    state,
                    sourceEvent,
                    targets,
                    AttackBonus);

                // An empty row also completes this Pet-start opportunity.
            });
        }
    }
}