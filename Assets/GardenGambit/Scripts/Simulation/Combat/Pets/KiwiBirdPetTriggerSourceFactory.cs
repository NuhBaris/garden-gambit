using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        KiwiBirdPetTriggerSourceFactory :
        ICombatPetTriggerSourceFactory
    {
        private readonly CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        private readonly CombatArmorGainResolver
            _armorGainResolver;

        private readonly CombatEventLog
            _eventLog;

        public KiwiBirdPetTriggerSourceFactory(
            DefinitionId petDefinitionId,
            CombatPetCardTriggerUsageCommitter usageCommitter,
            CombatArmorGainResolver armorGainResolver,
            CombatEventLog eventLog)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "Kiwi Bird factory requires a valid " +
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
                    nameof(armorGainResolver));
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

            _armorGainResolver =
                armorGainResolver;

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

        public CombatArmorGainResolver
            ArmorGainResolver =>
                _armorGainResolver;

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
                    "Kiwi Bird source factory requires " +
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
                    "the Kiwi Bird factory registration.",
                    nameof(pet));
            }

            return new ICombatTriggerSource[]
            {
                new KiwiBirdPetTriggerSource(
                    side,
                    pet.InstanceId,
                    _usageCommitter,
                    _armorGainResolver,
                    _eventLog)
            };
        }
    }
}
