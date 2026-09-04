using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        PolarFerretPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        public PolarFerretPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Polar Ferret factory requires a " +
                    "valid Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        targetDamageReductionRegistry));
            }

            PetDefinitionId =
                petDefinitionId;

            _usageCommitter =
                usageCommitter;

            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public
            CombatNormalAttackTargetDamageReductionRegistry
            TargetDamageReductionRegistry =>
                _targetDamageReductionRegistry;

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
                    "Polar Ferret source factory " +
                    "requires Player or Enemy side.");
            }

            if (pet == null)
            {
                throw new ArgumentNullException(
                    nameof(pet));
            }

            if (pet.DefinitionId !=
                PetDefinitionId)
            {
                throw new ArgumentException(
                    "Pet DefinitionId does not match the " +
                    "Polar Ferret factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new PolarFerretPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _targetDamageReductionRegistry)
            };
        }
    }
}