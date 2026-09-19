using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatAttackGainBatchResolverTests
    {
        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Batch_PreservesRequestedOrderAndEventAncestry(CombatSide side)
        {
            var e = new Environment(side);
            var events = e.Resolver.TryApplyAttackGainBatch(
                e.State, e.Root, new[] { e.SecondPosition, e.FirstPosition }, 3);

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(e.First.Attack, Is.EqualTo(5));
            Assert.That(e.Second.Attack, Is.EqualTo(7));
            Assert.That(events[0].TargetInstanceId, Is.EqualTo(e.Second.InstanceId));
            Assert.That(events[1].TargetInstanceId, Is.EqualTo(e.First.InstanceId));
            Assert.That(events[0].PreviousAttack, Is.EqualTo(4));
            Assert.That(events[0].CurrentAttack, Is.EqualTo(7));
            Assert.That(events[1].PreviousAttack, Is.EqualTo(2));
            Assert.That(events[1].CurrentAttack, Is.EqualTo(5));
            for (var i = 0; i < events.Count; i++)
            {
                Assert.That(events[i].ActualGainedAmount, Is.EqualTo(3));
                Assert.That(events[i].Metadata.ParentEventId.Value, Is.EqualTo(e.Root.Metadata.EventId));
                Assert.That(events[i].Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.TriggerRootId));
                Assert.That(events[i].Metadata.EventId.Value, Is.EqualTo(i + 2));
                Assert.That(events[i].Metadata.SequenceNo.Value, Is.EqualTo(i + 2));
                Assert.That(e.Log.Events[i + 1], Is.SameAs(events[i]));
            }
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(e.First.CurrentHp, Is.EqualTo(3));
            Assert.That(e.Second.CurrentHp, Is.EqualTo(3));
            Assert.That(e.First.Armor, Is.EqualTo(1));
            Assert.That(e.Second.Armor, Is.EqualTo(1));
        }

        [Test]
        public void Batch_OverflowOnLastTarget_DoesNotPartiallyApplyAndCanRetry()
        {
            var e = new Environment(secondAttack: int.MaxValue);
            Assert.Throws<OverflowException>(() => e.Apply());
            e.AssertUnchanged(2, int.MaxValue);
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
            Assert.That(e.Sequences.LastIssuedValue, Is.EqualTo(1));

            e.Second.ReduceAttack(1);
            var events = e.Apply();
            Assert.That(e.First.Attack, Is.EqualTo(3));
            Assert.That(e.Second.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(events[0].Metadata.EventId.Value, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(3));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Batch_EmptyOrZero_DoesNotChangeOrAllocate(bool empty)
        {
            var e = new Environment();
            var result = e.Resolver.TryApplyAttackGainBatch(e.State, e.Root,
                empty ? Array.Empty<BoardPosition>() : e.Positions, empty ? 1 : 0);
            Assert.That(result, Is.Empty);
            e.AssertUnchanged();
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
            Assert.That(e.Sequences.LastIssuedValue, Is.EqualTo(1));
        }

        [Test]
        public void Batch_DuplicateTarget_RejectsBeforeAnyChangeOrAllocation()
        {
            var e = new Environment();
            Assert.Throws<ArgumentException>(() => e.Resolver.TryApplyAttackGainBatch(
                e.State, e.Root, new[] { e.FirstPosition, e.FirstPosition }, 1));
            e.AssertUnchanged();
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Batch_LastTargetEmpty_RejectsEvenForZeroAmount(int amount)
        {
            var e = new Environment();
            e.State.GetSide(e.SecondPosition.Side).RemoveCard(e.SecondPosition);
            Assert.Throws<InvalidOperationException>(() => e.Resolver.TryApplyAttackGainBatch(
                e.State, e.Root, e.Positions, amount));
            e.AssertUnchanged();
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
        }

        [Test]
        public void Batch_InvalidLastPosition_RejectsBeforeAnyChangeOrAllocation()
        {
            var e = new Environment();
            Assert.Throws<ArgumentException>(() => e.Resolver.TryApplyAttackGainBatch(
                e.State, e.Root, new[] { e.FirstPosition, default(BoardPosition) }, 1));
            e.AssertUnchanged();
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
        }

        [Test]
        public void Batch_InvalidArguments_RejectEvenWithEmptyTargets()
        {
            var e = new Environment();
            var empty = Array.Empty<BoardPosition>();
            Assert.Throws<ArgumentNullException>(() => e.Resolver.TryApplyAttackGainBatch(null, e.Root, empty, 1));
            Assert.Throws<ArgumentNullException>(() => e.Resolver.TryApplyAttackGainBatch(e.State, null, empty, 1));
            Assert.Throws<ArgumentNullException>(() => e.Resolver.TryApplyAttackGainBatch(e.State, e.Root, null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => e.Resolver.TryApplyAttackGainBatch(e.State, e.Root, empty, -1));
            var otherReference = new CombatStartedCombatEvent(e.Root.Metadata);
            Assert.Throws<ArgumentException>(() => e.Resolver.TryApplyAttackGainBatch(e.State, otherReference, empty, 1));
            var id = new CombatEventId(100);
            var unlogged = new CombatStartedCombatEvent(new CombatEventMetadata(
                id, new CombatSequenceNumber(100), null, id));
            Assert.Throws<ArgumentException>(() => e.Resolver.TryApplyAttackGainBatch(e.State, unlogged, empty, 1));
            e.AssertUnchanged();
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
            Assert.That(e.Sequences.LastIssuedValue, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Batch_MetadataExhaustionOnLastTarget_DoesNotChangeCardsOrLog(bool exhaustIds)
        {
            var e = new Environment();
            var factory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(exhaustIds ? long.MaxValue - 1 : 1),
                new CombatSequenceNumberAllocator(exhaustIds ? 1 : long.MaxValue - 1));
            var resolver = new CombatAttackGainResolver(factory, e.Log);
            Assert.Throws<InvalidOperationException>(() => resolver.TryApplyAttackGainBatch(
                e.State, e.Root, e.Positions, 1));
            e.AssertUnchanged();
        }

        [Test]
        public void Batch_LogRejectsLastCandidate_DoesNotAppendFirstOrChangeAnyCard()
        {
            var e = new Environment();
            var occupiedId = new CombatEventId(3);
            var otherRoot = new CombatStartedCombatEvent(new CombatEventMetadata(
                occupiedId, new CombatSequenceNumber(2), null, occupiedId));
            e.Log.Append(otherRoot);
            var factory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(1), new CombatSequenceNumberAllocator(2));
            var resolver = new CombatAttackGainResolver(factory, e.Log);

            Assert.Throws<ArgumentException>(() => resolver.TryApplyAttackGainBatch(
                e.State, e.Root, e.Positions, 1));
            Assert.That(e.First.Attack, Is.EqualTo(2));
            Assert.That(e.Second.Attack, Is.EqualTo(4));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Log.Events[1], Is.SameAs(otherRoot));
            Assert.That(e.Log.ContainsEvent(new CombatEventId(2)), Is.False);
        }

        [Test]
        public void Batch_ThrowingEnumeration_DoesNotApplyEarlierTarget()
        {
            var e = new Environment();
            Assert.Throws<InvalidOperationException>(() => e.Resolver.TryApplyAttackGainBatch(
                e.State, e.Root, ThrowAfterFirst(e.FirstPosition), 1));
            e.AssertUnchanged();
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(1));
        }

        [Test]
        public void Batch_ChildParentAndThresholdTarget_PreserveGeneralResolverContract()
        {
            var e = new Environment();
            var parent = e.Resolver.TryApplyAttackGain(e.State, e.Root, e.FirstPosition, 1);
            e.Second.SetCurrentHpToZero();
            var events = e.Resolver.TryApplyAttackGainBatch(e.State, parent, e.Positions, 1);
            Assert.That(e.First.Attack, Is.EqualTo(4));
            Assert.That(e.Second.Attack, Is.EqualTo(5));
            Assert.That(e.Second.CurrentHp, Is.Zero);
            foreach (var gain in events)
            {
                Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(parent.Metadata.EventId));
                Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
            }
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        private static IEnumerable<BoardPosition> ThrowAfterFirst(BoardPosition position)
        {
            yield return position;
            throw new InvalidOperationException("Test enumeration failed.");
        }

        private sealed class Environment
        {
            public readonly CombatState State;
            public readonly CombatCardState First;
            public readonly CombatCardState Second;
            public readonly BoardPosition FirstPosition;
            public readonly BoardPosition SecondPosition;
            public readonly BoardPosition[] Positions;
            public readonly CombatEventIdAllocator Ids = new CombatEventIdAllocator();
            public readonly CombatSequenceNumberAllocator Sequences = new CombatSequenceNumberAllocator();
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatAttackGainResolver Resolver;

            public Environment(CombatSide side = CombatSide.Player, int secondAttack = 4)
            {
                FirstPosition = new BoardPosition(side, BoardRow.Front, new BoardColumn(1));
                SecondPosition = new BoardPosition(side, BoardRow.Front, new BoardColumn(2));
                Positions = new[] { FirstPosition, SecondPosition };
                First = Card(1, 2);
                Second = Card(2, secondAttack);
                var owner = MakeSide(side, new[]
                {
                    new CombatSlotState(new SlotId(1), FirstPosition, First.InstanceId),
                    new CombatSlotState(new SlotId(2), SecondPosition, Second.InstanceId)
                }, new[] { First, Second });
                var other = MakeSide(side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player,
                    Array.Empty<CombatSlotState>(), Array.Empty<CombatCardState>());
                State = new CombatState(side == CombatSide.Player ? owner : other,
                    side == CombatSide.Enemy ? owner : other);
                var factory = new CombatEventMetadataFactory(Ids, Sequences);
                Root = new CombatStartedCombatEvent(factory.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAttackGainResolver(factory, Log);
            }

            public IReadOnlyList<AttackGainCombatEvent> Apply()
            {
                return Resolver.TryApplyAttackGainBatch(State, Root, Positions, 1);
            }

            public void AssertUnchanged(int firstAttack = 2, int secondAttack = 4)
            {
                Assert.That(First.Attack, Is.EqualTo(firstAttack));
                Assert.That(Second.Attack, Is.EqualTo(secondAttack));
                Assert.That(Log.Count, Is.EqualTo(1));
                Assert.That(Log.Events[0], Is.SameAs(Root));
            }

            private static CombatCardState Card(long id, int attack)
            {
                return new CombatCardState(new DefinitionId("test.batch_target"), new InstanceId(id),
                    new CardRank(2), CombatCardSeason.Autumn, 5, 3, 1, attack);
            }

            private static CombatSideState MakeSide(
                CombatSide side, CombatSlotState[] slots, CombatCardState[] cards)
            {
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
