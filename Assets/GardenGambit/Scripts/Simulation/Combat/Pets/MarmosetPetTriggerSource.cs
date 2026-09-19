using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        MarmosetPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetBattleStartTriggerSource
            _battleStartTriggerSource;

        public MarmosetPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatArmorGainResolver armorGainResolver)
        {
            Handler =
                new MarmosetPetBattleStartTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    hpGainResolver,
                    armorGainResolver);

            _battleStartTriggerSource =
                new CombatPetBattleStartTriggerSource(
                    Handler);
        }

        public MarmosetPetBattleStartTriggerHandler
            Handler
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

        public CombatArmorGainResolver ArmorGainResolver =>
            Handler.ArmorGainResolver;

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
