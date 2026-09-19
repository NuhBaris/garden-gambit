using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class PhoenixPetTriggerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FriendlyRowDeath_RescuesToOneAndCommitsPetOnce(
            CombatSide side, BoardRow row)
        {
            var e = new Environment(side, row);
            var death = e.Death(e.TargetPosition, damage: 8);
            var source = e.Source(row);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.Pet(side, row).InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.RescueResolver, Is.SameAs(e.Rescue));
            var candidates = Discover(source, e.State, death);
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            candidates[0].Trigger.Resolve(e.State, death);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(e.Target.IsAtDeathThreshold, Is.False);
            Assert.That(e.Usage.HasTriggered(e.Pet(side, row).InstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
            var rescue = e.Log.Events[3] as RescueCombatEvent;
            Assert.That(rescue, Is.Not.Null);
            Assert.That(rescue.InstanceId, Is.EqualTo(e.Target.InstanceId));
            Assert.That(rescue.Position, Is.EqualTo(e.TargetPosition));
            Assert.That(rescue.PreviousHp, Is.EqualTo(-3));
            Assert.That(rescue.CurrentHp, Is.EqualTo(1));
            Assert.That(rescue.Metadata.ParentEventId, Is.EqualTo(death.Metadata.EventId));
            Assert.That(rescue.Metadata.TriggerRootId, Is.EqualTo(death.Metadata.TriggerRootId));
            source.Handler.Resolve(e.State, death);
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WrongRowOrSide_DoesNotRescueOrConsumeUsage(bool opposingSide)
        {
            var e = new Environment();
            var death = e.Death(e.TargetPosition, 5);
            PhoenixPetDeathTriggerHandler handler;
            if (opposingSide)
            {
                var pet = e.Pet(CombatSide.Enemy, BoardRow.Front);
                handler = new PhoenixPetDeathTriggerHandler(
                    CombatSide.Enemy, pet.InstanceId, e.Usage, e.Rescue);
            }
            else
            {
                handler = e.Source(BoardRow.Back).Handler;
            }
            Assert.That(handler.CanTrigger(e.State, death), Is.False);
            handler.Resolve(e.State, death);
            Assert.That(e.Target.CurrentHp, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(3));
        }

        [Test]
        public void OncePerPet_AppliesAcrossDifferentFriendlyCardsAndRebuiltSource()
        {
            var e = new Environment();
            var first = e.Death(e.TargetPosition, 5);
            e.Source(e.Row).Handler.Resolve(e.State, first);
            var second = e.Death(e.SecondPosition, 5);
            var rebuilt = e.Source(e.Row);
            Assert.That(rebuilt.Handler.CanTrigger(e.State, second), Is.False);
            rebuilt.Handler.Resolve(e.State, second);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(e.Second.CurrentHp, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(6));
        }

        [Test]
        public void RescueCompletedBeforePhoenixTurn_DoesNotConsumePhoenixUsage()
        {
            var e = new Environment();
            var death = e.Death(e.TargetPosition, 5);
            e.Rescue.ApplyRescue(e.State, death);
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, death), Is.False);
            handler.Resolve(e.State, death);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [Test]
        public void DirectDeletedDeathTarget_CannotBeRescued()
        {
            var e = new Environment();
            var death = e.Death(e.TargetPosition, 5);
            Assert.That(e.Source(e.Row).Handler.CanTrigger(e.State, death), Is.True);
            new CombatDirectDeleteResolver(e.Metadata, e.Log).ApplyDirectDelete(
                e.State, death, e.TargetPosition);
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, death), Is.False);
            handler.Resolve(e.State, death);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.State.GetSide(e.Side).Cards.Count, Is.EqualTo(1));
            Assert.That(e.Log.Events[e.Log.Count - 1], Is.TypeOf<DirectDeleteCombatEvent>());
        }

        [Test]
        public void TargetMovedAfterDeath_IsNotRescuedAtOldOrNewPosition()
        {
            var e = new Environment();
            var death = e.Death(e.TargetPosition, 5);
            var destination = e.Position(e.Row, 3);
            e.State.GetSide(e.Side).MoveCard(e.TargetPosition, destination);
            var handler = e.Source(e.Row).Handler;
            Assert.That(handler.CanTrigger(e.State, death), Is.False);
            handler.Resolve(e.State, death);
            Assert.That(e.Target.CurrentHp, Is.Zero);
            Assert.That(e.State.GetSide(e.Side).GetCardAt(destination), Is.SameAs(e.Target));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void UnloggedDeathCopy_FailsWithoutConsumingUsageAndOriginalCanRetry()
        {
            var e = new Environment();
            var death = e.Death(e.TargetPosition, 5);
            var copy = new DeathCombatEvent(death.Metadata, death.InstanceId,
                death.Position, death.PreviousHp, death.CurrentHp);
            var handler = e.Source(e.Row).Handler;
            Assert.Throws<ArgumentException>(() => handler.Resolve(e.State, copy));
            Assert.That(e.Target.CurrentHp, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            handler.Resolve(e.State, death);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void QueuedIndependentDeathTrigger_ContinuesAfterPhoenixRescue()
        {
            var e = new Environment();
            e.Death(e.TargetPosition, 5);
            var observerPet = e.Pet(e.Side, BoardRow.Back);
            var observer = new DeathObserver(e.Side, observerPet.InstanceId, e.TargetPosition);
            var sources = new CombatTriggerSourceRegistry(new ICombatTriggerSource[]
            {
                new CombatPetDeathTriggerSource(observer),
                e.Source(BoardRow.Front)
            });
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, sources);
            engine.Drain(30, 30);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(1));
            Assert.That(observer.ResolveCount, Is.EqualTo(1));
            Assert.That(observer.ObservedHp, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
            Assert.That(queue.PendingCount, Is.Zero);
        }

        [Test]
        public void NonDeathEvent_DoesNotCreateCandidate()
        {
            var e = new Environment();
            Assert.That(e.Source(e.Row).DiscoverTriggers(e.State, e.Root), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void ConstructorsAndFactory_ValidateDependenciesAndRegistration()
        {
            var e = new Environment();
            var pet = e.Pet(e.Side, e.Row);
            Assert.Throws<ArgumentNullException>(() =>
                new PhoenixPetDeathTriggerHandler(e.Side, pet.InstanceId, null, e.Rescue));
            Assert.Throws<ArgumentNullException>(() =>
                new PhoenixPetDeathTriggerHandler(e.Side, pet.InstanceId, e.Usage, null));
            Assert.Throws<ArgumentException>(() =>
                new PhoenixPetTriggerSourceFactory(default(DefinitionId), e.Usage, e.Rescue));
            var factory = e.Factory();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new List<ICombatTriggerSource>(factory.CreateSources((CombatSide)99, pet)));
            Assert.Throws<ArgumentNullException>(() =>
                new List<ICombatTriggerSource>(factory.CreateSources(e.Side, null)));
            var other = new CombatPetState(new DefinitionId("test.other"), pet.InstanceId);
            Assert.Throws<ArgumentException>(() =>
                new List<ICombatTriggerSource>(factory.CreateSources(e.Side, other)));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(factory.RescueResolver, Is.SameAs(e.Rescue));
        }

        private static List<CombatTriggerCandidate<ICombatTriggerHandler>> Discover(
            PhoenixPetTriggerSource source, CombatState state, CombatEvent sourceEvent) =>
            new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                source.DiscoverTriggers(state, sourceEvent));

        private sealed class DeathObserver : CombatPetDeathTriggerHandler
        {
            private readonly BoardPosition _position;
            public DeathObserver(CombatSide side, InstanceId petId, BoardPosition position)
                : base(side, petId) { _position = position; }
            public int ResolveCount { get; private set; }
            public int ObservedHp { get; private set; }
            protected override bool CanTriggerOnDeath(
                CombatPetDeathContext context, CombatPetState pet) => true;
            protected override void ResolveOnDeath(
                CombatPetDeathContext context, CombatPetState pet)
            {
                ResolveCount++;
                ObservedHp = context.State.GetSide(Side).GetCardAt(_position).CurrentHp;
            }
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatCardState Second;
            public readonly BoardPosition SourcePosition;
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(new CombatPetTriggerUsageRegistry());
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatRescueResolver Rescue;
            private readonly CombatPetState[] _playerPets;
            private readonly CombatPetState[] _enemyPets;

            public Environment(CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                Side = side;
                Row = row;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                SourcePosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                Target = Card(1);
                Second = Card(3);
                var attacker = Card(2);
                var otherRow = row == BoardRow.Front ? BoardRow.Back : BoardRow.Front;
                var own = new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(1), Position(row, 1), Target.InstanceId),
                    new CombatSlotState(new SlotId(3), Position(row, 2), Second.InstanceId),
                    new CombatSlotState(new SlotId(4), Position(row, 3)),
                    new CombatSlotState(new SlotId(5), Position(otherRow, 1)) }),
                    new CombatCardRegistry(new[] { Target, Second }),
                    new BattleHealth(20), new AttackMultiplier(1));
                var opposing = new CombatSideState(new CombatBoardState(other, new[] {
                    new CombatSlotState(new SlotId(2), SourcePosition, attacker.InstanceId) }),
                    new CombatCardRegistry(new[] { attacker }),
                    new BattleHealth(20), new AttackMultiplier(1));
                _playerPets = Pets(1000);
                _enemyPets = Pets(2000);
                State = new CombatState(side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Player ? opposing : own,
                    new CombatSidePetState(CombatSide.Player, new CombatPetRegistry(_playerPets)),
                    new CombatSidePetState(CombatSide.Enemy, new CombatPetRegistry(_enemyPets)));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Rescue = new CombatRescueResolver(Metadata, Log);
            }

            public BoardPosition TargetPosition => Position(Row, 1);
            public BoardPosition SecondPosition => Position(Row, 2);
            public BoardPosition Position(BoardRow row, int column) =>
                new BoardPosition(Side, row, new BoardColumn(column));

            public CombatPetState Pet(CombatSide side, BoardRow row) =>
                (side == CombatSide.Player ? _playerPets : _enemyPets)
                    [row == BoardRow.Front ? 0 : 1];

            public PhoenixPetTriggerSourceFactory Factory() =>
                new PhoenixPetTriggerSourceFactory(
                    Pet(Side, Row).DefinitionId, Usage, Rescue);

            public PhoenixPetTriggerSource Source(BoardRow row)
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory().CreateSources(Side, Pet(Side, row)));
                Assert.That(sources.Count, Is.EqualTo(1));
                return (PhoenixPetTriggerSource)sources[0];
            }

            public DeathCombatEvent Death(BoardPosition targetPosition, int damage)
            {
                var applied = new CombatDamageResolver(Metadata, Log)
                    .ApplyResolvedCardDamage(State, Root, SourcePosition, targetPosition, damage);
                var death = new CombatDeathEventResolver(Metadata, Log).AppendFromDamage(applied);
                Assert.That(death, Is.Not.Null);
                return death;
            }

            private static CombatPetState[] Pets(long prefix) => new[] {
                new CombatPetState(new DefinitionId("test.phoenix"), new InstanceId(prefix + 2)),
                new CombatPetState(new DefinitionId("test.phoenix"), new InstanceId(prefix + 1)) };

            private static CombatCardState Card(long id) =>
                new CombatCardState(new DefinitionId("test.phoenix_card"),
                    new InstanceId(id), new CardRank(4), CombatCardSeason.Spring,
                    10, 5, 0, 2);
        }
    }
}
