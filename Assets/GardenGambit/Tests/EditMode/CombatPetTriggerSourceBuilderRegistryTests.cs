using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceBuilderRegistryTests
    {
        [Test]
        public void
            BuildRegistry_WithEmptyPetSides_ReturnsEmptyRegistry()
        {
            var factoryRegistry =
                new
                    CombatPetTriggerSourceFactoryRegistry(
                        Array.Empty<
                            ICombatPetTriggerSourceFactory>());

            var builder =
                new CombatPetTriggerSourceBuilder(
                    factoryRegistry);

            var registry =
                builder.BuildRegistry(
                    CreateEmptyState());

            Assert.That(
                registry,
                Is.Not.Null);

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.Sources,
                Is.Empty);
        }

        [Test]
        public void
            BuildRegistry_PreservesPlayerThenEnemyPetOrder()
        {
            var definitionId =
                new DefinitionId(
                    "pet.test");

            var factory =
                new TestFactory(
                    definitionId);

            var builder =
                new CombatPetTriggerSourceBuilder(
                    new
                        CombatPetTriggerSourceFactoryRegistry(
                            new[]
                            {
                                factory
                            }));

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
                                CreatePet(
                                    definitionId,
                                    1001),

                                CreatePet(
                                    definitionId,
                                    1002)
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            new[]
                            {
                                CreatePet(
                                    definitionId,
                                    2001)
                            })));

            var registry =
                builder.BuildRegistry(
                    state);

            Assert.That(
                registry.Count,
                Is.EqualTo(3));

            AssertSource(
                registry.Sources[0],
                CombatSide.Player,
                new InstanceId(1001));

            AssertSource(
                registry.Sources[1],
                CombatSide.Player,
                new InstanceId(1002));

            AssertSource(
                registry.Sources[2],
                CombatSide.Enemy,
                new InstanceId(2001));
        }

        [Test]
        public void
            BuildRegistry_WithSunBirdFactory_DrivesStagedCombatAutomatically()
        {
            var sunBirdDefinitionId =
                new DefinitionId(
                    "pet.sun_bird");

            var sunBird =
                CreatePet(
                    sunBirdDefinitionId,
                    1001);

            var playerPosition =
                CreatePosition(
                    CombatSide.Player);

            var enemyPosition =
                CreatePosition(
                    CombatSide.Enemy);

            var playerCard =
                CreateCard(
                    "player-card",
                    1,
                    CombatCardSeason.Summer,
                    hp: 10,
                    attack: 2);

            var enemyCard =
                CreateCard(
                    "enemy-card",
                    2,
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

            var usageCommitter =
                new
                    CombatPetCardTriggerUsageCommitter(
                        new
                            CombatPetCardTriggerUsageRegistry());

            var modifierRegistry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var sunBirdFactory =
                new
                    SunBirdPetTriggerSourceFactory(
                        sunBirdDefinitionId,
                        usageCommitter,
                        modifierRegistry);

            var petSourceBuilder =
                new CombatPetTriggerSourceBuilder(
                    new
                        CombatPetTriggerSourceFactoryRegistry(
                            new[]
                            {
                                sunBirdFactory
                            }));

            var sourceRegistry =
                petSourceBuilder.BuildRegistry(
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
                    modifierRegistry);

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
                sourceRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                sourceRegistry.Sources[0],
                Is.TypeOf<
                    SunBirdPetTriggerSource>());

            Assert.That(
                playerAttackEvent.IsSummerAttack,
                Is.True);

            Assert.That(
                modifierRegistry
                    .GetTotalModifier(
                        playerAttackEvent
                            .Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                modifierRegistry
                    .ResolveDamage(
                        playerAttackEvent),
                Is.EqualTo(3));

            Assert.That(
                usageCommitter.HasTriggered(
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
            long instanceId,
            CombatCardSeason season,
            int hp,
            int attack)
        {
            return new CombatCardState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId),
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

        private static CombatPetState CreatePet(
            DefinitionId definitionId,
            long instanceId)
        {
            return new CombatPetState(
                definitionId,
                new InstanceId(
                    instanceId));
        }

        private static void AssertSource(
            ICombatTriggerSource source,
            CombatSide expectedSide,
            InstanceId expectedPetInstanceId)
        {
            var testSource =
                source as TestSource;

            Assert.That(
                testSource,
                Is.Not.Null);

            Assert.That(
                testSource.Side,
                Is.EqualTo(
                    expectedSide));

            Assert.That(
                testSource.PetInstanceId,
                Is.EqualTo(
                    expectedPetInstanceId));
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

        private sealed class TestFactory :
            ICombatPetTriggerSourceFactory
        {
            public TestFactory(
                DefinitionId petDefinitionId)
            {
                PetDefinitionId =
                    petDefinitionId;
            }

            public DefinitionId PetDefinitionId
            {
                get;
            }

            public IEnumerable<ICombatTriggerSource>
                CreateSources(
                    CombatSide side,
                    CombatPetState pet)
            {
                return new ICombatTriggerSource[]
                {
                    new TestSource(
                        side,
                        pet.InstanceId)
                };
            }
        }

        private sealed class TestSource :
            ICombatTriggerSource
        {
            public TestSource(
                CombatSide side,
                InstanceId petInstanceId)
            {
                Side =
                    side;

                PetInstanceId =
                    petInstanceId;
            }

            public CombatSide Side
            {
                get;
            }

            public InstanceId PetInstanceId
            {
                get;
            }

            public IEnumerable<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>
                DiscoverTriggers(
                    CombatState state,
                    CombatEvent sourceEvent)
            {
                return Array.Empty<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>();
            }
        }
    }
}