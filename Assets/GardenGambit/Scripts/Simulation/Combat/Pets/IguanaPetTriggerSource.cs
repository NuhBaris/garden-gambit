using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        IguanaPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetDeathTriggerSource
            _deathTriggerSource;

        public IguanaPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatEventLog eventLog)
        {
            Handler = new IguanaPetDeathTriggerHandler(
                side,
                petInstanceId,
                usageCommitter,
                hpGainResolver,
                eventLog);

            _deathTriggerSource =
                new CombatPetDeathTriggerSource(
                    Handler);
        }

        public IguanaPetDeathTriggerHandler Handler
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
                _deathTriggerSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _deathTriggerSource.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}
