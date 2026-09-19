using System;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class CombatPetTriggerUsageCommitter
    {
        private readonly CombatPetTriggerUsageRegistry _usageRegistry;

        public CombatPetTriggerUsageCommitter(CombatPetTriggerUsageRegistry usageRegistry)
        {
            if (usageRegistry == null)
            {
                throw new ArgumentNullException(nameof(usageRegistry));
            }

            _usageRegistry = usageRegistry;
        }

        public CombatPetTriggerUsageRegistry UsageRegistry => _usageRegistry;

        public bool HasTriggered(InstanceId petInstanceId)
        {
            return _usageRegistry.Contains(petInstanceId);
        }

        public bool TryCommit(InstanceId petInstanceId, Action resolveTrigger)
        {
            if (resolveTrigger == null)
            {
                throw new ArgumentNullException(nameof(resolveTrigger));
            }

            if (_usageRegistry.Contains(petInstanceId))
            {
                return false;
            }

            resolveTrigger();

            var wasRegistered = _usageRegistry.TryRegister(petInstanceId);

            if (!wasRegistered)
            {
                throw new InvalidOperationException(
                    "Pet trigger usage was registered while its resolution " +
                    "was still in progress.");
            }

            return true;
        }
    }
}
