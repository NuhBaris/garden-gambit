using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatArmorGainResolverTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatArmorGainResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatArmorGainResolver(
                        new CombatEventMetadataFactory(
                            new CombatEventIdAllocator(),
                            new
                                CombatSequenceNumberAllocator()),
                        null));
        }

        [Test]
        public void
            TryApplyArmorGain_WithNullStateOrParent_Throws()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        null,
                        environment.ParentEvent,
                        environment.Position,
                        1));

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        null,
                        environment.Position,
                        1));

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyArmorGain_WithPositiveAmount_MutatesAndAppendsEvent()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            var armorGainEvent =
                environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        requestedAmount: 3);

            Assert.That(
                armorGainEvent,
                Is.Not.Null);

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(5));

            Assert.That(
                armorGainEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.ArmorGain));

            Assert.That(
                armorGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.Card.InstanceId));

            Assert.That(
                armorGainEvent.TargetPosition,
                Is.EqualTo(
                    environment.Position));

            Assert.That(
                armorGainEvent.TargetSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                armorGainEvent.PreviousArmor,
                Is.EqualTo(2));

            Assert.That(
                armorGainEvent.CurrentArmor,
                Is.EqualTo(5));

            Assert.That(
                armorGainEvent.ActualGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                armorGainEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                armorGainEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    armorGainEvent));
        }

        [Test]
        public void
            TryApplyArmorGain_WithZeroAmount_DoesNotMutateOrEmit()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            var armorGainEvent =
                environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        requestedAmount: 0);

            Assert.That(
                armorGainEvent,
                Is.Null);

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyArmorGain_WithNegativeAmount_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        requestedAmount: -1));

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyArmorGain_WithInvalidPosition_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        environment.ParentEvent,
                        default(BoardPosition),
                        requestedAmount: 1));

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyArmorGain_WithUnloggedParent_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            var unloggedEventId =
                new CombatEventId(100);

            var unloggedParent =
                new CombatStartedCombatEvent(
                    new CombatEventMetadata(
                        unloggedEventId,
                        new CombatSequenceNumber(100),
                        null,
                        unloggedEventId));

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        unloggedParent,
                        environment.Position,
                        requestedAmount: 1));

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyArmorGain_WithDifferentParentReference_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    armor: 2);

            var differentReference =
                new CombatStartedCombatEvent(
                    environment.ParentEvent.Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        differentReference,
                        environment.Position,
                        requestedAmount: 1));

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyArmorGain_WhenArmorWouldOverflow_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    armor: int.MaxValue);

            Assert.Throws<OverflowException>(
                () => environment.Resolver
                    .TryApplyArmorGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        requestedAmount: 1));

            Assert.That(
                environment.Card.Armor,
                Is.EqualTo(
                    int.MaxValue));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        private static TestEnvironment
            CreateEnvironment(
                int armor)
        {
            var position =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var card =
                new CombatCardState(
                    new DefinitionId(
                        "target-card"),
                    new InstanceId(1),
                    new CardRank(5),
                    CombatCardSeason.Spring,
                    hpCapacity: 10,
                    currentHp: 10,
                    armor: armor,
                    attack: 2);

            var playerSide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Player,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                position,
                                card.InstanceId)
                        }),
                    new CombatCardRegistry(
                        new[]
                        {
                            card
                        }),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var enemySide =
                new CombatSideState(
                    new CombatBoardState(
                        CombatSide.Enemy,
                        Array.Empty<
                            CombatSlotState>()),
                    new CombatCardRegistry(
                        Array.Empty<
                            CombatCardState>()),
                    new BattleHealth(
                        BattleHealth
                            .NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));

            var state =
                new CombatState(
                    playerSide,
                    enemySide);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new
                        CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var parentEvent =
                new CombatStartedCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(
                parentEvent);

            return new TestEnvironment
            {
                State =
                    state,

                Card =
                    card,

                Position =
                    position,

                ParentEvent =
                    parentEvent,

                EventLog =
                    eventLog,

                Resolver =
                    new CombatArmorGainResolver(
                        metadataFactory,
                        eventLog)
            };
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatCardState Card
            {
                get;
                set;
            }

            public BoardPosition Position
            {
                get;
                set;
            }

            public CombatStartedCombatEvent ParentEvent
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatArmorGainResolver Resolver
            {
                get;
                set;
            }
        }
    }
}