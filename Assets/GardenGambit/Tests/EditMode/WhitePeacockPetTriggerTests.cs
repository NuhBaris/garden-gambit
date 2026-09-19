using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class WhitePeacockPetTriggerTests
    {
        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FourSuitSnapshot_BuffsEveryCurrentRowCard(
            CombatSide side,
            BoardRow row)
        {
            var e = new Environment(side, row);
            var source = e.Source();
            var candidates = Discover(
                source,
                e.State,
                e.Stage);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(source.Handler));
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(e.Pet.InstanceId));
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(e.Usage));
            Assert.That(
                source.HpGainResolver,
                Is.SameAs(e.Hp));
            Assert.That(
                source.ArmorGainResolver,
                Is.SameAs(e.Armor));
            Assert.That(
                source.AttackGainResolver,
                Is.SameAs(e.Attack));

            candidates[0].Trigger.Resolve(
                e.State,
                e.Stage);

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                AssertBuffed(
                    e.Card(row, column));
            }

            AssertUnchanged(
                e.Card(OtherRow(row), 1));
            Assert.That(
                e.Usage.HasTriggered(
                    e.Pet.InstanceId),
                Is.True);
            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.EqualTo(1));

            e.AssertGainEvents(
                expectedTargetCount: 5);
        }

        [TestCase(BoardRow.Front)]
        [TestCase(BoardRow.Back)]
        public void ThreeSuitSnapshot_DoesNotTriggerOrConsumeUsage(
            BoardRow row)
        {
            var e = new Environment(
                CombatSide.Player,
                row,
                qualified: false);
            var source = e.Source();

            Assert.That(
                Discover(source, e.State, e.Stage),
                Is.Empty);

            source.Handler.Resolve(
                e.State,
                e.Stage);

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                AssertUnchanged(
                    e.Card(row, column));
            }

            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void SnapshotConditionIsLockedButTargetsUseCurrentRow()
        {
            var e = new Environment();
            var moved = e.Card(BoardRow.Front, 4);

            e.State.GetSide(e.Side).MoveCard(
                e.Position(BoardRow.Front, 4),
                e.Position(BoardRow.Back, 4));

            var source = e.Source();

            Assert.That(
                Discover(source, e.State, e.Stage).Count,
                Is.EqualTo(1));

            source.Handler.Resolve(
                e.State,
                e.Stage);

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                if (column == 4)
                {
                    continue;
                }

                AssertBuffed(
                    e.Card(BoardRow.Front, column));
            }

            AssertUnchanged(moved);
            Assert.That(
                e.State.GetSide(e.Side)
                    .GetCardAt(
                        e.Position(BoardRow.Back, 4)),
                Is.SameAs(moved));
            e.AssertGainEvents(
                expectedTargetCount: 4);
        }

        [Test]
        public void OverflowPreflightLeavesAllStatsAndUsageRetryable()
        {
            var e = new Environment(
                overflowLastAttack: true);
            var source = e.Source();

            Assert.Throws<OverflowException>(() =>
                source.Handler.Resolve(
                    e.State,
                    e.Stage));

            for (var column = 1;
                 column <= 4;
                 column++)
            {
                AssertUnchanged(
                    e.Card(BoardRow.Front, column));
            }

            var overflowCard =
                e.Card(BoardRow.Front, 5);

            Assert.That(
                overflowCard.HpCapacity,
                Is.EqualTo(10));
            Assert.That(
                overflowCard.CurrentHp,
                Is.EqualTo(7));
            Assert.That(
                overflowCard.Armor,
                Is.EqualTo(2));
            Assert.That(
                overflowCard.Attack,
                Is.EqualTo(int.MaxValue));
            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));

            overflowCard.ReduceAttack(
                int.MaxValue - 4);

            source.Handler.Resolve(
                e.State,
                e.Stage);

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                AssertBuffed(
                    e.Card(BoardRow.Front, column));
            }

            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
            e.AssertGainEvents(
                expectedTargetCount: 5);
        }

        [Test]
        public void RebuiltSource_PreservesCompletedPetUsage()
        {
            var e = new Environment();

            e.Source().Handler.Resolve(
                e.State,
                e.Stage);

            var rebuilt = e.Source();

            Assert.That(
                Discover(rebuilt, e.State, e.Stage),
                Is.Empty);

            rebuilt.Handler.Resolve(
                e.State,
                e.Stage);

            for (var column = 1;
                 column <= 5;
                 column++)
            {
                AssertBuffed(
                    e.Card(BoardRow.Front, column));
            }

            e.AssertGainEvents(
                expectedTargetCount: 5);
            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void Factory_CreatesOneSourceWithSharedDependencies()
        {
            var e = new Environment();
            var factory = e.Factory();
            var sources =
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        e.Side,
                        e.Pet));

            Assert.That(sources.Count, Is.EqualTo(1));

            var source =
                sources[0] as WhitePeacockPetTriggerSource;

            Assert.That(source, Is.Not.Null);
            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(e.Pet.DefinitionId));
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(e.Usage));
            Assert.That(
                factory.HpGainResolver,
                Is.SameAs(e.Hp));
            Assert.That(
                factory.ArmorGainResolver,
                Is.SameAs(e.Armor));
            Assert.That(
                factory.AttackGainResolver,
                Is.SameAs(e.Attack));
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(e.Usage));
            Assert.That(
                source.HpGainResolver,
                Is.SameAs(e.Hp));
            Assert.That(
                source.ArmorGainResolver,
                Is.SameAs(e.Armor));
            Assert.That(
                source.AttackGainResolver,
                Is.SameAs(e.Attack));

            Assert.Throws<ArgumentException>(() =>
                new WhitePeacockPetTriggerSourceFactory(
                    default(DefinitionId),
                    e.Usage,
                    e.Hp,
                    e.Armor,
                    e.Attack));
            Assert.Throws<ArgumentNullException>(() =>
                new WhitePeacockPetTriggerSourceFactory(
                    e.Pet.DefinitionId,
                    null,
                    e.Hp,
                    e.Armor,
                    e.Attack));
            Assert.Throws<ArgumentNullException>(() =>
                new WhitePeacockPetTriggerSourceFactory(
                    e.Pet.DefinitionId,
                    e.Usage,
                    null,
                    e.Armor,
                    e.Attack));
            Assert.Throws<ArgumentNullException>(() =>
                new WhitePeacockPetTriggerSourceFactory(
                    e.Pet.DefinitionId,
                    e.Usage,
                    e.Hp,
                    null,
                    e.Attack));
            Assert.Throws<ArgumentNullException>(() =>
                new WhitePeacockPetTriggerSourceFactory(
                    e.Pet.DefinitionId,
                    e.Usage,
                    e.Hp,
                    e.Armor,
                    null));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        (CombatSide)99,
                        e.Pet)));
            Assert.Throws<ArgumentNullException>(() =>
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        e.Side,
                        null)));

            var otherPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                e.Pet.InstanceId);

            Assert.Throws<ArgumentException>(() =>
                new List<ICombatTriggerSource>(
                    factory.CreateSources(
                        e.Side,
                        otherPet)));
        }

        [Test]
        public void SlotStage_DoesNotCreateCandidate()
        {
            var e = new Environment();
            var slotStage =
                new BattleStartStageStartedCombatEvent(
                    e.Metadata.CreateChild(
                        e.Root.Metadata),
                    CombatBattleStartStage.Slot,
                    e.Snapshot);

            Assert.That(
                Discover(
                    e.Source(),
                    e.State,
                    slotStage),
                Is.Empty);
            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void SnapshotlessPetStage_DoesNotCreateCandidate()
        {
            var e = new Environment();
            var snapshotless =
                new BattleStartStageStartedCombatEvent(
                    e.Metadata.CreateChild(
                        e.Root.Metadata),
                    CombatBattleStartStage.Pet);

            Assert.That(
                Discover(
                    e.Source(),
                    e.State,
                    snapshotless),
                Is.Empty);

            e.Source().Handler.Resolve(
                e.State,
                snapshotless);

            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void FourSuitsOnOpposingRow_DoNotSatisfyPetCondition()
        {
            var e = new Environment(
                qualified: false,
                opposingQualified: true);

            Assert.That(
                Discover(
                    e.Source(),
                    e.State,
                    e.Stage),
                Is.Empty);
            Assert.That(
                e.Usage.UsageRegistry.Count,
                Is.Zero);
        }

        private static List<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            Discover(
                WhitePeacockPetTriggerSource source,
                CombatState state,
                CombatEvent sourceEvent)
        {
            return new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        state,
                        sourceEvent));
        }

        private static void AssertBuffed(
            CombatCardState card)
        {
            Assert.That(card.HpCapacity, Is.EqualTo(11));
            Assert.That(card.CurrentHp, Is.EqualTo(8));
            Assert.That(card.Armor, Is.EqualTo(3));
            Assert.That(card.Attack, Is.EqualTo(5));
        }

        private static void AssertUnchanged(
            CombatCardState card)
        {
            Assert.That(card.HpCapacity, Is.EqualTo(10));
            Assert.That(card.CurrentHp, Is.EqualTo(7));
            Assert.That(card.Armor, Is.EqualTo(2));
            Assert.That(card.Attack, Is.EqualTo(4));
        }

        private static BoardRow OtherRow(
            BoardRow row)
        {
            return row == BoardRow.Front
                ? BoardRow.Back
                : BoardRow.Front;
        }

        private sealed class Environment
        {
            private static readonly CombatCardSuit[]
                QualifiedSuits =
                {
                    CombatCardSuit.Fruit,
                    CombatCardSuit.Vegetable,
                    CombatCardSuit.Nut,
                    CombatCardSuit.Drink,
                    CombatCardSuit.Fruit
                };

            private static readonly CombatCardSuit[]
                UnqualifiedSuits =
                {
                    CombatCardSuit.Fruit,
                    CombatCardSuit.Vegetable,
                    CombatCardSuit.Nut,
                    CombatCardSuit.Fruit,
                    CombatCardSuit.Vegetable
                };

            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(
                    new CombatPetTriggerUsageRegistry());
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatBattleStartSnapshot Snapshot;
            public readonly BattleStartStageStartedCombatEvent Stage;
            public readonly CombatHpGainResolver Hp;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                bool qualified = true,
                bool opposingQualified = false,
                bool overflowLastAttack = false)
            {
                Side = side;
                Row = row;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                Pet = new CombatPetState(
                    new DefinitionId(
                        "test.white_peacock"),
                    new InstanceId(1001));

                var ownSide = CreateSide(
                    side,
                    row,
                    qualified,
                    instancePrefix: 0,
                    overflowLastAttack:
                        overflowLastAttack);

                var opposing = CreateSide(
                    opposingSide,
                    row,
                    opposingQualified,
                    instancePrefix: 100,
                    overflowLastAttack: false);

                var ownPets = CreateOwnPets(
                    side,
                    row,
                    Pet);

                var opposingPets =
                    new CombatSidePetState(
                        opposingSide,
                        new CombatPetRegistry(
                            Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? ownSide
                        : opposing,
                    side == CombatSide.Enemy
                        ? ownSide
                        : opposing,
                    side == CombatSide.Player
                        ? ownPets
                        : opposingPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : opposingPets);

                Root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
                Log.Append(Root);

                Snapshot =
                    new CombatBattleStartSnapshotResolver()
                        .Resolve(State);

                Stage =
                    new BattleStartStageStartedCombatEvent(
                        Metadata.CreateChild(
                            Root.Metadata),
                        CombatBattleStartStage.Pet,
                        Snapshot);
                Log.Append(Stage);

                Hp = new CombatHpGainResolver(
                    Metadata,
                    Log);
                Armor = new CombatArmorGainResolver(
                    Metadata,
                    Log);
                Attack = new CombatAttackGainResolver(
                    Metadata,
                    Log);
            }

            public WhitePeacockPetTriggerSource Source()
            {
                return new WhitePeacockPetTriggerSource(
                    Side,
                    Pet.InstanceId,
                    Usage,
                    Hp,
                    Armor,
                    Attack);
            }

            public WhitePeacockPetTriggerSourceFactory Factory()
            {
                return new WhitePeacockPetTriggerSourceFactory(
                    Pet.DefinitionId,
                    Usage,
                    Hp,
                    Armor,
                    Attack);
            }

            public BoardPosition Position(
                BoardRow row,
                int column)
            {
                return new BoardPosition(
                    Side,
                    row,
                    new BoardColumn(column));
            }

            public CombatCardState Card(
                BoardRow row,
                int column)
            {
                return State.GetSide(Side)
                    .GetCardAt(
                        Position(row, column));
            }

            public void AssertGainEvents(
                int expectedTargetCount)
            {
                var hpEvents = Events<HpGainCombatEvent>();
                var armorEvents =
                    Events<ArmorGainCombatEvent>();
                var attackEvents =
                    Events<AttackGainCombatEvent>();

                Assert.That(
                    hpEvents.Count,
                    Is.EqualTo(expectedTargetCount));
                Assert.That(
                    armorEvents.Count,
                    Is.EqualTo(expectedTargetCount));
                Assert.That(
                    attackEvents.Count,
                    Is.EqualTo(expectedTargetCount));

                var expectedTargets =
                    CurrentRowCards();

                Assert.That(
                    expectedTargets.Count,
                    Is.EqualTo(expectedTargetCount));

                for (var index = 0;
                     index < expectedTargetCount;
                     index++)
                {
                    var expected =
                        expectedTargets[index];

                    Assert.That(
                        hpEvents[index].TargetInstanceId,
                        Is.EqualTo(expected.InstanceId));
                    Assert.That(
                        hpEvents[index].SourceInstanceId,
                        Is.EqualTo(Pet.InstanceId));
                    Assert.That(
                        hpEvents[index].IsSelfSource,
                        Is.False);
                    Assert.That(
                        armorEvents[index].TargetInstanceId,
                        Is.EqualTo(expected.InstanceId));
                    Assert.That(
                        attackEvents[index].TargetInstanceId,
                        Is.EqualTo(expected.InstanceId));
                    Assert.That(
                        hpEvents[index].Metadata.ParentEventId.Value,
                        Is.EqualTo(
                            Stage.Metadata.EventId));
                    Assert.That(
                        armorEvents[index].Metadata.ParentEventId.Value,
                        Is.EqualTo(
                            Stage.Metadata.EventId));
                    Assert.That(
                        attackEvents[index].Metadata.ParentEventId.Value,
                        Is.EqualTo(
                            Stage.Metadata.EventId));
                }

                for (var index = 2;
                     index < 2 + expectedTargetCount;
                     index++)
                {
                    Assert.That(
                        Log.Events[index],
                        Is.TypeOf<HpGainCombatEvent>());
                }

                for (var index =
                         2 + expectedTargetCount;
                     index < 2 +
                         expectedTargetCount * 2;
                     index++)
                {
                    Assert.That(
                        Log.Events[index],
                        Is.TypeOf<ArmorGainCombatEvent>());
                }

                for (var index =
                         2 + expectedTargetCount * 2;
                     index < 2 +
                         expectedTargetCount * 3;
                     index++)
                {
                    Assert.That(
                        Log.Events[index],
                        Is.TypeOf<AttackGainCombatEvent>());
                }
            }

            private List<CombatCardState>
                CurrentRowCards()
            {
                var positions =
                    new List<BoardPosition>();

                foreach (var slot in
                         State.GetSide(Side).Board.Slots)
                {
                    if (slot.Position.Row == Row &&
                        slot.IsOccupied)
                    {
                        positions.Add(
                            slot.Position);
                    }
                }

                positions.Sort(
                    (left, right) =>
                        left.Column.CompareTo(
                            right.Column));

                var cards =
                    new List<CombatCardState>();

                foreach (var position in positions)
                {
                    cards.Add(
                        State.GetSide(Side)
                            .GetCardAt(position));
                }

                return cards;
            }

            private List<TEvent> Events<TEvent>()
                where TEvent : CombatEvent
            {
                var result = new List<TEvent>();

                foreach (var item in Log.Events)
                {
                    var typed = item as TEvent;

                    if (typed != null)
                    {
                        result.Add(typed);
                    }
                }

                return result;
            }

            private static CombatSidePetState CreateOwnPets(
                CombatSide side,
                BoardRow row,
                CombatPetState pet)
            {
                if (row == BoardRow.Front)
                {
                    return new CombatSidePetState(
                        side,
                        new CombatPetRegistry(
                            new[] { pet }));
                }

                var upperPet = new CombatPetState(
                    new DefinitionId("test.upper_pet"),
                    new InstanceId(1002));

                return new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { upperPet, pet }));
            }

            private static CombatSideState CreateSide(
                CombatSide side,
                BoardRow targetRow,
                bool qualified,
                long instancePrefix,
                bool overflowLastAttack)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var suits = qualified
                    ? QualifiedSuits
                    : UnqualifiedSuits;

                for (var column = 5;
                     column >= 1;
                     column--)
                {
                    var position = new BoardPosition(
                        side,
                        targetRow,
                        new BoardColumn(column));
                    var instanceId = new InstanceId(
                        instancePrefix + column);
                    var card = CreateCard(
                        instanceId,
                        suits[column - 1],
                        overflowLastAttack &&
                        column == 5);

                    cards.Add(card);
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(
                                column),
                            position,
                            instanceId));
                }

                var otherRow = OtherRow(targetRow);
                var otherInstanceId =
                    new InstanceId(
                        instancePrefix + 10);
                var otherCard = CreateCard(
                    otherInstanceId,
                    CombatCardSuit.Drink,
                    false);

                cards.Add(otherCard);

                for (var column = 5;
                     column >= 1;
                     column--)
                {
                    var position = new BoardPosition(
                        side,
                        otherRow,
                        new BoardColumn(column));

                    slots.Add(
                        column == 1
                            ? new CombatSlotState(
                                new SlotId(
                                    10 + column),
                                position,
                                otherInstanceId)
                            : new CombatSlotState(
                                new SlotId(
                                    10 + column),
                                position));
                }

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardState CreateCard(
                InstanceId instanceId,
                CombatCardSuit suit,
                bool overflowAttack)
            {
                return new CombatCardState(
                    new DefinitionId(
                        $"test.white_peacock_card." +
                        $"{instanceId.Value}"),
                    instanceId,
                    new CardRank(6),
                    suit,
                    CombatCardSeason.Spring,
                    10,
                    7,
                    2,
                    overflowAttack
                        ? int.MaxValue
                        : 4);
            }
        }
    }
}
