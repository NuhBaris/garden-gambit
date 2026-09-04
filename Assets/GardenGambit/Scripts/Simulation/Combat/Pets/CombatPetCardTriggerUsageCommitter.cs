using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetCardTriggerUsageCommitter
    {
        private readonly
            CombatPetCardTriggerUsageRegistry
            _usageRegistry;

        public CombatPetCardTriggerUsageCommitter(
            CombatPetCardTriggerUsageRegistry
                usageRegistry)
        {
            if (usageRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(usageRegistry));
            }

            _usageRegistry =
                usageRegistry;
        }

        public CombatPetCardTriggerUsageRegistry
            UsageRegistry =>
                _usageRegistry;

        public bool HasTriggered(
            CombatPetCardTriggerKey key)
        {
            return _usageRegistry.Contains(
                key);
        }

        public bool HasTriggered(
            InstanceId petInstanceId,
            InstanceId cardInstanceId)
        {
            return _usageRegistry.Contains(
                petInstanceId,
                cardInstanceId);
        }

        public bool TryCommit(
            CombatPetCardTriggerKey key,
            Action resolveTrigger)
        {
            if (resolveTrigger == null)
            {
                throw new ArgumentNullException(
                    nameof(resolveTrigger));
            }

            if (_usageRegistry.Contains(
                    key))
            {
                return false;
            }

            resolveTrigger();

            var wasRegistered =
                _usageRegistry.TryRegister(
                    key);

            if (!wasRegistered)
            {
                throw new InvalidOperationException(
                    "Pet card trigger usage was " +
                    "registered while its resolution " +
                    "was still in progress.");
            }

            return true;
        }

        public bool TryCommit(
            InstanceId petInstanceId,
            InstanceId cardInstanceId,
            Action resolveTrigger)
        {
            return TryCommit(
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    cardInstanceId),
                resolveTrigger);
        }
    }
}