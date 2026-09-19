using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        GerbilPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetDamageTriggerSource
            _petTriggerSource;

        public GerbilPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver,
            CombatEventLog eventLog)
        {
            Handler =
                new GerbilPetDamageTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    attackGainResolver,
                    eventLog);

            _petTriggerSource =
                new CombatPetDamageTriggerSource(
                    Handler);
        }

        public GerbilPetDamageTriggerHandler Handler
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

        public CombatAttackGainResolver
            AttackGainResolver =>
                Handler.AttackGainResolver;

        public CombatEventLog EventLog =>
            Handler.EventLog;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _petTriggerSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _petTriggerSource.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}
