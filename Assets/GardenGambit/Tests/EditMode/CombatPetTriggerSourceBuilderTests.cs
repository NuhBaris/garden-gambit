using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerSourceBuilderTests
    {
        [Test]
        public void
            Constructor_WithNullFactoryRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatPetTriggerSourceBuilder(
                        null));
        }

        [Test]
        public void
            Constructor_ExposesExactFactoryRegistry()
        {
            var registry =
                CreateRegistry();

            var builder =
                new CombatPetTriggerSourceBuilder(
                    registry);

            Assert.That(
                builder.FactoryRegistry,
                Is.SameAs(
                    registry));
        }

        [Test]
        public void BuildSources_WithNullState_Throws()
        {
            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry());

            Assert.Throws<ArgumentNullException>(
                () => builder.BuildSources(
                    null));
        }

        [Test]
        public void
            BuildSources_WithNoPets_ReturnsEmptyCollection()
        {
            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry());

            var sources =
                builder.BuildSources(
                    CreateState(
                        Array.Empty<
                            CombatPetState>(),
                        Array.Empty<
                            CombatPetState>()));

            Assert.That(
                sources,
                Is.Not.Null);

            Assert.That(
                sources,
                Is.Empty);
        }

        [Test]
        public void
            BuildSources_WithUnregisteredPet_Throws()
        {
            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry());

            var state =
                CreateState(
                    new[]
                    {
                        CreatePet(
                            "pet.unregistered",
                            1001)
                    },
                    Array.Empty<
                        CombatPetState>());

            Assert.Throws<InvalidOperationException>(
                () => builder.BuildSources(
                    state));
        }

        [Test]
        public void
            BuildSources_WhenFactoryReturnsNull_Throws()
        {
            var factory =
                new TestFactory(
                    new DefinitionId(
                        "pet.test"),
                    (side, pet) => null);

            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry(
                        factory));

            var state =
                CreateState(
                    new[]
                    {
                        CreatePet(
                            "pet.test",
                            1001)
                    },
                    Array.Empty<
                        CombatPetState>());

            Assert.Throws<InvalidOperationException>(
                () => builder.BuildSources(
                    state));
        }

        [Test]
        public void
            BuildSources_WhenFactoryContainsNullSource_Throws()
        {
            var factory =
                new TestFactory(
                    new DefinitionId(
                        "pet.test"),
                    (side, pet) =>
                        new ICombatTriggerSource[]
                        {
                            new TestSource(
                                "valid"),
                            null
                        });

            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry(
                        factory));

            var state =
                CreateState(
                    new[]
                    {
                        CreatePet(
                            "pet.test",
                            1001)
                    },
                    Array.Empty<
                        CombatPetState>());

            Assert.Throws<InvalidOperationException>(
                () => builder.BuildSources(
                    state));
        }

        [Test]
        public void
            BuildSources_PreservesPlayerThenEnemyAndPetOrder()
        {
            var factory =
                new TestFactory(
                    new DefinitionId(
                        "pet.test"),
                    (side, pet) =>
                        new ICombatTriggerSource[]
                        {
                            new TestSource(
                                $"{side}:" +
                                $"{pet.InstanceId}")
                        });

            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry(
                        factory));

            var state =
                CreateState(
                    new[]
                    {
                        CreatePet(
                            "pet.test",
                            1001),
                        CreatePet(
                            "pet.test",
                            1002)
                    },
                    new[]
                    {
                        CreatePet(
                            "pet.test",
                            2001),
                        CreatePet(
                            "pet.test",
                            2002)
                    });

            var sources =
                builder.BuildSources(
                    state);

            Assert.That(
                sources.Count,
                Is.EqualTo(4));

            AssertSourceName(
                sources[0],
                "Player:1001");

            AssertSourceName(
                sources[1],
                "Player:1002");

            AssertSourceName(
                sources[2],
                "Enemy:2001");

            AssertSourceName(
                sources[3],
                "Enemy:2002");
        }

        [Test]
        public void
            BuildSources_PreservesEachFactorysSourceOrder()
        {
            var factory =
                new TestFactory(
                    new DefinitionId(
                        "pet.test"),
                    (side, pet) =>
                        new ICombatTriggerSource[]
                        {
                            new TestSource(
                                "first"),
                            new TestSource(
                                "second"),
                            new TestSource(
                                "third")
                        });

            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry(
                        factory));

            var state =
                CreateState(
                    new[]
                    {
                        CreatePet(
                            "pet.test",
                            1001)
                    },
                    Array.Empty<
                        CombatPetState>());

            var sources =
                builder.BuildSources(
                    state);

            Assert.That(
                sources.Count,
                Is.EqualTo(3));

            AssertSourceName(
                sources[0],
                "first");

            AssertSourceName(
                sources[1],
                "second");

            AssertSourceName(
                sources[2],
                "third");
        }

        [Test]
        public void
            BuildSources_WithSunBirdFactory_CreatesConfiguredSunBirdSource()
        {
            var definitionId =
                new DefinitionId(
                    "pet.sun_bird");

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
                        definitionId,
                        usageCommitter,
                        modifierRegistry);

            var builder =
                new CombatPetTriggerSourceBuilder(
                    CreateRegistry(
                        sunBirdFactory));

            var pet =
                new CombatPetState(
                    definitionId,
                    new InstanceId(1001));

            var state =
                CreateState(
                    new[]
                    {
                        pet
                    },
                    Array.Empty<
                        CombatPetState>());

            var sources =
                builder.BuildSources(
                    state);

            Assert.That(
                sources.Count,
                Is.EqualTo(1));

            var source =
                sources[0]
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
                    pet.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                source.SourceDamageModifierRegistry,
                Is.SameAs(
                    modifierRegistry));
        }

        private static
            CombatPetTriggerSourceFactoryRegistry
            CreateRegistry(
                params
                    ICombatPetTriggerSourceFactory[]
                    factories)
        {
            return new
                CombatPetTriggerSourceFactoryRegistry(
                    factories);
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets,
            CombatPetState[] enemyPets)
        {
            return new CombatState(
                CreateEmptySide(
                    CombatSide.Player),
                CreateEmptySide(
                    CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        enemyPets)));
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

        private static CombatPetState CreatePet(
            string definitionId,
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(
                    definitionId),
                new InstanceId(
                    instanceId));
        }

        private static void AssertSourceName(
            ICombatTriggerSource source,
            string expectedName)
        {
            var testSource =
                source as TestSource;

            Assert.That(
                testSource,
                Is.Not.Null);

            Assert.That(
                testSource.Name,
                Is.EqualTo(
                    expectedName));
        }

        private sealed class TestFactory :
            ICombatPetTriggerSourceFactory
        {
            private readonly Func<
                CombatSide,
                CombatPetState,
                IEnumerable<ICombatTriggerSource>>
                _createAction;

            public TestFactory(
                DefinitionId petDefinitionId,
                Func<
                    CombatSide,
                    CombatPetState,
                    IEnumerable<ICombatTriggerSource>>
                    createAction)
            {
                PetDefinitionId =
                    petDefinitionId;

                _createAction =
                    createAction;
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
                return _createAction(
                    side,
                    pet);
            }
        }

        private sealed class TestSource :
            ICombatTriggerSource
        {
            public TestSource(
                string name)
            {
                Name =
                    name;
            }

            public string Name
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