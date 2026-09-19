using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ChinchillaPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatNormalAttackSourceDamageModifierRegistry
            _sourceDamageModifierRegistry;

        public ChinchillaPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatNormalAttackSourceDamageModifierRegistry
                sourceDamageModifierRegistry)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Chinchilla factory requires a valid " +
                    "Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

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

            PetDefinitionId =
                petDefinitionId;

            _usageCommitter =
                usageCommitter;

            _sourceDamageModifierRegistry =
                sourceDamageModifierRegistry;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public
            CombatNormalAttackSourceDamageModifierRegistry
            SourceDamageModifierRegistry =>
                _sourceDamageModifierRegistry;

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
                    "Chinchilla source factory requires " +
                    "Player or Enemy side.");
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
                    "Chinchilla factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new ChinchillaPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _sourceDamageModifierRegistry)
            };
        }
    }
}
