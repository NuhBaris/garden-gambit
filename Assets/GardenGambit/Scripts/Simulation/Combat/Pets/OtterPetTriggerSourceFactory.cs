using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        OtterPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatAttackGainResolver
            _attackGainResolver;

        public OtterPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatAttackGainResolver attackGainResolver)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Otter factory requires a valid " +
                    "Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            if (attackGainResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(attackGainResolver));
            }

            PetDefinitionId =
                petDefinitionId;

            _usageCommitter =
                usageCommitter;

            _attackGainResolver =
                attackGainResolver;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatAttackGainResolver
            AttackGainResolver =>
                _attackGainResolver;

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
                    "Otter source factory requires " +
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
                    "Pet DefinitionId does not match " +
                    "the Otter factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new OtterPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _attackGainResolver)
            };
        }
    }
}
