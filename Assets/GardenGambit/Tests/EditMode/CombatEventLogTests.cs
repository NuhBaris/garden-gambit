using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventLogTests
    {
        [Test]
        public void NewLog_IsEmptyAndReadOnly()
        {
            var log = new CombatEventLog();

            Assert.That(log.Count, Is.Zero);
            Assert.That(log.Events, Is.Empty);

            var collection =
                (ICollection<CombatEvent>)log.Events;

            Assert.That(collection.IsReadOnly, Is.True);

            Assert.Throws<NotSupportedException>(
                () => collection.Add(
                    CreateRootEvent(1, 1)));

            Assert.That(log.Count, Is.Zero);
        }

        [Test]
        public void Append_WithRootEvent_AddsEvent()
        {
            var log = new CombatEventLog();
            var root = CreateRootEvent(1, 1);

            log.Append(root);

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.Events[0], Is.SameAs(root));
        }

        [Test]
        public void Append_WithEventChain_PreservesSequenceOrder()
        {
            var log = new CombatEventLog();

            var root =
                CreateRootEvent(1, 1);

            var child =
                CreateChildEvent(2, 2, 1, 1);

            var grandchild =
                CreateChildEvent(3, 3, 2, 1);

            log.Append(root);
            log.Append(child);
            log.Append(grandchild);

            Assert.That(log.Count, Is.EqualTo(3));
            Assert.That(log.Events[0], Is.SameAs(root));
            Assert.That(log.Events[1], Is.SameAs(child));
            Assert.That(
                log.Events[2],
                Is.SameAs(grandchild));
        }

        [Test]
        public void Append_WithNullEvent_Throws()
        {
            var log = new CombatEventLog();

            Assert.Throws<ArgumentNullException>(
                () => log.Append(null));

            Assert.That(log.Count, Is.Zero);
        }

        [Test]
        public void Append_WithDuplicateEventId_ThrowsWithoutChangingLog()
        {
            var log = new CombatEventLog();

            var first =
                CreateRootEvent(1, 1);

            var duplicate =
                CreateRootEvent(1, 2);

            log.Append(first);

            Assert.Throws<ArgumentException>(
                () => log.Append(duplicate));

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.Events[0], Is.SameAs(first));
        }

        [TestCase(3L)]
        [TestCase(2L)]
        public void Append_WithNonIncreasingSequence_ThrowsWithoutChangingLog(
            long invalidSequence)
        {
            var log = new CombatEventLog();

            var first =
                CreateRootEvent(1, 3);

            var invalid =
                CreateRootEvent(
                    2,
                    invalidSequence);

            log.Append(first);

            Assert.Throws<ArgumentException>(
                () => log.Append(invalid));

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log.Events[0], Is.SameAs(first));
        }

        [Test]
        public void Append_WithMissingParent_ThrowsWithoutChangingLog()
        {
            var log = new CombatEventLog();

            var child =
                CreateChildEvent(2, 2, 1, 1);

            Assert.Throws<ArgumentException>(
                () => log.Append(child));

            Assert.That(log.Count, Is.Zero);
        }

        [Test]
        public void Append_WithDifferentParentRoot_ThrowsWithoutChangingLog()
        {
            var log = new CombatEventLog();

            var firstRoot =
                CreateRootEvent(1, 1);

            var secondRoot =
                CreateRootEvent(2, 2);

            var invalidChild =
                CreateChildEvent(3, 3, 1, 2);

            log.Append(firstRoot);
            log.Append(secondRoot);

            Assert.Throws<ArgumentException>(
                () => log.Append(invalidChild));

            Assert.That(log.Count, Is.EqualTo(2));
        }

        [Test]
        public void Append_WithMultipleRoots_AllowsIndependentChains()
        {
            var log = new CombatEventLog();

            var firstRoot =
                CreateRootEvent(1, 1);

            var secondRoot =
                CreateRootEvent(2, 2);

            log.Append(firstRoot);
            log.Append(secondRoot);

            Assert.That(log.Count, Is.EqualTo(2));

            Assert.That(
                log.Events[0].Metadata.IsTriggerRoot,
                Is.True);

            Assert.That(
                log.Events[1].Metadata.IsTriggerRoot,
                Is.True);
        }

        [Test]
        public void ContainsEvent_WithExistingEvent_ReturnsTrue()
        {
            var log = new CombatEventLog();
            var root = CreateRootEvent(1, 1);

            log.Append(root);

            Assert.That(
                log.ContainsEvent(
                    new CombatEventId(1)),
                Is.True);
        }

        [Test]
        public void ContainsEvent_WithMissingEvent_ReturnsFalse()
        {
            var log = new CombatEventLog();

            Assert.That(
                log.ContainsEvent(
                    new CombatEventId(999)),
                Is.False);
        }

        [Test]
        public void ContainsEvent_WithInvalidEventId_Throws()
        {
            var log = new CombatEventLog();

            Assert.Throws<ArgumentException>(
                () => log.ContainsEvent(
                    default(CombatEventId)));
        }

        [Test]
        public void GetEvent_WithExistingEvent_ReturnsEvent()
        {
            var log = new CombatEventLog();
            var expected = CreateRootEvent(1, 1);

            log.Append(expected);

            var result =
                log.GetEvent(new CombatEventId(1));

            Assert.That(result, Is.SameAs(expected));
        }

        [Test]
        public void GetEvent_WithMissingEvent_Throws()
        {
            var log = new CombatEventLog();

            Assert.Throws<KeyNotFoundException>(
                () => log.GetEvent(
                    new CombatEventId(999)));
        }

        [Test]
        public void GetEvent_WithInvalidEventId_Throws()
        {
            var log = new CombatEventLog();

            Assert.Throws<ArgumentException>(
                () => log.GetEvent(
                    default(CombatEventId)));
        }

        private static CombatEvent
            CreateRootEvent(
                long eventId,
                long sequenceNo)
        {
            var rootEventId =
                new CombatEventId(eventId);

            var metadata =
                new CombatEventMetadata(
                    rootEventId,
                    new CombatSequenceNumber(
                        sequenceNo),
                    null,
                    rootEventId);

            return new TestCombatEvent(metadata);
        }

        private static CombatEvent
            CreateChildEvent(
                long eventId,
                long sequenceNo,
                long parentEventId,
                long triggerRootId)
        {
            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(eventId),
                    new CombatSequenceNumber(
                        sequenceNo),
                    new CombatEventId(
                        parentEventId),
                    new CombatEventId(
                        triggerRootId));

            return new TestCombatEvent(metadata);
        }

        private sealed class TestCombatEvent :
            CombatEvent
        {
            public TestCombatEvent(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.CombatStarted)
            {
            }
        }
    }
}