using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class SunBirdPetTriggerSource :
        ICombatTriggerSource
    {
        private readonly
            CombatPetNormalAttackTriggerSource
            _normalAttackTriggerSource;

        public SunBirdPetTriggerSource(
            CombatSide side,
            InstanceId petInstanceId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
        {
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (sourceDamageModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        sourceDamageModifierRegistry));
            }

            Handler =
                new
                    SunBirdPetNormalAttackTriggerHandler(
                        side,
                        petInstanceId,
                        usageCommitter,
                        sourceDamageModifierRegistry);

            _normalAttackTriggerSource =
                new
                    CombatPetNormalAttackTriggerSource(
                        Handler);
        }

        public
            SunBirdPetNormalAttackTriggerHandler
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

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                Handler
                    .SourceDamageModifierRegistry;

        public CombatPetTriggerOrderKeyProvider
            OrderKeyProvider =>
                _normalAttackTriggerSource
                    .OrderKeyProvider;

        public IEnumerable<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            DiscoverTriggers(
                CombatState state,
                CombatEvent sourceEvent)
        {
            return _normalAttackTriggerSource
                .DiscoverTriggers(
                    state,
                    sourceEvent);
        }
    }
}