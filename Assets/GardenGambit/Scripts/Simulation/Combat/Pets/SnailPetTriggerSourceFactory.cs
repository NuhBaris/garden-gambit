using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        SnailPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly
            CombatPetTriggerUsageCommitter
            _usageCommitter;

        private readonly
            CombatArmorGainResolver
            _armorGainResolver;

        public SnailPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetTriggerUsageCommitter
                usageCommitter,
            CombatArmorGainResolver
                armorGainResolver)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Snail factory requires a valid " +
                    "Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (armorGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        armorGainResolver));
            }

            PetDefinitionId =
                petDefinitionId;

            _usageCommitter =
                usageCommitter;

            _armorGainResolver =
                armorGainResolver;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public
            CombatArmorGainResolver
            ArmorGainResolver =>
                _armorGainResolver;

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
                    "Snail source factory requires " +
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
                    "Snail factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new SnailPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _armorGainResolver)
            };
        }
    }
}
