using System;

namespace GardenGambit.Domain.Combat
{
    public readonly struct
        CombatSideResultContribution
    {
        private readonly bool
            _isInitialized;

        public CombatSideResultContribution(
            CombatSide side,
            int survivorCount,
            int totalSurvivorRankContribution,
            AttackMultiplier finalAttackMultiplier)
        {
            if (side != CombatSide.Player &&
                side != CombatSide.Enemy)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Result contribution requires " +
                    "Player or Enemy side.");
            }

            if (survivorCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(survivorCount),
                    survivorCount,
                    "Survivor count cannot be negative.");
            }

            if (survivorCount >
                CombatBoardState.MaximumSlotCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(survivorCount),
                    survivorCount,
                    "Survivor count cannot exceed the " +
                    "combat board slot count.");
            }

            if (totalSurvivorRankContribution < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(
                        totalSurvivorRankContribution),
                    totalSurvivorRankContribution,
                    "Total survivor Rank contribution " +
                    "cannot be negative.");
            }

            if (survivorCount == 0 &&
                totalSurvivorRankContribution != 0)
            {
                throw new ArgumentException(
                    "A side without survivors cannot have " +
                    "a survivor Rank contribution.",
                    nameof(
                        totalSurvivorRankContribution));
            }

            if (survivorCount > 0 &&
                totalSurvivorRankContribution == 0)
            {
                throw new ArgumentException(
                    "A side with survivors must have a " +
                    "positive survivor Rank contribution.",
                    nameof(
                        totalSurvivorRankContribution));
            }

            if (!finalAttackMultiplier.IsValid)
            {
                throw new ArgumentException(
                    "A valid final Attack Multiplier " +
                    "is required.",
                    nameof(finalAttackMultiplier));
            }

            var finalResultContribution =
                checked(
                    totalSurvivorRankContribution *
                    finalAttackMultiplier.Value);

            Side = side;
            SurvivorCount = survivorCount;

            TotalSurvivorRankContribution =
                totalSurvivorRankContribution;

            FinalAttackMultiplier =
                finalAttackMultiplier;

            FinalResultContribution =
                finalResultContribution;

            _isInitialized = true;
        }

        public CombatSide Side { get; }

        public int SurvivorCount { get; }

        public int TotalSurvivorRankContribution
        {
            get;
        }

        public AttackMultiplier FinalAttackMultiplier
        {
            get;
        }

        public int FinalResultContribution { get; }

        public bool HasSurvivors =>
            SurvivorCount > 0;

        public bool HasPositiveContribution =>
            FinalResultContribution > 0;

        public bool IsValid
        {
            get
            {
                if (!_isInitialized)
                {
                    return false;
                }

                if (Side != CombatSide.Player &&
                    Side != CombatSide.Enemy)
                {
                    return false;
                }

                if (SurvivorCount < 0 ||
                    SurvivorCount >
                    CombatBoardState.MaximumSlotCount)
                {
                    return false;
                }

                if (TotalSurvivorRankContribution < 0 ||
                    !FinalAttackMultiplier.IsValid)
                {
                    return false;
                }

                if (SurvivorCount == 0)
                {
                    if (TotalSurvivorRankContribution != 0)
                    {
                        return false;
                    }
                }
                else if (
                    TotalSurvivorRankContribution == 0)
                {
                    return false;
                }

                var expectedContribution =
                    (long)
                    TotalSurvivorRankContribution *
                    FinalAttackMultiplier.Value;

                if (expectedContribution < 0 ||
                    expectedContribution > int.MaxValue)
                {
                    return false;
                }

                return FinalResultContribution ==
                       (int)expectedContribution;
            }
        }
    }
}