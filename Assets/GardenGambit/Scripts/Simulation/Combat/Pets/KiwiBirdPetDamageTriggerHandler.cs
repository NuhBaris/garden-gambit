using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        KiwiBirdPetDamageTriggerHandler :
        CombatPetDamageTriggerHandler
    {
        public const int ArmorBonus = 1;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatArmorGainResolver
            _armorGainResolver;

        private readonly CombatEventLog
            _eventLog;

        public KiwiBirdPetDamageTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver,
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

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            _usageCommitter = usageCommitter;
            _armorGainResolver = armorGainResolver;
            _eventLog = eventLog;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatArmorGainResolver
            ArmorGainResolver =>
                _armorGainResolver;

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
                    _armorGainResolver
                        .TryApplyArmorGain(
                            context.State,
                            context.SourceEvent,
                            attackerPosition,
                            ArmorBonus));
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

            var affectedRow =
                context.GetAffectedRow(
                    pet);

            if (damageEvent.SourcePosition.Row !=
                affectedRow)
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

            CombatBattleStartSnapshot snapshot;

            if (!TryGetBattleStartSnapshot(
                    damageEvent,
                    out snapshot))
            {
                return false;
            }

            if (snapshot.GetSide(
                        context.Side)
                    .GetPokerHand(
                        affectedRow) !=
                CombatPokerHand.Straight)
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

                if (attacker.IsAtDeathThreshold)
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

        private bool TryGetBattleStartSnapshot(
            DamageAppliedCombatEvent damageEvent,
            out CombatBattleStartSnapshot snapshot)
        {
            snapshot = null;

            for (var index = _eventLog.Count - 1;
                 index >= 0;
                 index--)
            {
                var combatStartedEvent =
                    _eventLog.Events[index]
                        as CombatStartedCombatEvent;

                if (combatStartedEvent == null ||
                    !combatStartedEvent
                        .HasBattleStartSnapshot ||
                    combatStartedEvent.Metadata.SequenceNo >=
                    damageEvent.Metadata.SequenceNo)
                {
                    continue;
                }

                snapshot =
                    combatStartedEvent
                        .BattleStartSnapshot;

                return true;
            }

            return false;
        }
    }
}
