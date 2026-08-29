using System;

namespace GardenGambit.Domain.Combat
{
    public sealed class
        CombatResultCalculatedCombatEvent :
        CombatEvent
    {
        public CombatResultCalculatedCombatEvent(
            CombatEventMetadata metadata,
            CombatResultDamageCalculation
                calculation,
            int resolvedIncomingDamageToPlayer,
            int resolvedIncomingDamageToEnemy)
            : this(
                metadata,
                CreateResolution(
                    calculation,
                    resolvedIncomingDamageToPlayer,
                    resolvedIncomingDamageToEnemy))
        {
        }

        public CombatResultCalculatedCombatEvent(
            CombatEventMetadata metadata,
            CombatResultDamageResolution resolution)
            : base(
                metadata,
                CombatEventKind
                    .CombatResultCalculated)
        {
            if (!resolution.IsValid)
            {
                throw new ArgumentException(
                    "Combat result event requires a " +
                    "valid damage resolution.",
                    nameof(resolution));
            }

            Resolution =
                resolution;
        }

        public CombatResultDamageResolution
            Resolution
        {
            get;
        }

        public CombatResultDamageCalculation
            Calculation =>
                Resolution.Calculation;

        public CombatSideResultContribution
            PlayerContribution =>
                Calculation.PlayerContribution;

        public CombatSideResultContribution
            EnemyContribution =>
                Calculation.EnemyContribution;

        public int BaseIncomingDamageToPlayer =>
            Resolution.BaseIncomingDamageToPlayer;

        public int BaseIncomingDamageToEnemy =>
            Resolution.BaseIncomingDamageToEnemy;

        public int ResolvedIncomingDamageToPlayer =>
            Resolution
                .ResolvedIncomingDamageToPlayer;

        public int ResolvedIncomingDamageToEnemy =>
            Resolution
                .ResolvedIncomingDamageToEnemy;

        public long PlayerDamageDelta =>
            Resolution.PlayerDamageDelta;

        public long EnemyDamageDelta =>
            Resolution.EnemyDamageDelta;

        public long PreventedDamageForPlayer =>
            Resolution.PreventedDamageForPlayer;

        public long PreventedDamageForEnemy =>
            Resolution.PreventedDamageForEnemy;

        public long AddedDamageToPlayer =>
            Resolution.AddedDamageToPlayer;

        public long AddedDamageToEnemy =>
            Resolution.AddedDamageToEnemy;

        public bool HasAnyDamageModification =>
            Resolution.HasAnyDamageModification;

        public bool HasResolvedDamageToPlayer =>
            ResolvedIncomingDamageToPlayer > 0;

        public bool HasResolvedDamageToEnemy =>
            ResolvedIncomingDamageToEnemy > 0;

        public bool HasMutualResolvedDamage =>
            HasResolvedDamageToPlayer &&
            HasResolvedDamageToEnemy;

        private static CombatResultDamageResolution
            CreateResolution(
                CombatResultDamageCalculation
                    calculation,
                int resolvedIncomingDamageToPlayer,
                int resolvedIncomingDamageToEnemy)
        {
            if (!calculation.IsValid)
            {
                throw new ArgumentException(
                    "Combat result event requires a " +
                    "valid damage calculation.",
                    nameof(calculation));
            }

            if (resolvedIncomingDamageToPlayer < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(
                        resolvedIncomingDamageToPlayer),
                    resolvedIncomingDamageToPlayer,
                    "Resolved incoming damage to Player " +
                    "cannot be negative.");
            }

            if (resolvedIncomingDamageToEnemy < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(
                        resolvedIncomingDamageToEnemy),
                    resolvedIncomingDamageToEnemy,
                    "Resolved incoming damage to Enemy " +
                    "cannot be negative.");
            }

            return new CombatResultDamageResolution(
                calculation,
                resolvedIncomingDamageToPlayer,
                resolvedIncomingDamageToEnemy);
        }
    }
}