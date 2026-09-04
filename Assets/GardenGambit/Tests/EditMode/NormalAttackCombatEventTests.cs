using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        NormalAttackCombatEventTests
    {
        [Test]
        public void Constructor_WithPlayerAttack_SetsState()
        {
            var metadata =
                CreateDirectRootChildMetadata();

            var attackerInstanceId =
                new InstanceId(1);

            var targetInstanceId =
                new InstanceId(101);

            var attackerPosition =
                CreatePosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    1);

            var targetPosition =
                CreatePosition(
                    CombatSide.Enemy,
                    BoardRow.Back,
                    2);

            var attackEvent =
                new NormalAttackCombatEvent(
                    metadata,
                    attackerInstanceId,
                    attackerPosition,
                    targetInstanceId,
                    targetPosition,
                    baseDamage: 5);

            Assert.That(
                attackEvent.Kind,
                Is.EqualTo(
                    CombatEventKind.NormalAttack));

            Assert.That(
                attackEvent.Metadata.EventId,
                Is.EqualTo(
                    metadata.EventId));

            Assert.That(
                attackEvent.AttackerInstanceId,
                Is.EqualTo(
                    attackerInstanceId));

            Assert.That(
                attackEvent.AttackerPosition,
                Is.EqualTo(
                    attackerPosition));

            Assert.That(
                attackEvent.AttackerSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                attackEvent.TargetInstanceId,
                Is.EqualTo(
                    targetInstanceId));

            Assert.That(
                attackEvent.TargetPosition,
                Is.EqualTo(
                    targetPosition));

            Assert.That(
                attackEvent.TargetSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(5));

            Assert.That(
                attackEvent.IsPlayerAttack,
                Is.True);

            Assert.That(
                attackEvent.IsEnemyAttack,
                Is.False);
        }

        [Test]
        public void Constructor_WithEnemyAttack_SetsSideFlags()
        {
            var attackEvent =
                CreateValidEvent(
                    attackerSide:
                        CombatSide.Enemy,
                    targetSide:
                        CombatSide.Player,
                    baseDamage: 7);

            Assert.That(
                attackEvent.AttackerSide,
                Is.EqualTo(
                    CombatSide.Enemy));

            Assert.That(
                attackEvent.TargetSide,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                attackEvent.IsEnemyAttack,
                Is.True);

            Assert.That(
                attackEvent.IsPlayerAttack,
                Is.False);
        }

        [Test]
        public void Constructor_WithZeroDamage_AllowsEvent()
        {
            var attackEvent =
                CreateValidEvent(
                    attackerSide:
                        CombatSide.Player,
                    targetSide:
                        CombatSide.Enemy,
                    baseDamage: 0);

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithInvalidMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        default(CombatEventMetadata),
                        new InstanceId(1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        new InstanceId(101),
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithRootMetadata_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateRootMetadata(),
                        new InstanceId(1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        new InstanceId(101),
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithNestedTriggerChain_AllowsEvent()
        {
            var metadata =
                CreateNonDirectRootChildMetadata();

            var attackEvent =
                new NormalAttackCombatEvent(
                    metadata,
                    new InstanceId(1),
                    CreatePosition(
                        CombatSide.Player,
                        BoardRow.Front,
                        1),
                    new InstanceId(101),
                    CreatePosition(
                        CombatSide.Enemy,
                        BoardRow.Front,
                        1),
                    baseDamage: 5);

            Assert.That(
                attackEvent.Metadata.ParentEventId.Value,
                Is.EqualTo(
                    new CombatEventId(2)));

            Assert.That(
                attackEvent.Metadata.TriggerRootId,
                Is.EqualTo(
                    new CombatEventId(1)));

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WithInvalidAttackerInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateDirectRootChildMetadata(),
                        default(InstanceId),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        new InstanceId(101),
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithInvalidAttackerPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateDirectRootChildMetadata(),
                        new InstanceId(1),
                        default(BoardPosition),
                        new InstanceId(101),
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithInvalidTargetInstanceId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateDirectRootChildMetadata(),
                        new InstanceId(1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        default(InstanceId),
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithInvalidTargetPosition_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateDirectRootChildMetadata(),
                        new InstanceId(1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        new InstanceId(101),
                        default(BoardPosition),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithSameCardInstances_Throws()
        {
            var instanceId =
                new InstanceId(1);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateDirectRootChildMetadata(),
                        instanceId,
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        instanceId,
                        CreatePosition(
                            CombatSide.Enemy,
                            BoardRow.Front,
                            1),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithSameSidePositions_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new NormalAttackCombatEvent(
                        CreateDirectRootChildMetadata(),
                        new InstanceId(1),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Front,
                            1),
                        new InstanceId(2),
                        CreatePosition(
                            CombatSide.Player,
                            BoardRow.Back,
                            2),
                        baseDamage: 5));
        }

        [Test]
        public void Constructor_WithNegativeDamage_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () => _ =
                    CreateValidEvent(
                        attackerSide:
                            CombatSide.Player,
                        targetSide:
                            CombatSide.Enemy,
                        baseDamage: -1));
        }

        [Test]
        public void Constructor_WithMaximumDamage_AllowsEvent()
        {
            var attackEvent =
                CreateValidEvent(
                    attackerSide:
                        CombatSide.Player,
                    targetSide:
                        CombatSide.Enemy,
                    baseDamage: int.MaxValue);

            Assert.That(
                attackEvent.BaseDamage,
                Is.EqualTo(
                    int.MaxValue));
        }

        private static NormalAttackCombatEvent
            CreateValidEvent(
                CombatSide attackerSide,
                CombatSide targetSide,
                int baseDamage)
        {
            return new NormalAttackCombatEvent(
                CreateDirectRootChildMetadata(),
                new InstanceId(1),
                CreatePosition(
                    attackerSide,
                    BoardRow.Front,
                    1),
                new InstanceId(101),
                CreatePosition(
                    targetSide,
                    BoardRow.Front,
                    1),
                baseDamage);
        }

        private static BoardPosition
            CreatePosition(
                CombatSide side,
                BoardRow row,
                int column)
        {
            return new BoardPosition(
                side,
                row,
                new BoardColumn(column));
        }

        private static CombatEventMetadata
            CreateRootMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                rootEventId,
                new CombatSequenceNumber(1),
                null,
                rootEventId);
        }

        private static CombatEventMetadata
            CreateDirectRootChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                rootEventId,
                rootEventId);
        }

        private static CombatEventMetadata
            CreateNonDirectRootChildMetadata()
        {
            var rootEventId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(3),
                new CombatSequenceNumber(3),
                new CombatEventId(2),
                rootEventId);
        }
    }
}