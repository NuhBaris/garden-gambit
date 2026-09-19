using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class SnailPetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetDamageTriggerSource _petTriggerSource;
        private readonly CombatTriggerSourceRegistry _sources;

        public SnailPetTriggerSource(
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

            Handler = new SnailPetDamageTriggerHandler(
                side, petInstanceId, usageCommitter, armorGainResolver);
            _petTriggerSource = new CombatPetDamageTriggerSource(Handler);
            ArmorRemovalHandler = new SnailPetArmorRemovalTriggerHandler(
                side, petInstanceId, usageCommitter, armorGainResolver);
            _sources = new CombatTriggerSourceRegistry(new ICombatTriggerSource[]
            {
                _petTriggerSource,
                new CombatPetTriggerSource(side, petInstanceId, ArmorRemovalHandler)
            });
        }

        public SnailPetDamageTriggerHandler Handler { get; }
        public SnailPetArmorRemovalTriggerHandler ArmorRemovalHandler { get; }
        public CombatSide Side => Handler.Side;
        public InstanceId PetInstanceId => Handler.PetInstanceId;
        public CombatPetTriggerUsageCommitter UsageCommitter => Handler.UsageCommitter;
        public CombatArmorGainResolver ArmorGainResolver => Handler.ArmorGainResolver;
        public CombatPetTriggerOrderKeyProvider OrderKeyProvider => _petTriggerSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>> DiscoverTriggers(
            CombatState state, CombatEvent sourceEvent)
        {
            return _sources.DiscoverTriggers(state, sourceEvent);
        }
    }
}
