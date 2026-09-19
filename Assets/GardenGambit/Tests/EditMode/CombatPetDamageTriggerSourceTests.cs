using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetDamageTriggerSourceTests
    {
        [Test]
        public void Constructor_RejectsNullHandler()
        {
            Assert.Throws<ArgumentNullException>(() => new CombatPetDamageTriggerSource(null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Discovery_PreservesHandlerOwnerAndResolvesThroughCandidate(CombatSide side, BoardRow row)
        {
            var e = new Environment();
            var damage = e.Damage(4);
            var pet = e.Pet(side, row);
            var handler = new Probe(side, pet.InstanceId);
            var source = new CombatPetDamageTriggerSource(handler);
            Assert.That(source.Handler, Is.SameAs(handler));
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(pet.InstanceId));
            Assert.That(source.OrderKeyProvider.Side, Is.EqualTo(side));
            Assert.That(source.OrderKeyProvider.PetInstanceId, Is.EqualTo(pet.InstanceId));
            var candidates = Discover(source, e.State, damage);
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(handler));
            Assert.That(handler.ResolveCount, Is.Zero);
            candidates[0].Trigger.Resolve(e.State, damage);
            Assert.That(handler.ResolveCount, Is.EqualTo(1));
            Assert.That(handler.LastPet, Is.SameAs(pet));
            Assert.That(handler.LastContext.SourceEvent, Is.SameAs(damage));
            Assert.That(handler.LastContext.GetAffectedRow(pet), Is.EqualTo(row));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void Discovery_UsesCurrentHandlerConditionWithoutCachingRejection()
        {
            var e = new Environment();
            var damage = e.Damage(1);
            var handler = new Probe(CombatSide.Player, e.Pet(CombatSide.Player, BoardRow.Front).InstanceId)
                { Allowed = false };
            var source = new CombatPetDamageTriggerSource(handler);
            Assert.That(Discover(source, e.State, damage), Is.Empty);
            handler.Allowed = true;
            Assert.That(Discover(source, e.State, damage).Count, Is.EqualTo(1));
            Assert.That(handler.ResolveCount, Is.Zero);
        }

        [Test]
        public void Discovery_RejectsWrongEventAndValidatesNullArguments()
        {
            var e = new Environment();
            var handler = new Probe(CombatSide.Player, e.Pet(CombatSide.Player, BoardRow.Front).InstanceId);
            var source = new CombatPetDamageTriggerSource(handler);
            Assert.That(Discover(source, e.State, e.Root), Is.Empty);
            Assert.Throws<ArgumentNullException>(() => Discover(source, null, e.Root));
            Assert.Throws<ArgumentNullException>(() => Discover(source, e.State, null));
            Assert.That(handler.CanCount, Is.Zero);
            Assert.That(handler.ResolveCount, Is.Zero);
        }

        [Test]
        public void Discovery_DoesNotIntroducePositiveDamageFilter()
        {
            var e = new Environment();
            var damage = e.Damage(0);
            var handler = new Probe(CombatSide.Enemy, e.Pet(CombatSide.Enemy, BoardRow.Back).InstanceId);
            var source = new CombatPetDamageTriggerSource(handler);
            Assert.That(Discover(source, e.State, damage).Count, Is.EqualTo(1));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(5));
            Assert.That(e.Target.Armor, Is.EqualTo(3));
        }

        private static List<CombatTriggerCandidate<ICombatTriggerHandler>> Discover(
            CombatPetDamageTriggerSource source, CombatState state, CombatEvent sourceEvent)
        {
            return new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                source.DiscoverTriggers(state, sourceEvent));
        }

        [Test]
        public void Engine_ResumesFailedPetWithoutRepeatingEarlierPetOrDamage()
        {
            var e = new Environment();
            e.Damage(4);
            var order = new List<InstanceId>();
            var upperPlayer = new Probe(CombatSide.Player, e.Pet(CombatSide.Player, BoardRow.Front).InstanceId) { Order = order };
            var upperEnemy = new Probe(CombatSide.Enemy, e.Pet(CombatSide.Enemy, BoardRow.Front).InstanceId) { Order = order, FailOnce = true };
            var lowerPlayer = new Probe(CombatSide.Player, e.Pet(CombatSide.Player, BoardRow.Back).InstanceId) { Order = order };
            var lowerEnemy = new Probe(CombatSide.Enemy, e.Pet(CombatSide.Enemy, BoardRow.Back).InstanceId) { Order = order };
            var queue = new CombatEventQueue(e.Log);
            // Registration is intentionally opposite to source priority.
            var engine = new CombatTriggerEngine(e.State, queue,
                e.Registry(lowerEnemy, lowerPlayer, upperEnemy, upperPlayer));
            Assert.Throws<InvalidOperationException>(() => engine.Drain(20, 20));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(order, Is.EqualTo(new[] { upperPlayer.PetInstanceId }));
            engine.Drain(20, 20);
            Assert.That(order, Is.EqualTo(new[] {
                upperPlayer.PetInstanceId, upperEnemy.PetInstanceId,
                lowerPlayer.PetInstanceId, lowerEnemy.PetInstanceId }));
            Assert.That(upperPlayer.ResolveCount, Is.EqualTo(1));
            Assert.That(upperEnemy.ResolveCount, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(engine.HasActiveBatch, Is.False);
            Assert.That(queue.PendingCount, Is.Zero);
        }

        private sealed class Probe : CombatPetDamageTriggerHandler
        {
            public bool Allowed = true;
            public bool ObserveTarget = true;
            public bool FailOnce;
            public int CanCount;
            public int ResolveCount;
            public int ObservedHp;
            public int ObservedArmor;
            public CombatPetDamageContext LastContext;
            public CombatPetState LastPet;
            public List<InstanceId> Order;
            public Probe(CombatSide side, InstanceId id) : base(side, id) { }
            protected override bool CanTriggerOnDamage(CombatPetDamageContext context, CombatPetState pet)
            {
                CanCount++; LastContext = context; LastPet = pet;
                return Allowed;
            }
            protected override void ResolveOnDamage(CombatPetDamageContext context, CombatPetState pet)
            {
                ResolveCount++; LastContext = context; LastPet = pet;
                if (FailOnce) { FailOnce = false; throw new InvalidOperationException("Test-only callback failure."); }
                if (ObserveTarget)
                {
                    var target = context.State.GetSide(context.SourceEvent.TargetPosition.Side)
                        .GetCardAt(context.SourceEvent.TargetPosition);
                    ObservedHp = target.CurrentHp; ObservedArmor = target.Armor;
                }
                if (Order != null) { Order.Add(pet.InstanceId); }
            }
        }

        private sealed class Environment
        {
            public readonly CombatState State;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatStartedCombatEvent Root;
            public readonly BoardPosition TargetPosition = new BoardPosition(CombatSide.Player, BoardRow.Front, new BoardColumn(1));
            public readonly BoardPosition SourcePosition = new BoardPosition(CombatSide.Enemy, BoardRow.Front, new BoardColumn(1));
            public readonly CombatCardState Target;
            private readonly CombatPetState[] _playerPets;
            private readonly CombatPetState[] _enemyPets;
            public Environment()
            {
                Target = Card(1, 3);
                var attacker = Card(2, 0);
                _playerPets = Pets(1000);
                _enemyPets = Pets(2000);
                State = new CombatState(
                    SideState(CombatSide.Player, Target, TargetPosition),
                    SideState(CombatSide.Enemy, attacker, SourcePosition),
                    new CombatSidePetState(CombatSide.Player, new CombatPetRegistry(_playerPets)),
                    new CombatSidePetState(CombatSide.Enemy, new CombatPetRegistry(_enemyPets)));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
            }
            public CombatPetState Pet(CombatSide side, BoardRow row) =>
                (side == CombatSide.Player ? _playerPets : _enemyPets)[row == BoardRow.Front ? 0 : 1];
            public DamageAppliedCombatEvent Damage(int amount) => new CombatDamageResolver(Metadata, Log)
                .ApplyResolvedCardDamage(State, Root, SourcePosition, TargetPosition, amount);
            public CombatTriggerSourceRegistry Registry(params Probe[] handlers)
            {
                var sources = new List<ICombatTriggerSource>();
                foreach (var handler in handlers)
                { sources.Add(new CombatPetDamageTriggerSource(handler)); }
                return new CombatTriggerSourceRegistry(sources);
            }
            private static CombatPetState[] Pets(long prefix) => new[] {
                new CombatPetState(new DefinitionId("test.damage_pet"), new InstanceId(prefix + 2)),
                new CombatPetState(new DefinitionId("test.damage_pet"), new InstanceId(prefix + 1)) };
            private static CombatCardState Card(long id, int armor) => new CombatCardState(
                new DefinitionId("test.damage_card"), new InstanceId(id), new CardRank(4),
                CombatCardSeason.Spring, 10, 5, armor, 2);
            private static CombatSideState SideState(CombatSide side, CombatCardState card, BoardPosition position) =>
                new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(card.InstanceId.Value), position, card.InstanceId) }),
                    new CombatCardRegistry(new[] { card }), new BattleHealth(20), new AttackMultiplier(1));
        }
    }
}
