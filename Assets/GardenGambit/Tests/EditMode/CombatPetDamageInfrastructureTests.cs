using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetDamageInfrastructureTests
    {
        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Context_PreservesEventAndUsesPetOwnersSide(CombatSide side)
        {
            var e = new Environment();
            var damage = e.Damage(4);
            var context = new CombatPetDamageContext(e.State, side, damage);
            Assert.That(context.State, Is.SameAs(e.State));
            Assert.That(context.SourceEvent, Is.SameAs(damage));
            Assert.That(context.Side, Is.EqualTo(side));
            Assert.That(context.SideState, Is.SameAs(e.State.GetSide(side)));
            Assert.That(context.OpposingSideState, Is.SameAs(e.State.GetOpposingSide(side)));
            Assert.That(context.SidePetState, Is.SameAs(e.State.GetPets(side)));
            Assert.That(context.SourceEvent.Result.PreviousArmor, Is.EqualTo(3));
            Assert.That(context.SourceEvent.Result.CurrentArmor, Is.Zero);
            Assert.That(context.SourceEvent.Result.PreviousHp, Is.EqualTo(5));
            Assert.That(context.SourceEvent.Result.CurrentHp, Is.EqualTo(4));
            // A later state change does not rewrite the event's recorded result.
            e.Target.Heal(1);
            Assert.That(context.SourceEvent.Result.CurrentHp, Is.EqualTo(4));
            Assert.That(context.State.Player.GetCardAt(e.TargetPosition).CurrentHp, Is.EqualTo(5));
        }

        [Test]
        public void Context_ValidatesArgumentsAndUnknownPet()
        {
            var e = new Environment();
            var damage = e.Damage(1);
            Assert.Throws<ArgumentNullException>(() => new CombatPetDamageContext(null, CombatSide.Player, damage));
            Assert.Throws<ArgumentNullException>(() => new CombatPetDamageContext(e.State, CombatSide.Player, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatPetDamageContext(e.State, (CombatSide)99, damage));
            var context = new CombatPetDamageContext(e.State, CombatSide.Player, damage);
            Assert.Throws<ArgumentNullException>(() => context.GetAffectedRow(null));
            Assert.Throws<ArgumentException>(() => context.GetAffectedRow(
                new CombatPetState(new DefinitionId("test.damage_pet"), new InstanceId(9999))));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Context_UsesRegisteredPetSlotForAffectedRow(CombatSide side, BoardRow row)
        {
            var e = new Environment();
            var context = new CombatPetDamageContext(e.State, side, e.Damage(1));
            Assert.That(context.GetAffectedRow(e.Pet(side, row)), Is.EqualTo(row));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Handler_ForwardsTypedContextAndRegisteredOwner(CombatSide side)
        {
            var e = new Environment();
            var damage = e.Damage(4);
            var pet = e.Pet(side, BoardRow.Back);
            var handler = new Probe(side, pet.InstanceId);
            Assert.That(handler.CanTrigger(e.State, damage), Is.True);
            Assert.That(handler.LastPet, Is.SameAs(pet));
            Assert.That(handler.LastContext.SourceEvent, Is.SameAs(damage));
            Assert.That(handler.LastContext.Side, Is.EqualTo(side));
            Assert.That(handler.ResolveCount, Is.Zero);
            handler.Resolve(e.State, damage);
            Assert.That(handler.ResolveCount, Is.EqualTo(1));
            Assert.That(handler.LastPet, Is.SameAs(pet));
            Assert.That(handler.LastContext.GetAffectedRow(pet), Is.EqualTo(BoardRow.Back));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(4));
        }

        [Test]
        public void Handler_FalseConditionPreventsDiscoveryButResolveDoesNotCallCanImplicitly()
        {
            var e = new Environment();
            var damage = e.Damage(4);
            var pet = e.Pet(CombatSide.Player, BoardRow.Front);
            var handler = new Probe(CombatSide.Player, pet.InstanceId) { Allowed = false };
            var source = new CombatPetTriggerSource(CombatSide.Player, pet.InstanceId, handler);
            Assert.That(source.DiscoverTriggers(e.State, damage), Is.Empty);
            Assert.That(handler.ResolveCount, Is.Zero);
            var canCount = handler.CanCount;
            // Matches the existing abstract handler contract. Concrete abilities
            // must recheck their own conditions inside their Resolve callback.
            handler.Resolve(e.State, damage);
            Assert.That(handler.CanCount, Is.EqualTo(canCount));
            Assert.That(handler.ResolveCount, Is.EqualTo(1));
        }

        [Test]
        public void Handler_RejectsWrongEventAndNullInputBeforeCallbacks()
        {
            var e = new Environment();
            var handler = new Probe(CombatSide.Player, e.Pet(CombatSide.Player, BoardRow.Front).InstanceId);
            Assert.That(handler.CanTrigger(e.State, e.Root), Is.False);
            Assert.Throws<ArgumentException>(() => handler.Resolve(e.State, e.Root));
            Assert.Throws<ArgumentNullException>(() => handler.CanTrigger(null, e.Root));
            Assert.Throws<ArgumentNullException>(() => handler.Resolve(e.State, null));
            Assert.That(handler.CanCount, Is.Zero);
            Assert.That(handler.ResolveCount, Is.Zero);
        }

        [TestCase(0, 3, 5)]
        [TestCase(1, 2, 5)]
        [TestCase(3, 0, 5)]
        [TestCase(4, 0, 4)]
        [TestCase(9, 0, -1)]
        public void Engine_ObservesAppliedDamageWithoutApplyingItAgain(int amount, int expectedArmor, int expectedHp)
        {
            var e = new Environment();
            var damage = e.Damage(amount);
            var handler = new Probe(CombatSide.Player, e.Pet(CombatSide.Player, BoardRow.Front).InstanceId);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, e.Registry(handler));
            engine.Drain(20, 20);
            Assert.That(handler.ResolveCount, Is.EqualTo(1));
            Assert.That(handler.LastContext.SourceEvent, Is.SameAs(damage));
            Assert.That(handler.ObservedHp, Is.EqualTo(expectedHp));
            Assert.That(handler.ObservedArmor, Is.EqualTo(expectedArmor));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(expectedHp));
            Assert.That(e.Target.Armor, Is.EqualTo(expectedArmor));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [Test]
        public void RemovedTarget_DoesNotPreventIndependentPetFromReceivingRecordedEvent()
        {
            var e = new Environment();
            var damage = e.Damage(9);
            e.State.Player.RemoveCardFromCombat(e.TargetPosition);
            var pet = e.Pet(CombatSide.Player, BoardRow.Front);
            var context = new CombatPetDamageContext(e.State, CombatSide.Player, damage);
            Assert.That(context.SourceEvent.TargetInstanceId, Is.EqualTo(e.Target.InstanceId));
            Assert.That(context.SourceEvent.Result.CurrentHp, Is.EqualTo(-1));
            Assert.That(context.GetAffectedRow(pet), Is.EqualTo(BoardRow.Front));
            var handler = new Probe(CombatSide.Player, pet.InstanceId) { ObserveTarget = false };
            Assert.That(handler.CanTrigger(e.State, damage), Is.True);
            handler.Resolve(e.State, damage);
            Assert.That(handler.ResolveCount, Is.EqualTo(1));
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
                { sources.Add(new CombatPetTriggerSource(handler.Side, handler.PetInstanceId, handler)); }
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
