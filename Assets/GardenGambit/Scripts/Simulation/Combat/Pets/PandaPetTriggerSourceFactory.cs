using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class PandaPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetTriggerUsageCommitter
            _activationUsageCommitter;

        private readonly CombatPetLimitedTriggerUsageCommitter
            _limitedUsageCommitter;

        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _targetDamageReductionRegistry;

        public PandaPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetTriggerUsageCommitter
                activationUsageCommitter,
            CombatPetLimitedTriggerUsageCommitter
                limitedUsageCommitter,
            CombatNormalAttackTargetDamageReductionRegistry
                targetDamageReductionRegistry)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Panda factory requires a valid " +
                    "Pet DefinitionId.",
                    nameof(petDefinitionId));
            }

            if (activationUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(activationUsageCommitter));
            }

            if (limitedUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(limitedUsageCommitter));
            }

            if (targetDamageReductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageReductionRegistry));
            }

            PetDefinitionId = petDefinitionId;
            _activationUsageCommitter =
                activationUsageCommitter;
            _limitedUsageCommitter =
                limitedUsageCommitter;
            _targetDamageReductionRegistry =
                targetDamageReductionRegistry;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetTriggerUsageCommitter
            ActivationUsageCommitter =>
                _activationUsageCommitter;

        public CombatPetLimitedTriggerUsageCommitter
            LimitedUsageCommitter =>
                _limitedUsageCommitter;

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
                    "Panda source factory requires " +
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
                    "the Panda factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new PandaPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _activationUsageCommitter,
                    _limitedUsageCommitter,
                    _targetDamageReductionRegistry)
            };
        }
    }
}
