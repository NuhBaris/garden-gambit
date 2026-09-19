using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatHpGainResolverTests
    {
        [Test]
        public void
            Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatHpGainResolver(
                        null,
                        new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatHpGainResolver(
                        new CombatEventMetadataFactory(
                            new CombatEventIdAllocator(),
                            new
                                CombatSequenceNumberAllocator()),
                        null));
        }

        [Test]
        public void
            GainMethods_WithNullStateOrParent_Throw()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryApplyHpStatGain(
                        null,
                        environment.ParentEvent,
                        environment.Position,
                        1));

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryApplyHeal(
                        null,
                        environment.ParentEvent,
                        environment.Position,
                        1));

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryApplyHpStatGain(
                        environment.State,
                        null,
                        environment.Position,
                        1));

            Assert.Throws<ArgumentNullException>(
                () => environment.Resolver
                    .TryApplyHeal(
                        environment.State,
                        null,
                        environment.Position,
                        1));
        }

        [Test]
        public void
            TryApplyHpStatGain_WithPositiveAmount_MutatesAndAppendsEvent()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHpStatGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        3);

            Assert.That(
                hpGainEvent,
                Is.Not.Null);

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(13));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                hpGainEvent.TargetInstanceId,
                Is.EqualTo(
                    environment.Card.InstanceId));

            Assert.That(
                hpGainEvent.TargetPosition,
                Is.EqualTo(
                    environment.Position));

            Assert.That(
                hpGainEvent.PreviousHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                hpGainEvent.CurrentHpCapacity,
                Is.EqualTo(13));

            Assert.That(
                hpGainEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                hpGainEvent.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                hpGainEvent.IsHpStatGain,
                Is.True);

            Assert.That(
                hpGainEvent.Metadata
                    .ParentEventId.Value,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                hpGainEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    hpGainEvent));
        }

        [Test]
        public void
            TryApplyHpStatGain_WithZeroAmount_DoesNotMutateOrEmit()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHpStatGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        0);

            Assert.That(
                hpGainEvent,
                Is.Null);

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHpStatGain_WithNegativeAmount_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .TryApplyHpStatGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        -1));

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHpStatGain_WithUnloggedParent_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

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
                    .TryApplyHpStatGain(
                        environment.State,
                        unloggedParent,
                        environment.Position,
                        1));

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHpStatGain_WithDifferentParentReference_Throws()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            var differentReference =
                new CombatStartedCombatEvent(
                    environment.ParentEvent.Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .TryApplyHpStatGain(
                        environment.State,
                        differentReference,
                        environment.Position,
                        1));

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHpStatGain_WhenCapacityWouldOverflow_ThrowsWithoutMutation()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: int.MaxValue,
                    currentHp: 4);

            Assert.Throws<OverflowException>(
                () => environment.Resolver
                    .TryApplyHpStatGain(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        1));

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(
                    int.MaxValue));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHeal_WithAvailableCapacity_MutatesAndAppendsEvent()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHeal(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        3);

            Assert.That(
                hpGainEvent,
                Is.Not.Null);

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                hpGainEvent.PreviousHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                hpGainEvent.CurrentHpCapacity,
                Is.EqualTo(10));

            Assert.That(
                hpGainEvent.PreviousHp,
                Is.EqualTo(4));

            Assert.That(
                hpGainEvent.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(3));

            Assert.That(
                hpGainEvent.IsHeal,
                Is.True);

            Assert.That(
                hpGainEvent.IsHpStatGain,
                Is.False);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(
                    hpGainEvent));
        }

        [Test]
        public void
            TryApplyHeal_AboveMissingHp_CapsAtCapacity()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 8);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHeal(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        10);

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                hpGainEvent.ActualGainedAmount,
                Is.EqualTo(2));

            Assert.That(
                hpGainEvent.PreviousHp,
                Is.EqualTo(8));

            Assert.That(
                hpGainEvent.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void
            TryApplyHeal_AtFullHp_DoesNotEmitEvent()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 10);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHeal(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        5);

            Assert.That(
                hpGainEvent,
                Is.Null);

            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHeal_WithZeroAmount_DoesNotEmitEvent()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 4);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHeal(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        0);

            Assert.That(
                hpGainEvent,
                Is.Null);

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryApplyHeal_FromDeathThreshold_CanRestorePositiveHp()
        {
            var environment =
                CreateEnvironment(
                    hpCapacity: 10,
                    currentHp: 0);

            var hpGainEvent =
                environment.Resolver
                    .TryApplyHeal(
                        environment.State,
                        environment.ParentEvent,
                        environment.Position,
                        2);

            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(2));

            Assert.That(
                hpGainEvent.WasAtDeathThreshold,
                Is.True);

            Assert.That(
                hpGainEvent.IsAtDeathThreshold,
                Is.False);

            Assert.That(
                hpGainEvent
                    .RestoredAboveDeathThreshold,
                Is.True);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void GainMethods_WithExplicitSource_PreserveIdentityAndValues(
            bool heal,
            bool selfSource)
        {
            var environment = CreateEnvironment(
                hpCapacity: 10,
                currentHp: 8);

            var sourceInstanceId = selfSource
                ? environment.Card.InstanceId
                : new InstanceId(2);

            var gainEvent = ApplyGainWithSource(
                environment,
                heal,
                sourceInstanceId,
                5);

            Assert.That(gainEvent, Is.Not.Null);
            Assert.That(
                gainEvent.SourceInstanceId,
                Is.EqualTo(sourceInstanceId));
            Assert.That(
                gainEvent.TargetInstanceId,
                Is.EqualTo(environment.Card.InstanceId));
            Assert.That(
                gainEvent.TargetPosition,
                Is.EqualTo(environment.Position));
            Assert.That(
                gainEvent.IsSelfSource,
                Is.EqualTo(selfSource));
            Assert.That(
                gainEvent.IsFromAnotherSource,
                Is.EqualTo(!selfSource));

            Assert.That(gainEvent.IsHeal, Is.EqualTo(heal));
            Assert.That(gainEvent.IsHpStatGain, Is.EqualTo(!heal));
            Assert.That(gainEvent.PreviousHpCapacity, Is.EqualTo(10));
            Assert.That(gainEvent.PreviousHp, Is.EqualTo(8));
            Assert.That(
                gainEvent.ActualGainedAmount,
                Is.EqualTo(heal ? 2 : 5));
            Assert.That(
                environment.Card.HpCapacity,
                Is.EqualTo(heal ? 10 : 15));
            Assert.That(
                environment.Card.CurrentHp,
                Is.EqualTo(heal ? 10 : 13));
            Assert.That(
                gainEvent.CurrentHpCapacity,
                Is.EqualTo(environment.Card.HpCapacity));
            Assert.That(
                gainEvent.CurrentHp,
                Is.EqualTo(environment.Card.CurrentHp));

            Assert.That(
                gainEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(environment.ParentEvent.Metadata.EventId));
            Assert.That(
                gainEvent.Metadata.TriggerRootId,
                Is.EqualTo(environment.ParentEvent.Metadata.TriggerRootId));
            Assert.That(environment.EventLog.Count, Is.EqualTo(2));
            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(gainEvent));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GainMethods_WithLegacyOverload_KeepSelfSource(
            bool heal)
        {
            var environment = CreateEnvironment(
                hpCapacity: 10,
                currentHp: 8);

            var gainEvent = heal
                ? environment.Resolver.TryApplyHeal(
                    environment.State,
                    environment.ParentEvent,
                    environment.Position,
                    1)
                : environment.Resolver.TryApplyHpStatGain(
                    environment.State,
                    environment.ParentEvent,
                    environment.Position,
                    1);

            Assert.That(gainEvent, Is.Not.Null);
            Assert.That(
                gainEvent.SourceInstanceId,
                Is.EqualTo(environment.Card.InstanceId));
            Assert.That(
                gainEvent.TargetInstanceId,
                Is.EqualTo(environment.Card.InstanceId));
            Assert.That(gainEvent.IsSelfSource, Is.True);
            Assert.That(gainEvent.IsFromAnotherSource, Is.False);
        }

        [TestCase(false, 0)]
        [TestCase(false, 1)]
        [TestCase(true, 0)]
        [TestCase(true, 1)]
        public void GainMethods_WithInvalidSource_RejectWithoutSideEffects(
            bool heal,
            int requestedAmount)
        {
            var environment = CreateEnvironment(
                hpCapacity: 10,
                currentHp: 8);

            var exception = Assert.Throws<ArgumentException>(
                () => ApplyGainWithSource(
                    environment,
                    heal,
                    default(InstanceId),
                    requestedAmount));

            Assert.That(
                exception.ParamName,
                Is.EqualTo("sourceInstanceId"));
            Assert.That(environment.Card.HpCapacity, Is.EqualTo(10));
            Assert.That(environment.Card.CurrentHp, Is.EqualTo(8));
            Assert.That(environment.EventLog.Count, Is.EqualTo(1));

            var gainEvent = ApplyGainWithSource(
                environment,
                heal,
                new InstanceId(2),
                1);

            Assert.That(gainEvent, Is.Not.Null);
            Assert.That(
                gainEvent.Metadata.EventId.Value,
                Is.EqualTo(2L));
            Assert.That(
                gainEvent.Metadata.SequenceNo.Value,
                Is.EqualTo(2L));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GainMethods_WithExplicitSourceAndZeroAmount_DoNotEmit(
            bool heal)
        {
            var environment = CreateEnvironment(
                hpCapacity: 10,
                currentHp: 8);

            var gainEvent = ApplyGainWithSource(
                environment,
                heal,
                new InstanceId(2),
                0);

            Assert.That(gainEvent, Is.Null);
            Assert.That(environment.Card.HpCapacity, Is.EqualTo(10));
            Assert.That(environment.Card.CurrentHp, Is.EqualTo(8));
            Assert.That(environment.EventLog.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryApplyHeal_WithExplicitSourceAtFullHp_DoesNotEmit()
        {
            var environment = CreateEnvironment(
                hpCapacity: 10,
                currentHp: 10);

            var gainEvent = ApplyGainWithSource(
                environment,
                true,
                new InstanceId(2),
                5);

            Assert.That(gainEvent, Is.Null);
            Assert.That(environment.Card.HpCapacity, Is.EqualTo(10));
            Assert.That(environment.Card.CurrentHp, Is.EqualTo(10));
            Assert.That(environment.EventLog.Count, Is.EqualTo(1));
        }

        private static HpGainCombatEvent ApplyGainWithSource(
            TestEnvironment environment,
            bool heal,
            InstanceId sourceInstanceId,
            int requestedAmount)
        {
            return heal
                ? environment.Resolver.TryApplyHeal(
                    environment.State,
                    environment.ParentEvent,
                    sourceInstanceId,
                    environment.Position,
                    requestedAmount)
                : environment.Resolver.TryApplyHpStatGain(
                    environment.State,
                    environment.ParentEvent,
                    sourceInstanceId,
                    environment.Position,
                    requestedAmount);
        }

        private static TestEnvironment
            CreateEnvironment(
                int hpCapacity,
                int currentHp)
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
                    hpCapacity,
                    currentHp,
                    armor: 0,
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
                        new CombatSlotState[0]),
                    new CombatCardRegistry(
                        new CombatCardState[0]),
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
                    new CombatHpGainResolver(
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

            public CombatHpGainResolver Resolver
            {
                get;
                set;
            }
        }
    }
}