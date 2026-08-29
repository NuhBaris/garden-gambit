using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatColumnFrontlineResolverTests
    {
        [Test]
        public void TryGetExchangePositions_WithNullState_Throws()
        {
            var resolver =
                new CombatColumnFrontlineResolver();

            var playerPosition =
                default(BoardPosition);

            var enemyPosition =
                default(BoardPosition);

            Assert.Throws<ArgumentNullException>(
                () => resolver.TryGetExchangePositions(
                    null,
                    new BoardColumn(1),
                    out playerPosition,
                    out enemyPosition));
        }

        [Test]
        public void TryGetExchangePositions_WithInvalidColumn_Throws()
        {
            var environment =
                CreateEnvironment();

            var playerPosition =
                default(BoardPosition);

            var enemyPosition =
                default(BoardPosition);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        default(BoardColumn),
                        out playerPosition,
                        out enemyPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithBothLivingFrontCards_ReturnsExactPositions()
        {
            var environment =
                CreateEnvironment();

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var found =
                environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition);

            Assert.That(found, Is.True);

            Assert.That(
                playerPosition,
                Is.EqualTo(
                    environment.PlayerFrontPosition));

            Assert.That(
                enemyPosition,
                Is.EqualTo(
                    environment.EnemyFrontPosition));

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithoutPlayerFrontCard_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false);

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var found =
                environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition);

            Assert.That(found, Is.False);
            Assert.That(playerPosition.IsValid, Is.False);
            Assert.That(enemyPosition.IsValid, Is.False);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithoutEnemyFrontCard_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    enemyFrontOccupied: false);

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var found =
                environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition);

            Assert.That(found, Is.False);
            Assert.That(playerPosition.IsValid, Is.False);
            Assert.That(enemyPosition.IsValid, Is.False);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithOnlyBackCards_ReturnsFalseWithoutAdvancingCards()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false,
                    playerBackOccupied: true,
                    enemyBackOccupied: true);

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var found =
                environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition);

            Assert.That(found, Is.False);
            Assert.That(playerPosition.IsValid, Is.False);
            Assert.That(enemyPosition.IsValid, Is.False);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerBackPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Board.GetSlot(
                        environment.EnemyBackPosition)
                    .IsOccupied,
                Is.True);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Player)
                    .Board.GetSlot(
                        environment.PlayerFrontPosition)
                    .IsOccupied,
                Is.False);

            Assert.That(
                environment.State
                    .GetSide(CombatSide.Enemy)
                    .Board.GetSlot(
                        environment.EnemyFrontPosition)
                    .IsOccupied,
                Is.False);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithoutFrontSlots_ReturnsFalse()
        {
            var environment =
                CreateEnvironment(
                    playerFrontOccupied: false,
                    enemyFrontOccupied: false,
                    includePlayerFrontSlot: false,
                    includeEnemyFrontSlot: false);

            BoardPosition playerPosition;
            BoardPosition enemyPosition;

            var found =
                environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition);

            Assert.That(found, Is.False);
            Assert.That(playerPosition.IsValid, Is.False);
            Assert.That(enemyPosition.IsValid, Is.False);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithPlayerFrontAtDeathThreshold_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment(
                    playerFrontHp: 0);

            var playerPosition =
                default(BoardPosition);

            var enemyPosition =
                default(BoardPosition);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition));

            Assert.That(
                environment.PlayerFrontCard.CurrentHp,
                Is.Zero);

            AssertEnvironmentUnchanged(
                environment);
        }

        [Test]
        public void TryGetExchangePositions_WithEnemyFrontAtDeathThreshold_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment(
                    enemyFrontHp: 0);

            var playerPosition =
                default(BoardPosition);

            var enemyPosition =
                default(BoardPosition);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .TryGetExchangePositions(
                        environment.State,
                        environment.Column,
                        out playerPosition,
                        out enemyPosition));

            Assert.That(
                environment.EnemyFrontCard.CurrentHp,
                Is.Zero);

            AssertEnvironmentUnchanged(
                environment);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool playerFrontOccupied = true,
                bool enemyFrontOccupied = true,
                bool playerBackOccupied = false,
                bool enemyBackOccupied = false,
                int playerFrontHp = 5,
                int enemyFrontHp = 5,
                bool includePlayerFrontSlot = true,
                bool includeEnemyFrontSlot = true)
        {
            var column =
                new BoardColumn(1);

            CombatCardState playerFrontCard;
            CombatCardState playerBackCard;

            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    column,
                    1,
                    100,
                    includePlayerFrontSlot,
                    playerFrontOccupied,
                    playerBackOccupied,
                    playerFrontHp,
                    out playerFrontCard,
                    out playerBackCard);

            CombatCardState enemyFrontCard;
            CombatCardState enemyBackCard;

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    column,
                    3,
                    200,
                    includeEnemyFrontSlot,
                    enemyFrontOccupied,
                    enemyBackOccupied,
                    enemyFrontHp,
                    out enemyFrontCard,
                    out enemyBackCard);

            return new TestEnvironment
            {
                State =
                    new CombatState(
                        playerSide,
                        enemySide),
                Column = column,
                PlayerFrontPosition =
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        column),
                PlayerBackPosition =
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Back,
                        column),
                EnemyFrontPosition =
                    new BoardPosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        column),
                EnemyBackPosition =
                    new BoardPosition(
                        CombatSide.Enemy,
                        BoardRow.Back,
                        column),
                PlayerFrontCard =
                    playerFrontCard,
                PlayerBackCard =
                    playerBackCard,
                EnemyFrontCard =
                    enemyFrontCard,
                EnemyBackCard =
                    enemyBackCard,
                Resolver =
                    new CombatColumnFrontlineResolver()
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            BoardColumn column,
            int firstSlotId,
            long firstInstanceId,
            bool includeFrontSlot,
            bool frontOccupied,
            bool backOccupied,
            int frontHp,
            out CombatCardState frontCard,
            out CombatCardState backCard)
        {
            var slots =
                new List<CombatSlotState>();

            var cards =
                new List<CombatCardState>();

            frontCard = null;
            backCard = null;

            var frontPosition =
                new BoardPosition(
                    side,
                    BoardRow.Front,
                    column);

            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    column);

            if (includeFrontSlot)
            {
                if (frontOccupied)
                {
                    frontCard =
                        CreateCard(
                            side,
                            "front",
                            firstInstanceId,
                            frontHp);

                    cards.Add(frontCard);

                    slots.Add(
                        new CombatSlotState(
                            new SlotId(firstSlotId),
                            frontPosition,
                            frontCard.InstanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(firstSlotId),
                            frontPosition));
                }
            }

            if (backOccupied)
            {
                backCard =
                    CreateCard(
                        side,
                        "back",
                        firstInstanceId + 1,
                        5);

                cards.Add(backCard);

                slots.Add(
                    new CombatSlotState(
                        new SlotId(firstSlotId + 1),
                        backPosition,
                        backCard.InstanceId));
            }
            else
            {
                slots.Add(
                    new CombatSlotState(
                        new SlotId(firstSlotId + 1),
                        backPosition));
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            CombatSide side,
            string rowName,
            long instanceId,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card.{side}.{rowName}"),
                new InstanceId(instanceId),
                new CardRank(2),
                10,
                currentHp,
                0,
                3);
        }

        private static void AssertEnvironmentUnchanged(
            TestEnvironment environment)
        {
            if (environment.PlayerFrontCard != null)
            {
                Assert.That(
                    environment.PlayerFrontCard.CurrentHp,
                    Is.EqualTo(
                        environment.PlayerFrontCard
                            .CurrentHp));
            }

            if (environment.EnemyFrontCard != null)
            {
                Assert.That(
                    environment.EnemyFrontCard.CurrentHp,
                    Is.EqualTo(
                        environment.EnemyFrontCard
                            .CurrentHp));
            }

            if (environment.PlayerBackCard != null)
            {
                Assert.That(
                    environment.State
                        .GetSide(CombatSide.Player)
                        .GetCardAt(
                            environment.PlayerBackPosition),
                    Is.SameAs(
                        environment.PlayerBackCard));
            }

            if (environment.EnemyBackCard != null)
            {
                Assert.That(
                    environment.State
                        .GetSide(CombatSide.Enemy)
                        .GetCardAt(
                            environment.EnemyBackPosition),
                    Is.SameAs(
                        environment.EnemyBackCard));
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public BoardColumn Column { get; set; }

            public BoardPosition PlayerFrontPosition
            {
                get;
                set;
            }

            public BoardPosition PlayerBackPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyFrontPosition
            {
                get;
                set;
            }

            public BoardPosition EnemyBackPosition
            {
                get;
                set;
            }

            public CombatCardState PlayerFrontCard
            {
                get;
                set;
            }

            public CombatCardState PlayerBackCard
            {
                get;
                set;
            }

            public CombatCardState EnemyFrontCard
            {
                get;
                set;
            }

            public CombatCardState EnemyBackCard
            {
                get;
                set;
            }

            public CombatColumnFrontlineResolver
                Resolver
            {
                get;
                set;
            }
        }
    }
}