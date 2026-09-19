using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class WombatPetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetDeathTriggerSource _deathTriggerSource;

        public WombatPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
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

            Handler = new WombatPetDeathTriggerHandler(side, petInstanceId, usageCommitter, attackGainResolver);
            _deathTriggerSource = new CombatPetDeathTriggerSource(Handler);
        }

        public WombatPetDeathTriggerHandler Handler { get; }
        public CombatSide Side => Handler.Side;
        public InstanceId PetInstanceId => Handler.PetInstanceId;
        public CombatPetTriggerUsageCommitter UsageCommitter => Handler.UsageCommitter;
        public CombatAttackGainResolver AttackGainResolver => Handler.AttackGainResolver;
        public CombatPetTriggerOrderKeyProvider OrderKeyProvider => _deathTriggerSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>> DiscoverTriggers(
            CombatState state, CombatEvent sourceEvent)
        {
            return _deathTriggerSource.DiscoverTriggers(state, sourceEvent);
        }
    }
}
