using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetLimitedTriggerUsageCommitter
    {
        private readonly CombatPetLimitedTriggerUsageRegistry
            _usageRegistry;

        public CombatPetLimitedTriggerUsageCommitter(
            CombatPetLimitedTriggerUsageRegistry usageRegistry)
        {
            if (usageRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(usageRegistry));
            }

            _usageRegistry = usageRegistry;
        }

        public CombatPetLimitedTriggerUsageRegistry
            UsageRegistry =>
                _usageRegistry;

        public int GetUsageCount(
            InstanceId petInstanceId)
        {
            return _usageRegistry.GetUsageCount(
                petInstanceId);
        }

        public bool HasReachedLimit(
            InstanceId petInstanceId,
            int maximumUsageCount)
        {
            return _usageRegistry.HasReachedLimit(
                petInstanceId,
                maximumUsageCount);
        }

        public bool TryCommit(
            InstanceId petInstanceId,
            int maximumUsageCount,
            Action resolveTrigger)
        {
            if (resolveTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(resolveTrigger));
            }

            if (_usageRegistry.HasReachedLimit(
                    petInstanceId,
                    maximumUsageCount))
            {
                return false;
            }

            resolveTrigger();

            var wasRegistered =
                _usageRegistry.TryRegisterUse(
                    petInstanceId,
                    maximumUsageCount);

            if (!wasRegistered)
            {
                throw new InvalidOperationException(
                    "Limited Pet trigger usage reached " +
                    "its limit while resolution was " +
                    "still in progress.");
            }

            return true;
        }
    }
}
