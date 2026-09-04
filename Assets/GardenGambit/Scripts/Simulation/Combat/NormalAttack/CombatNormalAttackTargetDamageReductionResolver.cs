using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackTargetDamageReductionResolver
    {
        private readonly
            CombatNormalAttackTargetDamageReductionRegistry
            _reductionRegistry;

        private readonly
            CombatPetCardTriggerUsageCommitter
            _usageCommitter;

        public
            CombatNormalAttackTargetDamageReductionResolver(
                CombatNormalAttackTargetDamageReductionRegistry
                    reductionRegistry,
                CombatPetCardTriggerUsageCommitter
                    usageCommitter)
        {
            if (reductionRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(reductionRegistry));
            }

            if (usageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(usageCommitter));
            }

            _reductionRegistry =
                reductionRegistry;

            _usageCommitter =
                usageCommitter;
        }

        public
            CombatNormalAttackTargetDamageReductionRegistry
            ReductionRegistry =>
                _reductionRegistry;

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public int ResolveDamage(
            NormalAttackCombatEvent
                normalAttackEvent,
            int incomingDamage)
        {
            if (normalAttackEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(normalAttackEvent));
            }

            if (incomingDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incomingDamage),
                    incomingDamage,
                    "Incoming Normal Attack damage " +
                    "cannot be negative.");
            }

            var requests =
                _reductionRegistry.GetRequests(
                    normalAttackEvent
                        .Metadata.EventId);

            ValidateRequestsTarget(
                normalAttackEvent,
                requests);

            var resolvedDamage =
                incomingDamage;

            for (var index = 0;
                 index < requests.Count;
                 index++)
            {
                if (resolvedDamage <= 0)
                {
                    break;
                }

                var request =
                    requests[index];

                if (_usageCommitter.HasTriggered(
                        request.UsageKey))
                {
                    continue;
                }

                var actualReduction =
                    Math.Min(
                        resolvedDamage,
                        request.ReductionAmount);

                if (actualReduction <= 0)
                {
                    continue;
                }

                var damageAfterReduction =
                    checked(
                        resolvedDamage -
                        actualReduction);

                var wasCommitted =
                    _usageCommitter.TryCommit(
                        request.UsageKey,
                        () =>
                        {
                            resolvedDamage =
                                damageAfterReduction;
                        });

                if (!wasCommitted)
                {
                    continue;
                }
            }

            _reductionRegistry.RemoveRequests(
                normalAttackEvent
                    .Metadata.EventId);

            return resolvedDamage;
        }

        private static void ValidateRequestsTarget(
            NormalAttackCombatEvent
                normalAttackEvent,
            System.Collections.Generic.IReadOnlyList<
                CombatNormalAttackTargetDamageReductionRequest>
                requests)
        {
            for (var index = 0;
                 index < requests.Count;
                 index++)
            {
                var request =
                    requests[index];

                if (request.NormalAttackEventId !=
                    normalAttackEvent
                        .Metadata.EventId)
                {
                    throw new InvalidOperationException(
                        "Target damage reduction request " +
                        "belongs to a different Normal " +
                        "Attack event.");
                }

                if (request.TargetCardInstanceId !=
                    normalAttackEvent.TargetInstanceId)
                {
                    throw new InvalidOperationException(
                        "Target damage reduction request " +
                        "belongs to a different target " +
                        "card.");
                }
            }
        }
    }
}