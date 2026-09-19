using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        IguanaPetDeathTriggerHandler :
        CombatPetDeathTriggerHandler
    {
        public const int HpBonus = 1;
        public const int MinimumEligibleRank = 11;
        public const int MaximumEligibleRank = 14;

        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        private readonly CombatEventLog
            _eventLog;

        public IguanaPetDeathTriggerHandler(
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

            _usageCommitter = usageCommitter;
            _hpGainResolver = hpGainResolver;
            _eventLog = eventLog;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public CombatEventLog EventLog =>
            _eventLog;

        protected override bool CanTriggerOnDeath(
            CombatPetDeathContext context,
            CombatPetState pet)
        {
            BoardPosition killerPosition;

            return TryGetEligibleKillerPosition(
                context,
                pet,
                out killerPosition);
        }

        protected override void ResolveOnDeath(
            CombatPetDeathContext context,
            CombatPetState pet)
        {
            BoardPosition killerPosition;

            if (!TryGetEligibleKillerPosition(
                    context,
                    pet,
                    out killerPosition))
            {
                return;
            }

            var damageEvent = GetParentDamage(
                context.SourceEvent);

            _usageCommitter.TryCommit(
                pet.InstanceId,
                damageEvent.SourceInstanceId,
                () =>
                    _hpGainResolver.TryApplyHpStatGain(
                        context.State,
                        context.SourceEvent,
                        pet.InstanceId,
                        killerPosition,
                        HpBonus));
        }

        private bool TryGetEligibleKillerPosition(
            CombatPetDeathContext context,
            CombatPetState pet,
            out BoardPosition killerPosition)
        {
            killerPosition = default(BoardPosition);

            var damageEvent = GetParentDamage(
                context.SourceEvent);

            if (damageEvent == null ||
                !MatchesDeath(
                    damageEvent,
                    context.SourceEvent) ||
                damageEvent.SourcePosition.Side !=
                    context.Side ||
                damageEvent.SourcePosition.Row !=
                    context.GetAffectedRow(pet) ||
                _usageCommitter.HasTriggered(
                    pet.InstanceId,
                    damageEvent.SourceInstanceId))
            {
                return false;
            }

            var attackEvent = GetParentNormalAttack(
                damageEvent);

            if (attackEvent == null ||
                attackEvent.AttackerInstanceId !=
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

                var killer =
                    context.SideState.Cards.GetCard(
                        damageEvent.SourceInstanceId);

                if (killer.Rank.Value <
                        MinimumEligibleRank ||
                    killer.Rank.Value >
                        MaximumEligibleRank ||
                    killer.IsAtDeathThreshold)
                {
                    return false;
                }

                killerPosition = slot.Position;

                return true;
            }

            return false;
        }

        private DamageAppliedCombatEvent GetParentDamage(
            DeathCombatEvent deathEvent)
        {
            if (!deathEvent.Metadata.HasParent ||
                !_eventLog.ContainsEvent(
                    deathEvent.Metadata.ParentEventId.Value))
            {
                return null;
            }

            return _eventLog.GetEvent(
                    deathEvent.Metadata.ParentEventId.Value)
                as DamageAppliedCombatEvent;
        }

        private NormalAttackCombatEvent GetParentNormalAttack(
            DamageAppliedCombatEvent damageEvent)
        {
            if (!damageEvent.Metadata.HasParent ||
                !_eventLog.ContainsEvent(
                    damageEvent.Metadata.ParentEventId.Value))
            {
                return null;
            }

            return _eventLog.GetEvent(
                    damageEvent.Metadata.ParentEventId.Value)
                as NormalAttackCombatEvent;
        }

        private static bool MatchesDeath(
            DamageAppliedCombatEvent damageEvent,
            DeathCombatEvent deathEvent)
        {
            return damageEvent.TargetInstanceId ==
                       deathEvent.InstanceId &&
                   damageEvent.TargetPosition ==
                       deathEvent.Position &&
                   damageEvent.Result.EnteredDeathThreshold &&
                   damageEvent.Result.PreviousHp ==
                       deathEvent.PreviousHp &&
                   damageEvent.Result.CurrentHp ==
                       deathEvent.CurrentHp;
        }
    }
}
