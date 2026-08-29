using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatEventTriggerBatchTests
    {
        [Test]
        public void Constructor_WithValidValues_SetsSnapshot()
        {
            var sourceEvent =
                CreateSourceEvent();

            var firstCandidate =
                CreateCandidate(
                    "First",
                    0);

            var secondCandidate =
                CreateCandidate(
                    "Second",
                    1);

            var batch =
                new CombatEventTriggerBatch<TestTrigger>(
                    sourceEvent,
                    new[]
                    {
                        firstCandidate,
                        secondCandidate
                    });

            Assert.That(
                batch.SourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                batch.Count,
                Is.EqualTo(2));

            Assert.That(
                batch.Candidates[0],
                Is.SameAs(firstCandidate));

            Assert.That(
                batch.Candidates[1],
                Is.SameAs(secondCandidate));
        }

        [Test]
        public void Constructor_WithEmptyCandidates_AllowsBatch()
        {
            var sourceEvent =
                CreateSourceEvent();

            var batch =
                new CombatEventTriggerBatch<TestTrigger>(
                    sourceEvent,
                    new CombatTriggerCandidate<
                        TestTrigger>[0]);

            Assert.That(
                batch.SourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                batch.Count,
                Is.Zero);

            Assert.That(
                batch.Candidates,
                Is.Empty);
        }

        [Test]
        public void Constructor_WithNullSourceEvent_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventTriggerBatch<TestTrigger>(
                        null,
                        new CombatTriggerCandidate<
                            TestTrigger>[0]));
        }

        [Test]
        public void Constructor_WithNullCandidates_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatEventTriggerBatch<TestTrigger>(
                        CreateSourceEvent(),
                        null));
        }

        [Test]
        public void Constructor_WithNullCandidate_Throws()
        {
            var candidates =
                new CombatTriggerCandidate<TestTrigger>[]
                {
                    CreateCandidate(
                        "First",
                        0),
                    null
                };

            Assert.Throws<ArgumentException>(
                () => _ =
                    new CombatEventTriggerBatch<TestTrigger>(
                        CreateSourceEvent(),
                        candidates));
        }

        [Test]
        public void Constructor_CopiesSourceCollection()
        {
            var firstCandidate =
                CreateCandidate(
                    "First",
                    0);

            var secondCandidate =
                CreateCandidate(
                    "Second",
                    1);

            var sourceCandidates =
                new List<
                    CombatTriggerCandidate<TestTrigger>>
                {
                    firstCandidate
                };

            var batch =
                new CombatEventTriggerBatch<TestTrigger>(
                    CreateSourceEvent(),
                    sourceCandidates);

            sourceCandidates.Add(
                secondCandidate);

            Assert.That(
                sourceCandidates.Count,
                Is.EqualTo(2));

            Assert.That(
                batch.Count,
                Is.EqualTo(1));

            Assert.That(
                batch.Candidates[0],
                Is.SameAs(firstCandidate));
        }

        private static TestCombatEvent
            CreateSourceEvent()
        {
            var metadataFactory =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());

            return new TestCombatEvent(
                metadataFactory.CreateRoot());
        }

        private static CombatTriggerCandidate<TestTrigger>
            CreateCandidate(
                string name,
                int horizontalOrder)
        {
            var orderKey =
                new CombatTriggerOrderKey(
                    CombatTriggerSourceKind.Slot,
                    CombatSide.Player,
                    horizontalOrder,
                    0);

            return new CombatTriggerCandidate<TestTrigger>(
                orderKey,
                new TestTrigger(name));
        }

        private sealed class TestTrigger
        {
            public TestTrigger(string name)
            {
                Name = name;
            }

            public string Name { get; }
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
    }
}