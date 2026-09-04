using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackExecutionStateTests
    {
        [Test]
        public void Constructor_WithNullBatch_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackExecutionState(
                        null));
        }

        [Test]
        public void
            Constructor_WithBatch_StartsAtPreparedStage()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            Assert.That(
                executionState.Batch,
                Is.SameAs(environment.Batch));

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                executionState.HasDamageApplication,
                Is.False);

            Assert.That(
                executionState.DamageApplication,
                Is.Null);

            Assert.That(
                executionState.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            MarkAttackTriggersResolved_FromPrepared_AdvancesStage()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));

            Assert.That(
                executionState.HasDamageApplication,
                Is.False);

            Assert.That(
                executionState.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            MarkAttackTriggersResolved_WhenAlreadyResolved_ThrowsWithoutChangingStage()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            Assert.Throws<InvalidOperationException>(
                () => executionState
                    .MarkAttackTriggersResolved());

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));
        }

        [Test]
        public void
            SetDamageApplication_WithNullApplication_ThrowsWithoutChangingStage()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            Assert.Throws<ArgumentNullException>(
                () => executionState
                    .SetDamageApplication(
                        null));

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));

            Assert.That(
                executionState.HasDamageApplication,
                Is.False);
        }

        [Test]
        public void
            SetDamageApplication_BeforeTriggerResolution_Throws()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            Assert.Throws<InvalidOperationException>(
                () => executionState
                    .SetDamageApplication(
                        environment.Application));

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Prepared));

            Assert.That(
                executionState.HasDamageApplication,
                Is.False);
        }

        [Test]
        public void
            SetDamageApplication_WithMatchingBatch_StoresApplicationAndAdvancesStage()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            executionState
                .SetDamageApplication(
                    environment.Application);

            Assert.That(
                executionState.DamageApplication,
                Is.SameAs(
                    environment.Application));

            Assert.That(
                executionState.HasDamageApplication,
                Is.True);

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .DamageApplied));

            Assert.That(
                executionState.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            SetDamageApplication_WithDifferentBatch_ThrowsWithoutChangingState()
        {
            var firstEnvironment =
                CreateEnvironment();

            var secondEnvironment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    firstEnvironment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            Assert.Throws<ArgumentException>(
                () => executionState
                    .SetDamageApplication(
                        secondEnvironment
                            .Application));

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));

            Assert.That(
                executionState.HasDamageApplication,
                Is.False);

            Assert.That(
                executionState.DamageApplication,
                Is.Null);
        }

        [Test]
        public void
            MarkCompleted_BeforeDamageApplication_Throws()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            Assert.Throws<InvalidOperationException>(
                () => executionState
                    .MarkCompleted());

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .AttackTriggersResolved));

            Assert.That(
                executionState.IsCompleted,
                Is.False);
        }

        [Test]
        public void
            MarkCompleted_AfterDamageApplication_CompletesExecution()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            executionState
                .SetDamageApplication(
                    environment.Application);

            executionState.MarkCompleted();

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Completed));

            Assert.That(
                executionState.IsCompleted,
                Is.True);

            Assert.That(
                executionState.DamageApplication,
                Is.SameAs(
                    environment.Application));
        }

        [Test]
        public void
            MarkCompleted_WhenAlreadyCompleted_ThrowsWithoutLosingApplication()
        {
            var environment =
                CreateEnvironment();

            var executionState =
                new CombatNormalAttackExecutionState(
                    environment.Batch);

            executionState
                .MarkAttackTriggersResolved();

            executionState
                .SetDamageApplication(
                    environment.Application);

            executionState.MarkCompleted();

            Assert.Throws<InvalidOperationException>(
                () => executionState
                    .MarkCompleted());

            Assert.That(
                executionState.Stage,
                Is.EqualTo(
                    CombatNormalAttackExecutionStage
                        .Completed));

            Assert.That(
                executionState.IsCompleted,
                Is.True);

            Assert.That(
                executionState.DamageApplication,
                Is.SameAs(
                    environment.Application));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "card.player",
                    100,
                    10,
                    3);

            var enemyCard =
                CreateCard(
                    "card.enemy",
                    200,
                    10,
                    4);

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        new SlotId(1),
                        playerPosition,
                        playerCard),
                    CreateSide(
                        CombatSide.Enemy,
                        new SlotId(2),
                        enemyPosition,
                        enemyCard));

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var preparationResolver =
                new
                    CombatNormalAttackPreparationResolver(
                        metadataFactory,
                        eventLog);

            var batch =
                preparationResolver.Prepare(
                    state,
                    playerPosition,
                    enemyPosition);

            var resolution =
                new CombatNormalAttackDamageResolution(
                    batch,
                    2,
                    2);

            var applicationResolver =
                new
                    CombatNormalAttackDamageApplicationResolver(
                        metadataFactory,
                        eventLog);

            var application =
                applicationResolver.Apply(
                    state,
                    resolution);

            return new TestEnvironment
            {
                Batch = batch,
                Application = application
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            SlotId slotId,
            BoardPosition position,
            CombatCardState card)
        {
            var slot =
                new CombatSlotState(
                    slotId,
                    position,
                    card.InstanceId);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[] { slot }),
                new CombatCardRegistry(
                    new[] { card }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                0,
                attack);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestEnvironment
        {
            public CombatNormalAttackEventBatch Batch
            {
                get;
                set;
            }

            public CombatNormalAttackDamageApplication
                Application
            {
                get;
                set;
            }
        }
    }
}