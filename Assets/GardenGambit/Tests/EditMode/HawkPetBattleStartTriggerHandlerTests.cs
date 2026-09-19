using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HawkPetBattleStartTriggerHandlerTests
    {
        [Test]
        public void Constructor_ValidatesDependenciesAndPreservesReferences()
        {
            var e = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new HawkPetBattleStartTriggerHandler(
                    e.Side, e.Pet.InstanceId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new HawkPetBattleStartTriggerHandler(
                    e.Side, e.Pet.InstanceId, e.Usage, null));

            Assert.That(e.Handler.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Handler.AttackGainResolver, Is.SameAs(e.Attack));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void PetStart_BuffsOnlyTwoHighestAttackCards(
            CombatSide side,
            BoardRow row)
        {
            var e = new Environment(side, row);
            var source = e.StartEvent();

            Assert.That(e.Handler.CanTrigger(e.State, source), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);

            e.Handler.Resolve(e.State, source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(1));
            Assert.That(e.Card(2).Attack, Is.EqualTo(12));
            Assert.That(e.Card(3).Attack, Is.EqualTo(3));
            Assert.That(e.Card(4).Attack, Is.EqualTo(10));
            Assert.That(e.Card(5).Attack, Is.EqualTo(2));

            Assert.That(e.Card(2).CurrentHp, Is.EqualTo(5));
            Assert.That(e.Card(2).HpCapacity, Is.EqualTo(10));
            Assert.That(e.Card(2).Armor, Is.Zero);
            Assert.That(e.Card(2).Rank.Value, Is.EqualTo(4));

            var otherRow = row == BoardRow.Front
                ? BoardRow.Back
                : BoardRow.Front;

            Assert.That(
                e.State.GetSide(side).GetCardAt(
                    new BoardPosition(side, otherRow, new BoardColumn(2)))
                    .Attack,
                Is.EqualTo(9));

            var otherSide = side == CombatSide.Player
                ? CombatSide.Enemy
                : CombatSide.Player;

            Assert.That(
                e.State.GetSide(otherSide).GetCardAt(
                    new BoardPosition(otherSide, row, new BoardColumn(2)))
                    .Attack,
                Is.EqualTo(9));

            var gains = e.Gains();
            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(gains[0].TargetInstanceId, Is.EqualTo(e.Card(2).InstanceId));
            Assert.That(gains[1].TargetInstanceId, Is.EqualTo(e.Card(4).InstanceId));

            foreach (var gain in gains)
            {
                Assert.That(gain.ActualGainedAmount, Is.EqualTo(3));
                Assert.That(
                    gain.Metadata.ParentEventId.Value,
                    Is.EqualTo(source.Metadata.EventId));
                Assert.That(
                    gain.Metadata.TriggerRootId,
                    Is.EqualTo(source.Metadata.TriggerRootId));
            }

            Assert.That(
                gains[1].Metadata.SequenceNo.Value,
                Is.GreaterThan(gains[0].Metadata.SequenceNo.Value));

            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [Test]
        public void EqualAttack_SelectsLeftmostCardsDespiteUnsortedSlots()
        {
            var e = new Environment();

            for (var column = 1; column <= 5; column++)
            {
                var card = e.Card(column);
                card.ApplyAttackGain(9 - card.Attack);
            }

            e.Handler.Resolve(e.State, e.StartEvent());

            Assert.That(e.Card(1).Attack, Is.EqualTo(12));
            Assert.That(e.Card(2).Attack, Is.EqualTo(12));
            Assert.That(e.Card(3).Attack, Is.EqualTo(9));
            Assert.That(e.Card(4).Attack, Is.EqualTo(9));
            Assert.That(e.Card(5).Attack, Is.EqualTo(9));

            Assert.That(
                e.Gains()[0].TargetPosition.Column.Value,
                Is.EqualTo(1));
            Assert.That(
                e.Gains()[1].TargetPosition.Column.Value,
                Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void FewerThanTwoCards_BuffsAvailableTargetsAndCompletesUse(
            int remainingCards)
        {
            var e = new Environment();

            for (var column = remainingCards + 1; column <= 5; column++)
            {
                e.State.GetSide(e.Side).RemoveCardFromCombat(e.Position(column));
            }

            var source = e.StartEvent();
            e.Handler.Resolve(e.State, source);

            Assert.That(e.Gains().Count, Is.EqualTo(remainingCards));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);

            if (remainingCards >= 1)
            {
                Assert.That(e.Card(1).Attack, Is.EqualTo(4));
            }

            if (remainingCards == 2)
            {
                Assert.That(e.Card(2).Attack, Is.EqualTo(12));
            }

            var count = e.Log.Count;
            e.Handler.Resolve(e.State, source);
            Assert.That(e.Log.Count, Is.EqualTo(count));
        }

        [Test]
        public void SnapshotEvent_SelectsUsingCurrentAttackAtResolution()
        {
            var e = new Environment();
            var snapshot = new CombatBattleStartSnapshotResolver().Resolve(e.State);
            var source = new BattleStartStageStartedCombatEvent(
                e.Metadata.CreateChild(e.Root.Metadata),
                CombatBattleStartStage.Pet,
                snapshot);

            e.Log.Append(source);

            // Current combat stats change after the composition snapshot.
            e.Card(1).ApplyAttackGain(20);
            e.Handler.Resolve(e.State, source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(24));
            Assert.That(e.Card(2).Attack, Is.EqualTo(12));
            Assert.That(e.Card(4).Attack, Is.EqualTo(7));
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void OtherStartStages_AreRejected(CombatBattleStartStage stage)
        {
            var e = new Environment();
            var source = e.StartEvent(stage);

            Assert.That(e.Handler.CanTrigger(e.State, source), Is.False);
            Assert.Throws<InvalidOperationException>(() =>
                e.Handler.Resolve(e.State, source));

            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void Overflow_LeavesEntireBatchUnchangedAndAllowsRetry()
        {
            var e = new Environment();
            var target = e.Card(2);
            target.ApplyAttackGain(int.MaxValue - 1 - target.Attack);
            var source = e.StartEvent();
            var count = e.Log.Count;

            Assert.Throws<OverflowException>(() =>
                e.Handler.Resolve(e.State, source));

            Assert.That(target.Attack, Is.EqualTo(int.MaxValue - 1));
            Assert.That(e.Card(4).Attack, Is.EqualTo(7));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);

            target.ReduceAttack(int.MaxValue - 10);
            e.Handler.Resolve(e.State, source);

            Assert.That(target.Attack, Is.EqualTo(12));
            Assert.That(e.Card(4).Attack, Is.EqualTo(10));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [Test]
        public void RebuiltHandler_PreservesUsageAcrossRepeatedStartEvents()
        {
            var e = new Environment();
            e.Handler.Resolve(e.State, e.StartEvent());

            var rebuilt = new HawkPetBattleStartTriggerHandler(
                e.Side, e.Pet.InstanceId, e.Usage, e.Attack);
            var repeated = e.StartEvent();

            Assert.That(rebuilt.CanTrigger(e.State, repeated), Is.False);
            rebuilt.Resolve(e.State, repeated);

            Assert.That(e.Card(2).Attack, Is.EqualTo(12));
            Assert.That(e.Card(4).Attack, Is.EqualTo(10));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void DifferentParentObject_DoesNotMutateCardsOrConsumeUse()
        {
            var e = new Environment();
            var source = e.StartEvent();
            var copy = new BattleStartStageStartedCombatEvent(
                source.Metadata, source.Stage);

            Assert.Throws<ArgumentException>(() =>
                e.Handler.Resolve(e.State, copy));

            Assert.That(e.Card(2).Attack, Is.EqualTo(9));
            Assert.That(e.Card(4).Attack, Is.EqualTo(7));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);

            e.Handler.Resolve(e.State, source);
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void RemovedCard_IsExcludedWhenResolveSelectsTargets()
        {
            var e = new Environment();
            var source = e.StartEvent();

            Assert.That(e.Handler.CanTrigger(e.State, source), Is.True);
            e.State.GetSide(e.Side).RemoveCardFromCombat(e.Position(2));

            e.Handler.Resolve(e.State, source);

            Assert.That(e.Card(4).Attack, Is.EqualTo(10));
            Assert.That(e.Card(3).Attack, Is.EqualTo(6));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void ThresholdCardStillOnBoard_RemainsEligible()
        {
            var e = new Environment();
            e.Card(2).SetCurrentHpToZero();

            e.Handler.Resolve(e.State, e.StartEvent());

            Assert.That(e.Card(2).Attack, Is.EqualTo(12));
            Assert.That(e.Card(2).CurrentHp, Is.Zero);
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void Engine_DrainsBonusEventsWithoutRepeatingStartEffect()
        {
            var e = new Environment();
            e.StartEvent();

            var queue = new CombatEventQueue(e.Log);
            var sources = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[]
                {
                    new CombatPetTriggerSource(
                        e.Side, e.Pet.InstanceId, e.Handler)
                });

            var engine = new CombatTriggerEngine(e.State, queue, sources);
            engine.Drain(20, 20);

            Assert.That(e.Gains().Count, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatPetState Pet;

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
            public readonly HawkPetBattleStartTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front)
            {
                Side = side;
                Row = row;

                var upperPet = new CombatPetState(
                    new DefinitionId("test.hawk"),
                    new InstanceId(1002));

                var lowerPet = new CombatPetState(
                    new DefinitionId("test.hawk"),
                    new InstanceId(1001));

                Pet = row == BoardRow.Front ? upperPet : lowerPet;

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(new[] { upperPet, lowerPet }));

                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(Array.Empty<CombatPetState>()));

                State = new CombatState(
                    MakeSide(CombatSide.Player),
                    MakeSide(CombatSide.Enemy),
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);

                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);

                Attack = new CombatAttackGainResolver(Metadata, Log);
                Handler = new HawkPetBattleStartTriggerHandler(
                    side, Pet.InstanceId, Usage, Attack);
            }

            public BoardPosition Position(int column) =>
                new BoardPosition(Side, Row, new BoardColumn(column));

            public CombatCardState Card(int column) =>
                State.GetSide(Side).GetCardAt(Position(column));

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

            private static CombatSideState MakeSide(CombatSide side)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var attacks = new[] { 1, 9, 3, 7, 2 };

                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var id = (side == CombatSide.Player ? 0 : 100)
                            + (row == BoardRow.Front ? 0 : 10)
                            + column;

                        var card = new CombatCardState(
                            new DefinitionId("test.hawk_card"),
                            new InstanceId(id),
                            new CardRank(4),
                            CombatCardSeason.Spring,
                            hpCapacity: 10,
                            currentHp: 5,
                            armor: 0,
                            attack: attacks[column - 1]);

                        cards.Add(card);

                        slots.Add(new CombatSlotState(
                            new SlotId(id),
                            new BoardPosition(side, row, new BoardColumn(column)),
                            card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }
        }
    }
}