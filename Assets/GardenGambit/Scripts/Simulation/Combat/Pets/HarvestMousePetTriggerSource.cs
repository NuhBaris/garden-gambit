using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class HarvestMousePetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetDeathTriggerSource _deathTriggerSource;

        public HarvestMousePetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver,
            CombatCardLookup cardLookup)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(nameof(attackGainResolver));
            }

            if (cardLookup == null)
            {
                throw new ArgumentNullException(nameof(cardLookup));
            }

            Handler = new HarvestMousePetDeathTriggerHandler(
                side, petInstanceId, usageCommitter, attackGainResolver, cardLookup);
            _deathTriggerSource = new CombatPetDeathTriggerSource(Handler);
        }

        public HarvestMousePetDeathTriggerHandler Handler { get; }
        public CombatSide Side => Handler.Side;
        public InstanceId PetInstanceId => Handler.PetInstanceId;
        public CombatPetTriggerUsageCommitter UsageCommitter => Handler.UsageCommitter;
        public CombatAttackGainResolver AttackGainResolver => Handler.AttackGainResolver;
        public CombatCardLookup CardLookup => Handler.CardLookup;
        public CombatPetTriggerOrderKeyProvider OrderKeyProvider => _deathTriggerSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>> DiscoverTriggers(
            CombatState state, CombatEvent sourceEvent)
        {
            return _deathTriggerSource.DiscoverTriggers(state, sourceEvent);
        }
    }
}
