using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatPetTriggerUsageRegistry
    {
        private readonly HashSet<InstanceId> _registeredPetInstanceIds;
        private readonly List<InstanceId> _petInstanceIds;
        private readonly ReadOnlyCollection<InstanceId> _readOnlyPetInstanceIds;

        public CombatPetTriggerUsageRegistry()
        {
            _registeredPetInstanceIds = new HashSet<InstanceId>();
            _petInstanceIds = new List<InstanceId>();
            _readOnlyPetInstanceIds = _petInstanceIds.AsReadOnly();
        }

        public int Count => _petInstanceIds.Count;

        public IReadOnlyList<InstanceId> PetInstanceIds => _readOnlyPetInstanceIds;

        public bool Contains(InstanceId petInstanceId)
        {
            ValidatePetInstanceId(petInstanceId);
            return _registeredPetInstanceIds.Contains(petInstanceId);
        }

        public bool TryRegister(InstanceId petInstanceId)
        {
            ValidatePetInstanceId(petInstanceId);

            if (!_registeredPetInstanceIds.Add(petInstanceId))
            {
                return false;
            }

            _petInstanceIds.Add(petInstanceId);
            return true;
        }

        private static void ValidatePetInstanceId(InstanceId petInstanceId)
        {
            if (!petInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Pet trigger usage registry requires a valid Pet InstanceId.",
                    nameof(petInstanceId));
            }
        }
    }
}
