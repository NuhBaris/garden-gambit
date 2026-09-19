using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        GerbilPetDamageTriggerHandler :
        CombatPetDamageTriggerHandler
    {
        public const int AttackBonus = 1;
        public const int MinimumEligibleRank = 2;
        public const int MaximumEligibleRank = 6;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatAttackGainResolver
            _attackGainResolver;

        private readonly CombatEventLog
            _eventLog;

        public GerbilPetDamageTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver,
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

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(attackGainResolver));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _usageCommitter =
                usageCommitter;

            _attackGainResolver =
                attackGainResolver;

            _eventLog =
                eventLog;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatAttackGainResolver
            AttackGainResolver =>
                _attackGainResolver;

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
                    _attackGainResolver.TryApplyAttackGain(
                        context.State,
                        context.SourceEvent,
                        attackerPosition,
                        AttackBonus));
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

                if (attacker.Rank.Value <
                        MinimumEligibleRank ||
                    attacker.Rank.Value >
                        MaximumEligibleRank ||
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
