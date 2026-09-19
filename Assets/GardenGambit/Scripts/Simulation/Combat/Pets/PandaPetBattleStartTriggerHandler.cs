using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        PandaPetBattleStartTriggerHandler :
        CombatPetBattleStartTriggerHandler
    {
        public const int MinimumDistinctSeasonCount = 3;

        private readonly CombatPetTriggerUsageCommitter
            _activationUsageCommitter;

        public PandaPetBattleStartTriggerHandler(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter
                activationUsageCommitter)
            : base(
                side,
                petInstanceId)
        {
            if (activationUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(activationUsageCommitter));
            }

            _activationUsageCommitter =
                activationUsageCommitter;
        }

        public CombatPetTriggerUsageCommitter
            ActivationUsageCommitter =>
                _activationUsageCommitter;

        protected override bool CanTriggerAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            return !_activationUsageCommitter.HasTriggered(
                       pet.InstanceId) &&
                   HasRequiredSnapshotSeasonDiversity(
                       context,
                       pet);
        }

        protected override void ResolveAtBattleStart(
            CombatPetBattleStartContext context,
            CombatPetState pet)
        {
            if (_activationUsageCommitter.HasTriggered(
                    pet.InstanceId) ||
                !HasRequiredSnapshotSeasonDiversity(
                    context,
                    pet))
            {
                return;
            }

            _activationUsageCommitter.TryCommit(
                pet.InstanceId,
                () =>
                {
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

        private static bool
            HasRequiredSnapshotSeasonDiversity(
                CombatPetBattleStartContext context,
                CombatPetState pet)
        {
            var row = context.State.GetPets(
                    context.Side)
                .GetAffectedRow(
                    pet.InstanceId);
            var seasons =
                new HashSet<CombatCardSeason>();

            foreach (var card in context.SideSnapshot.Cards)
            {
                if (card.Row == row &&
                    card.HasSpecifiedSeason)
                {
                    seasons.Add(card.Season);
                }
            }

            return seasons.Count >=
                   MinimumDistinctSeasonCount;
        }
    }
}
