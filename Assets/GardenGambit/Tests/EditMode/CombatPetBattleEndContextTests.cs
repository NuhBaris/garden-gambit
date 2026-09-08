using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleEndContextTests
    {
        [Test]
        public void Constructor_WithNullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleEndContext(
                        null,
                        CombatSide.Player,
                        CreateSourceEvent(
                            includeSnapshot: true)));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatPetBattleEndContext(
                        environment.State,
                        default(CombatSide),
                        CreateSourceEvent(
                            includeSnapshot: true)));
        }

        [Test]
        public void
            Constructor_WithNullSourceEvent_Throws()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleEndContext(
                        environment.State,
                        CombatSide.Player,
                        null));
        }

        [Test]
        public void
            Constructor_WithPlayerSide_ExposesExactRuntimeDependencies()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                CreateSourceEvent(
                    includeSnapshot: true);

            var context =
                new CombatPetBattleEndContext(
                    environment.State,
                    CombatSide.Player,
                    sourceEvent);

            Assert.That(
                context.State,
                Is.SameAs(
                    environment.State));

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                context.SourceEvent,
                Is.SameAs(
                    sourceEvent));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    environment.State.Player));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    environment.State.Enemy));

            Assert.That(
                context.SidePetState,
                Is.SameAs(
                    environment.State.PlayerPets));
        }

        [Test]
        public void
            Constructor_WithEnemySide_ExposesCorrectSides()
        {
            var environment =
                CreateEnvironment();

            var context =
                new CombatPetBattleEndContext(
                    environment.State,
                    CombatSide.Enemy,
                    CreateSourceEvent(
                        includeSnapshot: true));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    environment.State.Enemy));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    environment.State.Player));

            Assert.That(
                context.SidePetState,
                Is.SameAs(
                    environment.State.EnemyPets));
        }

        [Test]
        public void
            Constructor_WithSnapshot_ExposesCorrectSnapshotSides()
        {
            var environment =
                CreateEnvironment();

            var sourceEvent =
                CreateSourceEvent(
                    includeSnapshot: true);

            var context =
                new CombatPetBattleEndContext(
                    environment.State,
                    CombatSide.Player,
                    sourceEvent);

            Assert.That(
                context.HasBattleStartSnapshot,
                Is.True);

            Assert.That(
                context.BattleStartSnapshot,
                Is.SameAs(
                    sourceEvent.BattleStartSnapshot));

            Assert.That(
                context.SideBattleStartSnapshot,
                Is.SameAs(
                    sourceEvent
                        .BattleStartSnapshot.Player));

            Assert.That(
                context.OpposingBattleStartSnapshot,
                Is.SameAs(
                    sourceEvent
                        .BattleStartSnapshot.Enemy));
        }

        [Test]
        public void
            Constructor_WithoutSnapshot_ReportsNullSnapshotSides()
        {
            var environment =
                CreateEnvironment();

            var context =
                new CombatPetBattleEndContext(
                    environment.State,
                    CombatSide.Player,
                    CreateSourceEvent(
                        includeSnapshot: false));

            Assert.That(
                context.HasBattleStartSnapshot,
                Is.False);

            Assert.That(
                context.BattleStartSnapshot,
                Is.Null);

            Assert.That(
                context.SideBattleStartSnapshot,
                Is.Null);

            Assert.That(
                context.OpposingBattleStartSnapshot,
                Is.Null);
        }

        [Test]
        public void
            GetAffectedRow_DelegatesToExactPetSidePlacement()
        {
            var environment =
                CreateEnvironment();

            var context =
                new CombatPetBattleEndContext(
                    environment.State,
                    CombatSide.Player,
                    CreateSourceEvent(
                        includeSnapshot: true));

            var expectedRow =
                environment.State.PlayerPets
                    .GetAffectedRow(
                        environment.PlayerPet
                            .InstanceId);

            var affectedRow =
                context.GetAffectedRow(
                    environment.PlayerPet);

            Assert.That(
                affectedRow,
                Is.EqualTo(
                    expectedRow));
        }

        [Test]
        public void GetAffectedRow_WithNullPet_Throws()
        {
            var environment =
                CreateEnvironment();

            var context =
                new CombatPetBattleEndContext(
                    environment.State,
                    CombatSide.Player,
                    CreateSourceEvent(
                        includeSnapshot: true));

            Assert.Throws<ArgumentNullException>(
                () => context.GetAffectedRow(
                    null));
        }

        private static TestEnvironment
            CreateEnvironment()
        {
            var playerPet =
                new CombatPetState(
                    new DefinitionId(
                        "player-pet"),
                    new InstanceId(1001));

            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                playerPet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            Array.Empty<
                                CombatPetState>())));

            return new TestEnvironment
            {
                State =
                    state,

                PlayerPet =
                    playerPet
            };
        }

        private static
            BattleEndStartedCombatEvent
            CreateSourceEvent(
                bool includeSnapshot)
        {
            var rootEventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    rootEventId,
                    rootEventId);

            if (!includeSnapshot)
            {
                return new
                    BattleEndStartedCombatEvent(
                        metadata);
            }

            return new BattleEndStartedCombatEvent(
                metadata,
                CreateSnapshot());
        }

        private static CombatBattleStartSnapshot
            CreateSnapshot()
        {
            return new CombatBattleStartSnapshot(
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>()),
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    Array.Empty<
                        CombatBattleStartCardSnapshot>()));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatPetState PlayerPet
            {
                get;
                set;
            }
        }
    }
}