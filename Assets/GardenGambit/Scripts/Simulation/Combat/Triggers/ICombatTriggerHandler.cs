using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public interface ICombatTriggerHandler
    {
        bool CanTrigger(
            CombatState state,
            CombatEvent sourceEvent);

        void Resolve(
            CombatState state,
            CombatEvent sourceEvent);
    }
}