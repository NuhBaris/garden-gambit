using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatPetTriggerSourceBuilder
    {
        private readonly
            CombatPetTriggerSourceFactoryRegistry
            _factoryRegistry;

        public CombatPetTriggerSourceBuilder(
            CombatPetTriggerSourceFactoryRegistry
                factoryRegistry)
        {
            if (factoryRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(factoryRegistry));
            }

            _factoryRegistry =
                factoryRegistry;
        }

        public CombatPetTriggerSourceFactoryRegistry
            FactoryRegistry =>
                _factoryRegistry;

        public IReadOnlyList<ICombatTriggerSource>
            BuildSources(
                CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            var sources =
                new List<ICombatTriggerSource>();

            AppendSideSources(
                state.PlayerPets,
                sources);

            AppendSideSources(
                state.EnemyPets,
                sources);

            return new ReadOnlyCollection<
                ICombatTriggerSource>(
                    sources);
        }

        public CombatTriggerSourceRegistry
            BuildRegistry(
                CombatState state)
        {
            return new CombatTriggerSourceRegistry(
                BuildSources(
                    state));
        }

        private void AppendSideSources(
            CombatSidePetState sidePetState,
            List<ICombatTriggerSource> sources)
        {
            foreach (var pet in
                     sidePetState.Pets.Pets)
            {
                ICombatPetTriggerSourceFactory
                    factory;

                var wasFound =
                    _factoryRegistry.TryGetFactory(
                        pet.DefinitionId,
                        out factory);

                if (!wasFound)
                {
                    throw new InvalidOperationException(
                        $"Pet trigger source factory was " +
                        $"not registered for Pet " +
                        $"DefinitionId: " +
                        $"{pet.DefinitionId}.");
                }

                var createdSources =
                    factory.CreateSources(
                        sidePetState.Side,
                        pet);

                if (createdSources == null)
                {
                    throw new InvalidOperationException(
                        $"Pet trigger source factory " +
                        $"{factory.PetDefinitionId} " +
                        $"returned null.");
                }

                foreach (var source in
                         createdSources)
                {
                    if (source == null)
                    {
                        throw new InvalidOperationException(
                            $"Pet trigger source factory " +
                            $"{factory.PetDefinitionId} " +
                            $"returned a null source.");
                    }

                    sources.Add(
                        source);
                }
            }
        }
    }
}