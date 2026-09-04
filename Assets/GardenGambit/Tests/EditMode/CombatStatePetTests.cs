using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatStatePetTests
    {
        [Test]
        public void LegacyConstructor_CreatesEmptyPetSides()
        {
            var player =
                CreateCardSide(
                    CombatSide.Player);

            var enemy =
                CreateCardSide(
                    CombatSide.Enemy);

            var state =
                new CombatState(
                    player,
                    enemy);

            Assert.That(
                state.PlayerPets,
                Is.Not.Null);

            Assert.That(
                state.PlayerPets.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                state.PlayerPets.Count,
                Is.Zero);

            Assert.That(
                state.EnemyPets,
                Is.Not.Null);

            Assert.That(
                state.EnemyPets.Side,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                state.EnemyPets.Count,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithPetSides_SetsExactStates()
        {
            var player =
                CreateCardSide(
                    CombatSide.Player);

            var enemy =
                CreateCardSide(
                    CombatSide.Enemy);

            var playerPets =
                CreatePetSide(
                    CombatSide.Player,
                    CreatePet(
                        "pet-player",
                        100));

            var enemyPets =
                CreatePetSide(
                    CombatSide.Enemy,
                    CreatePet(
                        "pet-enemy",
                        200));

            var state =
                new CombatState(
                    player,
                    enemy,
                    playerPets,
                    enemyPets);

            Assert.That(
                state.Player,
                Is.SameAs(player));

            Assert.That(
                state.Enemy,
                Is.SameAs(enemy));

            Assert.That(
                state.PlayerPets,
                Is.SameAs(playerPets));

            Assert.That(
                state.EnemyPets,
                Is.SameAs(enemyPets));
        }

        [Test]
        public void Constructor_WithNullPlayerPets_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player),
                        CreateCardSide(
                            CombatSide.Enemy),
                        null,
                        CreatePetSide(
                            CombatSide.Enemy)));
        }

        [Test]
        public void Constructor_WithNullEnemyPets_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player),
                        CreateCardSide(
                            CombatSide.Enemy),
                        CreatePetSide(
                            CombatSide.Player),
                        null));
        }

        [Test]
        public void Constructor_WithEnemyPetStateAsPlayerPets_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player),
                        CreateCardSide(
                            CombatSide.Enemy),
                        CreatePetSide(
                            CombatSide.Enemy),
                        CreatePetSide(
                            CombatSide.Enemy)));
        }

        [Test]
        public void Constructor_WithPlayerPetStateAsEnemyPets_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player),
                        CreateCardSide(
                            CombatSide.Enemy),
                        CreatePetSide(
                            CombatSide.Player),
                        CreatePetSide(
                            CombatSide.Player)));
        }

        [Test]
        public void GetPets_WithPlayerAndEnemy_ReturnsCorrectSides()
        {
            var playerPets =
                CreatePetSide(
                    CombatSide.Player,
                    CreatePet(
                        "pet-player",
                        100));

            var enemyPets =
                CreatePetSide(
                    CombatSide.Enemy,
                    CreatePet(
                        "pet-enemy",
                        200));

            var state =
                new CombatState(
                    CreateCardSide(
                        CombatSide.Player),
                    CreateCardSide(
                        CombatSide.Enemy),
                    playerPets,
                    enemyPets);

            Assert.That(
                state.GetPets(
                    CombatSide.Player),
                Is.SameAs(playerPets));

            Assert.That(
                state.GetPets(
                    CombatSide.Enemy),
                Is.SameAs(enemyPets));
        }

        [Test]
        public void GetPets_WithInvalidSide_Throws()
        {
            var state =
                new CombatState(
                    CreateCardSide(
                        CombatSide.Player),
                    CreateCardSide(
                        CombatSide.Enemy));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.GetPets(
                    default(CombatSide)));
        }

        [Test]
        public void Constructor_WithPlayerCardAndPetSharingInstanceId_Throws()
        {
            var sharedInstanceId =
                new InstanceId(100);

            var playerCard =
                CreateCard(
                    "card-player",
                    sharedInstanceId);

            var playerPet =
                CreatePet(
                    "pet-player",
                    sharedInstanceId);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player,
                            playerCard),
                        CreateCardSide(
                            CombatSide.Enemy),
                        CreatePetSide(
                            CombatSide.Player,
                            playerPet),
                        CreatePetSide(
                            CombatSide.Enemy)));
        }

        [Test]
        public void Constructor_WithEnemyCardAndPetSharingInstanceId_Throws()
        {
            var sharedInstanceId =
                new InstanceId(200);

            var enemyCard =
                CreateCard(
                    "card-enemy",
                    sharedInstanceId);

            var enemyPet =
                CreatePet(
                    "pet-enemy",
                    sharedInstanceId);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player),
                        CreateCardSide(
                            CombatSide.Enemy,
                            enemyCard),
                        CreatePetSide(
                            CombatSide.Player),
                        CreatePetSide(
                            CombatSide.Enemy,
                            enemyPet)));
        }

        [Test]
        public void Constructor_WithCrossSidePetsSharingInstanceId_Throws()
        {
            var sharedInstanceId =
                new InstanceId(300);

            var playerPet =
                CreatePet(
                    "pet-player",
                    sharedInstanceId);

            var enemyPet =
                CreatePet(
                    "pet-enemy",
                    sharedInstanceId);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatState(
                        CreateCardSide(
                            CombatSide.Player),
                        CreateCardSide(
                            CombatSide.Enemy),
                        CreatePetSide(
                            CombatSide.Player,
                            playerPet),
                        CreatePetSide(
                            CombatSide.Enemy,
                            enemyPet)));
        }

        [Test]
        public void Constructor_WithGloballyDistinctCardAndPetIds_AllowsState()
        {
            var playerCard =
                CreateCard(
                    "card-player",
                    new InstanceId(100));

            var enemyCard =
                CreateCard(
                    "card-enemy",
                    new InstanceId(200));

            var playerPet =
                CreatePet(
                    "pet-player",
                    new InstanceId(300));

            var enemyPet =
                CreatePet(
                    "pet-enemy",
                    new InstanceId(400));

            var state =
                new CombatState(
                    CreateCardSide(
                        CombatSide.Player,
                        playerCard),
                    CreateCardSide(
                        CombatSide.Enemy,
                        enemyCard),
                    CreatePetSide(
                        CombatSide.Player,
                        playerPet),
                    CreatePetSide(
                        CombatSide.Enemy,
                        enemyPet));

            Assert.That(
                state.Player.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                state.Enemy.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                state.PlayerPets.Count,
                Is.EqualTo(1));

            Assert.That(
                state.EnemyPets.Count,
                Is.EqualTo(1));
        }

        private static CombatSideState
            CreateCardSide(
                CombatSide side,
                params CombatCardState[] cards)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatSidePetState
            CreatePetSide(
                CombatSide side,
                params CombatPetState[] pets)
        {
            return new CombatSidePetState(
                side,
                new CombatPetRegistry(
                    pets));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            InstanceId instanceId)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                instanceId,
                new CardRank(2),
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 3);
        }

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return CreatePet(
                definitionId,
                new InstanceId(instanceId));
        }

        private static CombatPetState CreatePet(
            string definitionId,
            InstanceId instanceId)
        {
            return new CombatPetState(
                new DefinitionId(definitionId),
                instanceId);
        }
    }
}