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

        private readonly
            CombatPetLimitedTriggerUsageCommitter
            _limitedUsageCommitter;

        public
            CombatNormalAttackTargetDamageReductionResolver(
                CombatNormalAttackTargetDamageReductionRegistry
                    reductionRegistry,
            CombatPetCardTriggerUsageCommitter
                usageCommitter)
            : this(
                reductionRegistry,
                usageCommitter,
                new CombatPetLimitedTriggerUsageCommitter(
                    new
                        CombatPetLimitedTriggerUsageRegistry()))
        {
        }

        public
            CombatNormalAttackTargetDamageReductionResolver(
                CombatNormalAttackTargetDamageReductionRegistry
                    reductionRegistry,
                CombatPetCardTriggerUsageCommitter
                    usageCommitter,
                CombatPetLimitedTriggerUsageCommitter
                    limitedUsageCommitter)
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

            if (limitedUsageCommitter == null)
            {
                throw new ArgumentNullException(
                    nameof(limitedUsageCommitter));
            }

            _reductionRegistry =
                reductionRegistry;

            _usageCommitter =
                usageCommitter;

            _limitedUsageCommitter =
                limitedUsageCommitter;
        }

        public
            CombatNormalAttackTargetDamageReductionRegistry
            ReductionRegistry =>
                _reductionRegistry;

        public CombatPetCardTriggerUsageCommitter
            UsageCommitter =>
                _usageCommitter;

        public CombatPetLimitedTriggerUsageCommitter
            LimitedUsageCommitter =>
                _limitedUsageCommitter;

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

                if (HasReachedUsageLimit(
                        request))
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

                var wasCommitted = TryCommitUsage(
                    request,
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

        private bool HasReachedUsageLimit(
            CombatNormalAttackTargetDamageReductionRequest
                request)
        {
            if (request.UsesLimitedPetUsage)
            {
                return _limitedUsageCommitter
                    .HasReachedLimit(
                        request.PetInstanceId,
                        request.MaximumPetUsageCount);
            }

            return _usageCommitter.HasTriggered(
                request.UsageKey);
        }

        private bool TryCommitUsage(
            CombatNormalAttackTargetDamageReductionRequest
                request,
            Action resolveReduction)
        {
            if (request.UsesLimitedPetUsage)
            {
                return _limitedUsageCommitter.TryCommit(
                    request.PetInstanceId,
                    request.MaximumPetUsageCount,
                    resolveReduction);
            }

            return _usageCommitter.TryCommit(
                request.UsageKey,
                resolveReduction);
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
