using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        MarmosetPetBattleStartTriggerHandler :
        CombatPetBattleStartTriggerHandler
    {
        public const int StatBonus = 1;

        private readonly CombatPetTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        private readonly CombatArmorGainResolver
            _armorGainResolver;

        public MarmosetPetBattleStartTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatArmorGainResolver armorGainResolver)
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

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            _usageCommitter = usageCommitter;
            _hpGainResolver = hpGainResolver;
            _armorGainResolver = armorGainResolver;
        }

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public CombatArmorGainResolver ArmorGainResolver =>
            _armorGainResolver;

        protected override bool CanTriggerAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            BoardPosition targetPosition;

            return !_usageCommitter.HasTriggered(
                       pet.InstanceId) &&
                   TryGetLowestRankTarget(
                       context,
                       pet,
                       out targetPosition);
        }

        protected override void ResolveAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            BoardPosition targetPosition;

            if (_usageCommitter.HasTriggered(
                    pet.InstanceId) ||
                !TryGetLowestRankTarget(
                    context,
                    pet,
                    out targetPosition))
            {
                return;
            }

            ValidateGains(
                context.SideState.GetCardAt(
                    targetPosition));

            _usageCommitter.TryCommit(
                pet.InstanceId,
                () =>
                {
                    _hpGainResolver.TryApplyHpStatGain(
                        context.State,
                        context.SourceEvent,
                        pet.InstanceId,
                        targetPosition,
                        StatBonus);

                    _armorGainResolver.TryApplyArmorGain(
                        context.State,
                        context.SourceEvent,
                        targetPosition,
                        StatBonus);
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

        private static bool TryGetLowestRankTarget(
            CombatPetBattleStartContext context,
            CombatPetState pet,
            out BoardPosition targetPosition)
        {
            targetPosition = default(BoardPosition);

            var row = context.State.GetPets(
                    context.Side)
                .GetAffectedRow(
                    pet.InstanceId);

            CombatBattleStartCardSnapshot selected = null;

            foreach (var card in context.SideSnapshot.Cards)
            {
                if (card.Row != row)
                {
                    continue;
                }

                if (selected == null ||
                    card.Rank.Value <
                        selected.Rank.Value ||
                    card.Rank == selected.Rank &&
                    card.Column < selected.Column)
                {
                    selected = card;
                }
            }

            if (selected == null)
            {
                return false;
            }

            targetPosition = selected.Position;

            return true;
        }

        private static void ValidateGains(
            CombatCardState target)
        {
            if ((long)target.HpCapacity +
                    StatBonus > int.MaxValue ||
                (long)target.CurrentHp +
                    StatBonus > int.MaxValue ||
                (long)target.Armor +
                    StatBonus > int.MaxValue)
            {
                throw new OverflowException(
                    "Marmoset stat gain would overflow " +
                    "a target card value.");
            }
        }
    }
}
