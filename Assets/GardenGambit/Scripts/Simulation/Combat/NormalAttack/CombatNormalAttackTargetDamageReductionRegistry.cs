using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackTargetDamageReductionRegistry
    {
        private readonly Dictionary<
            CombatEventId,
            List<
                CombatNormalAttackTargetDamageReductionRequest>>
            _requestsByEventId;

        private int _count;

        public
            CombatNormalAttackTargetDamageReductionRegistry()
        {
            _requestsByEventId =
                new Dictionary<
                    CombatEventId,
                    List<
                        CombatNormalAttackTargetDamageReductionRequest>>();
        }

        public int Count =>
            _count;

        public int EventCount =>
            _requestsByEventId.Count;

        public bool TryRegister(
            CombatNormalAttackTargetDamageReductionRequest
                request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            List<
                CombatNormalAttackTargetDamageReductionRequest>
                requests;

            if (!_requestsByEventId.TryGetValue(
                    request.NormalAttackEventId,
                    out requests))
            {
                requests =
                    new List<
                        CombatNormalAttackTargetDamageReductionRequest>();

                _requestsByEventId.Add(
                    request.NormalAttackEventId,
                    requests);
            }

            for (var index = 0;
                 index < requests.Count;
                 index++)
            {
                if (requests[index].UsageKey ==
                    request.UsageKey)
                {
                    return false;
                }
            }

            requests.Add(
                request);

            _count =
                checked(
                    _count + 1);

            return true;
        }

        public bool HasRequests(
            CombatEventId normalAttackEventId)
        {
            ValidateEventId(
                normalAttackEventId);

            List<
                CombatNormalAttackTargetDamageReductionRequest>
                requests;

            return _requestsByEventId.TryGetValue(
                       normalAttackEventId,
                       out requests) &&
                   requests.Count > 0;
        }

        public IReadOnlyList<
            CombatNormalAttackTargetDamageReductionRequest>
            GetRequests(
                CombatEventId normalAttackEventId)
        {
            ValidateEventId(
                normalAttackEventId);

            List<
                CombatNormalAttackTargetDamageReductionRequest>
                requests;

            if (!_requestsByEventId.TryGetValue(
                    normalAttackEventId,
                    out requests))
            {
                return Array.Empty<
                    CombatNormalAttackTargetDamageReductionRequest>();
            }

            return requests.AsReadOnly();
        }

        public int RemoveRequests(
            CombatEventId normalAttackEventId)
        {
            ValidateEventId(
                normalAttackEventId);

            List<
                CombatNormalAttackTargetDamageReductionRequest>
                requests;

            if (!_requestsByEventId.TryGetValue(
                    normalAttackEventId,
                    out requests))
            {
                return 0;
            }

            _requestsByEventId.Remove(
                normalAttackEventId);

            _count =
                checked(
                    _count - requests.Count);

            return requests.Count;
        }

        private static void ValidateEventId(
            CombatEventId normalAttackEventId)
        {
            if (!normalAttackEventId.IsValid)
            {
                throw new ArgumentException(
                    "A valid Normal Attack EventId " +
                    "is required.",
                    nameof(normalAttackEventId));
            }
        }
    }
}