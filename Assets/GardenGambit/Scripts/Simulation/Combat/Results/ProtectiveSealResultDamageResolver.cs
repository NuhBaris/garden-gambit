using System;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        ProtectiveSealResultDamageResolver
    {
        private const int PercentageBase = 100;

        private const int RemainingDamagePercentage = 95;

        public int Resolve(
            int incomingDamage,
            int activeProtectiveSealCount)
        {
            if (incomingDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incomingDamage),
                    incomingDamage,
                    "Incoming result damage cannot be " +
                    "negative.");
            }

            if (activeProtectiveSealCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activeProtectiveSealCount),
                    activeProtectiveSealCount,
                    "Active Protective Seal count cannot " +
                    "be negative.");
            }

            var resolvedDamage =
                incomingDamage;

            for (var index = 0;
                 index < activeProtectiveSealCount;
                 index++)
            {
                var scaledDamage =
                    checked(
                        (long)resolvedDamage *
                        RemainingDamagePercentage);

                resolvedDamage =
                    checked(
                        (int)(
                            (scaledDamage +
                             PercentageBase - 1) /
                            PercentageBase));
            }

            return resolvedDamage;
        }
    }
}