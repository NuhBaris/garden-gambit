using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetNormalAttackContextTests
    {
        [Test]
        public void
            Constructor_WithPlayerSide_MapsPlayerAndEnemy()
        {
            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent(
                    CombatSide.Player);

            var context =
                new CombatPetNormalAttackContext(
                    state,
                    CombatSide.Player,
                    sourceEvent);

            Assert.That(
                context.State,
                Is.SameAs(
                    state));

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
                    state.Player));

            Assert.That(
                context.OpposingSideState,
                Is.SameAs(
                    state.Enemy));
        }

        [Test]
        public void
            Constructor_WithEnemySide_MapsEnemyAndPlayer()
        {
            var state =
                CreateEmptyState();

            var sourceEvent =
                CreateNormalAttackEvent(
                    CombatSide.Enemy);

            var context =
                new CombatPetNormalAttackContext(
                    state,
                    CombatSide.Enemy,
                    sourceEvent);

            Assert.That(
                context.State,
                Is.SameAs(
                    state));

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                context.SourceEvent,
                Is.SameAs(
                    sourceEvent));

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
        public void
            Constructor_WithOpposingSideAttack_AllowsPetToObserveEnemyAttack()
        {
            var state =
                CreateEmptyState();

            var enemyAttackEvent =
                CreateNormalAttackEvent(
                    CombatSide.Enemy);

            var context =
                new CombatPetNormalAttackContext(
                    state,
                    CombatSide.Player,
                    enemyAttackEvent);

            Assert.That(
                context.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                context.SourceEvent.AttackerSide,
                Is.EqualTo(
                    CombatSide.Enemy));

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
        public void Constructor_WithNullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetNormalAttackContext(
                        null,
                        CombatSide.Player,
                        CreateNormalAttackEvent(
                            CombatSide.Player)));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    new CombatPetNormalAttackContext(
                        CreateEmptyState(),
                        default(CombatSide),
                        CreateNormalAttackEvent(
                            CombatSide.Player)));
        }

        [Test]
        public void Constructor_WithNullSourceEvent_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetNormalAttackContext(
                        CreateEmptyState(),
                        CombatSide.Player,
                        null));
        }

        private static CombatState CreateEmptyState()
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
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static NormalAttackCombatEvent
            CreateNormalAttackEvent(
                CombatSide attackerSide)
        {
            CombatSide targetSide;

            if (attackerSide ==
                CombatSide.Player)
            {
                targetSide =
                    CombatSide.Enemy;
            }
            else if (attackerSide ==
                     CombatSide.Enemy)
            {
                targetSide =
                    CombatSide.Player;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackerSide));
            }

            var attackerPosition =
                new BoardPosition(
                    attackerSide,
                    BoardRow.Front,
                    new BoardColumn(1));

            var targetPosition =
                new BoardPosition(
                    targetSide,
                    BoardRow.Front,
                    new BoardColumn(1));

            return new NormalAttackCombatEvent(
                CreateChildMetadata(),
                new InstanceId(1),
                attackerPosition,
                new InstanceId(2),
                targetPosition,
                3);
        }

        private static CombatEventMetadata
            CreateChildMetadata()
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                triggerRootId,
                triggerRootId);
        }
    }
}