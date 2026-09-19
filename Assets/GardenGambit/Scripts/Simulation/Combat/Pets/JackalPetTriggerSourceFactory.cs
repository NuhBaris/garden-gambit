using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        JackalPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatFinalRankModifierRegistry
            _finalRankModifierRegistry;

        public JackalPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetCardTriggerUsageCommitter
                usageCommitter,
            CombatFinalRankModifierRegistry
                finalRankModifierRegistry)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Jackal factory requires a valid " +
                    "Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (finalRankModifierRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(
                        finalRankModifierRegistry));
            }

            PetDefinitionId =
                petDefinitionId;

            _usageCommitter =
                usageCommitter;

            _finalRankModifierRegistry =
                finalRankModifierRegistry;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatFinalRankModifierRegistry
            FinalRankModifierRegistry =>
                _finalRankModifierRegistry;

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
                    "Jackal source factory requires " +
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
                    "Jackal factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new JackalPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _finalRankModifierRegistry)
            };
        }
    }
}
