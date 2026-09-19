using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatArmorRemovalInfrastructureTests
    {
        [Test]
        public void EventKind_PreservesExistingStatEventNumbers()
        {
            Assert.That((int)CombatEventKind.ArmorGain, Is.EqualTo(19));
            Assert.That((int)CombatEventKind.AttackGain, Is.EqualTo(20));
            Assert.That((int)CombatEventKind.ArmorRemoved, Is.EqualTo(21));
        }

        [TestCase(5, 2, false)]
        [TestCase(5, 0, true)]
        public void Event_RecordsActualDecreaseAndDepletion(int previous, int current, bool depleted)
        {
            var e = new Environment();
            var metadata = e.Metadata.CreateChild(e.Root.Metadata);
            var ev = new ArmorRemovedCombatEvent(metadata, e.SourceId, e.Card.InstanceId, e.Position, previous, current);
            Assert.That(ev.Kind, Is.EqualTo(CombatEventKind.ArmorRemoved));
            Assert.That(ev.Metadata, Is.EqualTo(metadata));
            Assert.That(ev.SourceInstanceId, Is.EqualTo(e.SourceId));
            Assert.That(ev.TargetInstanceId, Is.EqualTo(e.Card.InstanceId));
            Assert.That(ev.TargetPosition, Is.EqualTo(e.Position));
            Assert.That(ev.TargetSide, Is.EqualTo(CombatSide.Player));
            Assert.That(ev.PreviousArmor, Is.EqualTo(previous));
            Assert.That(ev.CurrentArmor, Is.EqualTo(current));
            Assert.That(ev.ActualRemovedAmount, Is.EqualTo(previous - current));
            Assert.That(ev.DepletedArmor, Is.EqualTo(depleted));
        }

        [TestCase(0, 0)]
        [TestCase(2, 2)]
        [TestCase(2, 3)]
        [TestCase(-1, 0)]
        [TestCase(3, -1)]
        public void Event_RejectsNonDecreasingOrNegativeArmor(int previous, int current)
        {
            var e = new Environment();
            Assert.Catch<ArgumentException>(() => new ArmorRemovedCombatEvent(
                e.Metadata.CreateChild(e.Root.Metadata), e.SourceId, e.Card.InstanceId, e.Position, previous, current));
        }

        [Test]
        public void Event_ValidatesMetadataIdentityAndPosition()
        {
            var e = new Environment();
            var child = e.Metadata.CreateChild(e.Root.Metadata);
            Assert.Throws<ArgumentException>(() => new ArmorRemovedCombatEvent(e.Root.Metadata, e.SourceId, e.Card.InstanceId, e.Position, 2, 1));
            Assert.Throws<ArgumentException>(() => new ArmorRemovedCombatEvent(default(CombatEventMetadata), e.SourceId, e.Card.InstanceId, e.Position, 2, 1));
            Assert.Throws<ArgumentException>(() => new ArmorRemovedCombatEvent(child, default(InstanceId), e.Card.InstanceId, e.Position, 2, 1));
            Assert.Throws<ArgumentException>(() => new ArmorRemovedCombatEvent(child, e.SourceId, default(InstanceId), e.Position, 2, 1));
            Assert.Throws<ArgumentException>(() => new ArmorRemovedCombatEvent(child, e.SourceId, e.Card.InstanceId, default(BoardPosition), 2, 1));
        }

        [TestCase(CombatSide.Player, 1, 4)]
        [TestCase(CombatSide.Enemy, 1, 4)]
        [TestCase(CombatSide.Player, 5, 0)]
        [TestCase(CombatSide.Enemy, int.MaxValue, 0)]
        public void Resolver_RemovesAtMostAvailableArmorAndLogsChild(CombatSide side, int amount, int remaining)
        {
            var e = new Environment(side);
            var ev = e.Remove(amount);
            Assert.That(e.Card.Armor, Is.EqualTo(remaining));
            Assert.That(e.Card.CurrentHp, Is.EqualTo(7));
            Assert.That(e.Card.HpCapacity, Is.EqualTo(10));
            Assert.That(e.Card.Attack, Is.EqualTo(2));
            Assert.That(ev.SourceInstanceId, Is.EqualTo(e.SourceId));
            Assert.That(ev.TargetInstanceId, Is.EqualTo(e.Card.InstanceId));
            Assert.That(ev.TargetPosition, Is.EqualTo(e.Position));
            Assert.That(ev.PreviousArmor, Is.EqualTo(5));
            Assert.That(ev.CurrentArmor, Is.EqualTo(remaining));
            Assert.That(ev.ActualRemovedAmount, Is.EqualTo(5 - remaining));
            Assert.That(ev.Metadata.ParentEventId, Is.EqualTo(e.Root.Metadata.EventId));
            Assert.That(ev.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.TriggerRootId));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Log.Events[1], Is.SameAs(ev));
        }

        [TestCase(5, 0)]
        [TestCase(0, 1)]
        [TestCase(0, int.MaxValue)]
        public void Resolver_NoActualRemovalDoesNotEmitOrAllocate(int armor, int amount)
        {
            var e = new Environment(armor: armor);
            Assert.That(e.Remove(amount), Is.Null);
            Assert.That(e.Card.Armor, Is.EqualTo(armor));
            Assert.That(e.Log.Count, Is.EqualTo(1));
            e.Card.ApplyArmorGain(1);
            var actual = e.Remove(1);
            Assert.That(actual.Metadata.EventId.Value, Is.EqualTo(2));
            Assert.That(actual.Metadata.SequenceNo.Value, Is.EqualTo(2));
        }

        [Test]
        public void Resolver_ValidatesDependenciesAndRequestsBeforeMutation()
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new CombatArmorRemovalResolver(null, e.Log));
            Assert.Throws<ArgumentNullException>(() => new CombatArmorRemovalResolver(e.Metadata, null));
            Assert.Throws<ArgumentNullException>(() => e.Resolver.TryRemoveArmor(null, e.Root, e.SourceId, e.Position, 1));
            Assert.Throws<ArgumentNullException>(() => e.Resolver.TryRemoveArmor(e.State, null, e.SourceId, e.Position, 1));
            Assert.Throws<ArgumentException>(() => e.Resolver.TryRemoveArmor(e.State, e.Root, default(InstanceId), e.Position, 1));
            Assert.Throws<ArgumentException>(() => e.Resolver.TryRemoveArmor(e.State, e.Root, e.SourceId, default(BoardPosition), 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => e.Remove(-1));
            Assert.That(e.Card.Armor, Is.EqualTo(5));
            Assert.That(e.Log.Count, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Resolver_RejectsUnloggedOrCopiedParentEvenForZeroRequest(bool copy)
        {
            var e = new Environment();
            var metadata = copy ? e.Root.Metadata : RootMetadata(100);
            var parent = new CombatStartedCombatEvent(metadata);
            Assert.Throws<ArgumentException>(() => e.Resolver.TryRemoveArmor(e.State, parent, e.SourceId, e.Position, 0));
            Assert.That(e.Card.Armor, Is.EqualTo(5));
            Assert.That(e.Log.Count, Is.EqualTo(1));
        }

        [Test]
        public void Resolver_MissingOccupantIsRejectedEvenForZeroRequest()
        {
            var e = new Environment();
            e.State.GetSide(e.Side).RemoveCardFromCombat(e.Position);
            Assert.Throws<InvalidOperationException>(() => e.Remove(0));
            Assert.That(e.Card.Armor, Is.EqualTo(5));
            Assert.That(e.Log.Count, Is.EqualTo(1));
        }

        [Test]
        public void Resolver_LogRejectionLeavesArmorAndLogUnchanged()
        {
            var e = new Environment();
            // The shared factory is now behind the log's latest sequence,
            // although it can still allocate a child of the original root.
            e.Log.Append(new CombatStartedCombatEvent(RootMetadata(100)));
            Assert.Throws<ArgumentException>(() => e.Remove(1));
            Assert.That(e.Card.Armor, Is.EqualTo(5));
            Assert.That(e.Card.CurrentHp, Is.EqualTo(7));
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void Resolver_ThresholdCardCanLoseArmorWithoutDamageOrDeathEmission()
        {
            var e = new Environment();
            e.Card.SetCurrentHpToZero();
            e.Remove(int.MaxValue);
            Assert.That(e.Card.Armor, Is.Zero);
            Assert.That(e.Card.CurrentHp, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.State.GetSide(e.Side).GetCardAt(e.Position), Is.SameAs(e.Card));
            Assert.That(e.Log.Events[1], Is.TypeOf<ArmorRemovedCombatEvent>());
        }

        [Test]
        public void Resolver_SupportsSelfSourceAndKeepsEventSnapshotAfterLaterGain()
        {
            var e = new Environment();
            var ev = e.Resolver.TryRemoveArmor(e.State, e.Root, e.Card.InstanceId, e.Position, 5);
            e.Card.ApplyArmorGain(3);
            Assert.That(ev.SourceInstanceId, Is.EqualTo(ev.TargetInstanceId));
            Assert.That(ev.PreviousArmor, Is.EqualTo(5));
            Assert.That(ev.CurrentArmor, Is.Zero);
            Assert.That(ev.DepletedArmor, Is.True);
            Assert.That(e.Card.Armor, Is.EqualTo(3));
        }

        [Test]
        public void Event_TravelsThroughQueueWithoutApplyingRemovalAgain()
        {
            var e = new Environment();
            e.Remove(2);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue,
                new CombatTriggerSourceRegistry(Array.Empty<ICombatTriggerSource>()));
            engine.Drain(20, 20);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(e.Card.Armor, Is.EqualTo(3));
            Assert.That(e.Card.CurrentHp, Is.EqualTo(7));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [Test]
        public void Resolver_RecordsNestedRemovalUnderItsImmediateParent()
        {
            var e = new Environment();
            var first = e.Remove(1);
            var second = e.Resolver.TryRemoveArmor(e.State, first, e.SourceId, e.Position, 2);
            Assert.That(second.Metadata.ParentEventId, Is.EqualTo(first.Metadata.EventId));
            Assert.That(second.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
            Assert.That(second.Metadata.SequenceNo.Value, Is.GreaterThan(first.Metadata.SequenceNo.Value));
            Assert.That(second.PreviousArmor, Is.EqualTo(4));
            Assert.That(second.CurrentArmor, Is.EqualTo(2));
            Assert.That(e.Card.Armor, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(3));
        }

        private static CombatEventMetadata RootMetadata(long value)
        {
            var id = new CombatEventId(value);
            return new CombatEventMetadata(id, new CombatSequenceNumber(value), null, id);
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState Card;
            public readonly BoardPosition Position;
            public readonly InstanceId SourceId = new InstanceId(2);
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatArmorRemovalResolver Resolver;

            public Environment(CombatSide side = CombatSide.Player, int armor = 5)
            {
                Side = side;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Position = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
                Card = new CombatCardState(new DefinitionId("test.armor_removal_target"), new InstanceId(1),
                    new CardRank(4), CombatCardSeason.Spring, 10, 7, armor, 2);
                var own = new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(1), Position, Card.InstanceId) }),
                    new CombatCardRegistry(new[] { Card }), new BattleHealth(20), new AttackMultiplier(1));
                var opposing = new CombatSideState(new CombatBoardState(other, Array.Empty<CombatSlotState>()),
                    new CombatCardRegistry(Array.Empty<CombatCardState>()), new BattleHealth(20), new AttackMultiplier(1));
                State = new CombatState(side == CombatSide.Player ? own : opposing, side == CombatSide.Enemy ? own : opposing);
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatArmorRemovalResolver(Metadata, Log);
            }
            public ArmorRemovedCombatEvent Remove(int amount) =>
                Resolver.TryRemoveArmor(State, Root, SourceId, Position, amount);
        }
    }
}
