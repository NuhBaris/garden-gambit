using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HawkPetTriggerSourceFactoryTests
    {
        [Test]
        public void SourceConstructor_ValidatesDependenciesAndOwner()
        {
            var e = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new HawkPetTriggerSource(
                    e.Side, e.Pet.InstanceId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new HawkPetTriggerSource(
                    e.Side, e.Pet.InstanceId, e.Usage, null));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new HawkPetTriggerSource(
                    (CombatSide)99,
                    e.Pet.InstanceId,
                    e.Usage,
                    e.Attack));

            Assert.Throws<ArgumentException>(() =>
                new HawkPetTriggerSource(
                    e.Side,
                    default(InstanceId),
                    e.Usage,
                    e.Attack));
        }

        [Test]
        public void FactoryConstructor_ValidatesAndPreservesDependencies()
        {
            var e = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new HawkPetTriggerSourceFactory(
                    default(DefinitionId), e.Usage, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new HawkPetTriggerSourceFactory(
                    e.Pet.DefinitionId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new HawkPetTriggerSourceFactory(
                    e.Pet.DefinitionId, e.Usage, null));

            Assert.That(
                e.Factory.PetDefinitionId,
                Is.EqualTo(e.Pet.DefinitionId));

            Assert.That(e.Factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Factory.AttackGainResolver, Is.SameAs(e.Attack));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneSourceWithCorrectOwner(CombatSide side)
        {
            var e = new Environment(side);
            var sources = new List<ICombatTriggerSource>(
                e.Factory.CreateSources(side, e.Pet));

            Assert.That(sources.Count, Is.EqualTo(1));

            var source = sources[0] as HawkPetTriggerSource;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            Assert.That(source.Handler, Is.TypeOf<HawkPetBattleStartTriggerHandler>());
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(source.OrderKeyProvider.Side, Is.EqualTo(side));
            Assert.That(
                source.OrderKeyProvider.PetInstanceId,
                Is.EqualTo(e.Pet.InstanceId));
        }

        [Test]
        public void Factory_RejectsInvalidSideNullPetAndDefinitionMismatch()
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

        [Test]
        public void RecreatedSources_ShareUsageAndDoNotRepeatBonus()
        {
            var e = new Environment();
            var sourceEvent = e.StartEvent();
            var first = e.CreateSource();
            var second = e.CreateSource();

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.Handler, Is.Not.SameAs(first.Handler));
            Assert.That(
                second.UsageCommitter,
                Is.SameAs(first.UsageCommitter));

            first.Handler.Resolve(e.State, sourceEvent);

            Assert.That(
                second.DiscoverTriggers(e.State, sourceEvent),
                Is.Empty);

            second.Handler.Resolve(e.State, sourceEvent);

            Assert.That(e.Target.Attack, Is.EqualTo(8));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Source_DiscoveryAndEngineUseCorrectPetRowAndOrder(
            CombatSide side,
            BoardRow row)
        {
            var e = new Environment(side, row);
            var sourceEvent = e.StartEvent();
            var source = e.CreateSource();
            var candidates =
                new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(e.State, sourceEvent));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            Assert.That(
                candidates[0].OrderKey,
                Is.EqualTo(new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Pet,
                    side,
                    row == BoardRow.Front ? 0 : 1,
                    0)));

            Assert.That(e.Target.Attack, Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);

            var queue = new CombatEventQueue(e.Log);
            var registry = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[] { source });

            var engine = new CombatTriggerEngine(e.State, queue, registry);
            engine.Drain(20, 20);

            Assert.That(e.Target.Attack, Is.EqualTo(8));
            Assert.That(e.OtherRowCard.Attack, Is.EqualTo(5));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);

            var gains = e.Gains();
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(
                gains[0].TargetInstanceId,
                Is.EqualTo(e.Target.InstanceId));
            Assert.That(
                gains[0].Metadata.ParentEventId.Value,
                Is.EqualTo(sourceEvent.Metadata.EventId));

            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void Source_IgnoresOtherStagesAndValidatesDiscoveryInput(
            CombatBattleStartStage stage)
        {
            var e = new Environment();
            var source = e.CreateSource();
            var sourceEvent = e.StartEvent(stage);

            Assert.That(
                source.DiscoverTriggers(e.State, sourceEvent),
                Is.Empty);

            Assert.That(
                source.DiscoverTriggers(e.State, e.Root),
                Is.Empty);

            Assert.Throws<ArgumentNullException>(() =>
                source.DiscoverTriggers(null, sourceEvent));

            Assert.Throws<ArgumentNullException>(() =>
                source.DiscoverTriggers(e.State, null));

            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatCardState Target;
            public readonly CombatCardState OtherRowCard;

            public readonly CombatEventLog Log = new CombatEventLog();

            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(
                    new CombatPetTriggerUsageRegistry());

            public readonly CombatStartedCombatEvent Root;
            public readonly CombatAttackGainResolver Attack;
            public readonly HawkPetTriggerSourceFactory Factory;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                Side = side;

                var definitionId = new DefinitionId("test.hawk_factory");
                var upper = new CombatPetState(
                    definitionId, new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId, new InstanceId(1001));

                Pet = row == BoardRow.Front ? upper : lower;

                var frontCard = CreateCard(1);
                var backCard = CreateCard(2);

                Target = row == BoardRow.Front ? frontCard : backCard;
                OtherRowCard = row == BoardRow.Front ? backCard : frontCard;

                var own = new CombatSideState(
                    new CombatBoardState(side, new[]
                    {
                        new CombatSlotState(
                            new SlotId(2),
                            new BoardPosition(
                                side, BoardRow.Back, new BoardColumn(1)),
                            backCard.InstanceId),

                        new CombatSlotState(
                            new SlotId(1),
                            new BoardPosition(
                                side, BoardRow.Front, new BoardColumn(1)),
                            frontCard.InstanceId)
                    }),
                    new CombatCardRegistry(new[] { frontCard, backCard }),
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

                Attack = new CombatAttackGainResolver(Metadata, Log);
                Factory = new HawkPetTriggerSourceFactory(
                    definitionId, Usage, Attack);
            }

            public HawkPetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory.CreateSources(Side, Pet));

                return (HawkPetTriggerSource)sources[0];
            }

            public BattleStartStageStartedCombatEvent StartEvent(
                CombatBattleStartStage stage = CombatBattleStartStage.Pet)
            {
                var source = new BattleStartStageStartedCombatEvent(
                    Metadata.CreateChild(Root.Metadata),
                    stage);

                Log.Append(source);
                return source;
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

            private static CombatCardState CreateCard(long id)
            {
                return new CombatCardState(
                    new DefinitionId("test.hawk_factory_card"),
                    new InstanceId(id),
                    new CardRank(4),
                    CombatCardSeason.Spring,
                    hpCapacity: 10,
                    currentHp: 5,
                    armor: 0,
                    attack: 5);
            }
        }
    }
}