using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        IguanaPetTriggerSourceFactoryTests
    {
        [Test]
        public void FactoryPreservesRegistrationAndDependencies()
        {
            var environment = new Environment();
            var factory = environment.CreateFactory();

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
                Is.SameAs(factory));
        }

        [Test]
        public void FactoryConstructorRejectsInvalidInputs()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(
                () => new IguanaPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    environment.Log));
            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.HpGainResolver,
                    environment.Log));
            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));
            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FactoryCreatesOneSourceAndResolvesNormalKill(
            CombatSide side,
            BoardRow row)
        {
            var environment = new Environment(
                side,
                row);
            var source = environment.CreateSource();

            Assert.That(source.Side, Is.EqualTo(side));
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
                Is.EqualTo(side));
            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            var death = environment.CreateNormalKill();
            var candidates = Discover(
                source,
                environment.State,
                death);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(source.Handler));

            candidates[0].Trigger.Resolve(
                environment.State,
                death);

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
            Assert.That(
                environment.CountHpGainEvents(),
                Is.EqualTo(1));
            Assert.That(
                Discover(
                    source,
                    environment.State,
                    death),
                Is.Empty);
        }

        [Test]
        public void CreateSourcesRejectsInvalidInputs()
        {
            var environment = new Environment();
            var factory = environment.CreateFactory();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        (CombatSide)99,
                        environment.Pet)));
            Assert.Throws<ArgumentNullException>(
                () => new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        environment.Side,
                        null)));

            var wrongPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                environment.Pet.InstanceId);

            Assert.Throws<ArgumentException>(
                () => new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        environment.Side,
                        wrongPet)));
        }

        [Test]
        public void SourceConstructorRejectsInvalidInputs()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new IguanaPetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    environment.Log));
            Assert.Throws<ArgumentException>(
                () => new IguanaPetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    environment.Log));
            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.HpGainResolver,
                    environment.Log));
            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));
            Assert.Throws<ArgumentNullException>(
                () => new IguanaPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.HpGainResolver,
                    null));
        }

        [Test]
        public void DiscoveryIgnoresWrongEventAndIneligibleRank()
        {
            var environment = new Environment();
            var source = environment.CreateSource();

            Assert.That(
                Discover(
                    source,
                    environment.State,
                    environment.RootEvent),
                Is.Empty);

            var ineligible = new Environment(
                sourceRank: 10);
            var ineligibleSource =
                ineligible.CreateSource();

            Assert.That(
                Discover(
                    ineligibleSource,
                    ineligible.State,
                    ineligible.CreateNormalKill()),
                Is.Empty);
            Assert.That(
                ineligible.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RebuiltSourcePreservesSharedPerCardUsage()
        {
            var environment = new Environment();
            var firstSource = environment.CreateSource();
            var death = environment.CreateNormalKill();

            Discover(
                    firstSource,
                    environment.State,
                    death)[0]
                .Trigger.Resolve(
                    environment.State,
                    death);

            var rebuiltSource = environment.CreateSource();

            Assert.That(
                rebuiltSource,
                Is.Not.SameAs(firstSource));
            Assert.That(
                rebuiltSource.Handler,
                Is.Not.SameAs(firstSource.Handler));
            Assert.That(
                Discover(
                    rebuiltSource,
                    environment.State,
                    death),
                Is.Empty);
            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void EngineResolvesQueuedDeathExactlyOnce()
        {
            var environment = new Environment();
            var source = environment.CreateSource();
            environment.CreateNormalKill();

            var queue = new CombatEventQueue(
                environment.Log);
            var engine = new CombatTriggerEngine(
                environment.State,
                queue,
                new CombatTriggerSourceRegistry(
                    new ICombatTriggerSource[]
                    {
                        source
                    }));

            engine.Drain(20, 20);

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
                environment.CountHpGainEvents(),
                Is.EqualTo(1));
            Assert.That(engine.HasActiveBatch, Is.False);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                IguanaPetTriggerSource source,
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
            public readonly CombatStartedCombatEvent RootEvent;
            public readonly CombatPetCardTriggerUsageRegistry UsageRegistry;
            public readonly CombatPetCardTriggerUsageCommitter UsageCommitter;
            public readonly CombatHpGainResolver HpGainResolver;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int sourceRank = 12)
            {
                Side = side;
                Row = row;
                SourceCard = CreateCard(1, sourceRank, 10, 5);
                TargetCard = CreateCard(2, 8, 5, 1);

                var ownState = CreateSideState(
                    side,
                    SourcePosition,
                    SourceCard);
                var opposingState = CreateSideState(
                    OpposingSide,
                    TargetPosition,
                    TargetCard);
                var playerPets = CreatePets(CombatSide.Player);
                var enemyPets = CreatePets(CombatSide.Enemy);

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
                        new CombatPetRegistry(playerPets)),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(enemyPets)));

                Log = new CombatEventLog();
                MetadataFactory = new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
                RootEvent = new CombatStartedCombatEvent(
                    MetadataFactory.CreateRoot());
                Log.Append(RootEvent);
                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();
                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);
                HpGainResolver = new CombatHpGainResolver(
                    MetadataFactory,
                    Log);
            }

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

            public BoardPosition SourcePosition =>
                Position(Side, Row, 1);

            public BoardPosition TargetPosition =>
                Position(OpposingSide, BoardRow.Front, 1);

            public IguanaPetTriggerSourceFactory
                CreateFactory()
            {
                return new IguanaPetTriggerSourceFactory(
                    Pet.DefinitionId,
                    UsageCommitter,
                    HpGainResolver,
                    Log);
            }

            public IguanaPetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    CreateFactory().CreateSources(
                        Side,
                        Pet));

                Assert.That(sources.Count, Is.EqualTo(1));
                Assert.That(
                    sources[0],
                    Is.TypeOf<IguanaPetTriggerSource>());

                return (IguanaPetTriggerSource)sources[0];
            }

            public DeathCombatEvent CreateNormalKill()
            {
                var attack = new NormalAttackCombatEvent(
                    MetadataFactory.CreateChild(
                        RootEvent.Metadata),
                    SourceCard.InstanceId,
                    SourcePosition,
                    TargetCard.InstanceId,
                    TargetPosition,
                    SourceCard.Attack);
                Log.Append(attack);

                var damage = new CombatDamageResolver(
                        MetadataFactory,
                        Log)
                    .ApplyResolvedCardDamage(
                        State,
                        attack,
                        SourcePosition,
                        TargetPosition,
                        TargetCard.CurrentHp);

                return new CombatDeathEventResolver(
                        MetadataFactory,
                        Log)
                    .AppendFromDamage(damage);
            }

            public int CountHpGainEvents()
            {
                var count = 0;

                foreach (var combatEvent in Log.Events)
                {
                    if (combatEvent is HpGainCombatEvent)
                    {
                        count++;
                    }
                }

                return count;
            }

            private static CombatSideState CreateSideState(
                CombatSide side,
                BoardPosition occupiedPosition,
                CombatCardState card)
            {
                var otherRow = occupiedPosition.Row ==
                    BoardRow.Front
                        ? BoardRow.Back
                        : BoardRow.Front;

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                occupiedPosition,
                                card.InstanceId),
                            new CombatSlotState(
                                new SlotId(2),
                                Position(side, otherRow, 1))
                        }),
                    new CombatCardRegistry(
                        new[] { card }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatPetState[] CreatePets(
                CombatSide side)
            {
                var baseId = side == CombatSide.Player
                    ? 1000
                    : 2000;

                return new[]
                {
                    new CombatPetState(
                        new DefinitionId("test.iguana"),
                        new InstanceId(baseId + 1)),
                    new CombatPetState(
                        new DefinitionId("test.iguana"),
                        new InstanceId(baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                int rank,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId("test.iguana_card"),
                    new InstanceId(instanceId),
                    new CardRank(rank),
                    CombatCardSuit.Unspecified,
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    attack);
            }

            private static BoardPosition Position(
                CombatSide side,
                BoardRow row,
                int column)
            {
                return new BoardPosition(
                    side,
                    row,
                    new BoardColumn(column));
            }
        }
    }
}
