using System;
using GardenGambit.Domain.Combat;

namespace GardenGambit.Simulation.Combat
{
    public sealed class
        CombatColumnNormalAttackRunner
    {
        private readonly CombatState
            _state;

        private readonly CombatColumnFrontlineResolver
            _frontlineResolver;

        private readonly CombatNormalAttackRunner
            _normalAttackRunner;

        public CombatColumnNormalAttackRunner(
            CombatState state,
            CombatNormalAttackRunner
                normalAttackRunner)
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            if (normalAttackRunner == null)
            {
                throw new ArgumentNullException(
                    nameof(normalAttackRunner));
            }

            _state =
                state;

            _normalAttackRunner =
                normalAttackRunner;

            _frontlineResolver =
                new CombatColumnFrontlineResolver();
        }

        public bool HasActiveExecution =>
            _normalAttackRunner
                .HasActiveExecution;

        public CombatNormalAttackExecutionState
            ActiveExecutionState =>
                _normalAttackRunner
                    .ActiveExecutionState;

        public CombatNormalAttackEventBatch
            ActiveBatch =>
                _normalAttackRunner.ActiveBatch;

        public CombatNormalAttackExecutionStage
            ActiveStage =>
                _normalAttackRunner.ActiveStage;

        public CombatNormalAttackDamageApplication
            TryStartAndResolve(
                ColumnStartedCombatEvent
                    columnStartedEvent,
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (columnStartedEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(columnStartedEvent));
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            if (_normalAttackRunner
                    .HasActiveExecution)
            {
                throw new InvalidOperationException(
                    "The active Normal Attack execution " +
                    "must be resumed before another " +
                    "column attack can start.");
            }

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var canAttack =
                _frontlineResolver
                    .TryGetExchangePositions(
                        _state,
                        columnStartedEvent.Column,
                        out playerPosition,
                        out enemyPosition);

            if (!canAttack)
            {
                return null;
            }

            return _normalAttackRunner
                .StartAndResolveInColumn(
                    columnStartedEvent,
                    playerPosition,
                    enemyPosition,
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        public CombatNormalAttackDamageApplication
            ResumeActiveExecution(
                int maximumPassCount,
                int maximumEventCountPerPass,
                int maximumTriggerCountPerEvent)
        {
            if (!_normalAttackRunner
                    .HasActiveExecution)
            {
                throw new InvalidOperationException(
                    "There is no active column Normal " +
                    "Attack execution to resume.");
            }

            ValidateBudgets(
                maximumPassCount,
                maximumEventCountPerPass,
                maximumTriggerCountPerEvent);

            return _normalAttackRunner
                .ResumeActiveExecution(
                    maximumPassCount,
                    maximumEventCountPerPass,
                    maximumTriggerCountPerEvent);
        }

        private static void ValidateBudgets(
            int maximumPassCount,
            int maximumEventCountPerPass,
            int maximumTriggerCountPerEvent)
        {
            if (maximumPassCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPassCount),
                    maximumPassCount,
                    "Maximum pass count must be " +
                    "greater than zero.");
            }

            if (maximumEventCountPerPass <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumEventCountPerPass),
                    maximumEventCountPerPass,
                    "Maximum event count per pass must " +
                    "be greater than zero.");
            }

            if (maximumTriggerCountPerEvent <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTriggerCountPerEvent),
                    maximumTriggerCountPerEvent,
                    "Maximum trigger count per event must " +
                    "be greater than zero.");
            }
        }
    }
}