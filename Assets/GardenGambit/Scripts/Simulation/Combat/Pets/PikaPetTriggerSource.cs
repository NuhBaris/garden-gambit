using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class PikaPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly
            CombatPetBattleEndTriggerSource
            _battleEndTriggerSource;

        public PikaPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        finalRankModifierRegistry));
            }

            Handler =
                new
                    PikaPetBattleEndTriggerHandler(
                        side,
                        petInstanceId,
                        usageCommitter,
                        finalRankModifierRegistry);

            _battleEndTriggerSource =
                new
                    CombatPetBattleEndTriggerSource(
                        Handler);
        }

        public PikaPetBattleEndTriggerHandler
            Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                Handler.UsageCommitter;

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                Handler.FinalRankModifierRegistry;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _battleEndTriggerSource
                    .OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _battleEndTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}
