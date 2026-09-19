using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        ToucanPetTriggerSourceFactoryTests
    {
        [Test]
        public void ConstructorsValidateAndPreserveDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new ToucanPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.Usage,
                    environment.Hp));
            Assert.Throws<ArgumentNullException>(() =>
                new ToucanPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.Hp));
            Assert.Throws<ArgumentNullException>(() =>
                new ToucanPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.Usage,
                    null));
            Assert.Throws<ArgumentNullException>(() =>
                new ToucanPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.Hp));
            Assert.Throws<ArgumentNullException>(() =>
                new ToucanPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    null));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ToucanPetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Hp));
            Assert.Throws<ArgumentException>(() =>
                new ToucanPetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.Usage,
                    environment.Hp));

            Assert.That(
                environment.Factory.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                environment.Factory.HpGainResolver,
                Is.SameAs(environment.Hp));
            Assert.That(
                environment.Factory.PetDefinitionId,
                Is.EqualTo(environment.Pet.DefinitionId));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FactoryCreatesOneSourceWithCorrectOwner(
            CombatSide side)
        {
            var environment = new Environment(side);
            var sources = new List<ICombatTriggerSource>(
                environment.Factory.CreateSources(
                    side,
                    environment.Pet));

            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(
                sources[0],
                Is.TypeOf<ToucanPetTriggerSource>());

            var source = (ToucanPetTriggerSource)sources[0];

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
            Assert.That(
                source.Handler,
                Is.TypeOf<
                    ToucanPetBattleStartTriggerHandler>());
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                source.HpGainResolver,
                Is.SameAs(environment.Hp));
            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(side));
            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
        }

        [Test]
        public void FactoryRejectsInvalidOwner()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                environment.Factory.CreateSources(
                    (CombatSide)99,
                    environment.Pet));
            Assert.Throws<ArgumentNullException>(() =>
                environment.Factory.CreateSources(
                    environment.Side,
                    null));

            var otherPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                new InstanceId(2000));

            Assert.Throws<ArgumentException>(() =>
                environment.Factory.CreateSources(
                    environment.Side,
                    otherPet));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void SourceDiscoveryAndEngineResolveCorrectRow(
            CombatSide side,
            BoardRow row)
        {
            var environment = new Environment(side, row);
            var source = environment.CreateSource();
            var candidates = new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        environment.State,
                        environment.Source));

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

            var queue = new CombatEventQueue(environment.Log);
            var registry = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[] { source });
            var engine = new CombatTriggerEngine(
                environment.State,
                queue,
                registry);

            engine.Drain(20, 20);

            Assert.That(
                environment.Card(row, 1).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.Card(row, 2).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.Card(row, 3).HpCapacity,
                Is.EqualTo(10));
            Assert.That(
                environment.Card(
                    environment.OtherRow,
                    1).HpCapacity,
                Is.EqualTo(10));

            var gains = environment.Gains();

            Assert.That(gains.Count, Is.EqualTo(2));

            for (var index = 0;
                 index < gains.Count;
                 index++)
            {
                Assert.That(
                    gains[index].TargetInstanceId,
                    Is.EqualTo(
                        environment.Card(
                            row,
                            index + 1).InstanceId));
                Assert.That(
                    gains[index].SourceInstanceId,
                    Is.EqualTo(
                        environment.Pet.InstanceId));
                Assert.That(
                    gains[index].ActualGainedAmount,
                    Is.EqualTo(1));
            }

            Assert.That(
                environment.Usage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [Test]
        public void SourceValidatesSnapshotAndDiscoveryInputs()
        {
            var environment = new Environment();
            var source = environment.CreateSource();
            var withoutSnapshot =
                new BattleStartStageStartedCombatEvent(
                    environment.Metadata.CreateChild(
                        environment.Root.Metadata),
                    CombatBattleStartStage.Pet);

            environment.Log.Append(withoutSnapshot);

            Assert.That(
                source.DiscoverTriggers(
                    environment.State,
                    withoutSnapshot),
                Is.Empty);
            Assert.Throws<ArgumentNullException>(() =>
                source.DiscoverTriggers(
                    null,
                    environment.Source));
            Assert.Throws<ArgumentNullException>(() =>
                source.DiscoverTriggers(
                    environment.State,
                    null));
            Assert.That(
                source.DiscoverTriggers(
                    environment.State,
                    environment.Root),
                Is.Empty);
        }

        [Test]
        public void TwoSuitSnapshotProducesNoCandidate()
        {
            var environment = new Environment(
                distinctSuitCount: 2);
            var source = environment.CreateSource();

            Assert.That(
                source.DiscoverTriggers(
                    environment.State,
                    environment.Source),
                Is.Empty);
            Assert.That(environment.Gains(), Is.Empty);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void SourceIgnoresOtherStartStages(
            CombatBattleStartStage stage)
        {
            var environment = new Environment();
            var source = environment.CreateSource();
            var otherStage =
                new BattleStartStageStartedCombatEvent(
                    environment.Metadata.CreateChild(
                        environment.Root.Metadata),
                    stage,
                    environment.Source.BattleStartSnapshot);

            environment.Log.Append(otherStage);

            Assert.That(
                source.DiscoverTriggers(
                    environment.State,
                    otherStage),
                Is.Empty);
            Assert.That(environment.Gains(), Is.Empty);
        }

        [Test]
        public void RecreatedSourcesShareUsageAndDoNotRepeat()
        {
            var environment = new Environment();
            var first = environment.CreateSource();
            var rebuilt = environment.CreateSource();

            Assert.That(rebuilt, Is.Not.SameAs(first));
            Assert.That(
                rebuilt.Handler,
                Is.Not.SameAs(first.Handler));
            Assert.That(
                rebuilt.UsageCommitter,
                Is.SameAs(first.UsageCommitter));

            first.Handler.Resolve(
                environment.State,
                environment.Source);

            Assert.That(
                rebuilt.DiscoverTriggers(
                    environment.State,
                    environment.Source),
                Is.Empty);

            rebuilt.Handler.Resolve(
                environment.State,
                environment.Source);

            Assert.That(environment.Gains().Count, Is.EqualTo(2));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly BoardRow OtherRow;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(
                    new CombatPetTriggerUsageRegistry());
            public readonly CombatStartedCombatEvent Root;
            public readonly BattleStartStageStartedCombatEvent Source;
            public readonly CombatHpGainResolver Hp;
            public readonly ToucanPetTriggerSourceFactory Factory;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int distinctSuitCount = 3)
            {
                Side = side;
                Row = row;
                OtherRow = row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

                var definitionId = new DefinitionId(
                    "test.toucan_factory");
                var upper = new CombatPetState(
                    definitionId,
                    new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId,
                    new InstanceId(1001));

                Pet = row == BoardRow.Front
                    ? upper
                    : lower;

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;
                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { upper, lower }));
                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(
                        Array.Empty<CombatPetState>()));

                State = new CombatState(
                    MakeSide(
                        CombatSide.Player,
                        distinctSuitCount),
                    MakeSide(
                        CombatSide.Enemy,
                        distinctSuitCount),
                    side == CombatSide.Player
                        ? ownPets
                        : otherPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : otherPets);

                Root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
                Log.Append(Root);
                Source = new BattleStartStageStartedCombatEvent(
                    Metadata.CreateChild(Root.Metadata),
                    CombatBattleStartStage.Pet,
                    new CombatBattleStartSnapshotResolver()
                        .Resolve(State));
                Log.Append(Source);
                Hp = new CombatHpGainResolver(Metadata, Log);
                Factory = new ToucanPetTriggerSourceFactory(
                    definitionId,
                    Usage,
                    Hp);
            }

            public ToucanPetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory.CreateSources(Side, Pet));

                return (ToucanPetTriggerSource)sources[0];
            }

            public CombatCardState Card(
                BoardRow row,
                int column)
            {
                return State.GetSide(Side).GetCardAt(
                    new BoardPosition(
                        Side,
                        row,
                        new BoardColumn(column)));
            }

            public List<HpGainCombatEvent> Gains()
            {
                var result = new List<HpGainCombatEvent>();

                foreach (var item in Log.Events)
                {
                    var gain = item as HpGainCombatEvent;

                    if (gain != null)
                    {
                        result.Add(gain);
                    }
                }

                return result;
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                int distinctSuitCount)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();

                foreach (var row in new[]
                         {
                             BoardRow.Back,
                             BoardRow.Front
                         })
                {
                    foreach (var column in new[]
                             {
                                 5, 2, 1, 4, 3
                             })
                    {
                        var prefix =
                            (side == CombatSide.Player
                                ? 0
                                : 100) +
                            (row == BoardRow.Front
                                ? 0
                                : 10);
                        var suit = SuitForColumn(
                            column,
                            distinctSuitCount);
                        var card = new CombatCardState(
                            new DefinitionId(
                                "test.toucan_factory_card"),
                            new InstanceId(
                                prefix + 6 - column),
                            new CardRank(column + 1),
                            suit,
                            CombatCardSeason.Spring,
                            10,
                            5,
                            0,
                            2);

                        cards.Add(card);
                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(
                                side,
                                row,
                                new BoardColumn(column)),
                            card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardSuit SuitForColumn(
                int column,
                int distinctSuitCount)
            {
                if (distinctSuitCount <= 0)
                {
                    return CombatCardSuit.Unspecified;
                }

                if (distinctSuitCount == 1)
                {
                    return CombatCardSuit.Fruit;
                }

                if (distinctSuitCount == 2)
                {
                    return column % 2 == 0
                        ? CombatCardSuit.Vegetable
                        : CombatCardSuit.Fruit;
                }

                if (column == 2)
                {
                    return CombatCardSuit.Vegetable;
                }

                if (column == 3)
                {
                    return CombatCardSuit.Nut;
                }

                return CombatCardSuit.Fruit;
            }
        }
    }
}
