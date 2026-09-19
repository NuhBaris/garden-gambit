using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class LadybugPetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetDeathTriggerSource _deathTriggerSource;

        public LadybugPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(nameof(armorGainResolver));
            }

            Handler = new LadybugPetDeathTriggerHandler(side, petInstanceId, usageCommitter, armorGainResolver);
            _deathTriggerSource = new CombatPetDeathTriggerSource(Handler);
        }

        public LadybugPetDeathTriggerHandler Handler { get; }
        public CombatSide Side => Handler.Side;
        public InstanceId PetInstanceId => Handler.PetInstanceId;
        public CombatPetTriggerUsageCommitter UsageCommitter => Handler.UsageCommitter;
        public CombatArmorGainResolver ArmorGainResolver => Handler.ArmorGainResolver;
        public CombatPetTriggerOrderKeyProvider OrderKeyProvider => _deathTriggerSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>> DiscoverTriggers(
            CombatState state, CombatEvent sourceEvent)
        {
            return _deathTriggerSource.DiscoverTriggers(state, sourceEvent);
        }
    }
}
