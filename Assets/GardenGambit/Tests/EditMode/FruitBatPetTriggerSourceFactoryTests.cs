using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        FruitBatPetTriggerSourceFactoryTests
    {
        [Test]
        public void Factory_PreservesRegistrationAndDependencies()
        {
            var environment =
                new Environment();

            var factory =
                CreateFactory(
                    environment);

            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    environment.Pet.DefinitionId));

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                factory.HpGainResolver,
                Is.SameAs(
                    environment.HpGainResolver));

            Assert.That(
                factory.EventLog,
                Is.SameAs(
                    environment.Log));

            var registry =
                new CombatPetTriggerSourceFactoryRegistry(
                    new[] { factory });

            Assert.That(
                registry.GetFactory(
                    environment.Pet.DefinitionId),
                Is.SameAs(
                    factory));
        }

        [Test]
        public void FactoryConstructor_RejectsInvalidInputs()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentException>(
                () => new FruitBatPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Factory_CreatesOneSourceAndResolvesFruitSurvivor(
            CombatSide side,
            BoardRow row)
        {
            var environment =
                new Environment(
                    side,
                    row);

            var source =
                CreateSource(
                    CreateFactory(
                        environment),
                    side,
                    environment.Pet);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    side));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    environment.UsageCommitter));

            Assert.That(
                source.HpGainResolver,
                Is.SameAs(
                    environment.HpGainResolver));

            Assert.That(
                source.EventLog,
                Is.SameAs(
                    environment.Log));

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(
                    side));

            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            var damageEvent =
                environment.CreateNormalAttackDamage();

            var candidates =
                Discover(
                    source,
                    environment.State,
                    damageEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(
                    source.Handler));

            candidates[0].Trigger.Resolve(
                environment.State,
                damageEvent);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.SourceCard.CurrentHp,
                Is.EqualTo(11));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                Discover(
                    source,
                    environment.State,
                    damageEvent),
                Is.Empty);
        }

        [Test]
        public void CreateSources_RejectsInvalidSideNullPetAndWrongDefinition()
        {
            var environment =
                new Environment();

            var factory =
                CreateFactory(
                    environment);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSource(
                    factory,
                    (CombatSide)99,
                    environment.Pet));

            Assert.Throws<ArgumentNullException>(
                () => CreateSource(
                    factory,
                    environment.Side,
                    null));

            var wrongPet =
                new CombatPetState(
                    new DefinitionId(
                        "test.other_pet"),
                    environment.Pet.InstanceId);

            Assert.Throws<ArgumentException>(
                () => CreateSource(
                    factory,
                    environment.Side,
                    wrongPet));
        }

        [Test]
        public void SourceConstructor_RejectsInvalidInputs()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FruitBatPetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentException>(
                () => new FruitBatPetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.HpGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new FruitBatPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    null));
        }

        [Test]
        public void Discovery_IgnoresWrongEventAndNonFruitCard()
        {
            var straightEnvironment =
                new Environment();

            var straightSource =
                CreateSource(
                    CreateFactory(
                        straightEnvironment),
                    straightEnvironment.Side,
                    straightEnvironment.Pet);

            Assert.That(
                Discover(
                    straightSource,
                    straightEnvironment.State,
                    straightEnvironment.CombatStartedEvent),
                Is.Empty);

            var nonFruitEnvironment =
                new Environment(
                    sourceSuit:
                        CombatCardSuit.Vegetable);

            var nonFruitSource =
                CreateSource(
                    CreateFactory(
                        nonFruitEnvironment),
                    nonFruitEnvironment.Side,
                    nonFruitEnvironment.Pet);

            Assert.That(
                Discover(
                    nonFruitSource,
                    nonFruitEnvironment.State,
                    nonFruitEnvironment
                        .CreateNormalAttackDamage()),
                Is.Empty);

            Assert.That(
                nonFruitEnvironment.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RebuiltSourceAndFactory_PreserveSharedPerCardUsage()
        {
            var environment =
                new Environment();

            var firstSource =
                CreateSource(
                    CreateFactory(
                        environment),
                    environment.Side,
                    environment.Pet);

            var firstDamage =
                environment.CreateNormalAttackDamage();

            Discover(
                    firstSource,
                    environment.State,
                    firstDamage)[0]
                .Trigger.Resolve(
                    environment.State,
                    firstDamage);

            var rebuiltSource =
                CreateSource(
                    CreateFactory(
                        environment),
                    environment.Side,
                    environment.Pet);

            var secondDamage =
                environment.CreateNormalAttackDamage();

            Assert.That(
                rebuiltSource,
                Is.Not.SameAs(
                    firstSource));

            Assert.That(
                rebuiltSource.Handler,
                Is.Not.SameAs(
                    firstSource.Handler));

            Assert.That(
                Discover(
                    rebuiltSource,
                    environment.State,
                    secondDamage),
                Is.Empty);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FactorySource_EngineResolvesQueuedDamageExactlyOnce()
        {
            var environment =
                new Environment();

            var source =
                CreateSource(
                    CreateFactory(
                        environment),
                    environment.Side,
                    environment.Pet);

            environment.CreateNormalAttackDamage();

            var queue =
                new CombatEventQueue(
                    environment.Log);

            var engine =
                new CombatTriggerEngine(
                    environment.State,
                    queue,
                    new CombatTriggerSourceRegistry(
                        new ICombatTriggerSource[]
                        {
                            source
                        }));

            engine.Drain(
                20,
                20);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.SourceCard.CurrentHp,
                Is.EqualTo(11));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(99));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                engine.HasActiveBatch,
                Is.False);

            Assert.That(
                queue.PendingCount,
                Is.Zero);

            Assert.That(
                engine.Drain(
                    20,
                    20),
                Is.Zero);
        }

        private static FruitBatPetTriggerSourceFactory
            CreateFactory(
                Environment environment)
        {
            return new FruitBatPetTriggerSourceFactory(
                environment.Pet.DefinitionId,
                environment.UsageCommitter,
                environment.HpGainResolver,
                environment.Log);
        }

        private static FruitBatPetTriggerSource
            CreateSource(
                FruitBatPetTriggerSourceFactory factory,
                CombatSide side,
                CombatPetState pet)
        {
            var sources =
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        side,
                        pet));

            Assert.That(
                sources.Count,
                Is.EqualTo(1));

            Assert.That(
                sources[0],
                Is.TypeOf<FruitBatPetTriggerSource>());

            return (FruitBatPetTriggerSource)
                sources[0];
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                FruitBatPetTriggerSource source,
                CombatState state,
                CombatEvent sourceEvent)
        {
            return new List<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>(
                source.DiscoverTriggers(
                    state,
                    sourceEvent));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState SourceCard;
            public readonly CombatCardState TargetCard;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log;
            public readonly CombatEventMetadataFactory MetadataFactory;
            public readonly CombatStartedCombatEvent CombatStartedEvent;
            public readonly CombatPetCardTriggerUsageRegistry UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter UsageCommitter;
            public readonly CombatHpGainResolver HpGainResolver;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatCardSuit sourceSuit =
                    CombatCardSuit.Fruit)
            {
                Side = side;
                Row = row;

                SourceCard =
                    CreateCard(
                        1,
                        sourceSuit,
                        5,
                        10);

                TargetCard =
                    CreateCard(
                        2,
                        CombatCardSuit.Vegetable,
                        8,
                        100);

                var ownState =
                    CreateSideState(
                        side,
                        SourcePosition,
                        SourceCard);

                var opposingState =
                    CreateSideState(
                        OpposingSide,
                        TargetPosition,
                        TargetCard);

                var playerPets =
                    CreatePets(
                        CombatSide.Player);

                var enemyPets =
                    CreatePets(
                        CombatSide.Enemy);

                Pet = (side == CombatSide.Player
                    ? playerPets
                    : enemyPets)[
                        row == BoardRow.Front ? 0 : 1];

                State = new CombatState(
                    side == CombatSide.Player
                        ? ownState
                        : opposingState,
                    side == CombatSide.Player
                        ? opposingState
                        : ownState,
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            playerPets)),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            enemyPets)));

                Log = new CombatEventLog();

                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                CombatStartedEvent =
                    new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot());

                Log.Append(
                    CombatStartedEvent);

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                HpGainResolver =
                    new CombatHpGainResolver(
                        MetadataFactory,
                        Log);
            }

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            public BoardPosition SourcePosition =>
                Position(
                    Side,
                    Row,
                    1);

            public BoardPosition TargetPosition =>
                Position(
                    OpposingSide,
                    BoardRow.Front,
                    1);

            public DamageAppliedCombatEvent
                CreateNormalAttackDamage()
            {
                var playerCard =
                    Side == CombatSide.Player
                        ? SourceCard
                        : TargetCard;

                var playerPosition =
                    Side == CombatSide.Player
                        ? SourcePosition
                        : TargetPosition;

                var enemyCard =
                    Side == CombatSide.Enemy
                        ? SourceCard
                        : TargetCard;

                var enemyPosition =
                    Side == CombatSide.Enemy
                        ? SourcePosition
                        : TargetPosition;

                var exchangeEvent =
                    new NormalAttackExchangeCombatEvent(
                        MetadataFactory.CreateRoot(),
                        playerCard.InstanceId,
                        playerPosition,
                        playerCard.Attack,
                        enemyCard.InstanceId,
                        enemyPosition,
                        enemyCard.Attack);

                Log.Append(
                    exchangeEvent);

                var attackEvent =
                    new NormalAttackCombatEvent(
                        MetadataFactory.CreateChild(
                            exchangeEvent.Metadata),
                        SourceCard.InstanceId,
                        SourcePosition,
                        TargetCard.InstanceId,
                        TargetPosition,
                        SourceCard.Attack);

                Log.Append(
                    attackEvent);

                return new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        attackEvent,
                        SourcePosition,
                        TargetPosition,
                        1);
            }

            private static CombatSideState CreateSideState(
                CombatSide side,
                BoardPosition position,
                CombatCardState card)
            {
                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                position,
                                card.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[] { card }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatPetState[] CreatePets(
                CombatSide side)
            {
                var baseId =
                    side == CombatSide.Player
                        ? 1000
                        : 2000;

                return new[]
                {
                    new CombatPetState(
                        new DefinitionId(
                            "test.fruit_bat"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.fruit_bat"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                CombatCardSuit suit,
                int rank,
                int hp)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.fruit_bat_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(
                        rank),
                    suit,
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    2);
            }

            private static BoardPosition Position(
                CombatSide side,
                BoardRow row,
                int column)
            {
                return new BoardPosition(
                    side,
                    row,
                    new BoardColumn(
                        column));
            }
        }
    }
}
