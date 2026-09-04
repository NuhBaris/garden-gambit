using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetTriggerSourceFactoryRegistry
    {
        private readonly Dictionary<
            DefinitionId,
            ICombatPetTriggerSourceFactory>
            _factoriesByDefinitionId;

        private readonly List<
            ICombatPetTriggerSourceFactory>
            _factories;

        private readonly ReadOnlyCollection<
            ICombatPetTriggerSourceFactory>
            _readOnlyFactories;

        public CombatPetTriggerSourceFactoryRegistry(
            IEnumerable<
                ICombatPetTriggerSourceFactory>
                factories)
        {
            if (factories == null)
            {
                throw new ArgumentNullException(
                    nameof(factories));
            }

            _factoriesByDefinitionId =
                new Dictionary<
                    DefinitionId,
                    ICombatPetTriggerSourceFactory>();

            _factories =
                new List<
                    ICombatPetTriggerSourceFactory>();

            foreach (var factory in factories)
            {
                if (factory == null)
                {
                    throw new ArgumentException(
                        "Pet trigger source factory " +
                        "registry cannot contain a null " +
                        "factory.",
                        nameof(factories));
                }

                if (!factory.PetDefinitionId.IsValid)
                {
                    throw new ArgumentException(
                        "Pet trigger source factory " +
                        "registry requires every factory " +
                        "to expose a valid DefinitionId.",
                        nameof(factories));
                }

                if (_factoriesByDefinitionId
                    .ContainsKey(
                        factory.PetDefinitionId))
                {
                    throw new ArgumentException(
                        $"Duplicate Pet trigger source " +
                        $"factory registration detected: " +
                        $"{factory.PetDefinitionId}.",
                        nameof(factories));
                }

                _factoriesByDefinitionId.Add(
                    factory.PetDefinitionId,
                    factory);

                _factories.Add(
                    factory);
            }

            _readOnlyFactories =
                _factories.AsReadOnly();
        }

        public int Count =>
            _factories.Count;

        public IReadOnlyList<
            ICombatPetTriggerSourceFactory>
            Factories =>
                _readOnlyFactories;

        public bool Contains(
            DefinitionId petDefinitionId)
        {
            ValidateDefinitionId(
                petDefinitionId);

            return _factoriesByDefinitionId
                .ContainsKey(
                    petDefinitionId);
        }

        public ICombatPetTriggerSourceFactory
            GetFactory(
                DefinitionId petDefinitionId)
        {
            ValidateDefinitionId(
                petDefinitionId);

            ICombatPetTriggerSourceFactory
                factory;

            if (_factoriesByDefinitionId
                .TryGetValue(
                    petDefinitionId,
                    out factory))
            {
                return factory;
            }

            throw new KeyNotFoundException(
                $"Pet trigger source factory was not " +
                $"found for DefinitionId: " +
                $"{petDefinitionId}.");
        }

        public bool TryGetFactory(
            DefinitionId petDefinitionId,
            out ICombatPetTriggerSourceFactory
                factory)
        {
            ValidateDefinitionId(
                petDefinitionId);

            return _factoriesByDefinitionId
                .TryGetValue(
                    petDefinitionId,
                    out factory);
        }

        private static void ValidateDefinitionId(
            DefinitionId petDefinitionId)
        {
            if (!petDefinitionId.IsValid)
            {
                throw new ArgumentException(
                    "A valid Pet DefinitionId is required.",
                    nameof(petDefinitionId));
            }
        }
    }
}