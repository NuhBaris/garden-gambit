using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class PhoenixPetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetDeathTriggerSource _deathTriggerSource;

        public PhoenixPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatRescueResolver rescueResolver)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }
            if (rescueResolver == null)
            {
                throw new ArgumentNullException(nameof(rescueResolver));
            }

            Handler = new PhoenixPetDeathTriggerHandler(
                side, petInstanceId, usageCommitter, rescueResolver);
            _deathTriggerSource = new CombatPetDeathTriggerSource(Handler);
        }

        public PhoenixPetDeathTriggerHandler Handler { get; }
        public CombatSide Side => Handler.Side;
        public InstanceId PetInstanceId => Handler.PetInstanceId;
        public CombatPetTriggerUsageCommitter UsageCommitter => Handler.UsageCommitter;
        public CombatRescueResolver RescueResolver => Handler.RescueResolver;
        public CombatPetTriggerOrderKeyProvider OrderKeyProvider =>
            _deathTriggerSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>> DiscoverTriggers(
            CombatState state,
            CombatEvent sourceEvent)
        {
            return _deathTriggerSource.DiscoverTriggers(state, sourceEvent);
        }
    }
}
