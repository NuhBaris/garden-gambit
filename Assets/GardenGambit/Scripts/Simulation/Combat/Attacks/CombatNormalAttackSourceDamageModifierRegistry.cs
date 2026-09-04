using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackSourceDamageModifierRegistry
    {
        private readonly Dictionary<
            CombatEventId,
            int> _modifiers;

        public
            CombatNormalAttackSourceDamageModifierRegistry()
        {
            _modifiers =
                new Dictionary<
                    CombatEventId,
                    int>();
        }

        public int Count =>
            _modifiers.Count;

        public void AddModifier(
            CombatEventId normalAttackEventId,
            int damageDelta)
        {
            ValidateEventId(
                normalAttackEventId);

            if (damageDelta == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damageDelta),
                    damageDelta,
                    "Normal Attack source damage " +
                    "modifier cannot be zero.");
            }

            int existingModifier;

            if (!_modifiers.TryGetValue(
                    normalAttackEventId,
                    out existingModifier))
            {
                existingModifier = 0;
            }

            var combinedModifier =
                checked(
                    existingModifier +
                    damageDelta);

            _modifiers[normalAttackEventId] =
                combinedModifier;
        }

        public bool HasModifier(
            CombatEventId normalAttackEventId)
        {
            ValidateEventId(
                normalAttackEventId);

            return _modifiers.ContainsKey(
                normalAttackEventId);
        }

        public int GetTotalModifier(
            CombatEventId normalAttackEventId)
        {
            ValidateEventId(
                normalAttackEventId);

            int modifier;

            if (_modifiers.TryGetValue(
                    normalAttackEventId,
                    out modifier))
            {
                return modifier;
            }

            return 0;
        }

        public int ResolveDamage(
            NormalAttackCombatEvent
                normalAttackEvent)
        {
            if (normalAttackEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(normalAttackEvent));
            }

            var modifier =
                GetTotalModifier(
                    normalAttackEvent.Metadata.EventId);

            var resolvedDamage =
                (long)normalAttackEvent.BaseDamage +
                modifier;

            if (resolvedDamage <= 0L)
            {
                return 0;
            }

            if (resolvedDamage > int.MaxValue)
            {
                throw new OverflowException(
                    "Resolved Normal Attack source " +
                    "damage exceeds Int32 maximum value.");
            }

            return (int)resolvedDamage;
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