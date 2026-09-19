using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        FruitBatPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatHpGainResolver
            _hpGainResolver;

        private readonly CombatEventLog
            _eventLog;

        public FruitBatPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatHpGainResolver hpGainResolver,
            CombatEventLog eventLog)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Fruit Bat factory requires a valid " +
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

            if (eventLog == null)
            {
                throw new ArgumentNullException(
                    nameof(eventLog));
            }

            PetDefinitionId =
                petDefinitionId;

            _usageCommitter =
                usageCommitter;

            _hpGainResolver =
                hpGainResolver;

            _eventLog =
                eventLog;
        }

        public DefinitionId PetDefinitionId
        {
            get;
        }

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatHpGainResolver HpGainResolver =>
            _hpGainResolver;

        public CombatEventLog EventLog =>
            _eventLog;

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
                    "Fruit Bat source factory requires " +
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
                    "the Fruit Bat factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new FruitBatPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _hpGainResolver,
                    _eventLog)
            };
        }
    }
}
