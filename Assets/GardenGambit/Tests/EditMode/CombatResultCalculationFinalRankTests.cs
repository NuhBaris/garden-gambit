using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultCalculationFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullFinalRankRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultCalculationResolver(
                        CreateMetadataFactory(),
                        new CombatEventLog(),
                        null));
        }

        [Test]
        public void
            Constructor_ExposesExactFinalRankRegistry()
        {
            var registry =
                new CombatFinalRankModifierRegistry();

            var resolver =
                new CombatResultCalculationResolver(
                    CreateMetadataFactory(),
                    new CombatEventLog(),
                    registry);

            Assert.That(
                resolver.FinalRankModifierRegistry,
                Is.SameAs(
                    registry));
        }

        [Test]
        public void
            Resolve_WithFinalRankModifiers_LogsModifiedCrossedDamageSnapshot()
        {
            var playerCard =
                CreateCard(
                    instanceId: 100,
                    rank: 3);

            var enemyCard =
                CreateCard(
                    instanceId: 200,
                    rank: 5);

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        playerCard,
                        attackMultiplier: 2),
                    CreateSide(
                        CombatSide.Enemy,
                        enemyCard,
                        attackMultiplier: 3));

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var combatStartedEvent =
                new CombatStartResolver(
                    metadataFactory,
                    eventLog)
                    .Start(
                        state);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                playerCard.InstanceId,
                4);

            registry.AddModifier(
                enemyCard.InstanceId,
                2);

            var resolver =
                new CombatResultCalculationResolver(
                    metadataFactory,
                    eventLog,
                    registry);

            var resultEvent =
                resolver.Resolve(
                    state,
                    combatStartedEvent);

            Assert.That(
                resultEvent.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                resultEvent.PlayerContribution
                    .FinalAttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(2)));

            Assert.That(
                resultEvent.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(14));

            Assert.That(
                resultEvent.EnemyContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                resultEvent.EnemyContribution
                    .FinalAttackMultiplier,
                Is.EqualTo(
                    new AttackMultiplier(3)));

            Assert.That(
                resultEvent.EnemyContribution
                    .FinalResultContribution,
                Is.EqualTo(21));

            Assert.That(
                resultEvent.BaseIncomingDamageToPlayer,
                Is.EqualTo(21));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToPlayer,
                Is.EqualTo(21));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(14));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(14));

            Assert.That(
                resultEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    combatStartedEvent
                        .Metadata.EventId));

            Assert.That(
                eventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                eventLog.Events[1],
                Is.SameAs(
                    resultEvent));

            Assert.That(
                playerCard.Rank,
                Is.EqualTo(
                    new CardRank(3)));

            Assert.That(
                enemyCard.Rank,
                Is.EqualTo(
                    new CardRank(5)));
        }

        private static CombatCardState CreateCard(
            long instanceId,
            int rank)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"card-{instanceId}"),
                new InstanceId(
                    instanceId),
                new CardRank(
                    rank),
                hpCapacity: 10,
                currentHp: 10,
                armor: 0,
                attack: 3);
        }

        private static CombatSideState CreateSide(
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }
    }
}