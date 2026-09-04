using GardenGambit.Domain.Identity;

namespace GardenGambit.Domain.Combat
{
    public static class CombatPetDefinitionIds
    {
        public const string SunBirdValue =
            "pet.sun_bird";

        public const string PolarFerretValue =
            "pet.polar_ferret";

        public static DefinitionId SunBird =>
            new DefinitionId(
                SunBirdValue);

        public static DefinitionId PolarFerret =>
            new DefinitionId(
                PolarFerretValue);
    }
}