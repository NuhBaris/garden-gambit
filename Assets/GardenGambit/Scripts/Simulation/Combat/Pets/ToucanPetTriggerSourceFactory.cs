using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ToucanPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        public ToucanPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Toucan factory requires a valid " +
                    "Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (hpGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(hpGainResolver));
            }

            PetDefinitionId = petDefinitionId;
            _usageCommitter = usageCommitter;
            _hpGainResolver = hpGainResolver;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public IEnumerable<ICombatTriggerSource>
            CreateSources(
                CombatSide side,
                CombatPetState pet)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Toucan source factory requires " +
                    "Player or Enemy side.");
            }

            if (pet == null)
            {
                throw new ArgumentNullException(
                    nameof(pet));
            }

            if (pet.DefinitionId != PetDefinitionId)
            {
                throw new ArgumentException(
                    "Pet DefinitionId does not match " +
                    "the Toucan factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new ToucanPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _hpGainResolver)
            };
        }
    }
}
