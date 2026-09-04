using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackEventBatchTests
    {
        [Test]
        public void Constructor_WithValidEvents_SetsState()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            var playerAttackEvent =
                CreatePlayerAttack(
                    exchangeEvent);

            var enemyAttackEvent =
                CreateEnemyAttack(
                    exchangeEvent);

            var batch =
                new CombatNormalAttackEventBatch(
                    exchangeEvent,
                    playerAttackEvent,
                    enemyAttackEvent);

            Assert.That(
                batch.ExchangeEvent,
                Is.SameAs(exchangeEvent));

            Assert.That(
                batch.PlayerAttackEvent,
                Is.SameAs(
                    playerAttackEvent));

            Assert.That(
                batch.EnemyAttackEvent,
                Is.SameAs(
                    enemyAttackEvent));
        }

        [Test]
        public void Constructor_WithNullExchange_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        null,
                        CreatePlayerAttack(
                            exchangeEvent),
                        CreateEnemyAttack(
                            exchangeEvent)));
        }

        [Test]
        public void Constructor_WithNullPlayerAttack_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        null,
                        CreateEnemyAttack(
                            exchangeEvent)));
        }

        [Test]
        public void Constructor_WithNullEnemyAttack_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent),
                        null));
        }

        [Test]
        public void Constructor_WithEnemyEventAsPlayerAttack_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreateEnemyAttack(
                            exchangeEvent),
                        CreateEnemyAttack(
                            exchangeEvent)));
        }

        [Test]
        public void Constructor_WithPlayerEventAsEnemyAttack_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent),
                        CreatePlayerAttack(
                            exchangeEvent)));
        }

        [Test]
        public void Constructor_WithWrongParents_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            var wrongParentMetadata =
                CreateAttackMetadata(
                    eventId: 10,
                    sequenceNo: 10,
                    parentEventId:
                        new CombatEventId(999),
                    triggerRootId:
                        exchangeEvent.Metadata
                            .TriggerRootId);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            wrongParentMetadata),
                        CreateEnemyAttack(
                            exchangeEvent)));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent),
                        CreateEnemyAttack(
                            exchangeEvent,
                            wrongParentMetadata)));
        }

        [Test]
        public void Constructor_WithWrongTriggerRoots_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            var wrongRootMetadata =
                CreateAttackMetadata(
                    eventId: 10,
                    sequenceNo: 10,
                    parentEventId:
                        exchangeEvent.Metadata.EventId,
                    triggerRootId:
                        new CombatEventId(999));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            wrongRootMetadata),
                        CreateEnemyAttack(
                            exchangeEvent)));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent),
                        CreateEnemyAttack(
                            exchangeEvent,
                            wrongRootMetadata)));
        }

        [Test]
        public void Constructor_WithMismatchedPlayerFields_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            var enemyAttackEvent =
                CreateEnemyAttack(
                    exchangeEvent);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            attackerInstanceId:
                                new InstanceId(999)),
                        enemyAttackEvent));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            attackerPosition:
                                new BoardPosition(
                                    CombatSide.Player,
                                    BoardRow.Back,
                                    new BoardColumn(2))),
                        enemyAttackEvent));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            targetInstanceId:
                                new InstanceId(999)),
                        enemyAttackEvent));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            targetPosition:
                                new BoardPosition(
                                    CombatSide.Enemy,
                                    BoardRow.Back,
                                    new BoardColumn(2))),
                        enemyAttackEvent));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        CreatePlayerAttack(
                            exchangeEvent,
                            baseDamage: 6),
                        enemyAttackEvent));
        }

        [Test]
        public void Constructor_WithMismatchedEnemyFields_Throws()
        {
            var exchangeEvent =
                CreateExchangeEvent();

            var playerAttackEvent =
                CreatePlayerAttack(
                    exchangeEvent);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        playerAttackEvent,
                        CreateEnemyAttack(
                            exchangeEvent,
                            attackerInstanceId:
                                new InstanceId(999))));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        playerAttackEvent,
                        CreateEnemyAttack(
                            exchangeEvent,
                            attackerPosition:
                                new BoardPosition(
                                    CombatSide.Enemy,
                                    BoardRow.Back,
                                    new BoardColumn(2)))));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        playerAttackEvent,
                        CreateEnemyAttack(
                            exchangeEvent,
                            targetInstanceId:
                                new InstanceId(999))));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        playerAttackEvent,
                        CreateEnemyAttack(
                            exchangeEvent,
                            targetPosition:
                                new BoardPosition(
                                    CombatSide.Player,
                                    BoardRow.Back,
                                    new BoardColumn(2)))));

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatNormalAttackEventBatch(
                        exchangeEvent,
                        playerAttackEvent,
                        CreateEnemyAttack(
                            exchangeEvent,
                            baseDamage: 8)));
        }

        private static
            NormalAttackExchangeCombatEvent
            CreateExchangeEvent()
        {
            var eventId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    eventId,
                    new CombatSequenceNumber(1),
                    null,
                    eventId);

            return new
                NormalAttackExchangeCombatEvent(
                    metadata,
                    new InstanceId(1),
                    new BoardPosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    playerAttack: 5,
                    new InstanceId(101),
                    new BoardPosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        new BoardColumn(1)),
                    enemyAttack: 7);
        }

        private static NormalAttackCombatEvent
            CreatePlayerAttack(
                NormalAttackExchangeCombatEvent
                    exchangeEvent,
                CombatEventMetadata? metadata = null,
                InstanceId? attackerInstanceId = null,
                BoardPosition? attackerPosition = null,
                InstanceId? targetInstanceId = null,
                BoardPosition? targetPosition = null,
                int? baseDamage = null)
        {
            return new NormalAttackCombatEvent(
                metadata ??
                    CreateAttackMetadata(
                        eventId: 2,
                        sequenceNo: 2,
                        parentEventId:
                            exchangeEvent.Metadata.EventId,
                        triggerRootId:
                            exchangeEvent.Metadata
                                .TriggerRootId),
                attackerInstanceId ??
                    exchangeEvent.PlayerInstanceId,
                attackerPosition ??
                    exchangeEvent.PlayerPosition,
                targetInstanceId ??
                    exchangeEvent.EnemyInstanceId,
                targetPosition ??
                    exchangeEvent.EnemyPosition,
                baseDamage ??
                    exchangeEvent.PlayerAttack);
        }

        private static NormalAttackCombatEvent
            CreateEnemyAttack(
                NormalAttackExchangeCombatEvent
                    exchangeEvent,
                CombatEventMetadata? metadata = null,
                InstanceId? attackerInstanceId = null,
                BoardPosition? attackerPosition = null,
                InstanceId? targetInstanceId = null,
                BoardPosition? targetPosition = null,
                int? baseDamage = null)
        {
            return new NormalAttackCombatEvent(
                metadata ??
                    CreateAttackMetadata(
                        eventId: 3,
                        sequenceNo: 3,
                        parentEventId:
                            exchangeEvent.Metadata.EventId,
                        triggerRootId:
                            exchangeEvent.Metadata
                                .TriggerRootId),
                attackerInstanceId ??
                    exchangeEvent.EnemyInstanceId,
                attackerPosition ??
                    exchangeEvent.EnemyPosition,
                targetInstanceId ??
                    exchangeEvent.PlayerInstanceId,
                targetPosition ??
                    exchangeEvent.PlayerPosition,
                baseDamage ??
                    exchangeEvent.EnemyAttack);
        }

        private static CombatEventMetadata
            CreateAttackMetadata(
                long eventId,
                long sequenceNo,
                CombatEventId parentEventId,
                CombatEventId triggerRootId)
        {
            return new CombatEventMetadata(
                new CombatEventId(
                    eventId),
                new CombatSequenceNumber(
                    sequenceNo),
                parentEventId,
                triggerRootId);
        }
    }
}