using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ToucanPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetBattleStartTriggerSource
            _battleStartTriggerSource;

        public ToucanPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver)
        {
            Handler =
                new ToucanPetBattleStartTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    hpGainResolver);

            _battleStartTriggerSource =
                new CombatPetBattleStartTriggerSource(
                    Handler);
        }

        public ToucanPetBattleStartTriggerHandler Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                Handler.UsageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            Handler.HpGainResolver;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _battleStartTriggerSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _battleStartTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}
