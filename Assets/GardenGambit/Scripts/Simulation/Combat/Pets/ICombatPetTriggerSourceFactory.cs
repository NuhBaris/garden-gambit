using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;

namespace GardenGambit.Simulation.Combat
{
    public interface ICombatPetTriggerSourceFactory
    {
        DefinitionId PetDefinitionId
        {
            get;
        }

        IEnumerable<ICombatTriggerSource>
            CreateSources(
                CombatSide side,
                CombatPetState pet);
    }
}