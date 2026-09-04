using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetCardTriggerUsageRegistry
    {
        private readonly HashSet<
            CombatPetCardTriggerKey>
            _registeredKeys;

        private readonly List<
            CombatPetCardTriggerKey>
            _keys;

        private readonly ReadOnlyCollection<
            CombatPetCardTriggerKey>
            _readOnlyKeys;

        public CombatPetCardTriggerUsageRegistry()
        {
            _registeredKeys =
                new HashSet<
                    CombatPetCardTriggerKey>();

            _keys =
                new List<
                    CombatPetCardTriggerKey>();

            _readOnlyKeys =
                _keys.AsReadOnly();
        }

        public int Count =>
            _keys.Count;

        public IReadOnlyList<
            CombatPetCardTriggerKey>
            Keys =>
                _readOnlyKeys;

        public bool Contains(
            CombatPetCardTriggerKey key)
        {
            ValidateKey(
                key);

            return _registeredKeys.Contains(
                key);
        }

        public bool Contains(
            InstanceId petInstanceId,
            InstanceId cardInstanceId)
        {
            return Contains(
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    cardInstanceId));
        }

        public bool TryRegister(
            CombatPetCardTriggerKey key)
        {
            ValidateKey(
                key);

            if (!_registeredKeys.Add(
                    key))
            {
                return false;
            }

            _keys.Add(
                key);

            return true;
        }

        public bool TryRegister(
            InstanceId petInstanceId,
            InstanceId cardInstanceId)
        {
            return TryRegister(
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    cardInstanceId));
        }

        private static void ValidateKey(
            CombatPetCardTriggerKey key)
        {
            if (!key.IsValid)
            {
                throw new ArgumentException(
                    "Pet card trigger usage registry " +
                    "requires a valid key.",
                    nameof(key));
            }
        }
    }
}