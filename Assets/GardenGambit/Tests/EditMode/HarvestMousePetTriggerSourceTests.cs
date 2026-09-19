using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HarvestMousePetTriggerSourceTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Constructor_RejectsMissingDependencies(int missing)
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new HarvestMousePetTriggerSource(
                e.Side, e.Pet.InstanceId, missing == 0 ? null : e.Usage,
                missing == 1 ? null : e.Resolver, missing == 2 ? null : e.Lookup));
        }

        [Test]
        public void Constructor_RejectsInvalidSideOrPetIdentity()
        {
            var e = new Environment();
            Assert.Throws<ArgumentOutOfRangeException>(() => new HarvestMousePetTriggerSource(
                default(CombatSide), e.Pet.InstanceId, e.Usage, e.Resolver, e.Lookup));
            Assert.Throws<ArgumentException>(() => new HarvestMousePetTriggerSource(
                e.Side, default(InstanceId), e.Usage, e.Resolver, e.Lookup));
        }

        [Test]
        public void Constructor_ExposesExactIdentityAndSharedDependencies()
        {
            var e = new Environment();
            Assert.That(e.Source.Side, Is.EqualTo(e.Side));
            Assert.That(e.Source.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            Assert.That(e.Source.Handler, Is.TypeOf<HarvestMousePetDeathTriggerHandler>());
            Assert.That(e.Source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Source.AttackGainResolver, Is.SameAs(e.Resolver));
            Assert.That(e.Source.CardLookup, Is.SameAs(e.Lookup));
            Assert.That(e.Source.OrderKeyProvider.Side, Is.EqualTo(e.Side));
            Assert.That(e.Source.OrderKeyProvider.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Discover_EligibleDeath_ReturnsExactHandlerWithPetPositionOrder(CombatSide side, BoardRow row)
        {
            var e = new Environment(side, row);
            var death = e.Kill(side, row);
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                e.Source.DiscoverTriggers(e.State, death));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(e.Source.Handler));
            Assert.That(candidates[0].OrderKey, Is.EqualTo(new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Pet, side, row == BoardRow.Front ? 0 : 1, 0)));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Target.Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));

            candidates[0].Trigger.Resolve(e.State, death);
            Assert.That(e.Target.Attack, Is.EqualTo(3));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
            Assert.That(e.Source.DiscoverTriggers(e.State, death), Is.Empty);
            Assert.That(e.Log.Count, Is.EqualTo(3));
            var gain = (AttackGainCombatEvent)e.Log.Events[2];
            Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(death.Metadata.EventId));
            Assert.That(gain.TargetInstanceId, Is.EqualTo(e.Target.InstanceId));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Discover_UnrelatedEvent_ReturnsNoCandidateAndDoesNotConsumeUse(int scenario)
        {
            var e = new Environment(sourceSeason: scenario == 3 ? CombatCardSeason.Spring : CombatCardSeason.Autumn);
            CombatEvent sourceEvent = scenario == 0 ? (CombatEvent)e.Root :
                e.Kill(scenario == 1 ? CombatSide.Enemy : CombatSide.Player,
                    scenario == 2 ? BoardRow.Back : BoardRow.Front);
            var before = e.Log.Count;
            Assert.That(e.Source.DiscoverTriggers(e.State, sourceEvent), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Target.Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(before));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Engine_UsesConcreteSourceAndDrainsOnlyOnce(CombatSide side)
        {
            var e = new Environment(side);
            var death = e.Kill(side, BoardRow.Front);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue,
                new CombatTriggerSourceRegistry(new ICombatTriggerSource[] { e.Source }));
            Assert.That(engine.Drain(maximumEventCount: 10, maximumTriggerCountPerEvent: 10), Is.EqualTo(3));
            Assert.That(e.Target.Attack, Is.EqualTo(3));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
            Assert.That(engine.Drain(maximumEventCount: 10, maximumTriggerCountPerEvent: 10), Is.Zero);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(e.Source.DiscoverTriggers(e.State, death), Is.Empty);
            Assert.That(e.Log.Count, Is.EqualTo(3));
        }

        [Test]
        public void RecreatedSource_ForSamePetSharesUseAndCannotGrantAgain()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front);
            e.Source.Handler.Resolve(e.State, death);
            var recreated = new HarvestMousePetTriggerSource(
                e.Side, e.Pet.InstanceId, e.Usage, e.Resolver, e.Lookup);
            Assert.That(recreated.Handler, Is.Not.SameAs(e.Source.Handler));
            Assert.That(recreated.DiscoverTriggers(e.State, death), Is.Empty);
            recreated.Handler.Resolve(e.State, death);
            Assert.That(e.Target.Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(3));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatCardState Target;
            public readonly CombatEventIdAllocator Ids = new CombatEventIdAllocator();
            public readonly CombatEventMetadataFactory Metadata;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(new CombatPetTriggerUsageRegistry());
            public readonly CombatAttackGainResolver Resolver;
            public readonly CombatCardLookup Lookup;
            public readonly HarvestMousePetTriggerSource Source;

            public Environment(CombatSide side = CombatSide.Player, BoardRow row = BoardRow.Front,
                CombatCardSeason sourceSeason = CombatCardSeason.Autumn)
            {
                Side = side;
                var pets = new[]
                {
                    new CombatPetState(new DefinitionId("test.harvest_mouse"), new InstanceId(1002)),
                    new CombatPetState(new DefinitionId("test.harvest_mouse"), new InstanceId(1001))
                };
                Pet = pets[row == BoardRow.Front ? 0 : 1];
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(pets));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(CreateSide(CombatSide.Player, sourceSeason), CreateSide(CombatSide.Enemy, sourceSeason),
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Target = State.GetSide(side).GetCardAt(Position(side, row, 2));
                Metadata = new CombatEventMetadataFactory(Ids, new CombatSequenceNumberAllocator());
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAttackGainResolver(Metadata, Log);
                Lookup = new CombatCardLookup(Log);
                Source = new HarvestMousePetTriggerSource(side, Pet.InstanceId, Usage, Resolver, Lookup);
            }

            public DeathCombatEvent Kill(CombatSide side, BoardRow row)
            {
                var position = Position(side, row, 1);
                var card = State.GetSide(side).GetCardAt(position);
                var previousHp = card.CurrentHp;
                card.SetCurrentHpToZero();
                var death = new DeathCombatEvent(Metadata.CreateChild(Root.Metadata), card.InstanceId,
                    position, previousHp, card.CurrentHp);
                Log.Append(death);
                return death;
            }

            private static BoardPosition Position(CombatSide side, BoardRow row, int column)
            {
                return new BoardPosition(side, row, new BoardColumn(column));
            }

            private static CombatSideState CreateSide(CombatSide side, CombatCardSeason sourceSeason)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    for (var column = 1; column <= 2; column++)
                    {
                        var value = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        var card = new CombatCardState(new DefinitionId("test.mouse_source_card"), new InstanceId(value),
                            new CardRank(2), column == 1 ? sourceSeason : CombatCardSeason.Autumn, 3, 3, 0, 2);
                        cards.Add(card);
                        slots.Add(new CombatSlotState(new SlotId(value), Position(side, row, column), card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
