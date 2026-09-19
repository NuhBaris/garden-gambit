using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeTapirTests
    {
        [Test]
        public void Catalogue_RegistersTapirWithSharedDependencies()
        {
            var e = new Environment();
            Assert.That(CombatPetDefinitionIds.TapirValue, Is.EqualTo("pet.tapir"));
            Assert.That(CombatPetDefinitionIds.Tapir, Is.EqualTo(new DefinitionId("pet.tapir")));
            var registry = e.Catalogue();
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            var ids = new HashSet<DefinitionId>();
            foreach (var factory in registry.Factories) { Assert.That(ids.Add(factory.PetDefinitionId), Is.True); }
            var tapir = (TapirPetTriggerSourceFactory)registry.GetFactory(CombatPetDefinitionIds.Tapir);
            Assert.That(tapir.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(tapir.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(e.Runtime.FactoryRegistry.Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Tapir), Is.False);
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry(e.Armor).Count, Is.EqualTo(4));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry(e.Armor).Contains(CombatPetDefinitionIds.Tapir), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneCompositeSourceWithSharedHandlers(CombatSide side)
        {
            var e = new Environment(side);
            var factory = (TapirPetTriggerSourceFactory)e.Catalogue().GetFactory(CombatPetDefinitionIds.Tapir);
            var sources = new List<ICombatTriggerSource>(factory.CreateSources(side, e.Pets[0]));
            Assert.That(sources.Count, Is.EqualTo(1));
            var source = (TapirPetTriggerSource)sources[0];
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pets[0].InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(source.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(source.RescueHandler.UsageCommitter, Is.SameAs(source.HpGainHandler.UsageCommitter));
            Assert.That(source.RescueHandler.AttackGainResolver, Is.SameAs(source.HpGainHandler.AttackGainResolver));
            Assert.That(source.HpGainHandler.Side, Is.EqualTo(side));
            Assert.That(source.HpGainHandler.PetInstanceId, Is.EqualTo(source.PetInstanceId));
            Assert.That(source.OrderKeyProvider.PetInstanceId, Is.EqualTo(source.PetInstanceId));
        }

        [Test]
        public void Factory_RejectsInvalidRegistrationAndOwner()
        {
            var e = new Environment();
            Assert.Throws<ArgumentException>(() => new TapirPetTriggerSourceFactory(default(DefinitionId), e.Runtime.UsageCommitter, e.Attack));
            Assert.Throws<ArgumentNullException>(() => new TapirPetTriggerSourceFactory(CombatPetDefinitionIds.Tapir, null, e.Attack));
            Assert.Throws<ArgumentNullException>(() => new TapirPetTriggerSourceFactory(CombatPetDefinitionIds.Tapir, e.Runtime.UsageCommitter, null));
            var factory = (TapirPetTriggerSourceFactory)e.Catalogue().GetFactory(CombatPetDefinitionIds.Tapir);
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.CreateSources((CombatSide)99, e.Pets[0]));
            Assert.Throws<ArgumentNullException>(() => factory.CreateSources(e.Side, null));
            Assert.Throws<ArgumentException>(() => factory.CreateSources(e.Side,
                new CombatPetState(CombatPetDefinitionIds.Hummingbird, new InstanceId(999))));
        }

        [Test]
        public void Source_RejectsInvalidDependenciesAndDiscoveryInput()
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new TapirPetTriggerSource(e.Side, e.Pets[0].InstanceId, null, e.Attack));
            Assert.Throws<ArgumentNullException>(() => new TapirPetTriggerSource(e.Side, e.Pets[0].InstanceId, e.Runtime.UsageCommitter, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TapirPetTriggerSource((CombatSide)99, e.Pets[0].InstanceId, e.Runtime.UsageCommitter, e.Attack));
            Assert.Throws<ArgumentException>(() => new TapirPetTriggerSource(e.Side, default(InstanceId), e.Runtime.UsageCommitter, e.Attack));
            var source = (TapirPetTriggerSource)e.Sources().Sources[0];
            Assert.Throws<ArgumentNullException>(() => source.DiscoverTriggers(null, e.Root()));
            Assert.Throws<ArgumentNullException>(() => source.DiscoverTriggers(e.State, null));
            Assert.That(source.DiscoverTriggers(e.State, e.Root()), Is.Empty);
        }

        [TestCase(0, CombatSide.Player, BoardRow.Front)]
        [TestCase(0, CombatSide.Player, BoardRow.Back)]
        [TestCase(0, CombatSide.Enemy, BoardRow.Front)]
        [TestCase(0, CombatSide.Enemy, BoardRow.Back)]
        [TestCase(1, CombatSide.Player, BoardRow.Front)]
        [TestCase(1, CombatSide.Player, BoardRow.Back)]
        [TestCase(1, CombatSide.Enemy, BoardRow.Front)]
        [TestCase(1, CombatSide.Enemy, BoardRow.Back)]
        [TestCase(2, CombatSide.Player, BoardRow.Front)]
        [TestCase(2, CombatSide.Player, BoardRow.Back)]
        [TestCase(2, CombatSide.Enemy, BoardRow.Front)]
        [TestCase(2, CombatSide.Enemy, BoardRow.Back)]
        public void RuntimeSources_DiscoverExactlyOneCorrectHandler(int path, CombatSide side, BoardRow row)
        {
            var e = new Environment(side);
            var sourceEvent = e.Restore(path, row);
            var registry = e.Sources();
            Assert.That(registry.Count, Is.EqualTo(2));
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(registry.DiscoverTriggers(e.State, sourceEvent));
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger.GetType(), Is.EqualTo(path == 0 ? typeof(TapirPetRescueTriggerHandler) : typeof(TapirPetHpGainTriggerHandler)));
            Assert.That(candidates[0].OrderKey, Is.EqualTo(new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Pet, side, row == BoardRow.Front ? 0 : 1, 0)));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            var engine = new CombatTriggerEngine(e.State, e.Queue, registry);
            engine.Drain(30, 30);
            var card = e.Card(row);
            Assert.That(card.Attack, Is.EqualTo(4));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.Pets[row == BoardRow.Front ? 0 : 1].InstanceId, card.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            Assert.That(e.Gains()[0].Metadata.ParentEventId.Value, Is.EqualTo(sourceEvent.Metadata.EventId));
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(30, 30), Is.Zero);
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 0)]
        public void RebuiltRuntimeSources_PreserveUsageAcrossRescuePaths(int first, int second)
        {
            var e = new Environment();
            e.Restore(first);
            e.Engine().Drain(30, 30);
            var later = e.Restore(second);
            Assert.That(e.Sources().DiscoverTriggers(e.State, later), Is.Empty);
            e.Engine().Drain(30, 30);
            Assert.That(e.Card().Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BothNotifications_ProduceOneBonusThroughCompositeSource(bool hpFirst)
        {
            var e = new Environment();
            var rescue = (RescueCombatEvent)e.Restore(0);
            var card = e.Card();
            // Second notification of the same transition; no second HP mutation.
            var hp = new HpGainCombatEvent(e.Metadata.CreateChild(rescue.Metadata), card.InstanceId,
                rescue.Position, card.HpCapacity, card.HpCapacity, rescue.PreviousHp, rescue.CurrentHp);
            e.Log.Append(hp);
            var registry = e.Sources();
            var rescueCandidate = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(registry.DiscoverTriggers(e.State, rescue))[0];
            var hpCandidate = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(registry.DiscoverTriggers(e.State, hp))[0];
            if (hpFirst) { hpCandidate.Trigger.Resolve(e.State, hp); rescueCandidate.Trigger.Resolve(e.State, rescue); }
            else { rescueCandidate.Trigger.Resolve(e.State, rescue); hpCandidate.Trigger.Resolve(e.State, hp); }
            e.Engine().Drain(30, 30);
            Assert.That(card.Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void EngineOverflow_ResumesSameBatchWithoutRepeatingRescue(int path)
        {
            var e = new Environment();
            var card = e.Card();
            card.ApplyAttackGain(int.MaxValue - 1 - card.Attack);
            var restored = e.Restore(path);
            var count = e.Log.Count;
            var engine = e.Engine();
            Assert.Throws<OverflowException>(() => engine.Drain(30, 30));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(card.CurrentHp, Is.EqualTo(1));
            card.ReduceAttack(int.MaxValue - 3);
            engine.Drain(30, 30);
            Assert.That(card.Attack, Is.EqualTo(4));
            Assert.That(e.Log.Count, Is.EqualTo(count + 1));
            Assert.That(e.Gains()[0].Metadata.ParentEventId.Value, Is.EqualTo(restored.Metadata.EventId));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.HasActiveBatch, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void TwoTapirs_UseIndependentPetCardKeys(CombatSide side)
        {
            var e = new Environment(side);
            var card = e.Card();
            e.Restore(0);
            e.Engine().Drain(30, 30);
            e.State.GetSide(side).RemoveCardFromCombat(e.Position(BoardRow.Back));
            e.State.GetSide(side).MoveCard(e.Position(BoardRow.Front), e.Position(BoardRow.Back));
            e.Restore(1, BoardRow.Back);
            e.Engine().Drain(30, 30);
            Assert.That(card.Attack, Is.EqualTo(6));
            foreach (var pet in e.Pets) { Assert.That(e.Runtime.UsageCommitter.HasTriggered(pet.InstanceId, card.InstanceId), Is.True); }
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void FreshRuntime_AllowsSameIdsAgain()
        {
            var first = new Environment();
            first.Restore(0); first.Engine().Drain(30, 30);
            var second = new Environment();
            second.Restore(2); second.Engine().Drain(30, 30);
            Assert.That(first.Card().InstanceId, Is.EqualTo(second.Card().InstanceId));
            Assert.That(first.Card().Attack, Is.EqualTo(4));
            Assert.That(second.Card().Attack, Is.EqualTo(4));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeCreateResolutionRunner_AcceptsTapirRosterAndCompletes(CombatSide side)
        {
            var e = new Environment(side);
            var runner = e.Runtime.CreateResolutionRunner(e.State, e.Metadata, e.Log, e.Queue);
            Assert.That(runner.UsesStagedNormalAttackByDefault, Is.True);
            Assert.That(Start(runner), Is.Not.Null);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);
        }

        [TestCase(0, CombatSide.Player)]
        [TestCase(1, CombatSide.Player)]
        [TestCase(2, CombatSide.Player)]
        [TestCase(0, CombatSide.Enemy)]
        [TestCase(1, CombatSide.Enemy)]
        [TestCase(2, CombatSide.Enemy)]
        public void ResolutionRunner_ProcessesRescueAndTapirBonusBeforeCombatCompletes(int path, CombatSide side)
        {
            var e = new Environment(side);
            var runner = e.RunnerWithRescueFixture(path);
            var completed = Start(runner);
            Assert.That(e.Card().Attack, Is.EqualTo(4));
            Assert.That(e.Card().CurrentHp, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            Assert.That(e.Gains()[0].Metadata.SequenceNo.Value, Is.LessThan(completed.Metadata.SequenceNo.Value));
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ResolutionRunner_ResumesTapirOverflowWithoutRepeatingFixtureRescue(int path)
        {
            var e = new Environment();
            var card = e.Card();
            card.ApplyAttackGain(int.MaxValue - 1 - card.Attack);
            var runner = e.RunnerWithRescueFixture(path);
            Assert.Throws<OverflowException>(() => Start(runner));
            Assert.That(runner.HasActiveCombat, Is.True);
            Assert.That(card.CurrentHp, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);
            card.ReduceAttack(int.MaxValue - 3);
            var completed = runner.ResumeActiveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);
            Assert.That(completed, Is.Not.Null);
            Assert.That(card.Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            var deaths = 0;
            foreach (var item in e.Log.Events) { if (item is DeathCombatEvent) { deaths++; } }
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
        }

        private static CombatCompletedCombatEvent Start(CombatResolutionRunner runner) =>
            runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);

        // Test-only producer exercises real resolvers during the runner's event chain.
        // It is not a new gameplay ability or a production factory registration.
        private sealed class RestoreOnStart : ICombatTriggerHandler
        {
            private readonly Environment _environment;
            private readonly int _path;
            private bool _used;
            public RestoreOnStart(Environment environment, int path) { _environment = environment; _path = path; }
            public bool CanTrigger(CombatState state, CombatEvent sourceEvent) => !_used && sourceEvent is CombatStartedCombatEvent;
            public void Resolve(CombatState state, CombatEvent sourceEvent)
            {
                if (!CanTrigger(state, sourceEvent)) { return; }
                _environment.Restore(_path, parent: sourceEvent);
                _used = true;
            }
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState[] Pets;
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventQueue Queue;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatHpGainResolver Hp;
            private CombatStartedCombatEvent _root;

            public Environment(CombatSide side = CombatSide.Player)
            {
                Side = side;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Pets = new[]
                {
                    new CombatPetState(CombatPetDefinitionIds.Tapir, new InstanceId(1002)),
                    new CombatPetState(CombatPetDefinitionIds.Tapir, new InstanceId(1001))
                };
                var own = MakeSide(side, true);
                var opposing = MakeSide(other, false);
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(Pets));
                var otherPets = new CombatSidePetState(other, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : opposing, side == CombatSide.Enemy ? own : opposing,
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Hp = new CombatHpGainResolver(Metadata, Log);
            }

            public CombatPetTriggerSourceFactoryRegistry Catalogue() => Runtime.FactoryCatalog.CreateRegistry(
                Armor, Attack, new CombatCardLookup(Log), Runtime.PetUsageCommitter);
            public CombatTriggerSourceRegistry Sources() => Runtime.BuildSourceRegistry(State, Armor, Attack, new CombatCardLookup(Log));
            public CombatTriggerEngine Engine() => new CombatTriggerEngine(State, Queue, Sources());
            public BoardPosition Position(BoardRow row) => new BoardPosition(Side, row, new BoardColumn(1));
            public CombatCardState Card(BoardRow row = BoardRow.Front) => State.GetSide(Side).GetCardAt(Position(row));
            public CombatStartedCombatEvent Root()
            {
                if (_root == null) { _root = new CombatStartedCombatEvent(Metadata.CreateRoot()); Log.Append(_root); }
                return _root;
            }

            // 0 = explicit Rescue, 1 = Heal, 2 = HP stat gain.
            public CombatEvent Restore(int path, BoardRow row = BoardRow.Front, CombatEvent parent = null)
            {
                parent = parent ?? Root();
                var card = Card(row);
                var previousHp = card.CurrentHp;
                card.SetCurrentHpToZero();
                card.ApplyIncomingDamage(2);
                var death = new DeathCombatEvent(Metadata.CreateChild(parent.Metadata), card.InstanceId, Position(row), previousHp, card.CurrentHp);
                Log.Append(death);
                if (path == 0) { return new CombatRescueResolver(Metadata, Log).ApplyRescue(State, death); }
                if (path == 1) { return Hp.TryApplyHeal(State, death, Position(row), 3); }
                return Hp.TryApplyHpStatGain(State, death, Position(row), 3);
            }

            public CombatResolutionRunner RunnerWithRescueFixture(int path)
            {
                var sources = new List<ICombatTriggerSource>(Sources().Sources)
                {
                    new CombatPetTriggerSource(Side, Pets[0].InstanceId, new RestoreOnStart(this, path))
                };
                return new CombatResolutionRunner(State, Metadata, Log, Queue,
                    new CombatTriggerSourceRegistry(sources), Runtime.SourceDamageModifierRegistry,
                    Runtime.TargetDamageReductionResolver, Runtime.FinalRankModifierRegistry,
                    useStagedNormalAttackByDefault: true);
            }

            public List<AttackGainCombatEvent> Gains()
            {
                var events = new List<AttackGainCombatEvent>();
                foreach (var item in Log.Events) { if (item is AttackGainCombatEvent gain) { events.Add(gain); } }
                return events;
            }

            private static CombatSideState MakeSide(CombatSide side, bool occupied)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var id = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        CombatCardState card = null;
                        if (occupied && column == 1)
                        {
                            card = new CombatCardState(new DefinitionId("test.runtime_tapir_card"), new InstanceId(id),
                                new CardRank(4), CombatCardSeason.Spring, hpCapacity: 10, currentHp: 5, armor: 0, attack: 2);
                            cards.Add(card);
                        }
                        slots.Add(new CombatSlotState(new SlotId(id), new BoardPosition(side, row, new BoardColumn(column)),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(100), new AttackMultiplier(1));
            }
        }
    }
}
