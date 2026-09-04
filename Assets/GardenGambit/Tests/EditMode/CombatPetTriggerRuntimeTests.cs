using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeTests
    {
        [Test]
        public void
            Constructor_WithNullUsageRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetTriggerRuntime(
                        null,
                        new
                            CombatNormalAttackSourceDamageModifierRegistry()));
        }

        [Test]
        public void
            Constructor_WithNullModifierRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetTriggerRuntime(
                        new
                            CombatPetCardTriggerUsageRegistry(),
                        null));
        }

        [Test]
        public void
            DefaultConstructor_CreatesCompleteDependencyGraph()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            Assert.That(
                runtime.UsageRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.UsageCommitter,
                Is.Not.Null);

            Assert.That(
                runtime
                    .SourceDamageModifierRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.FactoryCatalog,
                Is.Not.Null);

            Assert.That(
                runtime.FactoryRegistry,
                Is.Not.Null);

            Assert.That(
                runtime.SourceBuilder,
                Is.Not.Null);

            Assert.That(
                runtime.UsageCommitter
                    .UsageRegistry,
                Is.SameAs(
                    runtime.UsageRegistry));

            Assert.That(
                runtime.FactoryCatalog
                    .UsageCommitter,
                Is.SameAs(
                    runtime.UsageCommitter));

            Assert.That(
                runtime.FactoryCatalog
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    runtime
                        .SourceDamageModifierRegistry));

            Assert.That(
                runtime.SourceBuilder
                    .FactoryRegistry,
                Is.SameAs(
                    runtime.FactoryRegistry));
        }

        [Test]
        public void
            Constructor_WithDependencies_PreservesExactInstances()
        {
            var usageRegistry =
                new
                    CombatPetCardTriggerUsageRegistry();

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var runtime =
                new CombatPetTriggerRuntime(
                    usageRegistry,
                    modifierRegistry);

            Assert.That(
                runtime.UsageRegistry,
                Is.SameAs(
                    usageRegistry));

            Assert.That(
                runtime
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));

            Assert.That(
                runtime.UsageCommitter
                    .UsageRegistry,
                Is.SameAs(
                    usageRegistry));

            Assert.That(
                runtime.FactoryCatalog
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));
        }

        [Test]
        public void
            BuildSourceRegistry_WithNullState_Throws()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            Assert.Throws<ArgumentNullException>(
                () => runtime.BuildSourceRegistry(
                    null));
        }

        [Test]
        public void
            BuildSourceRegistry_WithNoPets_ReturnsEmptyRegistry()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var registry =
                runtime.BuildSourceRegistry(
                    CreateEmptyState());

            Assert.That(
                registry,
                Is.Not.Null);

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void
            BuildSourceRegistry_WithSunBird_CreatesSourceUsingRuntimeDependencies()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var sunBird =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .SunBird,
                    new InstanceId(1001));

            var state =
                new CombatState(
                    CreateEmptySide(
                        CombatSide.Player),
                    CreateEmptySide(
                        CombatSide.Enemy),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                sunBird
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            Array.Empty<
                                CombatPetState>())));

            var registry =
                runtime.BuildSourceRegistry(
                    state);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            var source =
                registry.Sources[0]
                    as SunBirdPetTriggerSource;

            Assert.That(
                source,
                Is.Not.Null);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    sunBird.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    runtime.UsageCommitter));

            Assert.That(
                source
                    .SourceDamageModifierRegistry,
                Is.SameAs(
                    runtime
                        .SourceDamageModifierRegistry));
        }

        [Test]
        public void
            RuntimeBuiltRegistry_DrivesSunBirdThroughMainStagedPipeline()
        {
            var runtime =
                new CombatPetTriggerRuntime();

            var sunBird =
                new CombatPetState(
                    CombatPetDefinitionIds
                        .SunBird,
                    new InstanceId(1001));

            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "player-card",
                    new InstanceId(1),
                    CombatCardSeason.Summer,
                    hp: 10,
                    attack: 2);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    new InstanceId(2),
                    CombatCardSeason.Winter,
                    hp: 3,
                    attack: 0);

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        playerCard,
                        playerPosition,
                        new SlotId(1),
                        new SlotId(2)),
                    CreateSide(
                        CombatSide.Enemy,
                        enemyCard,
                        enemyPosition,
                        new SlotId(3),
                        new SlotId(4)),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                sunBird
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            Array.Empty<
                                CombatPetState>())));

            var sourceRegistry =
                runtime.BuildSourceRegistry(
                    state);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var runner =
                new CombatResolutionRunner(
                    state,
                    metadataFactory,
                    eventLog,
                    new CombatEventQueue(
                        eventLog),
                    sourceRegistry,
                    runtime
                        .SourceDamageModifierRegistry);

            var completedEvent =
                runner.StartAndResolveCombatStaged(
                    10,
                    100,
                    100,
                    100);

            var playerAttackEvent =
                GetFirstAttackEvent(
                    eventLog,
                    CombatSide.Player);

            Assert.That(
                completedEvent,
                Is.Not.Null);

            Assert.That(
                runner.HasActiveCombat,
                Is.False);

            Assert.That(
                runner.ResolvedExchangeCount,
                Is.EqualTo(1));

            Assert.That(
                playerAttackEvent.AttackerSeason,
                Is.EqualTo(
                    CombatCardSeason.Summer));

            Assert.That(
                runtime
                    .SourceDamageModifierRegistry
                    .GetTotalModifier(
                        playerAttackEvent
                            .Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                runtime
                    .SourceDamageModifierRegistry
                    .ResolveDamage(
                        playerAttackEvent),
                Is.EqualTo(3));

            Assert.That(
                runtime.UsageCommitter
                    .HasTriggered(
                        sunBird.InstanceId,
                        playerCard.InstanceId),
                Is.True);

            Assert.That(
                state.Enemy.Cards.Count,
                Is.Zero);
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

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState card,
            BoardPosition frontPosition,
            SlotId frontSlotId,
            SlotId backSlotId)
        {
            var backPosition =
                new BoardPosition(
                    side,
                    BoardRow.Back,
                    frontPosition.Column);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[]
                    {
                        new CombatSlotState(
                            frontSlotId,
                            frontPosition,
                            card.InstanceId),

                        new CombatSlotState(
                            backSlotId,
                            backPosition)
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

        private static CombatCardState CreateCard(
            string definitionId,
            InstanceId instanceId,
            CombatCardSeason season,
            int hp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                instanceId,
                new CardRank(2),
                season,
                hpCapacity: hp,
                currentHp: hp,
                armor: 0,
                attack: attack);
        }

        private static BoardPosition CreatePosition(
            CombatSide side)
        {
            return new BoardPosition(
                side,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static NormalAttackCombatEvent
            GetFirstAttackEvent(
                CombatEventLog eventLog,
                CombatSide attackerSide)
        {
            for (var index = 0;
                 index < eventLog.Count;
                 index++)
            {
                var attackEvent =
                    eventLog.Events[index]
                        as NormalAttackCombatEvent;

                if (attackEvent != null &&
                    attackEvent.AttackerSide ==
                    attackerSide)
                {
                    return attackEvent;
                }
            }

            throw new InvalidOperationException(
                $"Normal Attack event was not found " +
                $"for {attackerSide}.");
        }
    }
}