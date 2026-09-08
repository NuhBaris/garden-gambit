using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultDamageFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultDamageResolver(
                        null));
        }

        [Test]
        public void
            Constructor_ExposesExactModifierRegistry()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var resolver =
                new CombatResultDamageResolver(
                    registry);

            Assert.That(
                resolver.FinalRankModifierRegistry,
                Is.SameAs(
                    registry));
        }

        [Test]
        public void
            Resolve_WithBothSideModifiers_AppliesIndependentCrossedDamage()
        {
            var playerCard =
                CreateCard(
                    instanceId: 100,
                    rank: 3,
                    currentHp: 10);

            var enemyCard =
                CreateCard(
                    instanceId: 200,
                    rank: 5,
                    currentHp: 10);

            var state =
                new CombatState(
                    CreateSideWithPlacedCard(
                        CombatSide.Player,
                        playerCard,
                        attackMultiplier: 2),
                    CreateSideWithPlacedCard(
                        CombatSide.Enemy,
                        enemyCard,
                        attackMultiplier: 3));

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                playerCard.InstanceId,
                4);

            registry.AddModifier(
                enemyCard.InstanceId,
                2);

            var resolver =
                new CombatResultDamageResolver(
                    registry);

            var result =
                resolver.Resolve(
                    state);

            Assert.That(
                result.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                result.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(14));

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
                Is.EqualTo(14));

            Assert.That(
                playerCard.Rank,
                Is.EqualTo(
                    new CardRank(3)));

            Assert.That(
                enemyCard.Rank,
                Is.EqualTo(
                    new CardRank(5)));
        }

        [Test]
        public void
            Resolve_WithModifierForDeathThresholdCard_IgnoresModifier()
        {
            var deadPlayerCard =
                CreateCard(
                    instanceId: 100,
                    rank: 14,
                    currentHp: 0);

            var state =
                new CombatState(
                    CreateSideWithPlacedCard(
                        CombatSide.Player,
                        deadPlayerCard,
                        attackMultiplier: 1),
                    CreateEmptySide(
                        CombatSide.Enemy));

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                deadPlayerCard.InstanceId,
                100);

            var result =
                new CombatResultDamageResolver(
                    registry)
                    .Resolve(
                        state);

            Assert.That(
                result.PlayerContribution
                    .SurvivorCount,
                Is.Zero);

            Assert.That(
                result.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.Zero);

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.Zero);
        }

        [Test]
        public void
            Resolve_WithModifierForUnplacedCard_IgnoresModifier()
        {
            var unplacedPlayerCard =
                CreateCard(
                    instanceId: 100,
                    rank: 14,
                    currentHp: 10);

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        Array.Empty<
                            CombatSlotState>()),
                    new CombatCardRegistry(
                        new[]
                        {
                            unplacedPlayerCard
                        }),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var state =
                new CombatState(
                    playerSide,
                    CreateEmptySide(
                        CombatSide.Enemy));

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                unplacedPlayerCard.InstanceId,
                100);

            var result =
                new CombatResultDamageResolver(
                    registry)
                    .Resolve(
                        state);

            Assert.That(
                result.PlayerContribution
                    .SurvivorCount,
                Is.Zero);

            Assert.That(
                result.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.Zero);

            Assert.That(
                result.BaseIncomingDamageToEnemy,
                Is.Zero);
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank,
            int currentHp)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                hpCapacity: 10,
                currentHp: currentHp,
                armor: 0,
                attack: 3);
        }

        private static CombatSideState
            CreateSideWithPlacedCard(
                CombatSide side,
                CombatCardState card,
                int attackMultiplier)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(
                                side ==
                                CombatSide.Player
                                    ? 1
                                    : 2),
                            new BoardPosition(
                                side,
                                BoardRow.Front,
                                new BoardColumn(1)),
                            card.InstanceId)
                    }),
                new CombatCardRegistry(
                    new[]
                    {
                        card
                    }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    attackMultiplier));
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
    }
}