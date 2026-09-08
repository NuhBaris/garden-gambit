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

            Assert.That(
                context.SidePetState,
                Is.SameAs(
                    state.PlayerPets));
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

            Assert.That(
                context.SidePetState,
                Is.SameAs(
                    state.EnemyPets));
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

            Assert.That(
                context.SidePetState,
                Is.SameAs(
                    state.PlayerPets));
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

        [Test]
        public void
            GetAffectedRow_WithUpperPet_ReturnsFront()
        {
            var upperPet =
                CreatePet(
                    "pet-upper",
                    1001);

            var lowerPet =
                CreatePet(
                    "pet-lower",
                    1002);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerPet
                    },
                    Array.Empty<CombatPetState>());

            var context =
                new CombatPetNormalAttackContext(
                    state,
                    CombatSide.Player,
                    CreateNormalAttackEvent(
                        CombatSide.Player));

            var affectedRow =
                context.GetAffectedRow(
                    upperPet);

            Assert.That(
                affectedRow,
                Is.EqualTo(
                    BoardRow.Front));
        }

        [Test]
        public void
            GetAffectedRow_WithLowerPet_ReturnsBack()
        {
            var upperPet =
                CreatePet(
                    "pet-upper",
                    1001);

            var lowerPet =
                CreatePet(
                    "pet-lower",
                    1002);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerPet
                    },
                    Array.Empty<CombatPetState>());

            var context =
                new CombatPetNormalAttackContext(
                    state,
                    CombatSide.Player,
                    CreateNormalAttackEvent(
                        CombatSide.Player));

            var affectedRow =
                context.GetAffectedRow(
                    lowerPet);

            Assert.That(
                affectedRow,
                Is.EqualTo(
                    BoardRow.Back));
        }

        [Test]
        public void GetAffectedRow_WithNullPet_Throws()
        {
            var context =
                new CombatPetNormalAttackContext(
                    CreateEmptyState(),
                    CombatSide.Player,
                    CreateNormalAttackEvent(
                        CombatSide.Player));

            Assert.Throws<ArgumentNullException>(
                () => context.GetAffectedRow(
                    null));
        }

        [Test]
        public void
            GetAffectedRow_WithOpposingSidePet_Throws()
        {
            var playerPet =
                CreatePet(
                    "pet-player",
                    1001);

            var enemyPet =
                CreatePet(
                    "pet-enemy",
                    2001);

            var state =
                CreateState(
                    new[]
                    {
                        playerPet
                    },
                    new[]
                    {
                        enemyPet
                    });

            var context =
                new CombatPetNormalAttackContext(
                    state,
                    CombatSide.Player,
                    CreateNormalAttackEvent(
                        CombatSide.Player));

            Assert.Throws<ArgumentException>(
                () => context.GetAffectedRow(
                    enemyPet));
        }

        private static CombatState CreateEmptyState()
        {
            return CreateState(
                Array.Empty<CombatPetState>(),
                Array.Empty<CombatPetState>());
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets,
            CombatPetState[] enemyPets)
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        enemyPets)));
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

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
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