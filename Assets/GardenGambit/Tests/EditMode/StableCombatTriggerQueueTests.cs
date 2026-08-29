using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        StableCombatTriggerQueueTests
    {
        [Test]
        public void NewQueue_IsEmptyAndOperationsThrow()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            Assert.That(
                queue.Count,
                Is.Zero);

            Assert.That(
                queue.HasPending,
                Is.False);

            Assert.Throws<InvalidOperationException>(
                () => queue.PeekNext());

            Assert.Throws<InvalidOperationException>(
                () => queue.DequeueNext());
        }

        [Test]
        public void Enqueue_WithNullItem_ThrowsWithoutChangingQueue()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            Assert.Throws<ArgumentNullException>(
                () => queue.Enqueue(
                    null,
                    CreateKey()));

            Assert.That(
                queue.Count,
                Is.Zero);
        }

        [Test]
        public void Enqueue_WithInvalidOrderKey_ThrowsWithoutChangingQueue()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            Assert.Throws<ArgumentException>(
                () => queue.Enqueue(
                    new TestItem("invalid"),
                    default(CombatTriggerOrderKey)));

            Assert.That(
                queue.Count,
                Is.Zero);
        }

        [Test]
        public void DequeueNext_UsesLockedSourceKindPriority()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var enemySpecial =
                new TestItem("enemy");

            var card =
                new TestItem("card");

            var pet =
                new TestItem("pet");

            var slot =
                new TestItem("slot");

            queue.Enqueue(
                enemySpecial,
                CreateKey(
                    CombatTriggerSourceKind
                        .NormalEnemySpecial));

            queue.Enqueue(
                card,
                CreateKey(
                    CombatTriggerSourceKind.Card));

            queue.Enqueue(
                pet,
                CreateKey(
                    CombatTriggerSourceKind.Pet));

            queue.Enqueue(
                slot,
                CreateKey(
                    CombatTriggerSourceKind.Slot));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(slot));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(pet));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(card));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(enemySpecial));
        }

        [Test]
        public void DequeueNext_UsesHorizontalLeftToRightPriority()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var right =
                new TestItem("right");

            var left =
                new TestItem("left");

            queue.Enqueue(
                right,
                CreateKey(
                    horizontalOrder: 4));

            queue.Enqueue(
                left,
                CreateKey(
                    horizontalOrder: 0));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(left));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(right));
        }

        [Test]
        public void DequeueNext_UsesVerticalTopToBottomPriority()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var bottom =
                new TestItem("bottom");

            var top =
                new TestItem("top");

            queue.Enqueue(
                bottom,
                CreateKey(
                    horizontalOrder: 2,
                    verticalOrder: 1));

            queue.Enqueue(
                top,
                CreateKey(
                    horizontalOrder: 2,
                    verticalOrder: 0));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(top));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(bottom));
        }

        [Test]
        public void DequeueNext_WithEqualPosition_UsesPlayerFirst()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var enemy =
                new TestItem("enemy");

            var player =
                new TestItem("player");

            queue.Enqueue(
                enemy,
                CreateKey(
                    side: CombatSide.Enemy));

            queue.Enqueue(
                player,
                CreateKey(
                    side: CombatSide.Player));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(player));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(enemy));
        }

        [Test]
        public void EqualPriorityItems_PreserveEnqueueOrder()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var first =
                new TestItem("first");

            var second =
                new TestItem("second");

            var third =
                new TestItem("third");

            var key =
                CreateKey();

            queue.Enqueue(
                first,
                key);

            queue.Enqueue(
                second,
                key);

            queue.Enqueue(
                third,
                key);

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(first));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(second));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(third));

            Assert.That(
                queue.HasPending,
                Is.False);
        }

        [Test]
        public void PeekNext_DoesNotConsumeSelectedItem()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var item =
                new TestItem("item");

            queue.Enqueue(
                item,
                CreateKey());

            Assert.That(
                queue.PeekNext(),
                Is.SameAs(item));

            Assert.That(
                queue.PeekNext(),
                Is.SameAs(item));

            Assert.That(
                queue.Count,
                Is.EqualTo(1));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(item));

            Assert.That(
                queue.Count,
                Is.Zero);
        }

        [Test]
        public void LaterHigherPriorityItem_OvertakesEarlierLowerPriorityItem()
        {
            var queue =
                new StableCombatTriggerQueue<TestItem>();

            var lowerPriority =
                new TestItem("lower");

            var higherPriority =
                new TestItem("higher");

            queue.Enqueue(
                lowerPriority,
                CreateKey(
                    CombatTriggerSourceKind.Card));

            queue.Enqueue(
                higherPriority,
                CreateKey(
                    CombatTriggerSourceKind.Slot));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(higherPriority));

            Assert.That(
                queue.DequeueNext(),
                Is.SameAs(lowerPriority));
        }

        private static CombatTriggerOrderKey CreateKey(
            CombatTriggerSourceKind sourceKind =
                CombatTriggerSourceKind.Card,
            CombatSide side =
                CombatSide.Player,
            int horizontalOrder = 0,
            int verticalOrder = 0)
        {
            return new CombatTriggerOrderKey(
                sourceKind,
                side,
                horizontalOrder,
                verticalOrder);
        }

        private sealed class TestItem
        {
            public TestItem(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}