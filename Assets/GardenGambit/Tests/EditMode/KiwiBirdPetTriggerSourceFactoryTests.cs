using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        KiwiBirdPetTriggerSourceFactoryTests
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
                factory.ArmorGainResolver,
                Is.SameAs(
                    environment.ArmorGainResolver));

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
                () => new KiwiBirdPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.UsageCommitter,
                    environment.ArmorGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.ArmorGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.UsageCommitter,
                    environment.ArmorGainResolver,
                    null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Factory_CreatesOneSourceAndResolvesStraightSurvivor(
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
                source.ArmorGainResolver,
                Is.SameAs(
                    environment.ArmorGainResolver));

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
                environment.SourceCard.Armor,
                Is.EqualTo(1));

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
                () => new KiwiBirdPetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.ArmorGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentException>(
                () => new KiwiBirdPetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.UsageCommitter,
                    environment.ArmorGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.ArmorGainResolver,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    null,
                    environment.Log));

            Assert.Throws<ArgumentNullException>(
                () => new KiwiBirdPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.UsageCommitter,
                    environment.ArmorGainResolver,
                    null));
        }

        [Test]
        public void Discovery_IgnoresWrongEventAndNonStraightSnapshot()
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

            var pairEnvironment =
                new Environment(
                    pokerHand: CombatPokerHand.Pair);

            var pairSource =
                CreateSource(
                    CreateFactory(
                        pairEnvironment),
                    pairEnvironment.Side,
                    pairEnvironment.Pet);

            Assert.That(
                Discover(
                    pairSource,
                    pairEnvironment.State,
                    pairEnvironment
                        .CreateNormalAttackDamage()),
                Is.Empty);

            Assert.That(
                pairEnvironment.UsageRegistry.Count,
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
                environment.SourceCard.Armor,
                Is.EqualTo(1));

            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FactorySource_EngineResumesOverflowWithoutRepeatingAttack()
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

            environment.SourceCard.ApplyArmorGain(
                int.MaxValue);

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

            Assert.Throws<OverflowException>(
                () => engine.Drain(
                    20,
                    20));

            Assert.That(
                engine.HasActiveBatch,
                Is.True);

            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(99));

            environment.SourceCard.RemoveArmor(
                int.MaxValue);

            engine.Drain(
                20,
                20);

            Assert.That(
                environment.SourceCard.Armor,
                Is.EqualTo(1));

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

        private static KiwiBirdPetTriggerSourceFactory
            CreateFactory(
                Environment environment)
        {
            return new KiwiBirdPetTriggerSourceFactory(
                environment.Pet.DefinitionId,
                environment.UsageCommitter,
                environment.ArmorGainResolver,
                environment.Log);
        }

        private static KiwiBirdPetTriggerSource
            CreateSource(
                KiwiBirdPetTriggerSourceFactory factory,
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
                Is.TypeOf<KiwiBirdPetTriggerSource>());

            return (KiwiBirdPetTriggerSource)
                sources[0];
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                KiwiBirdPetTriggerSource source,
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
            public readonly CombatArmorGainResolver ArmorGainResolver;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                CombatPokerHand pokerHand =
                    CombatPokerHand.Straight)
            {
                Side = side;
                Row = row;

                SourceCard =
                    CreateCard(
                        1,
                        5,
                        10);

                TargetCard =
                    CreateCard(
                        2,
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

                var snapshot =
                    new CombatBattleStartSnapshotResolver()
                        .Resolve(
                            State,
                            Hand(
                                CombatSide.Player,
                                BoardRow.Front,
                                pokerHand),
                            Hand(
                                CombatSide.Player,
                                BoardRow.Back,
                                pokerHand),
                            Hand(
                                CombatSide.Enemy,
                                BoardRow.Front,
                                pokerHand),
                            Hand(
                                CombatSide.Enemy,
                                BoardRow.Back,
                                pokerHand));

                CombatStartedEvent =
                    new CombatStartedCombatEvent(
                        MetadataFactory.CreateRoot(),
                        snapshot);

                Log.Append(
                    CombatStartedEvent);

                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();

                UsageCommitter =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);

                ArmorGainResolver =
                    new CombatArmorGainResolver(
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

            private CombatPokerHand Hand(
                CombatSide side,
                BoardRow row,
                CombatPokerHand pokerHand)
            {
                return side == Side && row == Row
                    ? pokerHand
                    : CombatPokerHand.FlushFive;
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
                            "test.kiwi_bird"),
                        new InstanceId(
                            baseId + 1)),
                    new CombatPetState(
                        new DefinitionId(
                            "test.kiwi_bird"),
                        new InstanceId(
                            baseId + 2))
                };
            }

            private static CombatCardState CreateCard(
                long instanceId,
                int rank,
                int hp)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.kiwi_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(
                        rank),
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
