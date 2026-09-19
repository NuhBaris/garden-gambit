using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        MarmosetPetBattleStartTriggerHandlerTests
    {
        [Test]
        public void ConstructorValidatesAndPreservesDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.Hp,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    null,
                    environment.Armor));
            Assert.Throws<ArgumentNullException>(() =>
                new MarmosetPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Hp,
                    null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                environment.Handler.HpGainResolver,
                Is.SameAs(environment.Hp));
            Assert.That(
                environment.Handler.ArmorGainResolver,
                Is.SameAs(environment.Armor));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void LowestSnapshotRankGainsHpAndArmor(
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

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                var card = environment.Card(row, column);
                var selected = column == 4;

                Assert.That(
                    card.HpCapacity,
                    Is.EqualTo(selected ? 11 : 10));
                Assert.That(
                    card.CurrentHp,
                    Is.EqualTo(selected ? 6 : 5));
                Assert.That(
                    card.Armor,
                    Is.EqualTo(selected ? 1 : 0));
                Assert.That(card.Attack, Is.EqualTo(2));
            }

            Assert.That(
                environment.Card(
                    environment.OtherRow,
                    4).HpCapacity,
                Is.EqualTo(10));

            var gains = environment.Gains();

            Assert.That(gains.Count, Is.EqualTo(2));
            Assert.That(
                gains[0],
                Is.TypeOf<HpGainCombatEvent>());
            Assert.That(
                gains[1],
                Is.TypeOf<ArmorGainCombatEvent>());

            foreach (var gain in gains)
            {
                Assert.That(
                    gain.Metadata.ParentEventId.Value,
                    Is.EqualTo(
                        environment.Source.Metadata.EventId));
            }

            Assert.That(
                environment.Usage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
        }

        [Test]
        public void EqualLowestRankUsesLeftmostColumn()
        {
            var environment = new Environment(
                tieLowestRank: true);

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            Assert.That(
                environment.Card(environment.Row, 2).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.Card(environment.Row, 2).Armor,
                Is.EqualTo(1));
            Assert.That(
                environment.Card(environment.Row, 4).HpCapacity,
                Is.EqualTo(10));
            Assert.That(
                ((HpGainCombatEvent)
                    environment.Gains()[0])
                    .TargetPosition.Column.Value,
                Is.EqualTo(2));
        }

        [Test]
        public void SnapshotRankControlsSelectionAfterCurrentRanksChange()
        {
            var environment = new Environment();

            environment.Card(environment.Row, 1)
                .SetRank(new CardRank(2));
            environment.Card(environment.Row, 4)
                .SetRank(new CardRank(14));

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            Assert.That(
                environment.Card(environment.Row, 4).HpCapacity,
                Is.EqualTo(11));
            Assert.That(
                environment.Card(environment.Row, 1).HpCapacity,
                Is.EqualTo(10));
        }

        [Test]
        public void EmptyAffectedRowDoesNotTriggerOrConsumeUse()
        {
            var environment = new Environment(cardCount: 0);

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

        [TestCase(false)]
        [TestCase(true)]
        public void OverflowLeavesBothStatsAndUsageUnchanged(
            bool armorOverflow)
        {
            var environment = new Environment(
                hpOverflow: !armorOverflow,
                armorOverflow: armorOverflow);
            var target = environment.Card(
                environment.Row,
                4);
            var hpCapacity = target.HpCapacity;
            var currentHp = target.CurrentHp;
            var armor = target.Armor;
            var eventCount = environment.Log.Count;

            Assert.Throws<OverflowException>(() =>
                environment.Handler.Resolve(
                    environment.State,
                    environment.Source));

            Assert.That(target.HpCapacity, Is.EqualTo(hpCapacity));
            Assert.That(target.CurrentHp, Is.EqualTo(currentHp));
            Assert.That(target.Armor, Is.EqualTo(armor));
            Assert.That(environment.Log.Count, Is.EqualTo(eventCount));
            Assert.That(
                environment.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RebuiltHandlerSharesBattleScopedUsage()
        {
            var environment = new Environment();

            environment.Handler.Resolve(
                environment.State,
                environment.Source);

            var rebuilt =
                new MarmosetPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    environment.Hp,
                    environment.Armor);

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

        private sealed class Environment
        {
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
            public readonly CombatArmorGainResolver Armor;
            public readonly MarmosetPetBattleStartTriggerHandler Handler;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int cardCount = 5,
                bool tieLowestRank = false,
                bool hpOverflow = false,
                bool armorOverflow = false)
            {
                Side = side;
                Row = row;
                OtherRow = row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;

                var upper = new CombatPetState(
                    new DefinitionId("test.marmoset"),
                    new InstanceId(1002));
                var lower = new CombatPetState(
                    new DefinitionId("test.marmoset"),
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
                        cardCount,
                        tieLowestRank,
                        hpOverflow,
                        armorOverflow),
                    MakeSide(
                        CombatSide.Enemy,
                        side,
                        row,
                        cardCount,
                        tieLowestRank,
                        hpOverflow,
                        armorOverflow),
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
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Handler =
                    new MarmosetPetBattleStartTriggerHandler(
                        side,
                        Pet.InstanceId,
                        Usage,
                        Hp,
                        Armor);
                Source = MakeEvent();
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

            public List<CombatEvent> Gains()
            {
                var gains = new List<CombatEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    if (combatEvent is HpGainCombatEvent ||
                        combatEvent is ArmorGainCombatEvent)
                    {
                        gains.Add(combatEvent);
                    }
                }

                return gains;
            }

            private static CombatSideState MakeSide(
                CombatSide boardSide,
                CombatSide petSide,
                BoardRow affectedRow,
                int cardCount,
                bool tieLowestRank,
                bool hpOverflow,
                bool armorOverflow)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var ranks = new[] { 8, 3, 5, 2, 6 };

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
                        CombatCardState card = null;

                        var include = boardSide != petSide ||
                            row != affectedRow ||
                            column <= cardCount;

                        if (include)
                        {
                            var rank = ranks[column - 1];

                            if (tieLowestRank &&
                                boardSide == petSide &&
                                row == affectedRow &&
                                column == 2)
                            {
                                rank = 2;
                            }

                            var selected =
                                boardSide == petSide &&
                                row == affectedRow &&
                                column == 4;
                            var hpCapacity = selected &&
                                             hpOverflow
                                ? int.MaxValue
                                : 10;
                            var currentHp = selected &&
                                            hpOverflow
                                ? int.MaxValue
                                : 5;
                            var armor = selected &&
                                        armorOverflow
                                ? int.MaxValue
                                : 0;

                            card = new CombatCardState(
                                new DefinitionId(
                                    "test.marmoset_card"),
                                new InstanceId(
                                    prefix + 6 - column),
                                new CardRank(rank),
                                CombatCardSuit.Unspecified,
                                CombatCardSeason.Spring,
                                hpCapacity,
                                currentHp,
                                armor,
                                2);
                            cards.Add(card);
                        }

                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(
                                boardSide,
                                row,
                                new BoardColumn(column)),
                            card == null
                                ? (InstanceId?)null
                                : card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(boardSide, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }
        }
    }
}
