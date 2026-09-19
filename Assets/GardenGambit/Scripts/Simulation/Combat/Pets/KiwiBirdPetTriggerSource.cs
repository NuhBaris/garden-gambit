using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        KiwiBirdPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetDamageTriggerSource
            _petTriggerSource;

        public KiwiBirdPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver,
            CombatEventLog eventLog)
        {
            Handler =
                new KiwiBirdPetDamageTriggerHandler(
                    side,
                    petInstanceId,
                    usageCommitter,
                    armorGainResolver,
                    eventLog);

            _petTriggerSource =
                new CombatPetDamageTriggerSource(
                    Handler);
        }

        public KiwiBirdPetDamageTriggerHandler
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

        public CombatArmorGainResolver
            ArmorGainResolver =>
                Handler.ArmorGainResolver;

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
