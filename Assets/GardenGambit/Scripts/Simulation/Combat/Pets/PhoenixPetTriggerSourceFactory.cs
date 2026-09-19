using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class PhoenixPetTriggerSourceFactory : ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetTriggerUsageCommitter _usageCommitter;
        private readonly CombatRescueResolver _rescueResolver;

        public PhoenixPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetTriggerUsageCommitter usageCommitter,
            CombatRescueResolver rescueResolver)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Phoenix factory requires a valid Pet DefinitionId.",
                    nameof(petDefinitionId));
            }
            if (usageCommitter == null)
            {
                throw new ArgumentNullException(nameof(usageCommitter));
            }
            if (rescueResolver == null)
            {
                throw new ArgumentNullException(nameof(rescueResolver));
            }

            PetDefinitionId = petDefinitionId;
            _usageCommitter = usageCommitter;
            _rescueResolver = rescueResolver;
        }

        public DefinitionId PetDefinitionId { get; }
        public CombatPetTriggerUsageCommitter UsageCommitter => _usageCommitter;
        public CombatRescueResolver RescueResolver => _rescueResolver;

        public IEnumerable<ICombatTriggerSource> CreateSources(
            CombatSide side,
            CombatPetState pet)
        {
            if (side != CombatSide.Player && side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(nameof(side), side,
                    "Phoenix source factory requires Player or Enemy side.");
            }
            if (pet == null)
            {
                throw new ArgumentNullException(nameof(pet));
            }
            if (pet.DefinitionId != PetDefinitionId)
            {
                throw new ArgumentException(
                    "Pet DefinitionId does not match the Phoenix factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new PhoenixPetTriggerSource(
                    side, pet.InstanceId, _usageCommitter, _rescueResolver)
            };
        }
    }
}
