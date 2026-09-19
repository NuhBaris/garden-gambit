using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetDeathContextTests
    {
        [TestCase(CombatSide.Player, CombatSide.Player)]
        [TestCase(CombatSide.Player, CombatSide.Enemy)]
        [TestCase(CombatSide.Enemy, CombatSide.Player)]
        [TestCase(CombatSide.Enemy, CombatSide.Enemy)]
        public void Constructor_MapsPetSideAndPreservesExactDeathEvent(
            CombatSide petSide,
            CombatSide deathSide)
        {
            var state = CreateEmptyState();
            var sourceEvent = CreateDeathEvent(deathSide);

            var context = new CombatPetDeathContext(
                state,
                petSide,
                sourceEvent);

            var expectedSide = petSide == CombatSide.Player
                ? state.Player
                : state.Enemy;

            var expectedOpposingSide = petSide == CombatSide.Player
                ? state.Enemy
                : state.Player;

            var expectedPets = petSide == CombatSide.Player
                ? state.PlayerPets
                : state.EnemyPets;

            Assert.That(context.State, Is.SameAs(state));
            Assert.That(context.Side, Is.EqualTo(petSide));
            Assert.That(context.SourceEvent, Is.SameAs(sourceEvent));

            Assert.That(
                context.SourceEvent.Position.Side,
                Is.EqualTo(deathSide));
            Assert.That(
                context.SideState,
                Is.SameAs(expectedSide));
            Assert.That(
                context.OpposingSideState,
                Is.SameAs(expectedOpposingSide));
            Assert.That(
                context.SidePetState,
                Is.SameAs(expectedPets));
        }

        [Test]
        public void Constructor_WithNullState_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatPetDeathContext(
                    null,
                    CombatSide.Player,
                    CreateDeathEvent(CombatSide.Player)));
        }

        [Test]
        public void Constructor_WithInvalidSide_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new CombatPetDeathContext(
                    CreateEmptyState(),
                    (CombatSide)999,
                    CreateDeathEvent(CombatSide.Player)));
        }

        [Test]
        public void Constructor_WithNullSourceEvent_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatPetDeathContext(
                    CreateEmptyState(),
                    CombatSide.Player,
                    null));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void GetAffectedRow_UsesPetPlacementForSameDefinitionInstances(
            CombatSide side)
        {
            var upperPet = CreatePet(1002);
            var lowerPet = CreatePet(1001);

            var pets = new[] { upperPet, lowerPet };
            var emptyPets = Array.Empty<CombatPetState>();

            var state = CreateState(
                side == CombatSide.Player ? pets : emptyPets,
                side == CombatSide.Enemy ? pets : emptyPets);

            var context = new CombatPetDeathContext(
                state,
                side,
                CreateDeathEvent(side));

            Assert.That(
                upperPet.DefinitionId,
                Is.EqualTo(lowerPet.DefinitionId));

            Assert.That(
                context.GetAffectedRow(upperPet),
                Is.EqualTo(BoardRow.Front));

            Assert.That(
                context.GetAffectedRow(lowerPet),
                Is.EqualTo(BoardRow.Back));
        }

        [Test]
        public void GetAffectedRow_WithNullPet_Throws()
        {
            var context = new CombatPetDeathContext(
                CreateEmptyState(),
                CombatSide.Player,
                CreateDeathEvent(CombatSide.Player));

            Assert.Throws<ArgumentNullException>(
                () => context.GetAffectedRow(null));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void GetAffectedRow_WithOpposingSidePet_Throws(
            CombatSide side)
        {
            var playerPet = CreatePet(1001);
            var enemyPet = CreatePet(2001);

            var state = CreateState(
                new[] { playerPet },
                new[] { enemyPet });

            var opposingSide = side == CombatSide.Player
                ? CombatSide.Enemy
                : CombatSide.Player;

            var opposingPet = side == CombatSide.Player
                ? enemyPet
                : playerPet;

            var context = new CombatPetDeathContext(
                state,
                side,
                CreateDeathEvent(opposingSide));

            Assert.Throws<ArgumentException>(
                () => context.GetAffectedRow(opposingPet));
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
                CreateEmptySide(CombatSide.Player),
                CreateEmptySide(CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(enemyPets)));
        }

        private static CombatSideState CreateEmptySide(
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

        private static CombatPetState CreatePet(long instanceId)
        {
            return new CombatPetState(
                new DefinitionId("test.death_context_pet"),
                new InstanceId(instanceId));
        }

        private static DeathCombatEvent CreateDeathEvent(
            CombatSide side)
        {
            var rootId = new CombatEventId(1);

            var metadata = new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootId,
                rootId);

            return new DeathCombatEvent(
                metadata,
                new InstanceId(1),
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    new BoardColumn(1)),
                previousHp: 3,
                currentHp: 0);
        }
    }
}