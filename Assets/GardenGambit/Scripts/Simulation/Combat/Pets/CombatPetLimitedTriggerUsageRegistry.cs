using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetLimitedTriggerUsageRegistry
    {
        private readonly Dictionary<InstanceId, int>
            _usageCounts;

        private readonly List<InstanceId>
            _petInstanceIds;

        private readonly ReadOnlyCollection<InstanceId>
            _readOnlyPetInstanceIds;

        private int _totalUsageCount;

        public CombatPetLimitedTriggerUsageRegistry()
        {
            _usageCounts = new Dictionary<InstanceId, int>();
            _petInstanceIds = new List<InstanceId>();
            _readOnlyPetInstanceIds =
                _petInstanceIds.AsReadOnly();
        }

        public int Count =>
            _petInstanceIds.Count;

        public int TotalUsageCount =>
            _totalUsageCount;

        public IReadOnlyList<InstanceId>
            PetInstanceIds =>
                _readOnlyPetInstanceIds;

        public int GetUsageCount(
            InstanceId petInstanceId)
        {
            ValidatePetInstanceId(
                petInstanceId);

            int usageCount;

            return _usageCounts.TryGetValue(
                petInstanceId,
                out usageCount)
                ? usageCount
                : 0;
        }

        public bool HasReachedLimit(
            InstanceId petInstanceId,
            int maximumUsageCount)
        {
            ValidateMaximumUsageCount(
                maximumUsageCount);

            return GetUsageCount(
                       petInstanceId) >=
                   maximumUsageCount;
        }

        public bool TryRegisterUse(
            InstanceId petInstanceId,
            int maximumUsageCount)
        {
            ValidateMaximumUsageCount(
                maximumUsageCount);

            var usageCount = GetUsageCount(
                petInstanceId);

            if (usageCount >= maximumUsageCount)
            {
                return false;
            }

            if (usageCount == 0)
            {
                _usageCounts.Add(
                    petInstanceId,
                    1);
                _petInstanceIds.Add(
                    petInstanceId);
            }
            else
            {
                _usageCounts[petInstanceId] =
                    checked(usageCount + 1);
            }

            _totalUsageCount = checked(
                _totalUsageCount + 1);

            return true;
        }

        private static void ValidatePetInstanceId(
            InstanceId petInstanceId)
        {
            if (!petInstanceId.IsValid)
            {
                throw new ArgumentException(
                    "Limited Pet trigger usage requires " +
                    "a valid Pet InstanceId.",
                    nameof(petInstanceId));
            }
        }

        private static void ValidateMaximumUsageCount(
            int maximumUsageCount)
        {
            if (maximumUsageCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumUsageCount),
                    maximumUsageCount,
                    "Maximum Pet trigger usage count " +
                    "must be greater than zero.");
            }
        }
    }
}
