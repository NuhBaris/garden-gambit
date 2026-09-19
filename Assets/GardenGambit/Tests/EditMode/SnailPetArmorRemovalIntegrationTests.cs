using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class SnailPetArmorRemovalIntegrationTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void RuntimeSources_ResolveRemovalWithCorrectPetAndParent(CombatSide side, BoardRow row)
        {
            var e = new Environment(side, row);
            var removed = e.Remove(3);
            var sources = e.Sources();
            Assert.That(sources.Count, Is.EqualTo(4));
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                sources.DiscoverTriggers(e.State, removed));
            Assert.That(candidates.Count, Is.EqualTo(1));
            var handler = candidates[0].Trigger as SnailPetArmorRemovalTriggerHandler;
            Assert.That(handler, Is.Not.Null);
            Assert.That(handler.PetInstanceId, Is.EqualTo(e.Pet.InstanceId));
            Assert.That(handler.UsageCommitter, Is.SameAs(e.Usage));
            e.Drain();
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            var gain = (ArmorGainCombatEvent)e.Log.Events[2];
            Assert.That(gain.TargetInstanceId, Is.EqualTo(e.Target.InstanceId));
            Assert.That(gain.ActualGainedAmount, Is.EqualTo(2));
            Assert.That(gain.Metadata.ParentEventId, Is.EqualTo(removed.Metadata.EventId));
            Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(removed.Metadata.TriggerRootId));
            Assert.That(e.Sources().DiscoverTriggers(e.State, removed), Is.Empty);
            Assert.That(e.Log.Count, Is.EqualTo(3));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DamageAndRemoval_ShareSinglePetUsageInBothOrders(bool removalFirst)
        {
            var e = new Environment();
            if (removalFirst) { e.Remove(3); } else { e.Damage(3); }
            e.Drain();
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            if (removalFirst) { e.Damage(2); } else { e.Remove(2); }
            e.Drain();
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Target.CurrentHp, Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [Test]
        public void PartialRemoval_DoesNotUseOpportunityBeforeActualDepletion()
        {
            var e = new Environment();
            e.Remove(1); e.Drain();
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            e.Remove(2); e.Drain();
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [Test]
        public void Handler_ValidatesDependenciesAndSourcePreservesSharedReferences()
        {
            var e = new Environment();
            Assert.Throws<ArgumentNullException>(() => new SnailPetArmorRemovalTriggerHandler(e.Side, e.Pet.InstanceId, null, e.Armor));
            Assert.Throws<ArgumentNullException>(() => new SnailPetArmorRemovalTriggerHandler(e.Side, e.Pet.InstanceId, e.Usage, null));
            var source = e.Source();
            Assert.That(source.Handler.UsageCommitter, Is.SameAs(source.ArmorRemovalHandler.UsageCommitter));
            Assert.That(source.Handler.ArmorGainResolver, Is.SameAs(source.ArmorRemovalHandler.ArmorGainResolver));
            Assert.That(source.ArmorRemovalHandler.CanTrigger(e.State, e.Root), Is.False);
            Assert.Throws<ArgumentException>(() => source.ArmorRemovalHandler.Resolve(e.State, e.Root));
        }

        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        public void WrongSideOrRow_CannotClaimRemoval(CombatSide side, BoardRow row)
        {
            var e = new Environment();
            var removed = e.Remove(3);
            var handler = e.Source(side, row).ArmorRemovalHandler;
            Assert.That(handler.CanTrigger(e.State, removed), Is.False);
            handler.Resolve(e.State, removed);
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void RemovedTarget_DoesNotGiveBonusToReplacement()
        {
            var e = new Environment();
            var removed = e.Remove(3);
            e.State.GetSide(e.Side).RemoveCardFromCombat(e.TargetPosition);
            e.State.GetSide(e.Side).MoveCard(e.SecondPosition, e.TargetPosition);
            var handler = e.Source().ArmorRemovalHandler;
            Assert.That(handler.CanTrigger(e.State, removed), Is.False);
            handler.Resolve(e.State, removed);
            Assert.That(e.Second.Armor, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void MovedTargetWithinRow_IsResolvedByIdentity()
        {
            var e = new Environment();
            e.Remove(3);
            var destination = e.Position(e.Row, 3);
            e.State.GetSide(e.Side).MoveCard(e.TargetPosition, destination);
            e.State.GetSide(e.Side).MoveCard(e.SecondPosition, e.TargetPosition);
            e.Drain();
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Second.Armor, Is.EqualTo(3));
            Assert.That(((ArmorGainCombatEvent)e.Log.Events[2]).TargetPosition, Is.EqualTo(destination));
        }

        [Test]
        public void Resolve_RechecksRowAfterDiscovery()
        {
            var e = new Environment();
            var removed = e.Remove(3);
            var handler = e.Source().ArmorRemovalHandler;
            Assert.That(handler.CanTrigger(e.State, removed), Is.True);
            e.State.GetSide(e.Side).MoveCard(e.TargetPosition, e.Position(BoardRow.Back, 1));
            handler.Resolve(e.State, removed);
            Assert.That(e.Target.Armor, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void ThresholdTarget_ReceivesArmorWithoutHeal()
        {
            var e = new Environment();
            e.Target.SetCurrentHpToZero();
            e.Remove(3); e.Drain();
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Target.CurrentHp, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void RestoredArmor_DoesNotEraseRecordedDepletion()
        {
            var e = new Environment();
            var removed = e.Remove(3);
            e.Target.ApplyArmorGain(5);
            e.Drain();
            Assert.That(removed.CurrentArmor, Is.Zero);
            Assert.That(e.Target.Armor, Is.EqualTo(7));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void Overflow_ResumesWithSharedUsageUnconsumedUntilSuccess()
        {
            var e = new Environment();
            e.Remove(3);
            e.Target.ApplyArmorGain(int.MaxValue);
            var engine = new CombatTriggerEngine(e.State, e.Queue, e.Sources());
            Assert.Throws<OverflowException>(() => engine.Drain(40, 40));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Target.Armor, Is.EqualTo(int.MaxValue));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            e.Target.RemoveArmor(int.MaxValue);
            engine.Drain(40, 40);
            Assert.That(engine.HasActiveBatch, Is.False);
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(engine.Drain(40, 40), Is.Zero);
        }

        [Test]
        public void DuplicatePets_KeepIndependentUsageAcrossDifferentEventTypes()
        {
            var e = new Environment();
            var lower = e.Position(BoardRow.Back, 1);
            e.State.GetSide(e.Side).MoveCard(e.SecondPosition, lower);
            e.Remove(3); e.Drain();
            Assert.That(e.Usage.HasTriggered(e.PetId(e.Side, BoardRow.Back)), Is.False);
            e.DamageAt(lower, 3); e.Drain();
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Target.Armor, Is.EqualTo(2));
            Assert.That(e.Second.Armor, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(5));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatCardState Second;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatEventQueue Queue;
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public CombatPetTriggerUsageCommitter Usage => Runtime.PetUsageCommitter;
            public readonly CombatArmorGainResolver Armor;
            public BoardPosition TargetPosition => Position(Row, 1);
            public BoardPosition SecondPosition => Position(Row, 2);
            private readonly BoardPosition _sourcePosition;

            public Environment(CombatSide side = CombatSide.Player, BoardRow row = BoardRow.Front)
            {
                Side = side; Row = row;
                var other = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Target = Card(1); Second = Card(3);
                var attacker = Card(2);
                _sourcePosition = new BoardPosition(other, BoardRow.Front, new BoardColumn(1));
                var own = new CombatSideState(new CombatBoardState(side, new[] {
                    new CombatSlotState(new SlotId(1), TargetPosition, Target.InstanceId),
                    new CombatSlotState(new SlotId(3), SecondPosition, Second.InstanceId),
                    new CombatSlotState(new SlotId(4), Position(row, 3)),
                    new CombatSlotState(new SlotId(5), Position(row == BoardRow.Front ? BoardRow.Back : BoardRow.Front, 1)) }),
                    new CombatCardRegistry(new[] { Target, Second }), new BattleHealth(20), new AttackMultiplier(1));
                var opposing = new CombatSideState(new CombatBoardState(other, new[] {
                    new CombatSlotState(new SlotId(2), _sourcePosition, attacker.InstanceId) }),
                    new CombatCardRegistry(new[] { attacker }), new BattleHealth(20), new AttackMultiplier(1));
                var playerPets = Pets(CombatSide.Player);
                var enemyPets = Pets(CombatSide.Enemy);
                Pet = (side == CombatSide.Player ? playerPets : enemyPets)[row == BoardRow.Front ? 0 : 1];
                State = new CombatState(side == CombatSide.Player ? own : opposing,
                    side == CombatSide.Player ? opposing : own,
                    new CombatSidePetState(CombatSide.Player, new CombatPetRegistry(playerPets)),
                    new CombatSidePetState(CombatSide.Enemy, new CombatPetRegistry(enemyPets)));
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot()); Log.Append(Root);
                Queue = new CombatEventQueue(Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
            }
            public BoardPosition Position(BoardRow row, int column) => new BoardPosition(Side, row, new BoardColumn(column));
            public InstanceId PetId(CombatSide side, BoardRow row) =>
                new InstanceId((side == CombatSide.Player ? 1000 : 2000) + (row == BoardRow.Front ? 2 : 1));
            private CombatPetState[] Pets(CombatSide side) => new[] {
                new CombatPetState(CombatPetDefinitionIds.Snail, PetId(side, BoardRow.Front)),
                new CombatPetState(CombatPetDefinitionIds.Snail, PetId(side, BoardRow.Back)) };
            private static CombatCardState Card(long id) => new CombatCardState(
                new DefinitionId("test.snail_card"), new InstanceId(id), new CardRank(4), CombatCardSeason.Spring, 10, 5, 3, 2);
            public DamageAppliedCombatEvent Damage(int amount) => DamageAt(TargetPosition, amount);
            public DamageAppliedCombatEvent DamageAt(BoardPosition position, int amount) => new CombatDamageResolver(Metadata, Log)
                .ApplyResolvedCardDamage(State, Root, _sourcePosition, position, amount);
            public ArmorRemovedCombatEvent Remove(int amount) => new CombatArmorRemovalResolver(Metadata, Log)
                .TryRemoveArmor(State, Root, new InstanceId(2), TargetPosition, amount);
            public CombatTriggerSourceRegistry Sources() => Runtime.BuildSourceRegistry(
                State, Armor, new CombatAttackGainResolver(Metadata, Log), new CombatCardLookup(Log));
            public SnailPetTriggerSource Source(CombatSide side, BoardRow row) =>
                new SnailPetTriggerSource(side, PetId(side, row), Usage, Armor);
            public SnailPetTriggerSource Source() => Source(Side, Row);
            public void Drain() => new CombatTriggerEngine(State, Queue, Sources()).Drain(40, 40);
        }
    }
}
