using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class MacaquePetBattleStartTriggerHandlerTests
    {
        [Test]
        public void Constructor_ValidatesAndPreservesDependencies()
        {
            var e = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new MacaquePetBattleStartTriggerHandler(
                    e.Side, e.Pet.InstanceId, null, e.Attack));

            Assert.Throws<ArgumentNullException>(() =>
                new MacaquePetBattleStartTriggerHandler(
                    e.Side, e.Pet.InstanceId, e.Usage, null));

            Assert.That(e.Handler.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Handler.AttackGainResolver, Is.SameAs(e.Attack));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void MatchingSnapshotGroup_ReceivesBonusInColumnOrder(
            CombatSide side,
            BoardRow row)
        {
            var e = new Environment(side, row);

            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.True);
            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.True);
            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);

            e.Handler.Resolve(e.State, e.Source);

            for (var column = 1; column <= 5; column++)
            {
                Assert.That(
                    e.Card(column).Attack,
                    Is.EqualTo(column <= 3 ? 4 : 2));

                Assert.That(e.Card(column).CurrentHp, Is.EqualTo(5));
                Assert.That(e.Card(column).HpCapacity, Is.EqualTo(10));
                Assert.That(e.Card(column).Armor, Is.Zero);

                Assert.That(
                    e.State.GetSide(side).GetCardAt(
                        e.Position(column, e.OtherRow)).Attack,
                    Is.EqualTo(2));

                Assert.That(
                    e.State.GetSide(e.OtherSide).GetCardAt(
                        new BoardPosition(
                            e.OtherSide, row, new BoardColumn(column))).Attack,
                    Is.EqualTo(2));
            }

            var gains = e.Gains();
            Assert.That(gains.Count, Is.EqualTo(3));

            for (var index = 0; index < gains.Count; index++)
            {
                var gain = gains[index];

                Assert.That(
                    gain.TargetInstanceId,
                    Is.EqualTo(e.Card(index + 1).InstanceId));

                Assert.That(
                    gain.TargetPosition.Column.Value,
                    Is.EqualTo(index + 1));

                Assert.That(gain.ActualGainedAmount, Is.EqualTo(2));
                Assert.That(
                    gain.Metadata.ParentEventId.Value,
                    Is.EqualTo(e.Source.Metadata.EventId));

                Assert.That(
                    gain.Metadata.TriggerRootId,
                    Is.EqualTo(e.Source.Metadata.TriggerRootId));

                if (index > 0)
                {
                    Assert.That(
                        gain.Metadata.SequenceNo.Value,
                        Is.GreaterThan(
                            gains[index - 1].Metadata.SequenceNo.Value));
                }
            }

            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.False);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void MatchingRankThreshold_RequiresAtLeastThree(int matchingCount)
        {
            var e = new Environment(matchingCount: matchingCount);
            var expected = matchingCount >= 3;

            Assert.That(
                e.Handler.CanTrigger(e.State, e.Source),
                Is.EqualTo(expected));

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(
                e.Gains().Count,
                Is.EqualTo(expected ? matchingCount : 0));

            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.EqualTo(expected ? 1 : 0));

            for (var column = 1; column <= 5; column++)
            {
                Assert.That(
                    e.Card(column).Attack,
                    Is.EqualTo(expected && column <= matchingCount ? 4 : 2));
            }
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void OtherStages_DoNotApplyBonus(CombatBattleStartStage stage)
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
        public void MissingSnapshot_FailsWithoutMutation()
        {
            var e = new Environment();
            var source = e.MakeEvent(withSnapshot: false);

            Assert.Throws<InvalidOperationException>(() =>
                e.Handler.CanTrigger(e.State, source));

            Assert.Throws<InvalidOperationException>(() =>
                e.Handler.Resolve(e.State, source));

            Assert.That(e.Card(1).Attack, Is.EqualTo(2));
            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void ChangedCurrentRank_DoesNotInvalidateSnapshotMembership()
        {
            var e = new Environment();

            e.Card(1).SetRank(new CardRank(14));

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).Rank.Value, Is.EqualTo(14));
            Assert.That(e.Card(1).Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(3));
        }

        [Test]
        public void NewCurrentRankGroup_DoesNotReplaceFailedSnapshotCondition()
        {
            var e = new Environment(matchingCount: 2);

            e.Card(3).SetRank(new CardRank(7));

            Assert.That(e.Handler.CanTrigger(e.State, e.Source), Is.False);
            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(3).Attack, Is.EqualTo(2));
            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void ReplacementCard_DoesNotInheritRemovedCardsBonus()
        {
            var e = new Environment();
            var replacementPosition = e.Position(5, e.OtherRow);
            var replacement = e.State.GetSide(e.Side)
                .GetCardAt(replacementPosition);

            e.State.GetSide(e.Side).RemoveCardFromCombat(e.Position(1));
            e.State.GetSide(e.Side).MoveCard(
                replacementPosition, e.Position(1));

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(replacement.Attack, Is.EqualTo(2));
            Assert.That(e.Card(2).Attack, Is.EqualTo(4));
            Assert.That(e.Card(3).Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void EligibleCardMovedToOtherRow_IsExcluded()
        {
            var e = new Environment();
            var moved = e.Card(1);
            var destination = e.Position(5, e.OtherRow);

            e.State.GetSide(e.Side).RemoveCardFromCombat(destination);
            e.State.GetSide(e.Side).MoveCard(e.Position(1), destination);

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(moved.Attack, Is.EqualTo(2));
            Assert.That(e.Gains().Count, Is.EqualTo(2));
        }

        [Test]
        public void AllEligibleCardsRemoved_CompletesUseWithoutBuffingOthers()
        {
            var e = new Environment();

            for (var column = 1; column <= 3; column++)
            {
                e.State.GetSide(e.Side).RemoveCardFromCombat(
                    e.Position(column));
            }

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(4).Attack, Is.EqualTo(2));
            Assert.That(e.Card(5).Attack, Is.EqualTo(2));
            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [Test]
        public void LastTargetOverflow_LeavesWholeBatchUnchangedAndAllowsRetry()
        {
            var e = new Environment();
            var lastTarget = e.Card(3);
            lastTarget.ApplyAttackGain(int.MaxValue - 1 - lastTarget.Attack);
            var count = e.Log.Count;

            Assert.Throws<OverflowException>(() =>
                e.Handler.Resolve(e.State, e.Source));

            Assert.That(e.Card(1).Attack, Is.EqualTo(2));
            Assert.That(e.Card(2).Attack, Is.EqualTo(2));
            Assert.That(lastTarget.Attack, Is.EqualTo(int.MaxValue - 1));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);

            lastTarget.ReduceAttack(int.MaxValue - 3);
            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(4));
            Assert.That(e.Card(2).Attack, Is.EqualTo(4));
            Assert.That(lastTarget.Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(3));
            Assert.That(e.Usage.HasTriggered(e.Pet.InstanceId), Is.True);
        }

        [Test]
        public void DifferentParentObject_DoesNotConsumeUse()
        {
            var e = new Environment();
            var copy = new BattleStartStageStartedCombatEvent(
                e.Source.Metadata,
                e.Source.Stage,
                e.Source.BattleStartSnapshot);

            Assert.Throws<ArgumentException>(() =>
                e.Handler.Resolve(e.State, copy));

            Assert.That(e.Card(1).Attack, Is.EqualTo(2));
            Assert.That(e.Gains(), Is.Empty);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);

            e.Handler.Resolve(e.State, e.Source);
            Assert.That(e.Gains().Count, Is.EqualTo(3));
        }

        [Test]
        public void RebuiltHandler_DoesNotRepeatCompletedBonus()
        {
            var e = new Environment();

            e.Handler.Resolve(e.State, e.Source);

            var rebuilt = new MacaquePetBattleStartTriggerHandler(
                e.Side, e.Pet.InstanceId, e.Usage, e.Attack);

            Assert.That(rebuilt.CanTrigger(e.State, e.Source), Is.False);
            rebuilt.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(3));
        }

        [Test]
        public void Engine_DrainsGainEventsWithoutRepeatingBonus()
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

            Assert.That(e.Gains().Count, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        [Test]
        public void TwoPetInstances_ResolveTheirOwnSnapshotRowsIndependently()
        {
            var e = new Environment();

            var otherHandler = new MacaquePetBattleStartTriggerHandler(
                e.Side, e.OtherPet.InstanceId, e.Usage, e.Attack);

            e.Handler.Resolve(e.State, e.Source);
            otherHandler.Resolve(e.State, e.Source);

            Assert.That(e.Gains().Count, Is.EqualTo(6));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));

            Assert.That(e.Card(1).Attack, Is.EqualTo(4));
            Assert.That(
                e.State.GetSide(e.Side).GetCardAt(
                    e.Position(1, e.OtherRow)).Attack,
                Is.EqualTo(4));
        }

        [Test]
        public void FreshBattleUsage_AllowsSamePetIdsAgain()
        {
            var first = new Environment();
            first.Handler.Resolve(first.State, first.Source);

            var second = new Environment();
            second.Handler.Resolve(second.State, second.Source);

            Assert.That(first.Pet.InstanceId, Is.EqualTo(second.Pet.InstanceId));
            Assert.That(first.Card(1).Attack, Is.EqualTo(4));
            Assert.That(second.Card(1).Attack, Is.EqualTo(4));
            Assert.That(second.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void ThresholdCardStillOnBoard_RemainsTargetable()
        {
            var e = new Environment();

            e.Card(1).SetCurrentHpToZero();

            e.Handler.Resolve(e.State, e.Source);

            Assert.That(e.Card(1).CurrentHp, Is.Zero);
            Assert.That(e.Card(1).Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(3));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatSide OtherSide;
            public readonly BoardRow Row;
            public readonly BoardRow OtherRow;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatPetState OtherPet;

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
            public readonly MacaquePetBattleStartTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int matchingCount = 3)
            {
                Side = side;
                Row = row;

                OtherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

                OtherRow = row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

                var upper = new CombatPetState(
                    new DefinitionId("test.macaque"),
                    new InstanceId(1002));

                var lower = new CombatPetState(
                    new DefinitionId("test.macaque"),
                    new InstanceId(1001));

                Pet = row == BoardRow.Front ? upper : lower;
                OtherPet = row == BoardRow.Front ? lower : upper;

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(new[] { upper, lower }));

                var otherPets = new CombatSidePetState(
                    OtherSide,
                    new CombatPetRegistry(Array.Empty<CombatPetState>()));

                State = new CombatState(
                    MakeSide(CombatSide.Player, matchingCount),
                    MakeSide(CombatSide.Enemy, matchingCount),
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);

                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);

                Attack = new CombatAttackGainResolver(Metadata, Log);
                Handler = new MacaquePetBattleStartTriggerHandler(
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
                int matchingCount)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();

                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var prefix = (side == CombatSide.Player ? 0 : 100)
                            + (row == BoardRow.Front ? 0 : 10);

                        // IDs deliberately run opposite to column order.
                        var card = new CombatCardState(
                            new DefinitionId("test.macaque_card"),
                            new InstanceId(prefix + 6 - column),
                            new CardRank(column <= matchingCount ? 7 : 8 + column),
                            CombatCardSeason.Spring,
                            hpCapacity: 10,
                            currentHp: 5,
                            armor: 0,
                            attack: 2);

                        cards.Add(card);

                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
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