using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        NormalAttackExchangeCombatEventTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsSnapshot()
        {
            var metadata = CreateMetadata();
            var playerPosition =
                CreatePlayerPosition();

            var enemyPosition =
                CreateEnemyPosition();

            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    metadata,
                    new InstanceId(100),
                    playerPosition,
                    3,
                    new InstanceId(200),
                    enemyPosition,
                    4);

            Assert.That(
                exchangeEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.NormalAttackExchange));

            Assert.That(
                exchangeEvent.Metadata.EventId,
                Is.EqualTo(metadata.EventId));

            Assert.That(
                exchangeEvent.PlayerInstanceId,
                Is.EqualTo(new InstanceId(100)));

            Assert.That(
                exchangeEvent.PlayerPosition,
                Is.EqualTo(playerPosition));

            Assert.That(
                exchangeEvent.PlayerAttack,
                Is.EqualTo(3));

            Assert.That(
                exchangeEvent.EnemyInstanceId,
                Is.EqualTo(new InstanceId(200)));

            Assert.That(
                exchangeEvent.EnemyPosition,
                Is.EqualTo(enemyPosition));

            Assert.That(
                exchangeEvent.EnemyAttack,
                Is.EqualTo(4));
        }

        [Test]
        public void Constructor_WithZeroAttackValues_AllowsSnapshot()
        {
            var exchangeEvent =
                new NormalAttackExchangeCombatEvent(
                    CreateMetadata(),
                    new InstanceId(100),
                    CreatePlayerPosition(),
                    0,
                    new InstanceId(200),
                    CreateEnemyPosition(),
                    0);

            Assert.That(
                exchangeEvent.PlayerAttack,
                Is.Zero);

            Assert.That(
                exchangeEvent.EnemyAttack,
                Is.Zero);
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(100),
                        CreatePlayerPosition(),
                        3,
                        new InstanceId(200),
                        CreateEnemyPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithInvalidPlayerInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        default(InstanceId),
                        CreatePlayerPosition(),
                        3,
                        new InstanceId(200),
                        CreateEnemyPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithInvalidPlayerPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        default(BoardPosition),
                        3,
                        new InstanceId(200),
                        CreateEnemyPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithEnemyPositionAsPlayerPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreateEnemyPosition(),
                        3,
                        new InstanceId(200),
                        CreateEnemyPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithNegativePlayerAttack_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePlayerPosition(),
                        -1,
                        new InstanceId(200),
                        CreateEnemyPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithInvalidEnemyInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePlayerPosition(),
                        3,
                        default(InstanceId),
                        CreateEnemyPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithInvalidEnemyPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePlayerPosition(),
                        3,
                        new InstanceId(200),
                        default(BoardPosition),
                        3));
        }

        [Test]
        public void Constructor_WithPlayerPositionAsEnemyPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePlayerPosition(),
                        3,
                        new InstanceId(200),
                        CreatePlayerPosition(),
                        3));
        }

        [Test]
        public void Constructor_WithNegativeEnemyAttack_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ =
                    new NormalAttackExchangeCombatEvent(
                        CreateMetadata(),
                        new InstanceId(100),
                        CreatePlayerPosition(),
                        3,
                        new InstanceId(200),
                        CreateEnemyPosition(),
                        -1));
        }

        private static CombatEventMetadata
            CreateMetadata()
        {
            var eventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                eventId,
                new CombatSequenceNumber(1),
                null,
                eventId);
        }

        private static BoardPosition
            CreatePlayerPosition()
        {
            return new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));
        }

        private static BoardPosition
            CreateEnemyPosition()
        {
            return new BoardPosition(
                CombatSide.Enemy,
                BoardRow.Front,
                new BoardColumn(1));
        }
    }
}