using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetBattleStartContextTests
    {
        [Test]
        public void Constructor_WithPlayerSide_MapsPlayerAndEnemy()
        {
            var state =
                CreateEmptyState();

            var snapshot =
                CreateEmptySnapshot();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    snapshot);

            var context =
                new CombatPetBattleStartContext(
                    state,
                    CombatSide.Player,
                    sourceEvent);

            Assert.That(
                context.State,
                Is.SameAs(state));

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                context.SourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                context.BattleStartSnapshot,
                Is.SameAs(snapshot));

            Assert.That(
                context.SideSnapshot,
                Is.SameAs(
                    snapshot.Player));

            Assert.That(
                context.OpposingSideSnapshot,
                Is.SameAs(
                    snapshot.Enemy));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    state.Player));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    state.Enemy));
        }

        [Test]
        public void Constructor_WithEnemySide_MapsEnemyAndPlayer()
        {
            var state =
                CreateEmptyState();

            var snapshot =
                CreateEmptySnapshot();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    snapshot);

            var context =
                new CombatPetBattleStartContext(
                    state,
                    CombatSide.Enemy,
                    sourceEvent);

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                context.SideSnapshot,
                Is.SameAs(
                    snapshot.Enemy));

            Assert.That(
                context.OpposingSideSnapshot,
                Is.SameAs(
                    snapshot.Player));

            Assert.That(
                context.SideState,
                Is.SameAs(
                    state.Enemy));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    state.Player));
        }

        [Test]
        public void Constructor_WithNullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleStartContext(
                        null,
                        CombatSide.Player,
                        CreateStageEvent(
                            CombatBattleStartStage.Pet,
                            CreateEmptySnapshot())));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatPetBattleStartContext(
                        CreateEmptyState(),
                        default(CombatSide),
                        CreateStageEvent(
                            CombatBattleStartStage.Pet,
                            CreateEmptySnapshot())));
        }

        [Test]
        public void Constructor_WithNullSourceEvent_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetBattleStartContext(
                        CreateEmptyState(),
                        CombatSide.Player,
                        null));
        }

        [Test]
        public void Constructor_WithNonPetStage_Throws()
        {
            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Slot,
                    CreateEmptySnapshot());

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatPetBattleStartContext(
                        CreateEmptyState(),
                        CombatSide.Player,
                        sourceEvent));
        }

        [Test]
        public void Constructor_WithoutSnapshot_Throws()
        {
            var sourceEvent =
                new BattleStartStageStartedCombatEvent(
                    CreateDirectRootChildMetadata(),
                    CombatBattleStartStage.Pet);

            Assert.That(
                sourceEvent.HasBattleStartSnapshot,
                Is.False);

            Assert.Throws<InvalidOperationException>(
                () => _ =
                    new CombatPetBattleStartContext(
                        CreateEmptyState(),
                        CombatSide.Player,
                        sourceEvent));
        }

        [Test]
        public void Constructor_SeparatesInitialSnapshotFromLiveState()
        {
            var card =
                new CombatCardState(
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(1),
                    new CardRank(4),
                    hpCapacity: 10,
                    currentHp: 5,
                    armor: 1,
                    attack: 3);

            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var player =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                position,
                                card.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            card
                        }),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var state =
                new CombatState(
                    player,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var snapshotResolver =
                new CombatBattleStartSnapshotResolver();

            var snapshot =
                snapshotResolver.Resolve(
                    state);

            card.SetCurrentHpToZero();

            var sourceEvent =
                CreateStageEvent(
                    CombatBattleStartStage.Pet,
                    snapshot);

            var context =
                new CombatPetBattleStartContext(
                    state,
                    CombatSide.Player,
                    sourceEvent);

            var initialCard =
                context.SideSnapshot.GetCard(
                    card.InstanceId);

            var liveCard =
                context.SideState.GetCardAt(
                    position);

            Assert.That(
                initialCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                initialCard.WasAlive,
                Is.True);

            Assert.That(
                liveCard.CurrentHp,
                Is.EqualTo(0));

            Assert.That(
                liveCard.IsAtDeathThreshold,
                Is.True);
        }

        private static
            BattleStartStageStartedCombatEvent
            CreateStageEvent(
                CombatBattleStartStage stage,
                CombatBattleStartSnapshot snapshot)
        {
            return new BattleStartStageStartedCombatEvent(
                CreateDirectRootChildMetadata(),
                stage,
                snapshot);
        }

        private static CombatEventMetadata
            CreateDirectRootChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }

        private static CombatBattleStartSnapshot
            CreateEmptySnapshot()
        {
            return new CombatBattleStartSnapshot(
                new CombatBattleStartSideSnapshot(
                    CombatSide.Player,
                    new CombatBattleStartCardSnapshot[0]),
                new CombatBattleStartSideSnapshot(
                    CombatSide.Enemy,
                    new CombatBattleStartCardSnapshot[0]));
        }

        private static CombatState
            CreateEmptyState()
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }
    }
}