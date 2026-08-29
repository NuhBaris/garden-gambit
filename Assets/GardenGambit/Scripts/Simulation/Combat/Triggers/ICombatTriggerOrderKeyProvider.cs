using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public interface ICombatTriggerOrderKeyProvider
    {
        CombatTriggerOrderKey GetOrderKey(
            CombatState state,
            CombatEvent sourceEvent);
    }
}