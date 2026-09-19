using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        ToucanPetBattleStartTriggerHandlerTests
    {
        [Test]
        public void ConstructorValidatesAndPreservesDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new ToucanPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.Hp));
            Assert.Throws<ArgumentNullException>(() =>
                new ToucanPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    null));
            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                environment.Handler.HpGainResolver,
                Is.SameAs(environment.Hp));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void ThreeSnapshotSuitsBuffTwoLowestCurrentHpCards(
            CombatSide side,
            BoardRow row)
        {
            var environment = new Environment(side, row);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    environment.Source),
                Is.True);

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            AssertCard(
                environment.Card(row, 1),
                10,
                5);
            AssertCard(
                environment.Card(row, 2),
                11,
                2);
            AssertCard(
                environment.Card(row, 3),
                10,
                4);
            AssertCard(
                environment.Card(row, 4),
                11,
                3);
            AssertCard(
                environment.Card(row, 5),
                10,
                3);

            var gains = environment.Gains();

            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(
                gains[0].TargetInstanceId,
                Is.EqualTo(
                    environment.Card(row, 2).InstanceId));
            Assert.That(
                gains[1].TargetInstanceId,
                Is.EqualTo(
                    environment.Card(row, 4).InstanceId));

            foreach (var gain in gains)
            {
                Assert.That(
                    gain.SourceInstanceId,
                    Is.EqualTo(
                        environment.Pet.InstanceId));
                Assert.That(gain.IsHpStatGain, Is.True);
                Assert.That(
                    gain.ActualGainedAmount,
                    Is.EqualTo(1));
                Assert.That(
                    gain.Metadata.ParentEventId.Value,
                    Is.EqualTo(
                        environment.Source.Metadata.EventId));
                Assert.That(
                    gain.Metadata.TriggerRootId,
                    Is.EqualTo(
                        environment.Source.Metadata.TriggerRootId));
            }

            Assert.That(
                environment.Card(
                    environment.OtherRow,
                    2).HpCapacity,
                Is.EqualTo(10));
            Assert.That(
                environment.Usage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void FewerThanThreeSpecifiedSnapshotSuitsDoNotTrigger(
            int distinctSuitCount)
        {
            var environment = new Environment(
                distinctSuitCount: distinctSuitCount);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    environment.Source),
                Is.False);

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            Assert.That(environment.Gains(), Is.Empty);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void TargetSelectionUsesCurrentHpAfterSnapshot()
        {
            var environment = new Environment();

            environment.Card(environment.Row, 2)
                .ApplyHpStatGain(10);

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            var gains = environment.Gains();

            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(
                gains[0].TargetInstanceId,
                Is.EqualTo(
                    environment.Card(
                        environment.Row,
                        4).InstanceId));
            Assert.That(
                gains[1].TargetInstanceId,
                Is.EqualTo(
                    environment.Card(
                        environment.Row,
                        5).InstanceId));
            Assert.That(
                environment.Card(
                    environment.Row,
                    2).HpCapacity,
                Is.EqualTo(20));
        }

        [Test]
        public void EqualCurrentHpUsesLeftmostTargetsDespiteUnsortedSlots()
        {
            var environment = new Environment(equalHp: true);

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            var gains = environment.Gains();

            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(
                gains[0].TargetPosition.Column.Value,
                Is.EqualTo(1));
            Assert.That(
                gains[1].TargetPosition.Column.Value,
                Is.EqualTo(2));
        }

        [Test]
        public void LaterTargetOverflowLeavesAllTargetsUnchanged()
        {
            var environment = new Environment(
                overflowColumnOne: true);

            for (var column = 3;
                 column <= 5;
                 column++)
            {
                environment.State.GetSide(
                        environment.Side)
                    .RemoveCardFromCombat(
                        environment.Position(column));
            }

            var firstTarget = environment.Card(
                environment.Row,
                2);
            var firstCapacity = firstTarget.HpCapacity;
            var firstHp = firstTarget.CurrentHp;
            var eventCount = environment.Log.Count;

            Assert.Throws<OverflowException>(() =>
                environment.Handler.Resolve(
                    environment.State,
                    environment.Source));

            Assert.That(
                firstTarget.HpCapacity,
                Is.EqualTo(firstCapacity));
            Assert.That(
                firstTarget.CurrentHp,
                Is.EqualTo(firstHp));
            Assert.That(
                environment.Log.Count,
                Is.EqualTo(eventCount));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);

            environment.State.GetSide(environment.Side)
                .RemoveCardFromCombat(
                    environment.Position(1));

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            Assert.That(
                firstTarget.HpCapacity,
                Is.EqualTo(firstCapacity + 1));
            Assert.That(environment.Gains().Count, Is.EqualTo(1));
            Assert.That(
                environment.Usage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
        }

        [Test]
        public void RebuiltHandlerPreservesUsage()
        {
            var environment = new Environment();

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            var rebuilt =
                new ToucanPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Hp);

            Assert.That(
                rebuilt.CanTrigger(
                    environment.State,
                    environment.Source),
                Is.False);

            rebuilt.Resolve(
                environment.State,
                environment.Source);

            Assert.That(environment.Gains().Count, Is.EqualTo(2));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void MissingSnapshotUsesLegacyNoOpWithoutMutation()
        {
            var environment = new Environment();
            var source = environment.MakeEvent(
                withSnapshot: false);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    source),
                Is.False);

            environment.Handler.Resolve(
                environment.State,
                source);

            Assert.That(environment.Gains(), Is.Empty);
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void OtherStartStagesDoNotApplyBonus(
            CombatBattleStartStage stage)
        {
            var environment = new Environment();
            var source = environment.MakeEvent(stage);

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    source),
                Is.False);
            Assert.Throws<InvalidOperationException>(() =>
                environment.Handler.Resolve(
                    environment.State,
                    source));
            Assert.That(environment.Gains(), Is.Empty);
        }

        [Test]
        public void EngineDrainsBothGainEventsWithoutRepeating()
        {
            var environment = new Environment();
            var queue = new CombatEventQueue(environment.Log);
            var registry = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[]
                {
                    new CombatPetBattleStartTriggerSource(
                        environment.Handler)
                });
            var engine = new CombatTriggerEngine(
                environment.State,
                queue,
                registry);

            engine.Drain(20, 20);

            Assert.That(environment.Gains().Count, Is.EqualTo(2));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(20, 20), Is.Zero);
        }

        private static void AssertCard(
            CombatCardState card,
            int hpCapacity,
            int currentHp)
        {
            Assert.That(
                card.HpCapacity,
                Is.EqualTo(hpCapacity));
            Assert.That(
                card.CurrentHp,
                Is.EqualTo(currentHp));
        }

        private sealed class Environment
        {
            private static readonly int[] CurrentHpValues =
            {
                5,
                1,
                4,
                2,
                3
            };

            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly BoardRow OtherRow;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(
                    new CombatPetTriggerUsageRegistry());
            public readonly CombatStartedCombatEvent Root;
            public readonly BattleStartStageStartedCombatEvent Source;
            public readonly CombatHpGainResolver Hp;
            public readonly ToucanPetBattleStartTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int distinctSuitCount = 3,
                bool equalHp = false,
                bool overflowColumnOne = false)
            {
                Side = side;
                Row = row;
                OtherRow = row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

                var definitionId = new DefinitionId(
                    "test.toucan");
                var upper = new CombatPetState(
                    definitionId,
                    new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId,
                    new InstanceId(1001));

                Pet = row == BoardRow.Front
                    ? upper
                    : lower;

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;
                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { upper, lower }));
                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(
                        Array.Empty<CombatPetState>()));

                State = new CombatState(
                    MakeSide(
                        CombatSide.Player,
                        side,
                        row,
                        distinctSuitCount,
                        equalHp,
                        overflowColumnOne),
                    MakeSide(
                        CombatSide.Enemy,
                        side,
                        row,
                        distinctSuitCount,
                        equalHp,
                        overflowColumnOne),
                    side == CombatSide.Player
                        ? ownPets
                        : otherPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : otherPets);

                Root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
                Log.Append(Root);
                Hp = new CombatHpGainResolver(Metadata, Log);
                Handler =
                    new ToucanPetBattleStartTriggerHandler(
                        side,
                        Pet.InstanceId,
                        Usage,
                        Hp);
                Source = MakeEvent();
            }

            public BoardPosition Position(int column)
            {
                return new BoardPosition(
                    Side,
                    Row,
                    new BoardColumn(column));
            }

            public CombatCardState Card(
                BoardRow row,
                int column)
            {
                return State.GetSide(Side).GetCardAt(
                    new BoardPosition(
                        Side,
                        row,
                        new BoardColumn(column)));
            }

            public BattleStartStageStartedCombatEvent MakeEvent(
                CombatBattleStartStage stage =
                    CombatBattleStartStage.Pet,
                bool withSnapshot = true)
            {
                var metadata = Metadata.CreateChild(
                    Root.Metadata);
                var source = withSnapshot
                    ? new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage,
                        new CombatBattleStartSnapshotResolver()
                            .Resolve(State))
                    : new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage);

                Log.Append(source);

                return source;
            }

            public List<HpGainCombatEvent> Gains()
            {
                var result = new List<HpGainCombatEvent>();

                foreach (var item in Log.Events)
                {
                    var gain = item as HpGainCombatEvent;

                    if (gain != null)
                    {
                        result.Add(gain);
                    }
                }

                return result;
            }

            private static CombatSideState MakeSide(
                CombatSide boardSide,
                CombatSide petSide,
                BoardRow affectedRow,
                int distinctSuitCount,
                bool equalHp,
                bool overflowColumnOne)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();

                foreach (var row in new[]
                         {
                             BoardRow.Back,
                             BoardRow.Front
                         })
                {
                    foreach (var column in new[]
                             {
                                 5, 2, 1, 4, 3
                             })
                    {
                        var prefix =
                            (boardSide == CombatSide.Player
                                ? 0
                                : 100) +
                            (row == BoardRow.Front
                                ? 0
                                : 10);
                        var selectedFixture =
                            boardSide == petSide &&
                            row == affectedRow;
                        var hpCapacity =
                            selectedFixture &&
                            overflowColumnOne &&
                            column == 1
                                ? int.MaxValue
                                : 10;
                        var currentHp =
                            selectedFixture &&
                            overflowColumnOne &&
                            column == 1
                                ? int.MaxValue
                                : equalHp && selectedFixture
                                    ? 5
                                    : CurrentHpValues[
                                        column - 1];
                        var card = new CombatCardState(
                            new DefinitionId(
                                "test.toucan_card"),
                            new InstanceId(
                                prefix + 6 - column),
                            new CardRank(column + 1),
                            SuitForColumn(
                                column,
                                distinctSuitCount),
                            CombatCardSeason.Spring,
                            hpCapacity,
                            currentHp,
                            0,
                            2);

                        cards.Add(card);
                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(
                                boardSide,
                                row,
                                new BoardColumn(column)),
                            card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(boardSide, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardSuit SuitForColumn(
                int column,
                int distinctSuitCount)
            {
                if (distinctSuitCount <= 0)
                {
                    return CombatCardSuit.Unspecified;
                }

                if (distinctSuitCount == 1)
                {
                    return CombatCardSuit.Fruit;
                }

                if (distinctSuitCount == 2)
                {
                    return column % 2 == 0
                        ? CombatCardSuit.Vegetable
                        : CombatCardSuit.Fruit;
                }

                if (column == 2)
                {
                    return CombatCardSuit.Vegetable;
                }

                if (column == 3)
                {
                    return CombatCardSuit.Nut;
                }

                return CombatCardSuit.Fruit;
            }
        }
    }
}
