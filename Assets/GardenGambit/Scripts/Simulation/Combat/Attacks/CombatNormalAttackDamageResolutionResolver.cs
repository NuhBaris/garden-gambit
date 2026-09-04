using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackDamageResolutionResolver
    {
        public CombatNormalAttackDamageResolution
            Resolve(
                CombatNormalAttackEventBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(
                    nameof(batch));
            }

            return new
                CombatNormalAttackDamageResolution(
                    batch,
                    resolvedDamageToEnemy:
                        batch.PlayerAttackEvent
                            .BaseDamage,
                    resolvedDamageToPlayer:
                        batch.EnemyAttackEvent
                            .BaseDamage);
        }

        public CombatNormalAttackDamageResolution
            Resolve(
                CombatNormalAttackEventBatch batch,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(
                    nameof(batch));
            }

            if (resolveDamage == null)
            {
                throw new ArgumentNullException(
                    nameof(resolveDamage));
            }

            var resolvedDamageToEnemy =
                resolveDamage(
                    batch.PlayerAttackEvent);

            ValidateResolvedDamage(
                resolvedDamageToEnemy,
                CombatSide.Player);

            var resolvedDamageToPlayer =
                resolveDamage(
                    batch.EnemyAttackEvent);

            ValidateResolvedDamage(
                resolvedDamageToPlayer,
                CombatSide.Enemy);

            return new
                CombatNormalAttackDamageResolution(
                    batch,
                    resolvedDamageToEnemy,
                    resolvedDamageToPlayer);
        }

        public CombatNormalAttackDamageResolution
            Resolve(
                CombatNormalAttackEventBatch batch,
                Func<NormalAttackCombatEvent, int>
                    resolveDamage,
                CombatNormalAttackTargetDamageReductionResolver
                    targetReductionResolver)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(
                    nameof(batch));
            }

            if (resolveDamage == null)
            {
                throw new ArgumentNullException(
                    nameof(resolveDamage));
            }

            if (targetReductionResolver == null)
            {
                throw new ArgumentNullException(
                    nameof(targetReductionResolver));
            }

            var sourceResolvedDamageToEnemy =
                resolveDamage(
                    batch.PlayerAttackEvent);

            ValidateResolvedDamage(
                sourceResolvedDamageToEnemy,
                CombatSide.Player);

            var sourceResolvedDamageToPlayer =
                resolveDamage(
                    batch.EnemyAttackEvent);

            ValidateResolvedDamage(
                sourceResolvedDamageToPlayer,
                CombatSide.Enemy);

            var resolvedDamageToEnemy =
                targetReductionResolver
                    .ResolveDamage(
                        batch.PlayerAttackEvent,
                        sourceResolvedDamageToEnemy);

            var resolvedDamageToPlayer =
                targetReductionResolver
                    .ResolveDamage(
                        batch.EnemyAttackEvent,
                        sourceResolvedDamageToPlayer);

            ValidateResolvedDamage(
                resolvedDamageToEnemy,
                CombatSide.Player);

            ValidateResolvedDamage(
                resolvedDamageToPlayer,
                CombatSide.Enemy);

            return new
                CombatNormalAttackDamageResolution(
                    batch,
                    resolvedDamageToEnemy,
                    resolvedDamageToPlayer);
        }

        private static void ValidateResolvedDamage(
            int resolvedDamage,
            CombatSide attackerSide)
        {
            if (resolvedDamage >= 0)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Resolved {attackerSide} normal attack " +
                $"damage cannot be negative.");
        }
    }
}