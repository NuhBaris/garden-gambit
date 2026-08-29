using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultDamageResolverTests
    {
        [Test]
        public void Resolve_WithNullState_Throws()
        {
            var resolver =
                new CombatResultDamageResolver();

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(null));
        }

        [Test]
        public void Resolve_WithEmptyBoards_ReturnsZeroDamage()
        {
            var state =
                CreateState(
                    new CombatCardState[0],
                    new CombatCardState[0],
                    2,
                    new CombatCardState[0],
                    new CombatCardState[0],
                    3);

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.IsValid,
                Is.True);

            Assert.That(
                result.PlayerContribution
                    .SurvivorCount,
                Is.Zero);

            Assert.That(
                result.EnemyContribution
                    .SurvivorCount,
                Is.Zero);

            Assert.That(
                result.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                result.HasMutualIncomingDamage,
                Is.False);
        }

        [Test]
        public void Resolve_WithBothSidesPopulated_CalculatesAndCrossesContributions()
        {
            var firstPlayerCard =
                CreateCard(
                    100,
                    2,
                    5);

            var secondPlayerCard =
                CreateCard(
                    101,
                    10,
                    7);

            var enemyCard =
                CreateCard(
                    200,
                    7,
                    4);

            var playerCards =
                new[]
                {
                    firstPlayerCard,
                    secondPlayerCard
                };

            var enemyCards =
                new[]
                {
                    enemyCard
                };

            var state =
                CreateState(
                    playerCards,
                    playerCards,
                    2,
                    enemyCards,
                    enemyCards,
                    3);

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.PlayerContribution
                    .SurvivorCount,
                Is.EqualTo(2));

            Assert.That(
                result.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(12));

            Assert.That(
                result.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(24));

            Assert.That(
                result.EnemyContribution
                    .SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result.EnemyContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                result.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(21));

            Assert.That(
                result.BaseIncomingDamageToPlayer,
                Is.EqualTo(21));

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.EqualTo(24));

            Assert.That(
                result.HasMutualIncomingDamage,
                Is.True);
        }

        [Test]
        public void Resolve_WithOnlyPlayerSurvivor_DamagesOnlyEnemy()
        {
            var playerCard =
                CreateCard(
                    100,
                    8,
                    5);

            var state =
                CreateState(
                    new[] { playerCard },
                    new[] { playerCard },
                    2,
                    new CombatCardState[0],
                    new CombatCardState[0],
                    4);

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(16));

            Assert.That(
                result.EnemyContribution
                    .FinalResultContribution,
                Is.Zero);

            Assert.That(
                result.BaseIncomingDamageToPlayer,
                Is.Zero);

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.EqualTo(16));

            Assert.That(
                result.HasIncomingDamageToPlayer,
                Is.False);

            Assert.That(
                result.HasIncomingDamageToEnemy,
                Is.True);
        }

        [Test]
        public void Resolve_WithOnlyEnemySurvivor_DamagesOnlyPlayer()
        {
            var enemyCard =
                CreateCard(
                    200,
                    5,
                    5);

            var state =
                CreateState(
                    new CombatCardState[0],
                    new CombatCardState[0],
                    2,
                    new[] { enemyCard },
                    new[] { enemyCard },
                    4);

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.PlayerContribution
                    .FinalResultContribution,
                Is.Zero);

            Assert.That(
                result.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(20));

            Assert.That(
                result.BaseIncomingDamageToPlayer,
                Is.EqualTo(20));

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.Zero);

            Assert.That(
                result.HasIncomingDamageToPlayer,
                Is.True);

            Assert.That(
                result.HasIncomingDamageToEnemy,
                Is.False);
        }

        [Test]
        public void Resolve_WithDeathThresholdCards_ExcludesThemFromBothSides()
        {
            var deadPlayerCard =
                CreateCard(
                    100,
                    14,
                    0);

            var livingPlayerCard =
                CreateCard(
                    101,
                    3,
                    1);

            var deadEnemyCard =
                CreateCard(
                    200,
                    13,
                    -2);

            var livingEnemyCard =
                CreateCard(
                    201,
                    4,
                    2);

            var playerCards =
                new[]
                {
                    deadPlayerCard,
                    livingPlayerCard
                };

            var enemyCards =
                new[]
                {
                    deadEnemyCard,
                    livingEnemyCard
                };

            var state =
                CreateState(
                    playerCards,
                    playerCards,
                    1,
                    enemyCards,
                    enemyCards,
                    1);

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.PlayerContribution
                    .SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(3));

            Assert.That(
                result.EnemyContribution
                    .SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result.EnemyContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(4));

            Assert.That(
                result.BaseIncomingDamageToPlayer,
                Is.EqualTo(4));

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.EqualTo(3));
        }

        [Test]
        public void Resolve_WithUnplacedRegistryCards_IgnoresThemOnBothSides()
        {
            var placedPlayerCard =
                CreateCard(
                    100,
                    2,
                    5);

            var unplacedPlayerCard =
                CreateCard(
                    101,
                    14,
                    10);

            var placedEnemyCard =
                CreateCard(
                    200,
                    3,
                    5);

            var unplacedEnemyCard =
                CreateCard(
                    201,
                    13,
                    10);

            var state =
                CreateState(
                    new[]
                    {
                        placedPlayerCard,
                        unplacedPlayerCard
                    },
                    new[]
                    {
                        placedPlayerCard
                    },
                    1,
                    new[]
                    {
                        placedEnemyCard,
                        unplacedEnemyCard
                    },
                    new[]
                    {
                        placedEnemyCard
                    },
                    1);

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                state.GetSide(CombatSide.Player)
                    .Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                state.GetSide(CombatSide.Enemy)
                    .Cards.Count,
                Is.EqualTo(2));

            Assert.That(
                result.PlayerContribution
                    .SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(2));

            Assert.That(
                result.EnemyContribution
                    .SurvivorCount,
                Is.EqualTo(1));

            Assert.That(
                result.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(3));

            Assert.That(
                result.BaseIncomingDamageToPlayer,
                Is.EqualTo(3));

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WithValidState_DoesNotChangeCardsBoardsOrBattleHealth()
        {
            var playerCard =
                CreateCard(
                    100,
                    9,
                    6);

            var enemyCard =
                CreateCard(
                    200,
                    7,
                    4);

            var state =
                CreateState(
                    new[] { playerCard },
                    new[] { playerCard },
                    2,
                    new[] { enemyCard },
                    new[] { enemyCard },
                    3);

            var playerSide =
                state.GetSide(
                    CombatSide.Player);

            var enemySide =
                state.GetSide(
                    CombatSide.Enemy);

            var playerPosition =
                CreateFrontPosition(
                    CombatSide.Player);

            var enemyPosition =
                CreateFrontPosition(
                    CombatSide.Enemy);

            var previousPlayerBattleHealth =
                playerSide.BattleHealth;

            var previousEnemyBattleHealth =
                enemySide.BattleHealth;

            var resolver =
                new CombatResultDamageResolver();

            var result =
                resolver.Resolve(state);

            Assert.That(
                result.IsValid,
                Is.True);

            Assert.That(
                playerCard.CurrentHp,
                Is.EqualTo(6));

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                playerCard.Rank.Value,
                Is.EqualTo(9));

            Assert.That(
                enemyCard.Rank.Value,
                Is.EqualTo(7));

            Assert.That(
                playerSide.GetCardAt(
                    playerPosition),
                Is.SameAs(playerCard));

            Assert.That(
                enemySide.GetCardAt(
                    enemyPosition),
                Is.SameAs(enemyCard));

            Assert.That(
                playerSide.BattleHealth,
                Is.EqualTo(
                    previousPlayerBattleHealth));

            Assert.That(
                enemySide.BattleHealth,
                Is.EqualTo(
                    previousEnemyBattleHealth));
        }

        [Test]
        public void Resolve_WhenEnemyContributionOverflows_ThrowsWithoutChangingState()
        {
            var playerCard =
                CreateCard(
                    100,
                    2,
                    5);

            var enemyCard =
                CreateCard(
                    200,
                    14,
                    10);

            var state =
                CreateState(
                    new[] { playerCard },
                    new[] { playerCard },
                    1,
                    new[] { enemyCard },
                    new[] { enemyCard },
                    int.MaxValue);

            var playerSide =
                state.GetSide(
                    CombatSide.Player);

            var enemySide =
                state.GetSide(
                    CombatSide.Enemy);

            var resolver =
                new CombatResultDamageResolver();

            Assert.Throws<OverflowException>(
                () => resolver.Resolve(state));

            Assert.That(
                playerCard.CurrentHp,
                Is.EqualTo(5));

            Assert.That(
                enemyCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                playerSide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                enemySide.Cards.Count,
                Is.EqualTo(1));

            Assert.That(
                playerSide.BattleHealth.Value,
                Is.EqualTo(
                    BattleHealth.NormalBaselineValue));

            Assert.That(
                enemySide.BattleHealth.Value,
                Is.EqualTo(
                    BattleHealth.NormalBaselineValue));
        }

        private static CombatState CreateState(
            IEnumerable<CombatCardState>
                playerRegisteredCards,
            IReadOnlyList<CombatCardState>
                playerPlacedCards,
            int playerAttackMultiplier,
            IEnumerable<CombatCardState>
                enemyRegisteredCards,
            IReadOnlyList<CombatCardState>
                enemyPlacedCards,
            int enemyAttackMultiplier)
        {
            var playerSide =
                CreateSide(
                    CombatSide.Player,
                    playerRegisteredCards,
                    playerPlacedCards,
                    1,
                    playerAttackMultiplier);

            var enemySide =
                CreateSide(
                    CombatSide.Enemy,
                    enemyRegisteredCards,
                    enemyPlacedCards,
                    11,
                    enemyAttackMultiplier);

            return new CombatState(
                playerSide,
                enemySide);
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            IEnumerable<CombatCardState>
                registeredCards,
            IReadOnlyList<CombatCardState>
                placedCards,
            int firstSlotId,
            int attackMultiplier)
        {
            var slots =
                new List<CombatSlotState>();

            var nextSlotId =
                firstSlotId;

            for (var columnValue =
                     BoardColumn.MinimumValue;
                 columnValue <=
                 BoardColumn.MaximumValue;
                 columnValue++)
            {
                var column =
                    new BoardColumn(
                        columnValue);

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

                var placedCardIndex =
                    columnValue -
                    BoardColumn.MinimumValue;

                if (placedCardIndex <
                    placedCards.Count)
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition,
                            placedCards[
                                placedCardIndex]
                                .InstanceId));
                }
                else
                {
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                nextSlotId),
                            frontPosition));
                }

                nextSlotId++;

                slots.Add(
                    new CombatSlotState(
                        new SlotId(
                            nextSlotId),
                        backPosition));

                nextSlotId++;
            }

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    registeredCards),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    attackMultiplier));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    "result-damage-card-" +
                    instanceId),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                10,
                currentHp,
                0,
                3);
        }

        private static BoardPosition
            CreateFrontPosition(
                CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}