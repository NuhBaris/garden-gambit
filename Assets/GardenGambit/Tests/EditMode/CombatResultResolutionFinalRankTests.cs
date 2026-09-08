using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResultResolutionFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullFinalRankRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResultResolutionResolver(
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
                new CombatResultResolutionResolver(
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
            ResolveAfterNormalColumns_WithFinalRankModifier_AppliesModifiedResultAndCompletesCombat()
        {
            var playerCard =
                new CombatCardState(
                    new DefinitionId(
                        "player-card"),
                    new InstanceId(100),
                    new CardRank(3),
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: 0,
                    attack: 3);

            var state =
                new CombatState(
                    CreateSideWithCard(
                        CombatSide.Player,
                        playerCard),
                    CreateEmptySide(
                        CombatSide.Enemy));

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

            var columnStartResolver =
                new CombatColumnStartResolver(
                    metadataFactory,
                    eventLog);

            for (var columnValue = 1;
                 columnValue <= 5;
                 columnValue++)
            {
                columnStartResolver.StartColumn(
                    state,
                    combatStartedEvent,
                    new BoardColumn(
                        columnValue));
            }

            var battleEndEvent =
                new CombatBattleEndResolver(
                    metadataFactory,
                    eventLog)
                    .StartBattleEnd(
                        state,
                        combatStartedEvent);

            var registry =
                new CombatFinalRankModifierRegistry();

            registry.AddModifier(
                playerCard.InstanceId,
                4);

            var resolver =
                new CombatResultResolutionResolver(
                    metadataFactory,
                    eventLog,
                    registry);

            var completedEvent =
                resolver.ResolveAfterNormalColumns(
                    state,
                    combatStartedEvent);

            var resultEvent =
                GetSingleResultEvent(
                    eventLog);

            Assert.That(
                resultEvent.PlayerContribution
                    .TotalSurvivorRankContribution,
                Is.EqualTo(7));

            Assert.That(
                resultEvent.PlayerContribution
                    .FinalResultContribution,
                Is.EqualTo(7));

            Assert.That(
                resultEvent.BaseIncomingDamageToEnemy,
                Is.EqualTo(7));

            Assert.That(
                resultEvent
                    .ResolvedIncomingDamageToEnemy,
                Is.EqualTo(7));

            Assert.That(
                state.Enemy.BattleHealth,
                Is.EqualTo(
                    new BattleHealth(13)));

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(13)));

            Assert.That(
                battleEndEvent.Metadata.SequenceNo,
                Is.LessThan(
                    resultEvent.Metadata.SequenceNo));

            Assert.That(
                resultEvent.Metadata.SequenceNo,
                Is.LessThan(
                    completedEvent.Metadata.SequenceNo));

            Assert.That(
                playerCard.Rank,
                Is.EqualTo(
                    new CardRank(3)));
        }

        private static
            CombatResultCalculatedCombatEvent
            GetSingleResultEvent(
                CombatEventLog eventLog)
        {
            CombatResultCalculatedCombatEvent
                resultEvent = null;

            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var candidate =
                    eventLog.Events[index]
                        as
                        CombatResultCalculatedCombatEvent;

                if (candidate == null)
                {
                    continue;
                }

                if (resultEvent != null)
                {
                    throw new InvalidOperationException(
                        "Multiple Combat Result events " +
                        "were found.");
                }

                resultEvent =
                    candidate;
            }

            if (resultEvent == null)
            {
                throw new InvalidOperationException(
                    "Combat Result event was not found.");
            }

            return resultEvent;
        }

        private static CombatSideState
            CreateSideWithCard(
                CombatSide side,
                CombatCardState card)
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
                    AttackMultiplier.BaseValue));
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

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }
    }
}