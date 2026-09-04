using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public sealed class CombatPetRegistry
    {
        private readonly List<CombatPetState>
            _pets;

        private readonly ReadOnlyCollection<
            CombatPetState> _readOnlyPets;

        public CombatPetRegistry(
            IEnumerable<CombatPetState> pets)
        {
            if (pets == null)
            {
                throw new ArgumentNullException(
                    nameof(pets));
            }

            var instanceIds =
                new HashSet<InstanceId>();

            _pets =
                new List<CombatPetState>();

            foreach (var pet in pets)
            {
                if (pet == null)
                {
                    throw new ArgumentException(
                        "Combat Pet registry cannot " +
                        "contain a null Pet.",
                        nameof(pets));
                }

                if (!instanceIds.Add(
                        pet.InstanceId))
                {
                    throw new ArgumentException(
                        $"Duplicate Pet InstanceId " +
                        $"detected: {pet.InstanceId}.",
                        nameof(pets));
                }

                _pets.Add(
                    pet);
            }

            _readOnlyPets =
                _pets.AsReadOnly();
        }

        public int Count =>
            _pets.Count;

        public IReadOnlyList<CombatPetState> Pets =>
            _readOnlyPets;

        public CombatPetState GetPet(
            InstanceId instanceId)
        {
            if (!instanceId.IsValid)
            {
                throw new ArgumentException(
                    "A valid Pet InstanceId is required.",
                    nameof(instanceId));
            }

            foreach (var pet in _pets)
            {
                if (pet.InstanceId ==
                    instanceId)
                {
                    return pet;
                }
            }

            throw new KeyNotFoundException(
                $"Combat Pet was not found: " +
                $"{instanceId}.");
        }
    }
}