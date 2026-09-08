using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatResolutionRunnerFinalRankTests
    {
        [Test]
        public void
            Constructor_WithNullFinalRankModifierRegistry_Throws()
        {
            var state =
                CreatePlayerOnlyState(
                    out _);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatResolutionRunner(
                        state,
                        metadataFactory,
                        eventLog,
                        new CombatEventQueue(
                            eventLog),
                        CreateSourceRegistry(),
                        new
                            CombatNormalAttackSourceDamageModifierRegistry(),
                        CreateTargetReductionResolver(),
                        null));
        }

        [Test]
        public void
            Constructor_WithExplicitFinalRankRegistry_ExposesExactRegistry()
        {
            var state =
                CreatePlayerOnlyState(
                    out _);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    new CombatEventQueue(
                        eventLog),
                    CreateSourceRegistry(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry(),
                    CreateTargetReductionResolver(),
                    finalRankRegistry);

            Assert.That(
                runner.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankRegistry));
        }

        [Test]
        public void
            Constructor_WithoutExplicitFinalRankRegistry_CreatesEmptyRegistry()
        {
            var state =
                CreatePlayerOnlyState(
                    out _);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    new CombatEventQueue(
                        eventLog),
                    CreateSourceRegistry());

            Assert.That(
                runner.FinalRankModifierRegistry,
                Is.Not.Null);

            Assert.That(
                runner.FinalRankModifierRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            StartAndResolveCombat_WithFinalRankModifier_AppliesModifierBeforeMultiplier()
        {
            CombatCardState playerCard;

            var state =
                CreatePlayerOnlyState(
                    out playerCard);

            var metadataFactory =
                CreateMetadataFactory();

            var eventLog =
                new CombatEventLog();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            finalRankRegistry.AddModifier(
                playerCard.InstanceId,
                rankDelta: 1);

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    new CombatEventQueue(
                        eventLog),
                    CreateSourceRegistry(),
                    new
                        CombatNormalAttackSourceDamageModifierRegistry(),
                    CreateTargetReductionResolver(),
                    finalRankRegistry);

            var completedEvent =
                runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 10,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);

            Assert.That(
                completedEvent.Outcome,
                Is.EqualTo(
                    CombatOutcome.PlayerVictory));

            Assert.That(
                completedEvent.PlayerBattleHealth,
                Is.EqualTo(
                    new BattleHealth(20)));

            Assert.That(
                completedEvent.EnemyBattleHealth,
                Is.EqualTo(
                    new BattleHealth(14)));

            Assert.That(
                finalRankRegistry.GetTotalModifier(
                    playerCard.InstanceId),
                Is.EqualTo(1));

            Assert.That(
                runner.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankRegistry));
        }

        private static CombatState
            CreatePlayerOnlyState(
                out CombatCardState playerCard)
        {
            playerCard =
                new CombatCardState(
                    new DefinitionId(
                        "test.player-card"),
                    new InstanceId(100),
                    new CardRank(2),
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: 0,
                    attack: 0);

            var frontPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var backPosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Back,
                    new BoardColumn(1));

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                frontPosition,
                                playerCard.InstanceId),
                            new CombatSlotState(
                                new SlotId(2),
                                backPosition)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            playerCard
                        }),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(2));

            return new CombatState(
                playerSide,
                CreateEmptySide(
                    CombatSide.Enemy));
        }

        private static CombatSideState
            CreateEmptySide(
                CombatSide side)
        {
            return CreateSideState(
                side,
                Array.Empty<
                    CombatSlotState>(),
                Array.Empty<
                    CombatCardState>());
        }

        private static CombatSideState
            CreateSideState(
                CombatSide side,
                CombatSlotState[] slots,
                CombatCardState[] cards)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static
            CombatNormalAttackTargetDamageReductionResolver
            CreateTargetReductionResolver()
        {
            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        new
                            CombatPetCardTriggerUsageRegistry());

            return new
                CombatNormalAttackTargetDamageReductionResolver(
                    new
                        CombatNormalAttackTargetDamageReductionRegistry(),
                    usageCommitter);
        }

        private static CombatTriggerSourceRegistry
            CreateSourceRegistry()
        {
            return new CombatTriggerSourceRegistry(
                Array.Empty<
                    ICombatTriggerSource>());
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new
                    CombatSequenceNumberAllocator());
        }
    }
}