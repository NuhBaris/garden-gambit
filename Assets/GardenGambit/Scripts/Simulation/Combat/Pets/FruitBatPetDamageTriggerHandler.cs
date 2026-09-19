using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        FruitBatPetDamageTriggerHandler :
        CombatPetDamageTriggerHandler
    {
        public const int HpBonus = 1;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        private readonly CombatEventLog
            _eventLog;

        public FruitBatPetDamageTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatEventLog eventLog)
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

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _usageCommitter =
                usageCommitter;

            _hpGainResolver =
                hpGainResolver;

            _eventLog =
                eventLog;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public CombatEventLog EventLog =>
            _eventLog;

        protected override bool CanTriggerOnDamage(
            CombatPetDamageContext context,
            CombatPetState pet)
        {
            BoardPosition attackerPosition;

            return TryGetEligibleAttackerPosition(
                context,
                pet,
                out attackerPosition);
        }

        protected override void ResolveOnDamage(
            CombatPetDamageContext context,
            CombatPetState pet)
        {
            BoardPosition attackerPosition;

            if (!TryGetEligibleAttackerPosition(
                    context,
                    pet,
                    out attackerPosition))
            {
                return;
            }

            _usageCommitter.TryCommit(
                pet.InstanceId,
                context.SourceEvent.SourceInstanceId,
                () =>
                    _hpGainResolver.TryApplyHpStatGain(
                        context.State,
                        context.SourceEvent,
                        pet.InstanceId,
                        attackerPosition,
                        HpBonus));
        }

        private bool TryGetEligibleAttackerPosition(
            CombatPetDamageContext context,
            CombatPetState pet,
            out BoardPosition attackerPosition)
        {
            attackerPosition =
                default(BoardPosition);

            var damageEvent =
                context.SourceEvent;

            if (damageEvent.SourcePosition.Side !=
                context.Side)
            {
                return false;
            }

            if (damageEvent.SourcePosition.Row !=
                context.GetAffectedRow(
                    pet))
            {
                return false;
            }

            if (_usageCommitter.HasTriggered(
                    pet.InstanceId,
                    damageEvent.SourceInstanceId))
            {
                return false;
            }

            NormalAttackCombatEvent attackEvent;

            if (!TryGetParentNormalAttack(
                    damageEvent,
                    out attackEvent))
            {
                return false;
            }

            if (attackEvent.AttackerInstanceId !=
                    damageEvent.SourceInstanceId ||
                attackEvent.AttackerPosition !=
                    damageEvent.SourcePosition ||
                attackEvent.TargetInstanceId !=
                    damageEvent.TargetInstanceId ||
                attackEvent.TargetPosition !=
                    damageEvent.TargetPosition)
            {
                return false;
            }

            foreach (var slot in
                     context.SideState.Board.Slots)
            {
                if (!slot.OccupantInstanceId.HasValue ||
                    slot.OccupantInstanceId.Value !=
                    damageEvent.SourceInstanceId)
                {
                    continue;
                }

                var attacker =
                    context.SideState.Cards.GetCard(
                        damageEvent.SourceInstanceId);

                if (!attacker.IsFruit ||
                    attacker.IsAtDeathThreshold)
                {
                    return false;
                }

                attackerPosition =
                    slot.Position;

                return true;
            }

            return false;
        }

        private bool TryGetParentNormalAttack(
            DamageAppliedCombatEvent damageEvent,
            out NormalAttackCombatEvent attackEvent)
        {
            attackEvent = null;

            if (!damageEvent.Metadata.HasParent)
            {
                return false;
            }

            var parentEventId =
                damageEvent.Metadata.ParentEventId.Value;

            if (!_eventLog.ContainsEvent(
                    parentEventId))
            {
                return false;
            }

            attackEvent =
                _eventLog.GetEvent(
                        parentEventId)
                    as NormalAttackCombatEvent;

            return attackEvent != null;
        }
    }
}
