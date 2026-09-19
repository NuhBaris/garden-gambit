using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HarvestMousePetTriggerSourceFactoryTests
    {
        [Test]
        public void Constructor_RejectsInvalidDefinitionId()
        {
            var e = new Environment();
            Assert.Throws<ArgumentException>(() => new HarvestMousePetTriggerSourceFactory(
                default(DefinitionId), e.Usage, e.Resolver, e.Lookup));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Constructor_RejectsMissingDependencies(int missing)
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new HarvestMousePetTriggerSourceFactory(
                e.DefinitionId, missing == 0 ? null : e.Usage,
                missing == 1 ? null : e.Resolver, missing == 2 ? null : e.Lookup));
        }

        [Test]
        public void Constructor_ExposesExactRegistrationAndDependencies()
        {
            var e = new Environment();
            Assert.That(e.Factory.PetDefinitionId, Is.EqualTo(e.DefinitionId));
            Assert.That(e.Factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Factory.AttackGainResolver, Is.SameAs(e.Resolver));
            Assert.That(e.Factory.CardLookup, Is.SameAs(e.Lookup));
            Assert.That(e.FactoryRegistry.GetFactory(e.DefinitionId), Is.SameAs(e.Factory));
        }

        [Test]
        public void CreateSources_RejectsInvalidSideNullPetAndMismatchedDefinitionImmediately()
        {
            var e = new Environment();
            var pet = e.State.PlayerPets.GetPetAt(0);
            Assert.Throws<ArgumentOutOfRangeException>(() => e.Factory.CreateSources(default(CombatSide), pet));
            Assert.Throws<ArgumentNullException>(() => e.Factory.CreateSources(CombatSide.Player, null));
            var wrongPet = new CombatPetState(new DefinitionId("test.other_pet"), new InstanceId(3001));
            Assert.Throws<ArgumentException>(() => e.Factory.CreateSources(CombatSide.Player, wrongPet));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void CreateSources_ReturnsOneConfiguredConcreteSource(CombatSide side)
        {
            var e = new Environment();
            var pet = e.State.GetPets(side).GetPetAt(0);
            var sources = new List<ICombatTriggerSource>(e.Factory.CreateSources(side, pet));
            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(sources[0], Is.TypeOf<HarvestMousePetTriggerSource>());
            var source = (HarvestMousePetTriggerSource)sources[0];
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(pet.InstanceId));
            Assert.That(source.OrderKeyProvider.Side, Is.EqualTo(side));
            Assert.That(source.OrderKeyProvider.PetInstanceId, Is.EqualTo(pet.InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.AttackGainResolver, Is.SameAs(e.Resolver));
            Assert.That(source.CardLookup, Is.SameAs(e.Lookup));
        }

        [Test]
        public void CreateSources_RepeatedCalls_CreateDistinctHandlersWithSharedDependencies()
        {
            var e = new Environment();
            var pet = e.State.PlayerPets.GetPetAt(0);
            var first = (HarvestMousePetTriggerSource)new List<ICombatTriggerSource>(
                e.Factory.CreateSources(CombatSide.Player, pet))[0];
            var second = (HarvestMousePetTriggerSource)new List<ICombatTriggerSource>(
                e.Factory.CreateSources(CombatSide.Player, pet))[0];
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.Handler, Is.Not.SameAs(second.Handler));
            Assert.That(first.UsageCommitter, Is.SameAs(second.UsageCommitter));
            Assert.That(first.AttackGainResolver, Is.SameAs(second.AttackGainResolver));
            Assert.That(first.CardLookup, Is.SameAs(second.CardLookup));
        }

        [Test]
        public void Builder_HandlesBothSidesAndSlots_WithPetPositionOrdering()
        {
            var e = new Environment();
            var sources = e.Builder.BuildSources(e.State);
            Assert.That(sources.Count, Is.EqualTo(4));
            var ordered = new List<HarvestMousePetTriggerSource>();
            foreach (var item in sources)
            {
                Assert.That(item, Is.TypeOf<HarvestMousePetTriggerSource>());
                var source = (HarvestMousePetTriggerSource)item;
                Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
                Assert.That(source.AttackGainResolver, Is.SameAs(e.Resolver));
                Assert.That(source.CardLookup, Is.SameAs(e.Lookup));
                ordered.Add(source);
            }
            ordered.Sort((left, right) => left.OrderKeyProvider.GetOrderKey(e.State, e.Root)
                .CompareTo(right.OrderKeyProvider.GetOrderKey(e.State, e.Root)));
            Assert.That(ordered[0].PetInstanceId, Is.EqualTo(e.State.PlayerPets.GetPetAt(0).InstanceId));
            Assert.That(ordered[1].PetInstanceId, Is.EqualTo(e.State.EnemyPets.GetPetAt(0).InstanceId));
            Assert.That(ordered[2].PetInstanceId, Is.EqualTo(e.State.PlayerPets.GetPetAt(1).InstanceId));
            Assert.That(ordered[3].PetInstanceId, Is.EqualTo(e.State.EnemyPets.GetPetAt(1).InstanceId));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void BuilderRegistry_ResolvesFourIndependentPets_AndRebuildPreservesUsedCounters()
        {
            var e = new Environment();
            var deaths = new List<DeathCombatEvent>();
            foreach (var side in new[] { CombatSide.Player, CombatSide.Enemy })
            {
                foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                {
                    deaths.Add(e.Kill(side, row));
                }
            }
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, e.Builder.BuildRegistry(e.State));
            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.EqualTo(9));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            foreach (var death in deaths)
            {
                var target = e.Target(death.Position.Side, death.Position.Row);
                Assert.That(target.Attack, Is.EqualTo(3));
                var matchingGains = 0;
                foreach (var combatEvent in e.Log.Events)
                {
                    var gain = combatEvent as AttackGainCombatEvent;
                    if (gain == null || gain.TargetInstanceId != target.InstanceId) { continue; }
                    matchingGains++;
                    Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(death.Metadata.EventId));
                    Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
                    Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
                }
                Assert.That(matchingGains, Is.EqualTo(1));
            }

            // Rebuild source objects using the same battle-scoped factory dependencies.
            var rebuiltEngine = new CombatTriggerEngine(e.State, queue, e.Builder.BuildRegistry(e.State));
            Assert.That(rebuiltEngine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.Zero);
            e.State.Player.GetCardAt(Position(CombatSide.Player, BoardRow.Front, 1)).Heal(1);
            e.Kill(CombatSide.Player, BoardRow.Front);
            Assert.That(rebuiltEngine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.EqualTo(1));
            Assert.That(e.Target(CombatSide.Player, BoardRow.Front).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            Assert.That(e.Log.Count, Is.EqualTo(10));
            Assert.That(queue.PendingCount, Is.Zero);
        }

        [Test]
        public void SeparateBattleDependencies_DoNotShareUsesEvenWithSamePetIds()
        {
            var first = new Environment();
            var second = new Environment();
            var petId = first.State.PlayerPets.GetPetAt(0).InstanceId;
            Assert.That(second.State.PlayerPets.GetPetAt(0).InstanceId, Is.EqualTo(petId));
            first.Kill(CombatSide.Player, BoardRow.Front);
            var firstEngine = new CombatTriggerEngine(first.State, new CombatEventQueue(first.Log),
                first.Builder.BuildRegistry(first.State));
            Assert.That(firstEngine.Drain(maximumEventCount: 10, maximumTriggerCountPerEvent: 10), Is.EqualTo(3));
            Assert.That(first.Usage.HasTriggered(petId), Is.True);
            Assert.That(second.Usage.HasTriggered(petId), Is.False);
            second.Kill(CombatSide.Player, BoardRow.Front);
            var secondEngine = new CombatTriggerEngine(second.State, new CombatEventQueue(second.Log),
                second.Builder.BuildRegistry(second.State));
            Assert.That(secondEngine.Drain(maximumEventCount: 10, maximumTriggerCountPerEvent: 10), Is.EqualTo(3));
            Assert.That(second.Usage.HasTriggered(petId), Is.True);
            Assert.That(first.Target(CombatSide.Player, BoardRow.Front).Attack, Is.EqualTo(3));
            Assert.That(second.Target(CombatSide.Player, BoardRow.Front).Attack, Is.EqualTo(3));
            Assert.That(first.Log.Count, Is.EqualTo(3));
            Assert.That(second.Log.Count, Is.EqualTo(3));
        }

        private static BoardPosition Position(CombatSide side, BoardRow row, int column)
        {
            return new BoardPosition(side, row, new BoardColumn(column));
        }

        private sealed class Environment
        {
            public readonly DefinitionId DefinitionId = new DefinitionId("test.factory_harvest_mouse");
            public readonly CombatState State;
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(new CombatPetTriggerUsageRegistry());
            public readonly CombatAttackGainResolver Resolver;
            public readonly CombatCardLookup Lookup;
            public readonly HarvestMousePetTriggerSourceFactory Factory;
            public readonly CombatPetTriggerSourceFactoryRegistry FactoryRegistry;
            public readonly CombatPetTriggerSourceBuilder Builder;

            public Environment()
            {
                State = new CombatState(CreateSide(CombatSide.Player), CreateSide(CombatSide.Enemy),
                    CreatePets(CombatSide.Player), CreatePets(CombatSide.Enemy));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAttackGainResolver(Metadata, Log);
                Lookup = new CombatCardLookup(Log);
                Factory = new HarvestMousePetTriggerSourceFactory(DefinitionId, Usage, Resolver, Lookup);
                FactoryRegistry = new CombatPetTriggerSourceFactoryRegistry(new ICombatPetTriggerSourceFactory[] { Factory });
                Builder = new CombatPetTriggerSourceBuilder(FactoryRegistry);
            }

            public CombatCardState Target(CombatSide side, BoardRow row)
            {
                return State.GetSide(side).GetCardAt(Position(side, row, 2));
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

            private CombatSidePetState CreatePets(CombatSide side)
            {
                var baseId = side == CombatSide.Player ? 1000 : 2000;
                return new CombatSidePetState(side, new CombatPetRegistry(new[]
                {
                    new CombatPetState(DefinitionId, new InstanceId(baseId + 2)),
                    new CombatPetState(DefinitionId, new InstanceId(baseId + 1))
                }));
            }

            private static CombatSideState CreateSide(CombatSide side)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    for (var column = 1; column <= 2; column++)
                    {
                        var value = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        var card = new CombatCardState(new DefinitionId("test.factory_card"), new InstanceId(value),
                            new CardRank(2), CombatCardSeason.Autumn, 3, 3, 0, 2);
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
