using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        JackalopePetTriggerSourceFactoryTests
    {
        [TestCase(0)]
        [TestCase(1)]
        public void SourceConstructor_RejectsMissingDependency(
            int missingDependency)
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new JackalopePetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    missingDependency == 0
                        ? null
                        : environment.Usage,
                    missingDependency == 1
                        ? null
                        : environment.Modifiers));
        }

        [Test]
        public void SourceConstructor_RejectsInvalidIdentity()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new JackalopePetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Modifiers));

            Assert.Throws<ArgumentException>(() =>
                new JackalopePetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.Usage,
                    environment.Modifiers));
        }

        [Test]
        public void Source_ExposesExactIdentityAndDependencies()
        {
            var environment = new Environment();
            var source = environment.Source();

            Assert.That(source.Side, Is.EqualTo(environment.Side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
            Assert.That(
                source.Handler,
                Is.TypeOf<
                    JackalopePetBattleEndTriggerHandler>());
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                source.FinalRankModifierRegistry,
                Is.SameAs(environment.Modifiers));
            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(environment.Side));
            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
        }

        [TestCase(CombatSide.Player, BoardRow.Front, 3)]
        [TestCase(CombatSide.Player, BoardRow.Back, 2)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 3)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 2)]
        public void Discover_EligibleBattleEndReturnsExactHandler(
            CombatSide side,
            BoardRow row,
            int expectedBonus)
        {
            var environment = new Environment(side, row);
            var source = environment.Source();
            var candidates = Discover(
                source,
                environment.State,
                environment.BattleEnd);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(source.Handler));
            Assert.That(
                candidates[0].OrderKey,
                Is.EqualTo(
                    new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Pet,
                        side,
                        row == BoardRow.Front
                            ? 0
                            : 1,
                        0)));
            Assert.That(environment.Modifiers.Count, Is.Zero);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);

            candidates[0].Trigger.Resolve(
                environment.State,
                environment.BattleEnd);

            var target = environment.Target(row);

            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    target.InstanceId),
                Is.EqualTo(expectedBonus));
            Assert.That(
                environment.Usage.HasTriggered(
                    environment.Pet.InstanceId,
                    target.InstanceId),
                Is.True);
            Assert.That(
                Discover(
                    source,
                    environment.State,
                    environment.BattleEnd),
                Is.Empty);
        }

        [Test]
        public void BattleEndWithoutSnapshot_ReturnsNoCandidate()
        {
            var environment = new Environment();

            Assert.That(
                Discover(
                    environment.Source(),
                    environment.State,
                    environment.LegacyBattleEnd),
                Is.Empty);
            Assert.That(environment.Modifiers.Count, Is.Zero);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void UnrelatedEvent_ReturnsNoCandidate()
        {
            var environment = new Environment();

            Assert.That(
                Discover(
                    environment.Source(),
                    environment.State,
                    environment.Root),
                Is.Empty);
            Assert.That(environment.Modifiers.Count, Is.Zero);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RecreatedSource_SharesCompletedCardUsage()
        {
            var environment = new Environment();
            var first = environment.Source();

            first.Handler.Resolve(
                environment.State,
                environment.BattleEnd);

            var recreated = environment.Source();

            Assert.That(recreated, Is.Not.SameAs(first));
            Assert.That(
                recreated.Handler,
                Is.Not.SameAs(first.Handler));
            Assert.That(
                Discover(
                    recreated,
                    environment.State,
                    environment.BattleEnd),
                Is.Empty);

            recreated.Handler.Resolve(
                environment.State,
                environment.BattleEnd);

            Assert.That(
                environment.Modifiers.GetTotalModifier(
                    environment.Target(BoardRow.Front)
                        .InstanceId),
                Is.EqualTo(3));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FactoryConstructor_RejectsInvalidDefinitionId()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new JackalopePetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.Usage,
                    environment.Modifiers));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void FactoryConstructor_RejectsMissingDependency(
            int missingDependency)
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new JackalopePetTriggerSourceFactory(
                    environment.DefinitionId,
                    missingDependency == 0
                        ? null
                        : environment.Usage,
                    missingDependency == 1
                        ? null
                        : environment.Modifiers));
        }

        [Test]
        public void Factory_RejectsInvalidCreationInputs()
        {
            var environment = new Environment();
            var factory = environment.Factory();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                factory.CreateSources(
                    (CombatSide)99,
                    environment.Pet));
            Assert.Throws<ArgumentNullException>(() =>
                factory.CreateSources(
                    environment.Side,
                    null));

            var otherPet = new CombatPetState(
                new DefinitionId("test.pet.other"),
                new InstanceId(9001));

            Assert.Throws<ArgumentException>(() =>
                factory.CreateSources(
                    environment.Side,
                    otherPet));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneConfiguredSource(
            CombatSide side)
        {
            var environment = new Environment(side);
            var factory = environment.Factory();
            var sources =
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        side,
                        environment.Pet));

            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(environment.DefinitionId));
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                factory.FinalRankModifierRegistry,
                Is.SameAs(environment.Modifiers));

            var source =
                sources[0] as JackalopePetTriggerSource;

            Assert.That(source, Is.Not.Null);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                source.FinalRankModifierRegistry,
                Is.SameAs(environment.Modifiers));
        }

        [Test]
        public void Factory_RepeatedCallsCreateDistinctHandlers()
        {
            var environment = new Environment();
            var factory = environment.Factory();

            var first =
                (JackalopePetTriggerSource)
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        environment.Side,
                        environment.Pet))[0];
            var second =
                (JackalopePetTriggerSource)
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        environment.Side,
                        environment.Pet))[0];

            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(
                first.Handler,
                Is.Not.SameAs(second.Handler));
            Assert.That(
                first.UsageCommitter,
                Is.SameAs(second.UsageCommitter));
            Assert.That(
                first.FinalRankModifierRegistry,
                Is.SameAs(
                    second.FinalRankModifierRegistry));
        }

        private static List<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            Discover(
                ICombatTriggerSource source,
                CombatState state,
                CombatEvent sourceEvent)
        {
            return new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        state,
                        sourceEvent));
        }

        private sealed class Environment
        {
            public readonly DefinitionId DefinitionId =
                new DefinitionId("test.pet.jackalope");
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatPetCardTriggerUsageCommitter
                Usage =
                    new CombatPetCardTriggerUsageCommitter(
                        new CombatPetCardTriggerUsageRegistry());
            public readonly CombatFinalRankModifierRegistry
                Modifiers =
                    new CombatFinalRankModifierRegistry();
            public readonly CombatStartedCombatEvent Root;
            public readonly BattleEndStartedCombatEvent BattleEnd;
            public readonly BattleEndStartedCombatEvent
                LegacyBattleEnd;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                Side = side;
                Row = row;

                var player = CreateSide(CombatSide.Player);
                var enemy = CreateSide(CombatSide.Enemy);
                var playerPets = CreatePets(
                    CombatSide.Player,
                    side == CombatSide.Player
                        ? DefinitionId
                        : new DefinitionId("test.pet.other"));
                var enemyPets = CreatePets(
                    CombatSide.Enemy,
                    side == CombatSide.Enemy
                        ? DefinitionId
                        : new DefinitionId("test.pet.other"));

                State = new CombatState(
                    player,
                    enemy,
                    playerPets,
                    enemyPets);

                Pet = State.GetPets(side).GetPetAt(
                    row == BoardRow.Front
                        ? 0
                        : 1);

                var metadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                Root =
                    new CombatStartedCombatEvent(
                        metadataFactory.CreateRoot());

                var snapshot =
                    new CombatBattleStartSnapshotResolver().Resolve(
                        State,
                        side == CombatSide.Player
                            ? CombatPokerHand.HighCard
                            : CombatPokerHand.FlushFive,
                        side == CombatSide.Player
                            ? CombatPokerHand.Pair
                            : CombatPokerHand.FlushFive,
                        side == CombatSide.Enemy
                            ? CombatPokerHand.HighCard
                            : CombatPokerHand.FlushFive,
                        side == CombatSide.Enemy
                            ? CombatPokerHand.Pair
                            : CombatPokerHand.FlushFive);

                var battleEndMetadata =
                    metadataFactory.CreateChild(
                        Root.Metadata);

                BattleEnd =
                    new BattleEndStartedCombatEvent(
                        battleEndMetadata,
                        snapshot);
                LegacyBattleEnd =
                    new BattleEndStartedCombatEvent(
                        battleEndMetadata);
            }

            public JackalopePetTriggerSource Source()
            {
                return new JackalopePetTriggerSource(
                    Side,
                    Pet.InstanceId,
                    Usage,
                    Modifiers);
            }

            public JackalopePetTriggerSourceFactory Factory()
            {
                return new JackalopePetTriggerSourceFactory(
                    DefinitionId,
                    Usage,
                    Modifiers);
            }

            public CombatCardState Target(
                BoardRow row)
            {
                return State.GetSide(Side).GetCardAt(
                    Position(
                        Side,
                        row,
                        1));
            }

            private static CombatSidePetState CreatePets(
                CombatSide side,
                DefinitionId definitionId)
            {
                var baseId =
                    side == CombatSide.Player
                        ? 1000
                        : 2000;

                return new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[]
                        {
                            new CombatPetState(
                                definitionId,
                                new InstanceId(baseId + 1)),
                            new CombatPetState(
                                definitionId,
                                new InstanceId(baseId + 2))
                        }));
            }

            private static CombatSideState CreateSide(
                CombatSide side)
            {
                var cards =
                    new List<CombatCardState>();
                var slots =
                    new List<CombatSlotState>();

                foreach (var row in
                         new[]
                         {
                             BoardRow.Front,
                             BoardRow.Back
                         })
                {
                    var instanceId =
                        (side == CombatSide.Player
                            ? 0
                            : 100) +
                        (row == BoardRow.Front
                            ? 1
                            : 11);

                    var card =
                        new CombatCardState(
                            new DefinitionId(
                                "test.jackalope.source.card"),
                            new InstanceId(instanceId),
                            new CardRank(2),
                            5,
                            5,
                            0,
                            2);

                    cards.Add(card);
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(instanceId),
                            Position(
                                side,
                                row,
                                1),
                            card.InstanceId));
                }

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(
                        cards),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));
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
