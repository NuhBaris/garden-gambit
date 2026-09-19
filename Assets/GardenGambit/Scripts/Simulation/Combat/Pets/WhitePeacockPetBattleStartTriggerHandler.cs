using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        WhitePeacockPetBattleStartTriggerHandler :
        CombatPetBattleStartTriggerHandler
    {
        public const int RequiredDistinctSuitCount = 4;
        public const int StatBonus = 1;

        private readonly CombatPetTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        private readonly CombatArmorGainResolver
            _armorGainResolver;

        private readonly CombatAttackGainResolver
            _attackGainResolver;

        public WhitePeacockPetBattleStartTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatArmorGainResolver armorGainResolver,
            CombatAttackGainResolver attackGainResolver)
            : base(side, petInstanceId)
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

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(attackGainResolver));
            }

            _usageCommitter = usageCommitter;
            _hpGainResolver = hpGainResolver;
            _armorGainResolver = armorGainResolver;
            _attackGainResolver = attackGainResolver;
        }

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public CombatArmorGainResolver ArmorGainResolver =>
            _armorGainResolver;

        public CombatAttackGainResolver AttackGainResolver =>
            _attackGainResolver;

        protected override bool CanTriggerAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(
                    pet.InstanceId))
            {
                return false;
            }

            var row =
                context.State.GetPets(Side)
                    .GetAffectedRow(
                        pet.InstanceId);

            return context.SideSnapshot
                       .CountDistinctSpecifiedSuitsInRow(
                           row) ==
                   RequiredDistinctSuitCount;
        }

        protected override void ResolveAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            if (!CanTriggerAtBattleStart(
                    context,
                    pet))
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                () => ApplyBonus(
                    context,
                    pet));
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

        private void ApplyBonus(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            var row =
                context.State.GetPets(Side)
                    .GetAffectedRow(
                        pet.InstanceId);

            var targets = GetOrderedTargets(
                context.SideState,
                row);

            ValidateAllGains(
                context.SideState,
                targets);

            for (var index = 0;
                 index < targets.Count;
                 index++)
            {
                _hpGainResolver.TryApplyHpStatGain(
                    context.State,
                    context.SourceEvent,
                    pet.InstanceId,
                    targets[index],
                    StatBonus);
            }

            _armorGainResolver.TryApplyArmorGainBatch(
                context.State,
                context.SourceEvent,
                targets,
                StatBonus);

            _attackGainResolver.TryApplyAttackGainBatch(
                context.State,
                context.SourceEvent,
                targets,
                StatBonus);
        }

        private static List<BoardPosition>
            GetOrderedTargets(
                CombatSideState sideState,
                BoardRow row)
        {
            var targets =
                new List<BoardPosition>();

            foreach (var slot in sideState.Board.Slots)
            {
                if (slot.Position.Row == row &&
                    slot.IsOccupied)
                {
                    targets.Add(
                        slot.Position);
                }
            }

            targets.Sort(
                (left, right) =>
                    left.Column.CompareTo(
                        right.Column));

            return targets;
        }

        private static void ValidateAllGains(
            CombatSideState sideState,
            IEnumerable<BoardPosition> targets)
        {
            foreach (var target in targets)
            {
                var card =
                    sideState.GetCardAt(
                        target);

                if ((long)card.HpCapacity +
                        StatBonus >
                    int.MaxValue ||
                    (long)card.CurrentHp +
                        StatBonus >
                    int.MaxValue ||
                    (long)card.Armor +
                        StatBonus >
                    int.MaxValue ||
                    (long)card.Attack +
                        StatBonus >
                    int.MaxValue)
                {
                    throw new OverflowException(
                        "White Peacock stat gain would " +
                        "overflow a target card value.");
                }
            }
        }
    }
}
