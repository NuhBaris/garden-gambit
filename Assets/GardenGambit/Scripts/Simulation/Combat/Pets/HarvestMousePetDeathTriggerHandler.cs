using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class HarvestMousePetDeathTriggerHandler : CombatPetDeathTriggerHandler
    {
        public const int AttackBonus = 1;

        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatAttackGainResolver _attackGainResolver;
        private readonly CombatCardLookup _cardLookup;

        public HarvestMousePetDeathTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup)
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

            if (cardLookup == null)
            {
                throw new ArgumentNullException(nameof(cardLookup));
            }

            _usageCommitter = usageCommitter;
            _attackGainResolver = attackGainResolver;
            _cardLookup = cardLookup;
        }

        public CombatPetTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatAttackGainResolver AttackGainResolver => _attackGainResolver;
        public CombatCardLookup CardLookup => _cardLookup;

        protected override bool CanTriggerOnDeath(CombatPetDeathContext context, CombatPetState pet)
        {
            return !_usageCommitter.HasTriggered(pet.InstanceId) && IsEligibleDeath(context, pet);
        }

        protected override void ResolveOnDeath(CombatPetDeathContext context, CombatPetState pet)
        {
            if (_usageCommitter.HasTriggered(pet.InstanceId) || !IsEligibleDeath(context, pet))
            {
                return;
            }

            _usageCommitter.TryCommit(pet.InstanceId, () =>
            {
                var positions = new List<BoardPosition>();
                var sideState = context.State.GetSide(context.Side);
                var row = context.GetAffectedRow(pet);

                foreach (var slot in sideState.Board.Slots)
                {
                    if (slot.Position.Row != row || !slot.OccupantInstanceId.HasValue)
                    {
                        continue;
                    }

                    var card = sideState.Cards.GetCard(slot.OccupantInstanceId.Value);
                    if (card.IsAutumn && !card.IsAtDeathThreshold)
                    {
                        positions.Add(slot.Position);
                    }
                }

                positions.Sort((left, right) => left.Column.CompareTo(right.Column));

                _attackGainResolver.TryApplyAttackGainBatch(
                    context.State, context.SourceEvent, positions, AttackBonus);
                // "First friendly Autumn death" consumes the use even if this
                // eligible death currently has no living Autumn targets.
            });
        }

        private bool IsEligibleDeath(CombatPetDeathContext context, CombatPetState pet)
        {
            if (context.SourceEvent.Position.Side != context.Side ||
                context.SourceEvent.Position.Row != context.GetAffectedRow(pet))
            {
                return false;
            }

            // The death event keeps its original position. A rescue or removal
            // does not cancel a death trigger queued for this independent Pet.
            return _cardLookup.Get(context.State, context.SourceEvent.InstanceId).Season
                == CombatCardSeason.Autumn;
        }
    }
}
