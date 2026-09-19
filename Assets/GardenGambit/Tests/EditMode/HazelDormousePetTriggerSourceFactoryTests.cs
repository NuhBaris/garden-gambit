using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        HazelDormousePetTriggerSourceFactoryTests
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
                factory.TargetDamageReductionRegistry,
                Is.SameAs(
                    environment.ReductionRegistry));

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
                () => new
                    HazelDormousePetTriggerSourceFactory(
                        default(DefinitionId),
                        environment.UsageCommitter,
                        environment.ReductionRegistry));

            Assert.Throws<ArgumentNullException>(
                () => new
                    HazelDormousePetTriggerSourceFactory(
                        environment.Pet.DefinitionId,
                        null,
                        environment.ReductionRegistry));

            Assert.Throws<ArgumentNullException>(
                () => new
                    HazelDormousePetTriggerSourceFactory(
                        environment.Pet.DefinitionId,
                        environment.UsageCommitter,
                        null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Factory_CreatesSourceAndReducesNutDamage(
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
                source.TargetDamageReductionRegistry,
                Is.SameAs(
                    environment.ReductionRegistry));

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(
                    side));

            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            var attackEvent =
                environment.CreateAttack();

            var candidates =
                Discover(
                    source,
                    environment.State,
                    attackEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(
                    source.Handler));

            candidates[0].Trigger.Resolve(
                environment.State,
                attackEvent);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.ReductionResolver.ResolveDamage(
                    attackEvent,
                    incomingDamage: 5),
                Is.EqualTo(4));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                Discover(
                    source,
                    environment.State,
                    attackEvent),
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
                () => new HazelDormousePetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.ReductionRegistry));

            Assert.Throws<ArgumentException>(
                () => new HazelDormousePetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.UsageCommitter,
                    environment.ReductionRegistry));

            Assert.Throws<ArgumentNullException>(
                () => new HazelDormousePetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.ReductionRegistry));

            Assert.Throws<ArgumentNullException>(
                () => new HazelDormousePetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null));
        }

        [Test]
        public void Discovery_IgnoresWrongEventAndNonNutCard()
        {
            var nutEnvironment =
                new Environment();

            var nutSource =
                CreateSource(
                    CreateFactory(
                        nutEnvironment),
                    nutEnvironment.Side,
                    nutEnvironment.Pet);

            Assert.That(
                Discover(
                    nutSource,
                    nutEnvironment.State,
                    new CombatStartedCombatEvent(
                        CreateRootMetadata())),
                Is.Empty);

            var fruitEnvironment =
                new Environment(
                    targetSuit:
                        CombatCardSuit.Fruit);

            var fruitSource =
                CreateSource(
                    CreateFactory(
                        fruitEnvironment),
                    fruitEnvironment.Side,
                    fruitEnvironment.Pet);

            Assert.That(
                Discover(
                    fruitSource,
                    fruitEnvironment.State,
                    fruitEnvironment.CreateAttack()),
                Is.Empty);

            Assert.That(
                fruitEnvironment.UsageRegistry.Count,
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

            var firstAttack =
                environment.CreateAttack();

            Discover(
                    firstSource,
                    environment.State,
                    firstAttack)[0]
                .Trigger.Resolve(
                    environment.State,
                    firstAttack);

            Assert.That(
                environment.ReductionResolver.ResolveDamage(
                    firstAttack,
                    incomingDamage: 5),
                Is.EqualTo(4));

            var rebuiltSource =
                CreateSource(
                    CreateFactory(
                        environment),
                    environment.Side,
                    environment.Pet);

            var secondAttack =
                environment.CreateAttack();

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
                    secondAttack),
                Is.Empty);

            Assert.That(
                environment.ReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void RemovedCard_IsNotDiscoveredThroughSource()
        {
            var environment =
                new Environment();

            var source =
                CreateSource(
                    CreateFactory(
                        environment),
                    environment.Side,
                    environment.Pet);

            var attackEvent =
                environment.CreateAttack();

            environment.State.GetSide(
                    environment.Side)
                .RemoveCardFromCombat(
                    environment.TargetPosition);

            Assert.That(
                Discover(
                    source,
                    environment.State,
                    attackEvent),
                Is.Empty);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);
        }

        private static HazelDormousePetTriggerSourceFactory
            CreateFactory(
                Environment environment)
        {
            return new HazelDormousePetTriggerSourceFactory(
                environment.Pet.DefinitionId,
                environment.UsageCommitter,
                environment.ReductionRegistry);
        }

        private static HazelDormousePetTriggerSource
            CreateSource(
                HazelDormousePetTriggerSourceFactory factory,
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
                Is.TypeOf<
                    HazelDormousePetTriggerSource>());

            return (HazelDormousePetTriggerSource)
                sources[0];
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                HazelDormousePetTriggerSource source,
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
            private long _nextEventId = 2;

            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState TargetCard;
            public readonly CombatCardState AttackerCard;
            public readonly CombatPetState Pet;
            public readonly CombatPetCardTriggerUsageRegistry
                UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter
                UsageCommitter;
            public readonly
                CombatNormalAttackTargetDamageReductionRegistry
                ReductionRegistry;
            public readonly
                CombatNormalAttackTargetDamageReductionResolver
                ReductionResolver;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatCardSuit targetSuit =
                    CombatCardSuit.Nut)
            {
                Side = side;
                Row = row;

                TargetCard =
                    CreateCard(
                        1,
                        targetSuit);

                AttackerCard =
                    CreateCard(
                        2,
                        CombatCardSuit.Fruit);

                var ownState =
                    CreateSideState(
                        side,
                        TargetPosition,
                        TargetCard);

                var opposingState =
                    CreateSideState(
                        OpposingSide,
                        AttackerPosition,
                        AttackerCard);

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

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                ReductionRegistry =
                    new
                        CombatNormalAttackTargetDamageReductionRegistry();

                ReductionResolver =
                    new
                        CombatNormalAttackTargetDamageReductionResolver(
                            ReductionRegistry,
                            UsageCommitter);
            }

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            public BoardPosition TargetPosition =>
                Position(
                    Side,
                    Row,
                    1);

            public BoardPosition AttackerPosition =>
                Position(
                    OpposingSide,
                    BoardRow.Front,
                    1);

            public NormalAttackCombatEvent CreateAttack()
            {
                var eventId =
                    _nextEventId++;

                return new NormalAttackCombatEvent(
                    CreateChildMetadata(
                        eventId),
                    AttackerCard.InstanceId,
                    AttackerPosition,
                    TargetCard.InstanceId,
                    TargetPosition,
                    baseDamage: 5);
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
                            "test.hazel_dormouse"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.hazel_dormouse"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                CombatCardSuit suit)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.hazel_dormouse_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(5),
                    suit,
                    CombatCardSeason.Spring,
                    10,
                    10,
                    0,
                    5);
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

        private static CombatEventMetadata CreateRootMetadata()
        {
            return new CombatEventMetadata(
                new CombatEventId(100),
                new CombatSequenceNumber(100),
                null,
                new CombatEventId(100));
        }

        private static CombatEventMetadata CreateChildMetadata(
            long eventId)
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(
                    eventId),
                new CombatSequenceNumber(
                    eventId),
                triggerRootId,
                triggerRootId);
        }
    }
}
