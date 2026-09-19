using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class TapirPetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetTriggerSource _rescueSource;
        private readonly CombatTriggerSourceRegistry _sources;

        public TapirPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(nameof(attackGainResolver));
            }

            RescueHandler = new TapirPetRescueTriggerHandler(
                side, petInstanceId, usageCommitter, attackGainResolver);
            HpGainHandler = new TapirPetHpGainTriggerHandler(
                side, petInstanceId, usageCommitter, attackGainResolver);
            _rescueSource = new CombatPetTriggerSource(side, petInstanceId, RescueHandler);
            _sources = new CombatTriggerSourceRegistry(new ICombatTriggerSource[]
            {
                _rescueSource,
                new CombatPetTriggerSource(side, petInstanceId, HpGainHandler)
            });
        }

        public TapirPetRescueTriggerHandler RescueHandler { get; }
        public TapirPetHpGainTriggerHandler HpGainHandler { get; }
        public CombatSide Side => RescueHandler.Side;
        public InstanceId PetInstanceId => RescueHandler.PetInstanceId;
        public CombatPetCardTriggerUsageCommitter UsageCommitter => RescueHandler.UsageCommitter;
        public CombatAttackGainResolver AttackGainResolver => RescueHandler.AttackGainResolver;
        public CombatPetTriggerOrderKeyProvider OrderKeyProvider => _rescueSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>> DiscoverTriggers(
            CombatState state, CombatEvent sourceEvent)
        {
            return _sources.DiscoverTriggers(state, sourceEvent);
        }
    }
}
