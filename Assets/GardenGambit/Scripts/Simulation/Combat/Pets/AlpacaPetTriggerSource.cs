using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class AlpacaPetTriggerSource : ICombatTriggerSource
    {
        private readonly CombatPetTriggerSource _petTriggerSource;

        public AlpacaPetTriggerSource(
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

            Handler = new AlpacaPetBattleStartTriggerHandler(
                side,
                petInstanceId,
                usageCommitter,
                attackGainResolver);

            _petTriggerSource = new CombatPetTriggerSource(
                side,
                petInstanceId,
                Handler);
        }

        public AlpacaPetBattleStartTriggerHandler Handler { get; }

        public CombatSide Side => Handler.Side;

        public InstanceId PetInstanceId => Handler.PetInstanceId;

        public CombatPetTriggerUsageCommitter UsageCommitter =>
            Handler.UsageCommitter;

        public CombatAttackGainResolver AttackGainResolver =>
            Handler.AttackGainResolver;

        public CombatPetTriggerOrderKeyProvider OrderKeyProvider =>
            _petTriggerSource.OrderKeyProvider;

        public IEnumerable<CombatTriggerCandidate<ICombatTriggerHandler>>
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