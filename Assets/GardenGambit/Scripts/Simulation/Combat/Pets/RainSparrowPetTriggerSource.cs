using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class RainSparrowPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly CombatPetHpGainTriggerSource
            _hpGainTriggerSource;

        public RainSparrowPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(armorGainResolver));
            }

            Handler = new RainSparrowPetHpGainTriggerHandler(
                side,
                petInstanceId,
                usageCommitter,
                armorGainResolver);

            _hpGainTriggerSource =
                new CombatPetHpGainTriggerSource(Handler);
        }

        public RainSparrowPetHpGainTriggerHandler Handler
        {
            get;
        }

        public CombatSide Side =>
            Handler.Side;

        public InstanceId PetInstanceId =>
            Handler.PetInstanceId;

        public CombatPetCardTriggerUsageCommitter UsageCommitter =>
            Handler.UsageCommitter;

        public CombatArmorGainResolver ArmorGainResolver =>
            Handler.ArmorGainResolver;

        public CombatPetTriggerOrderKeyProvider OrderKeyProvider =>
            _hpGainTriggerSource.OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _hpGainTriggerSource.DiscoverTriggers(
                state,
                sourceEvent);
        }
    }
}