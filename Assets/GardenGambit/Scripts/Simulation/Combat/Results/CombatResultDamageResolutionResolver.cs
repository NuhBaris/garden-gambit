using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatResultDamageResolutionResolver
    {
        private readonly ProtectiveSealCountResolver
            _protectiveSealCountResolver;

        private readonly
            ProtectiveSealResultDamageResolver
            _protectiveSealDamageResolver;

        public CombatResultDamageResolutionResolver()
        {
            _protectiveSealCountResolver =
                new ProtectiveSealCountResolver();

            _protectiveSealDamageResolver =
                new ProtectiveSealResultDamageResolver();
        }

        public CombatResultDamageResolution Resolve(
            CombatState state,
            CombatResultDamageCalculation calculation)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (!calculation.IsValid)
            {
                throw new ArgumentException(
                    "A valid combat result damage " +
                    "calculation is required.",
                    nameof(calculation));
            }

            var playerProtectiveSealCount =
                _protectiveSealCountResolver.Resolve(
                    state,
                    CombatSide.Player);

            var enemyProtectiveSealCount =
                _protectiveSealCountResolver.Resolve(
                    state,
                    CombatSide.Enemy);

            var resolvedIncomingDamageToPlayer =
                _protectiveSealDamageResolver.Resolve(
                    calculation
                        .BaseIncomingDamageToPlayer,
                    playerProtectiveSealCount);

            var resolvedIncomingDamageToEnemy =
                _protectiveSealDamageResolver.Resolve(
                    calculation
                        .BaseIncomingDamageToEnemy,
                    enemyProtectiveSealCount);

            return new CombatResultDamageResolution(
                calculation,
                resolvedIncomingDamageToPlayer,
                resolvedIncomingDamageToEnemy);
        }
    }
}