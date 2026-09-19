using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class MacaquePetTriggerSourceFactoryTests
    {
        [Test]
        public void Constructors_ValidateAndPreserveDependencies()
        {
            var e = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new MacaquePetTriggerSourceFactory(
                    default(DefinitionId), e.Usage, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new MacaquePetTriggerSourceFactory(
                    e.Pet.DefinitionId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new MacaquePetTriggerSourceFactory(
                    e.Pet.DefinitionId, e.Usage, null));

            Assert.Throws<ArgumentNullException>(() =>
                new MacaquePetTriggerSource(
                    e.Side, e.Pet.InstanceId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new MacaquePetTriggerSource(
                    e.Side, e.Pet.InstanceId, e.Usage, null));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MacaquePetTriggerSource(
                    (CombatSide)99, e.Pet.InstanceId, e.Usage, e.Attack));

            Assert.Throws<ArgumentException>(() =>
                new MacaquePetTriggerSource(
                    e.Side, default(InstanceId), e.Usage, e.Attack));

            Assert.That(e.Factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Factory.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(
                e.Factory.PetDefinitionId,
                Is.EqualTo(e.Pet.DefinitionId));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneSourceWithCorrectOwner(CombatSide side)
        {
            var e = new Environment(side);
            var sources = new List<ICombatTriggerSource>(
                e.Factory.CreateSources(side, e.Pet));

            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(sources[0], Is.TypeOf<MacaquePetTriggerSource>());

            var source = (MacaquePetTriggerSource)sources[0];

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            Assert.That(
                source.Handler,
                Is.TypeOf<MacaquePetBattleStartTriggerHandler>());
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(source.OrderKeyProvider.Side, Is.EqualTo(side));
            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(e.Pet.InstanceId));
        }

        [Test]
        public void Factory_RejectsInvalidOwner()
        {
            var e = new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                e.Factory.CreateSources((CombatSide)99, e.Pet));

            Assert.Throws<ArgumentNullException>(() =>
                e.Factory.CreateSources(e.Side, null));

            var otherPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                new InstanceId(2000));

            Assert.Throws<ArgumentException>(() =>
                e.Factory.CreateSources(e.Side, otherPet));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Source_ResolvesSnapshotGroupWithCorrectRowAndOrder(
            CombatSide side,
            BoardRow row)
        {
            var e = new Environment(side, row);
            var source = e.CreateSource();

            var candidates =
                new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(e.State, e.Source));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            Assert.That(
                candidates[0].OrderKey,
                Is.EqualTo(new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Pet,
                    side,
                    row == BoardRow.Front ? 0 : 1,
                    0)));

            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);

            var queue = new CombatEventQueue(e.Log);
            var registry = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[] { source });

            var engine = new CombatTriggerEngine(e.State, queue, registry);
            engine.Drain(20, 20);

            for (var column = 1; column <= 3; column++)
            {
                Assert.That(e.Card(row, column).Attack, Is.EqualTo(4));
                Assert.That(
                    e.Card(e.OtherRow, column).Attack,
                    Is.EqualTo(2));
            }

            var gains = e.Gains();
            Assert.That(gains.Count, Is.EqualTo(3));

            for (var index = 0; index < gains.Count; index++)
            {
                Assert.That(
                    gains[index].TargetInstanceId,
                    Is.EqualTo(e.Card(row, index + 1).InstanceId));

                Assert.That(
                    gains[index].Metadata.ParentEventId.Value,
                    Is.EqualTo(e.Source.Metadata.EventId));

                Assert.That(gains[index].ActualGainedAmount, Is.EqualTo(2));
            }

            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [Test]
        public void Source_RequiresSnapshotAndValidDiscoveryInput()
        {
            var e = new Environment();
            var source = e.CreateSource();

            var withoutSnapshot = new BattleStartStageStartedCombatEvent(
                e.Metadata.CreateChild(e.Root.Metadata),
                CombatBattleStartStage.Pet);

            e.Log.Append(withoutSnapshot);

            Assert.Throws<InvalidOperationException>(() =>
                source.DiscoverTriggers(e.State, withoutSnapshot));

            Assert.Throws<ArgumentNullException>(() =>
                source.DiscoverTriggers(null, e.Source));

            Assert.Throws<ArgumentNullException>(() =>
                source.DiscoverTriggers(e.State, null));

            Assert.That(source.DiscoverTriggers(e.State, e.Root), Is.Empty);
            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void Source_IgnoresOtherStartStages(CombatBattleStartStage stage)
        {
            var e = new Environment();
            var source = e.CreateSource();

            var otherStage = new BattleStartStageStartedCombatEvent(
                e.Metadata.CreateChild(e.Root.Metadata),
                stage,
                e.Source.BattleStartSnapshot);

            e.Log.Append(otherStage);

            Assert.That(
                source.DiscoverTriggers(e.State, otherStage),
                Is.Empty);

            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void RecreatedSources_ShareUsageAndDoNotRepeatBonus()
        {
            var e = new Environment();
            var first = e.CreateSource();
            var rebuilt = e.CreateSource();

            Assert.That(rebuilt, Is.Not.SameAs(first));
            Assert.That(rebuilt.Handler, Is.Not.SameAs(first.Handler));
            Assert.That(
                rebuilt.UsageCommitter,
                Is.SameAs(first.UsageCommitter));

            first.Handler.Resolve(e.State, e.Source);

            Assert.That(
                rebuilt.DiscoverTriggers(e.State, e.Source),
                Is.Empty);

            rebuilt.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Gains().Count, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));

            for (var column = 1; column <= 3; column++)
            {
                Assert.That(e.Card(e.Row, column).Attack, Is.EqualTo(4));
            }
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly BoardRow OtherRow;
            public readonly CombatState State;
            public readonly CombatPetState Pet;

            public readonly CombatEventLog Log = new CombatEventLog();

            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(
                    new CombatPetTriggerUsageRegistry());

            public readonly CombatStartedCombatEvent Root;
            public readonly BattleStartStageStartedCombatEvent Source;
            public readonly CombatAttackGainResolver Attack;
            public readonly MacaquePetTriggerSourceFactory Factory;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                Side = side;
                Row = row;
                OtherRow = row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

                var definitionId = new DefinitionId("test.macaque_factory");

                var upper = new CombatPetState(
                    definitionId, new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId, new InstanceId(1001));

                Pet = row == BoardRow.Front ? upper : lower;

                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();

                foreach (var cardRow in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 3, 1, 2 })
                    {
                        var prefix = cardRow == BoardRow.Front ? 0 : 10;

                        var card = new CombatCardState(
                            new DefinitionId("test.macaque_factory_card"),
                            new InstanceId(prefix + 4 - column),
                            new CardRank(7),
                            CombatCardSeason.Spring,
                            hpCapacity: 10,
                            currentHp: 5,
                            armor: 0,
                            attack: 2);

                        cards.Add(card);

                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(
                                side, cardRow, new BoardColumn(column)),
                            card.InstanceId));
                    }
                }

                var own = new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

                var opposing = new CombatSideState(
                    new CombatBoardState(
                        otherSide, Array.Empty<CombatSlotState>()),
                    new CombatCardRegistry(Array.Empty<CombatCardState>()),
                    new BattleHealth(20),
                    new AttackMultiplier(1));

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(new[] { upper, lower }));

                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Enemy ? own : opposing,
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);

                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);

                Source = new BattleStartStageStartedCombatEvent(
                    Metadata.CreateChild(Root.Metadata),
                    CombatBattleStartStage.Pet,
                    new CombatBattleStartSnapshotResolver().Resolve(State));

                Log.Append(Source);

                Attack = new CombatAttackGainResolver(Metadata, Log);
                Factory = new MacaquePetTriggerSourceFactory(
                    definitionId, Usage, Attack);
            }

            public CombatCardState Card(BoardRow row, int column) =>
                State.GetSide(Side).GetCardAt(
                    new BoardPosition(Side, row, new BoardColumn(column)));

            public MacaquePetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory.CreateSources(Side, Pet));

                return (MacaquePetTriggerSource)sources[0];
            }

            public List<AttackGainCombatEvent> Gains()
            {
                var result = new List<AttackGainCombatEvent>();

                foreach (var item in Log.Events)
                {
                    if (item is AttackGainCombatEvent gain)
                    {
                        result.Add(gain);
                    }
                }

                return result;
            }
        }
    }
}