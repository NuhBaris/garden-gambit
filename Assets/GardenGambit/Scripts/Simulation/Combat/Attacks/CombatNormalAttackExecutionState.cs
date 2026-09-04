using System;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatNormalAttackExecutionState
    {
        private
            CombatNormalAttackDamageApplication
            _damageApplication;

        public CombatNormalAttackExecutionState(
            CombatNormalAttackEventBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(
                    nameof(batch));
            }

            Batch =
                batch;

            Stage =
                CombatNormalAttackExecutionStage
                    .Prepared;
        }

        public CombatNormalAttackEventBatch Batch
        {
            get;
        }

        public CombatNormalAttackExecutionStage Stage
        {
            get;
            private set;
        }

        public bool HasDamageApplication =>
            _damageApplication != null;

        public CombatNormalAttackDamageApplication
            DamageApplication =>
                _damageApplication;

        public bool IsCompleted =>
            Stage ==
            CombatNormalAttackExecutionStage.Completed;

        public void MarkAttackTriggersResolved()
        {
            EnsureStage(
                CombatNormalAttackExecutionStage
                    .Prepared);

            Stage =
                CombatNormalAttackExecutionStage
                    .AttackTriggersResolved;
        }

        public void SetDamageApplication(
            CombatNormalAttackDamageApplication
                damageApplication)
        {
            if (damageApplication == null)
            {
                throw new ArgumentNullException(
                    nameof(damageApplication));
            }

            EnsureStage(
                CombatNormalAttackExecutionStage
                    .AttackTriggersResolved);

            if (!ReferenceEquals(
                    damageApplication.Batch,
                    Batch))
            {
                throw new ArgumentException(
                    "Damage application must belong to " +
                    "the active Normal Attack batch.",
                    nameof(damageApplication));
            }

            _damageApplication =
                damageApplication;

            Stage =
                CombatNormalAttackExecutionStage
                    .DamageApplied;
        }

        public void MarkCompleted()
        {
            EnsureStage(
                CombatNormalAttackExecutionStage
                    .DamageApplied);

            Stage =
                CombatNormalAttackExecutionStage
                    .Completed;
        }

        private void EnsureStage(
            CombatNormalAttackExecutionStage
                expectedStage)
        {
            if (Stage == expectedStage)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Normal Attack execution is at " +
                $"{Stage}, but {expectedStage} was " +
                $"required.");
        }
    }
}