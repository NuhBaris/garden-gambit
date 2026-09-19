using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        OtterPetTriggerSourceFactoryTests
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
                factory.AttackGainResolver,
                Is.SameAs(
                    environment.AttackGainResolver));

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
                () => new OtterPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.UsageCommitter,
                    environment.AttackGainResolver));

            Assert.Throws<ArgumentNullException>(
                () => new OtterPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.AttackGainResolver));

            Assert.Throws<ArgumentNullException>(
                () => new OtterPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Factory_CreatesSourceAndResolvesDrinkAttack(
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
                source.AttackGainResolver,
                Is.SameAs(
                    environment.AttackGainResolver));

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
                environment.SourceCard.Attack,
                Is.EqualTo(6));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(5));

            Assert.That(
                environment.AttackGains().Count,
                Is.EqualTo(1));

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
                () => new OtterPetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.AttackGainResolver));

            Assert.Throws<ArgumentException>(
                () => new OtterPetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.UsageCommitter,
                    environment.AttackGainResolver));

            Assert.Throws<ArgumentNullException>(
                () => new OtterPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.AttackGainResolver));

            Assert.Throws<ArgumentNullException>(
                () => new OtterPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null));
        }

        [Test]
        public void Discovery_IgnoresWrongEventAndNonDrinkCard()
        {
            var drinkEnvironment =
                new Environment();

            var drinkSource =
                CreateSource(
                    CreateFactory(
                        drinkEnvironment),
                    drinkEnvironment.Side,
                    drinkEnvironment.Pet);

            Assert.That(
                Discover(
                    drinkSource,
                    drinkEnvironment.State,
                    new CombatStartedCombatEvent(
                        CreateRootMetadata())),
                Is.Empty);

            var fruitEnvironment =
                new Environment(
                    sourceSuit:
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

            Assert.That(
                fruitEnvironment.AttackGains(),
                Is.Empty);
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
                environment.SourceCard.Attack,
                Is.EqualTo(6));

            Assert.That(
                environment.AttackGains().Count,
                Is.EqualTo(1));

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
                    environment.SourcePosition);

            Assert.That(
                Discover(
                    source,
                    environment.State,
                    attackEvent),
                Is.Empty);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.AttackGains(),
                Is.Empty);
        }

        private static OtterPetTriggerSourceFactory
            CreateFactory(
                Environment environment)
        {
            return new OtterPetTriggerSourceFactory(
                environment.Pet.DefinitionId,
                environment.UsageCommitter,
                environment.AttackGainResolver);
        }

        private static OtterPetTriggerSource
            CreateSource(
                OtterPetTriggerSourceFactory factory,
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
                Is.TypeOf<OtterPetTriggerSource>());

            return (OtterPetTriggerSource)
                sources[0];
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                OtterPetTriggerSource source,
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
            public readonly CombatPetCardTriggerUsageRegistry
                UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter
                UsageCommitter;
            public readonly CombatEventMetadataFactory
                MetadataFactory;
            public readonly CombatEventLog Log;
            public readonly CombatAttackGainResolver
                AttackGainResolver;

            private readonly CombatStartedCombatEvent
                _rootEvent;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatCardSuit sourceSuit =
                    CombatCardSuit.Drink)
            {
                Side = side;
                Row = row;

                SourceCard =
                    CreateCard(
                        1,
                        sourceSuit);

                TargetCard =
                    CreateCard(
                        2,
                        CombatCardSuit.Fruit);

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

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                Log =
                    new CombatEventLog();

                _rootEvent =
                    new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot());

                Log.Append(
                    _rootEvent);

                AttackGainResolver =
                    new CombatAttackGainResolver(
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

            public NormalAttackCombatEvent CreateAttack()
            {
                var attackEvent =
                    new NormalAttackCombatEvent(
                        MetadataFactory.CreateChild(
                            _rootEvent.Metadata),
                        SourceCard.InstanceId,
                        SourcePosition,
                        TargetCard.InstanceId,
                        TargetPosition,
                        SourceCard.Attack);

                Log.Append(
                    attackEvent);

                return attackEvent;
            }

            public List<AttackGainCombatEvent> AttackGains()
            {
                var gains =
                    new List<AttackGainCombatEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    var gain =
                        combatEvent as AttackGainCombatEvent;

                    if (gain != null)
                    {
                        gains.Add(
                            gain);
                    }
                }

                return gains;
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
                            "test.otter"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.otter"),
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
                        "test.otter_card"),
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
    }
}
