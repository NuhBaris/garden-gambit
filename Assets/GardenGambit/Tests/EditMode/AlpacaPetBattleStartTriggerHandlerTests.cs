using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class AlpacaPetBattleStartTriggerHandlerTests
    {
        [Test]
        public void Constructor_ValidatesAndPreservesDependencies()
        {
            var e = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new AlpacaPetBattleStartTriggerHandler(
                    e.Side, e.Pet.InstanceId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new AlpacaPetBattleStartTriggerHandler(
                    e.Side, e.Pet.InstanceId, e.Usage, null));

            Assert.That(e.Handler.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Handler.AttackGainResolver, Is.SameAs(e.Attack));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FiveUniqueRanks_BuffTwoLowestAttackCards(
            CombatSide side,
            BoardRow row)
        {
            var e = new Environment(side, row);

            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(5));
            Assert.That(e.Card(2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(3).Attack, Is.EqualTo(4));
            Assert.That(e.Card(4).Attack, Is.EqualTo(3));
            Assert.That(e.Card(5).Attack, Is.EqualTo(3));

            Assert.That(e.Card(2).CurrentHp, Is.EqualTo(5));
            Assert.That(e.Card(2).HpCapacity, Is.EqualTo(10));
            Assert.That(e.Card(2).Armor, Is.Zero);
            Assert.That(e.Card(2).Rank.Value, Is.EqualTo(3));

            var otherRow = row == BoardRow.Front
                ? BoardRow.Back
                : BoardRow.Front;

            Assert.That(
                e.State.GetSide(side).GetCardAt(
                    e.Position(2, otherRow)).Attack,
                Is.EqualTo(1));

            var otherSide = side == CombatSide.Player
                ? CombatSide.Enemy
                : CombatSide.Player;

            Assert.That(
                e.State.GetSide(otherSide).GetCardAt(
                    new BoardPosition(
                        otherSide, row, new BoardColumn(2))).Attack,
                Is.EqualTo(1));

            var gains = e.Gains();
            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(
                gains[0].TargetInstanceId,
                Is.EqualTo(e.Card(2).InstanceId));
            Assert.That(
                gains[1].TargetInstanceId,
                Is.EqualTo(e.Card(4).InstanceId));

            foreach (var gain in gains)
            {
                Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
                Assert.That(
                    gain.Metadata.ParentEventId.Value,
                    Is.EqualTo(e.Source.Metadata.EventId));
                Assert.That(
                    gain.Metadata.TriggerRootId,
                    Is.EqualTo(e.Source.Metadata.TriggerRootId));
            }

            Assert.That(
                gains[1].Metadata.SequenceNo.Value,
                Is.GreaterThan(gains[0].Metadata.SequenceNo.Value));

            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void Snapshot_RequiresFiveCards(int cardCount)
        {
            var e = new Environment(cardCount: cardCount);
            var expected = cardCount == 5;

            Assert.That(
                e.Handler.CanTrigger(e.State, e.Source),
                Is.EqualTo(expected));

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(
                e.Gains().Count,
                Is.EqualTo(expected ? 2 : 0));
            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.EqualTo(expected ? 1 : 0));
        }

        [Test]
        public void DuplicateSnapshotRank_PreventsBonus()
        {
            var e = new Environment(duplicateRank: true);

            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.False);
            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void CurrentRanksBecomeDuplicated_SnapshotConditionRemainsValid()
        {
            var e = new Environment();

            e.Card(5).SetRank(new CardRank(2));

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Gains().Count, Is.EqualTo(2));
            Assert.That(e.Card(2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(4).Attack, Is.EqualTo(3));
        }

        [Test]
        public void CurrentRanksBecomeUnique_FailedSnapshotRemainsFailed()
        {
            var e = new Environment(duplicateRank: true);

            e.Card(5).SetRank(new CardRank(6));

            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.False);
            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void TargetSelection_UsesCurrentAttackAfterSnapshot()
        {
            var e = new Environment();

            e.Card(2).ApplyAttackGain(10);

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(2).Attack, Is.EqualTo(11));
            Assert.That(e.Card(4).Attack, Is.EqualTo(3));
            Assert.That(e.Card(5).Attack, Is.EqualTo(4));

            Assert.That(
                e.Gains()[0].TargetInstanceId,
                Is.EqualTo(e.Card(4).InstanceId));
            Assert.That(
                e.Gains()[1].TargetInstanceId,
                Is.EqualTo(e.Card(5).InstanceId));
        }

        [Test]
        public void EqualAttack_UsesLeftmostTargetsDespiteUnsortedSlots()
        {
            var e = new Environment();

            for (var column = 1; column <= 5; column++)
            {
                var card = e.Card(column);
                card.ApplyAttackGain(5 - card.Attack);
            }

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(6));
            Assert.That(e.Card(2).Attack, Is.EqualTo(6));

            for (var column = 3; column <= 5; column++)
            {
                Assert.That(e.Card(column).Attack, Is.EqualTo(5));
            }

            Assert.That(
                e.Gains()[0].TargetPosition.Column.Value,
                Is.EqualTo(1));
            Assert.That(
                e.Gains()[1].TargetPosition.Column.Value,
                Is.EqualTo(2));
        }

        [Test]
        public void SecondTargetOverflow_LeavesFirstTargetUnchangedAndAllowsRetry()
        {
            var e = new Environment();

            // The snapshot qualifies, even though three cards are later removed.
            for (var column = 3; column <= 5; column++)
            {
                e.State.GetSide(e.Side).RemoveCardFromCombat(
                    e.Position(column));
            }

            var overflowTarget = e.Card(2);
            overflowTarget.ApplyAttackGain(
                int.MaxValue - overflowTarget.Attack);

            var count = e.Log.Count;

            Assert.Throws<OverflowException>(() =>
                e.Handler.Resolve(e.State, e.Source));

            Assert.That(e.Card(1).Attack, Is.EqualTo(5));
            Assert.That(overflowTarget.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);

            overflowTarget.ReduceAttack(int.MaxValue - 1);
            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(6));
            Assert.That(overflowTarget.Attack, Is.EqualTo(2));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [Test]
        public void RebuiltHandler_PreservesUsage()
        {
            var e = new Environment();
            e.Handler.Resolve(e.State, e.Source);

            var rebuilt = new AlpacaPetBattleStartTriggerHandler(
                e.Side, e.Pet.InstanceId, e.Usage, e.Attack);

            Assert.That(rebuilt.CanTrigger(e.State, e.Source), Is.False);
            rebuilt.Resolve(e.State, e.Source);

            Assert.That(e.Card(2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(4).Attack, Is.EqualTo(3));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void MissingSnapshot_FailsWithoutMutation()
        {
            var e = new Environment();
            var source = e.MakeEvent(withSnapshot: false);

            Assert.Throws<InvalidOperationException>(() =>
                e.Handler.CanTrigger(e.State, source));

            Assert.Throws<InvalidOperationException>(() =>
                e.Handler.Resolve(e.State, source));

            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void OtherStartStages_DoNotApplyBonus(CombatBattleStartStage stage)
        {
            var e = new Environment();
            var source = e.MakeEvent(stage);

            Assert.That(e.Handler.CanTrigger(e.State, source), Is.False);

            Assert.Throws<InvalidOperationException>(() =>
                e.Handler.Resolve(e.State, source));

            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void Engine_DrainsBonusEventsWithoutRepeatingEffect()
        {
            var e = new Environment();
            var queue = new CombatEventQueue(e.Log);

            var registry = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[]
                {
                    new CombatPetTriggerSource(
                        e.Side, e.Pet.InstanceId, e.Handler)
                });

            var engine = new CombatTriggerEngine(e.State, queue, registry);
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
            public readonly BattleStartStageStartedCombatEvent Source;
            public readonly CombatAttackGainResolver Attack;
            public readonly AlpacaPetBattleStartTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int cardCount = 5,
                bool duplicateRank = false)
            {
                Side = side;
                Row = row;

                var upper = new CombatPetState(
                    new DefinitionId("test.alpaca"),
                    new InstanceId(1002));

                var lower = new CombatPetState(
                    new DefinitionId("test.alpaca"),
                    new InstanceId(1001));

                Pet = row == BoardRow.Front ? upper : lower;

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(new[] { upper, lower }));

                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(Array.Empty<CombatPetState>()));

                State = new CombatState(
                    MakeSide(CombatSide.Player, cardCount, duplicateRank),
                    MakeSide(CombatSide.Enemy, cardCount, duplicateRank),
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);

                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);

                Attack = new CombatAttackGainResolver(Metadata, Log);
                Handler = new AlpacaPetBattleStartTriggerHandler(
                    side, Pet.InstanceId, Usage, Attack);

                Source = MakeEvent();
            }

            public BoardPosition Position(int column) =>
                Position(column, Row);

            public BoardPosition Position(int column, BoardRow row) =>
                new BoardPosition(Side, row, new BoardColumn(column));

            public CombatCardState Card(int column) =>
                State.GetSide(Side).GetCardAt(Position(column));

            public BattleStartStageStartedCombatEvent MakeEvent(
                CombatBattleStartStage stage = CombatBattleStartStage.Pet,
                bool withSnapshot = true)
            {
                var metadata = Metadata.CreateChild(Root.Metadata);

                var source = withSnapshot
                    ? new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage,
                        new CombatBattleStartSnapshotResolver().Resolve(State))
                    : new BattleStartStageStartedCombatEvent(metadata, stage);

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

            private static CombatSideState MakeSide(
                CombatSide side,
                int cardCount,
                bool duplicateRank)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var attacks = new[] { 5, 1, 4, 2, 3 };

                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var prefix = (side == CombatSide.Player ? 0 : 100)
                            + (row == BoardRow.Front ? 0 : 10);

                        CombatCardState card = null;

                        if (column <= cardCount)
                        {
                            var rank = duplicateRank && column == 5
                                ? 2
                                : column + 1;

                            card = new CombatCardState(
                                new DefinitionId("test.alpaca_card"),
                                new InstanceId(prefix + 6 - column),
                                new CardRank(rank),
                                CombatCardSeason.Spring,
                                hpCapacity: 10,
                                currentHp: 5,
                                armor: 0,
                                attack: attacks[column - 1]);

                            cards.Add(card);
                        }

                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(side, row, new BoardColumn(column)),
                            card == null ? (InstanceId?)null : card.InstanceId));
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