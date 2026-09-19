using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        MarmosetPetTriggerSourceFactoryTests
    {
        [Test]
        public void ConstructorsValidateAndPreserveDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new MarmosetPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.Usage,
                    environment.Hp,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.Hp,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.Usage,
                    null,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.Usage,
                    environment.Hp,
                    null));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.Hp,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    null,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Hp,
                    null));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MarmosetPetTriggerSource(
                    (CombatSide)99,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Hp,
                    environment.Armor));
            Assert.Throws<ArgumentException>(() =>
                new MarmosetPetTriggerSource(
                    environment.Side,
                    default(InstanceId),
                    environment.Usage,
                    environment.Hp,
                    environment.Armor));

            Assert.That(
                environment.Factory.PetDefinitionId,
                Is.EqualTo(
                    environment.Pet.DefinitionId));
            Assert.That(
                environment.Factory.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                environment.Factory.HpGainResolver,
                Is.SameAs(environment.Hp));
            Assert.That(
                environment.Factory.ArmorGainResolver,
                Is.SameAs(environment.Armor));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FactoryCreatesOneConfiguredSource(
            CombatSide side)
        {
            var environment = new Environment(side);
            var source = environment.CreateSource();

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                source.HpGainResolver,
                Is.SameAs(environment.Hp));
            Assert.That(
                source.ArmorGainResolver,
                Is.SameAs(environment.Armor));
            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(side));
        }

        [Test]
        public void FactoryRejectsInvalidOwnerInputs()
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

            var wrongPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                new InstanceId(3001));

            Assert.Throws<ArgumentException>(() =>
                environment.Factory.CreateSources(
                    environment.Side,
                    wrongPet));
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
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>(
                source.DiscoverTriggers(
                    environment.State,
                    environment.Source));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(source.Handler));

            var queue = new CombatEventQueue(environment.Log);
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
                environment.Card(row, 2).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.Card(row, 2).CurrentHp,
                Is.EqualTo(6));
            Assert.That(
                environment.Card(row, 2).Armor,
                Is.EqualTo(1));
            Assert.That(
                environment.Card(row, 1).HpCapacity,
                Is.EqualTo(10));
            Assert.That(
                environment.Card(environment.OtherRow, 2)
                    .HpCapacity,
                Is.EqualTo(10));
            Assert.That(environment.GainCount(), Is.EqualTo(2));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
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
            Assert.That(environment.GainCount(), Is.Zero);
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

            Assert.That(environment.GainCount(), Is.EqualTo(2));
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
            public readonly CombatArmorGainResolver Armor;
            public readonly MarmosetPetTriggerSourceFactory Factory;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                Side = side;
                Row = row;
                OtherRow = row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

                var definitionId =
                    new DefinitionId("test.marmoset_factory");
                var upper = new CombatPetState(
                    definitionId,
                    new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId,
                    new InstanceId(1001));
                Pet = row == BoardRow.Front
                    ? upper
                    : lower;

                var own = CreateSide(side);
                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;
                var opposing = new CombatSideState(
                    new CombatBoardState(
                        otherSide,
                        Array.Empty<CombatSlotState>()),
                    new CombatCardRegistry(
                        Array.Empty<CombatCardState>()),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { upper, lower }));
                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(
                        Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? own
                        : opposing,
                    side == CombatSide.Enemy
                        ? own
                        : opposing,
                    side == CombatSide.Player
                        ? ownPets
                        : otherPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : otherPets);

                Root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
                Log.Append(Root);
                Source = MakePetStage();
                Hp = new CombatHpGainResolver(Metadata, Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Factory = new MarmosetPetTriggerSourceFactory(
                    definitionId,
                    Usage,
                    Hp,
                    Armor);
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

            public MarmosetPetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory.CreateSources(Side, Pet));

                Assert.That(sources.Count, Is.EqualTo(1));
                Assert.That(
                    sources[0],
                    Is.TypeOf<MarmosetPetTriggerSource>());

                return (MarmosetPetTriggerSource)sources[0];
            }

            public int GainCount()
            {
                var count = 0;

                foreach (var combatEvent in Log.Events)
                {
                    if (combatEvent is HpGainCombatEvent ||
                        combatEvent is ArmorGainCombatEvent)
                    {
                        count++;
                    }
                }

                return count;
            }

            private BattleStartStageStartedCombatEvent
                MakePetStage()
            {
                var source =
                    new BattleStartStageStartedCombatEvent(
                        Metadata.CreateChild(Root.Metadata),
                        CombatBattleStartStage.Pet,
                        new CombatBattleStartSnapshotResolver()
                            .Resolve(State));
                Log.Append(source);

                return source;
            }

            private static CombatSideState CreateSide(
                CombatSide side)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();

                foreach (var row in new[]
                         {
                             BoardRow.Back,
                             BoardRow.Front
                         })
                {
                    foreach (var column in new[] { 2, 1 })
                    {
                        var prefix = row == BoardRow.Front
                            ? 0
                            : 10;
                        var card = new CombatCardState(
                            new DefinitionId(
                                "test.marmoset_factory_card"),
                            new InstanceId(
                                prefix + column),
                            new CardRank(
                                column == 1 ? 5 : 2),
                            CombatCardSuit.Unspecified,
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
        }
    }
}
