using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        FruitBatPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetDamageTriggerSource
            _petTriggerSource;

        public FruitBatPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatEventLog eventLog)
        {
            Handler =
                new FruitBatPetDamageTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    hpGainResolver,
                    eventLog);

            _petTriggerSource =
                new CombatPetDamageTriggerSource(
                    Handler);
        }

        public FruitBatPetDamageTriggerHandler
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

        public CombatHpGainResolver HpGainResolver =>
            Handler.HpGainResolver;

        public CombatEventLog EventLog =>
            Handler.EventLog;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _petTriggerSource
                    .OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _petTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}
