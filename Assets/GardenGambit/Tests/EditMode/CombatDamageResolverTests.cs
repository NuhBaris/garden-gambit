using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatDamageResolverTests
    {
        [Test]
        public void Constructor_WithNullMetadataFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatDamageResolver(
                    null,
                    new CombatEventLog()));
        }

        [Test]
        public void Constructor_WithNullEventLog_Throws()
        {
            var factory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatDamageResolver(
                    factory,
                    null));
        }

        [Test]
        public void ApplyResolvedCardDamage_WithValidInput_UpdatesTargetAndLogsEvent()
        {
            var environment =
                CreateEnvironment();

            var damageEvent =
                environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5);

            Assert.That(
                environment.TargetCard.Armor,
                Is.Zero);

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                environment.SourceCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                damageEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.DamageApplied));

            Assert.That(
                damageEvent.Result.ArmorAbsorbed,
                Is.EqualTo(2));

            Assert.That(
                damageEvent.Result.HpDamage,
                Is.EqualTo(3));

            Assert.That(
                damageEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.EventId));

            Assert.That(
                damageEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    environment.ParentEvent
                        .Metadata.TriggerRootId));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));

            Assert.That(
                environment.EventLog.Events[1],
                Is.SameAs(damageEvent));
        }

        [Test]
        public void ApplyResolvedCardDamage_WithZeroDamage_LogsValidNoOpEvent()
        {
            var environment =
                CreateEnvironment();

            var damageEvent =
                environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        0);

            Assert.That(
                environment.TargetCard.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                damageEvent.Result.IsValid,
                Is.True);

            Assert.That(
                damageEvent.Result.IncomingDamage,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void ApplyResolvedCardDamage_WithNegativeDamage_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        -1));

            Assert.That(
                environment.TargetCard.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyResolvedCardDamage_WithUnloggedParent_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var unloggedParent =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            Assert.Throws<KeyNotFoundException>(
                () => environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        unloggedParent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5));

            Assert.That(
                environment.TargetCard.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyResolvedCardDamage_WithDifferentParentObject_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var differentParentObject =
                new TestCombatEvent(
                    environment.ParentEvent.Metadata);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        differentParentObject,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyResolvedCardDamage_WithEmptyTargetSlot_ThrowsWithoutLogging()
        {
            var environment =
                CreateEnvironment();

            environment.State.Enemy.RemoveCard(
                environment.TargetPosition);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyResolvedCardDamage_WhenHpWouldOverflow_ThrowsWithoutChangingStateOrLog()
        {
            var environment =
                CreateEnvironment(
                    targetCurrentHp: int.MinValue,
                    targetArmor: 0);

            Assert.Throws<OverflowException>(
                () => environment.Resolver
                    .ApplyResolvedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        1));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(int.MinValue));

            Assert.That(
                environment.TargetCard.Armor,
                Is.Zero);

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithValidMetadata_UpdatesTargetAndLogsEvent()
        {
            var environment =
                CreateEnvironment();

            var preparedMetadata =
                environment.MetadataFactory.CreateChild(
                    environment.ParentEvent.Metadata);

            var damageEvent =
                environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        preparedMetadata);

            Assert.That(
                environment.TargetCard.Armor,
                Is.Zero);

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(4));

            Assert.That(
                damageEvent.Metadata.EventId,
                Is.EqualTo(
                    preparedMetadata.EventId));

            Assert.That(
                damageEvent.Metadata.SequenceNo,
                Is.EqualTo(
                    preparedMetadata.SequenceNo));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithInvalidMetadata_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        default(CombatEventMetadata)));

            Assert.That(
                environment.TargetCard.Armor,
                Is.EqualTo(2));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithDifferentParentId_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(3),
                    new CombatSequenceNumber(3),
                    new CombatEventId(2),
                    environment.ParentEvent
                        .Metadata.TriggerRootId);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        metadata));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithDifferentTriggerRoot_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(3),
                    new CombatSequenceNumber(3),
                    environment.ParentEvent
                        .Metadata.EventId,
                    new CombatEventId(2));

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        metadata));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithSequenceNotAfterParent_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(1),
                    environment.ParentEvent
                        .Metadata.EventId,
                    environment.ParentEvent
                        .Metadata.TriggerRootId);

            Assert.Throws<ArgumentException>(
                () => environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        metadata));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithDuplicateEventId_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var secondRoot =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(secondRoot);

            var duplicateMetadata =
                new CombatEventMetadata(
                    secondRoot.Metadata.EventId,
                    new CombatSequenceNumber(3),
                    environment.ParentEvent
                        .Metadata.EventId,
                    environment.ParentEvent
                        .Metadata.TriggerRootId);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        duplicateMetadata));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void ApplyPreparedCardDamage_WithSequenceBehindLog_ThrowsWithoutChangingState()
        {
            var environment =
                CreateEnvironment();

            var secondRoot =
                new TestCombatEvent(
                    environment.MetadataFactory
                        .CreateRoot());

            environment.EventLog.Append(secondRoot);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(3),
                    new CombatSequenceNumber(2),
                    environment.ParentEvent
                        .Metadata.EventId,
                    environment.ParentEvent
                        .Metadata.TriggerRootId);

            Assert.Throws<InvalidOperationException>(
                () => environment.Resolver
                    .ApplyPreparedCardDamage(
                        environment.State,
                        environment.ParentEvent,
                        environment.SourcePosition,
                        environment.TargetPosition,
                        5,
                        metadata));

            Assert.That(
                environment.TargetCard.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.EventLog.Count,
                Is.EqualTo(2));
        }

        private static TestEnvironment CreateEnvironment(
            int targetCurrentHp = 7,
            int targetArmor = 2)
        {
            var sourcePosition =
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1));

            var targetPosition =
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1));

            var sourceCard = CreateCard(
                "card.source",
                100,
                7,
                0);

            var targetCard = CreateCard(
                "card.target",
                200,
                targetCurrentHp,
                targetArmor);

            var player = CreateSideState(
                CombatSide.Player,
                sourceCard,
                sourcePosition);

            var enemy = CreateSideState(
                CombatSide.Enemy,
                targetCard,
                targetPosition);

            var state =
                new CombatState(player, enemy);

            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            var eventLog =
                new CombatEventLog();

            var parentEvent =
                new TestCombatEvent(
                    metadataFactory.CreateRoot());

            eventLog.Append(parentEvent);

            var resolver =
                new CombatDamageResolver(
                    metadataFactory,
                    eventLog);

            return new TestEnvironment
            {
                State = state,
                SourceCard = sourceCard,
                TargetCard = targetCard,
                SourcePosition = sourcePosition,
                TargetPosition = targetPosition,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                ParentEvent = parentEvent,
                Resolver = resolver
            };
        }

        private static CombatSideState CreateSideState(
            CombatSide side,
            CombatCardState card,
            BoardPosition position)
        {
            var slot = new CombatSlotState(
                new SlotId(1),
                position,
                card.InstanceId);

            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new[] { slot }),
                new CombatCardRegistry(
                    new[] { card }),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatCardState CreateCard(
            string definitionId,
            long instanceId,
            int currentHp,
            int armor)
        {
            return new CombatCardState(
                new DefinitionId(definitionId),
                new InstanceId(instanceId),
                new CardRank(2),
                7,
                currentHp,
                armor,
                3);
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }

            public CombatCardState SourceCard
            {
                get;
                set;
            }

            public CombatCardState TargetCard
            {
                get;
                set;
            }

            public BoardPosition SourcePosition
            {
                get;
                set;
            }

            public BoardPosition TargetPosition
            {
                get;
                set;
            }

            public CombatEventMetadataFactory
                MetadataFactory
            {
                get;
                set;
            }

            public CombatEventLog EventLog
            {
                get;
                set;
            }

            public CombatEvent ParentEvent
            {
                get;
                set;
            }

            public CombatDamageResolver Resolver
            {
                get;
                set;
            }
        }
    }
}