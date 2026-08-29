using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatTriggerDispatcherTests
    {
        [Test]
        public void Constructor_CreatesEmptyDispatcher()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            Assert.That(
                dispatcher.Count,
                Is.Zero);

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void Enqueue_WithNullCandidate_Throws()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            Assert.Throws<ArgumentNullException>(
                () => dispatcher.Enqueue(null));

            Assert.That(
                dispatcher.Count,
                Is.Zero);
        }

        [Test]
        public void Enqueue_WithValidCandidate_AddsTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.HasPending,
                Is.True);

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(trigger));
        }

        [Test]
        public void Enqueue_WithDifferentOrderKeys_UsesDeterministicPriority()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var petTrigger =
                new TestTrigger("Pet");

            var slotTrigger =
                new TestTrigger("Slot");

            dispatcher.Enqueue(
                CreateCandidate(
                    petTrigger,
                    CombatTriggerSourceKind.Pet,
                    0,
                    0));

            dispatcher.Enqueue(
                CreateCandidate(
                    slotTrigger,
                    CombatTriggerSourceKind.Slot,
                    4,
                    1));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(slotTrigger));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(petTrigger));

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void Enqueue_WithEqualOrderKeys_PreservesEnqueueOrder()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var firstTrigger =
                new TestTrigger("First");

            var secondTrigger =
                new TestTrigger("Second");

            var thirdTrigger =
                new TestTrigger("Third");

            dispatcher.Enqueue(
                CreateCandidate(
                    firstTrigger,
                    CombatTriggerSourceKind.Card,
                    2,
                    1));

            dispatcher.Enqueue(
                CreateCandidate(
                    secondTrigger,
                    CombatTriggerSourceKind.Card,
                    2,
                    1));

            dispatcher.Enqueue(
                CreateCandidate(
                    thirdTrigger,
                    CombatTriggerSourceKind.Card,
                    2,
                    1));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(firstTrigger));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(secondTrigger));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(thirdTrigger));
        }

        [Test]
        public void ProcessNext_InvokesCallbackAndRemovesTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            TestTrigger processedTrigger = null;

            var returnedTrigger =
                dispatcher.ProcessNext(
                    currentTrigger =>
                    {
                        processedTrigger =
                            currentTrigger;
                    });

            Assert.That(
                processedTrigger,
                Is.SameAs(trigger));

            Assert.That(
                returnedTrigger,
                Is.SameAs(trigger));

            Assert.That(
                dispatcher.Count,
                Is.Zero);

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void ProcessNext_WithNullCallback_ThrowsWithoutRemovingTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.Throws<ArgumentNullException>(
                () => dispatcher.ProcessNext(null));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(trigger));
        }

        [Test]
        public void ProcessNext_WhenCallbackThrows_LeavesTriggerPending()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.ProcessNext(
                    currentTrigger =>
                        throw new InvalidOperationException(
                            "Test trigger failure.")));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.HasPending,
                Is.True);

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(trigger));
        }

        [Test]
        public void EmptyDispatcher_PeekAndProcessThrow()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.PeekNext());

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.ProcessNext(
                    trigger => { }));

            Assert.That(
                dispatcher.Count,
                Is.Zero);
        }

        [Test]
        public void Drain_WithEmptyDispatcher_ReturnsZero()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var callbackCount = 0;

            var processedCount =
                dispatcher.Drain(
                    1,
                    trigger =>
                        callbackCount++);

            Assert.That(
                processedCount,
                Is.Zero);

            Assert.That(
                callbackCount,
                Is.Zero);

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WithInvalidMaximumCount_ThrowsWithoutRemovingTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => dispatcher.Drain(
                    0,
                    currentTrigger => { }));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => dispatcher.Drain(
                    -1,
                    currentTrigger => { }));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(trigger));
        }

        [Test]
        public void Drain_WithNullCallback_ThrowsWithoutRemovingTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.Throws<ArgumentNullException>(
                () => dispatcher.Drain(
                    1,
                    null));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(trigger));
        }

        [Test]
        public void Drain_WithExistingTriggers_ProcessesAllInPriorityOrder()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var cardTrigger =
                new TestTrigger("Card");

            var petTrigger =
                new TestTrigger("Pet");

            var slotTrigger =
                new TestTrigger("Slot");

            dispatcher.Enqueue(
                CreateCandidate(
                    cardTrigger,
                    CombatTriggerSourceKind.Card,
                    0,
                    0));

            dispatcher.Enqueue(
                CreateCandidate(
                    petTrigger,
                    CombatTriggerSourceKind.Pet,
                    0,
                    0));

            dispatcher.Enqueue(
                CreateCandidate(
                    slotTrigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            var processedTriggers =
                new List<TestTrigger>();

            var processedCount =
                dispatcher.Drain(
                    3,
                    processedTriggers.Add);

            Assert.That(
                processedCount,
                Is.EqualTo(3));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(slotTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(petTrigger));

            Assert.That(
                processedTriggers[2],
                Is.SameAs(cardTrigger));

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenCallbackEnqueuesTrigger_ProcessesItInSameDrain()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var initialTrigger =
                new TestTrigger("Initial");

            var appendedTrigger =
                new TestTrigger("Appended");

            dispatcher.Enqueue(
                CreateCandidate(
                    initialTrigger,
                    CombatTriggerSourceKind.Slot,
                    1,
                    0));

            var processedTriggers =
                new List<TestTrigger>();

            var processedCount =
                dispatcher.Drain(
                    2,
                    currentTrigger =>
                    {
                        processedTriggers.Add(
                            currentTrigger);

                        if (!ReferenceEquals(
                                currentTrigger,
                                initialTrigger))
                        {
                            return;
                        }

                        dispatcher.Enqueue(
                            CreateCandidate(
                                appendedTrigger,
                                CombatTriggerSourceKind.Slot,
                                0,
                                0));
                    });

            Assert.That(
                processedCount,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(initialTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(appendedTrigger));

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenTriggerCountEqualsBudget_CompletesSuccessfully()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            dispatcher.Enqueue(
                CreateCandidate(
                    new TestTrigger("First"),
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            dispatcher.Enqueue(
                CreateCandidate(
                    new TestTrigger("Second"),
                    CombatTriggerSourceKind.Slot,
                    1,
                    0));

            var callbackCount = 0;

            var processedCount =
                dispatcher.Drain(
                    2,
                    trigger =>
                        callbackCount++);

            Assert.That(
                processedCount,
                Is.EqualTo(2));

            Assert.That(
                callbackCount,
                Is.EqualTo(2));

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void Drain_WhenBudgetIsExhausted_LeavesRemainingTriggerPending()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var firstTrigger =
                new TestTrigger("First");

            var secondTrigger =
                new TestTrigger("Second");

            var thirdTrigger =
                new TestTrigger("Third");

            dispatcher.Enqueue(
                CreateCandidate(
                    firstTrigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            dispatcher.Enqueue(
                CreateCandidate(
                    secondTrigger,
                    CombatTriggerSourceKind.Slot,
                    1,
                    0));

            dispatcher.Enqueue(
                CreateCandidate(
                    thirdTrigger,
                    CombatTriggerSourceKind.Slot,
                    2,
                    0));

            var processedTriggers =
                new List<TestTrigger>();

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.Drain(
                    2,
                    processedTriggers.Add));

            Assert.That(
                processedTriggers.Count,
                Is.EqualTo(2));

            Assert.That(
                processedTriggers[0],
                Is.SameAs(firstTrigger));

            Assert.That(
                processedTriggers[1],
                Is.SameAs(secondTrigger));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(thirdTrigger));
        }

        [Test]
        public void Drain_WhenCallbackThrows_LeavesCurrentTriggerPending()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var trigger =
                new TestTrigger("Trigger");

            dispatcher.Enqueue(
                CreateCandidate(
                    trigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.Drain(
                    1,
                    currentTrigger =>
                        throw new InvalidOperationException(
                            "Test trigger failure.")));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(trigger));
        }

        [Test]
        public void ProcessNext_WhenCallbackEnqueuesThenThrows_DiscardsDeferredTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var currentTrigger =
                new TestTrigger("Current");

            var deferredTrigger =
                new TestTrigger("Deferred");

            dispatcher.Enqueue(
                CreateCandidate(
                    currentTrigger,
                    CombatTriggerSourceKind.Slot,
                    1,
                    0));

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.ProcessNext(
                    trigger =>
                    {
                        dispatcher.Enqueue(
                            CreateCandidate(
                                deferredTrigger,
                                CombatTriggerSourceKind.Slot,
                                0,
                                0));

                        throw new InvalidOperationException(
                            "Test trigger failure.");
                    }));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(currentTrigger));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(currentTrigger));

            Assert.That(
                dispatcher.Count,
                Is.Zero);

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        [Test]
        public void ProcessNext_WhenCallbackAttemptsNestedProcessing_ThrowsAndKeepsCurrentTrigger()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var currentTrigger =
                new TestTrigger("Current");

            dispatcher.Enqueue(
                CreateCandidate(
                    currentTrigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.ProcessNext(
                    trigger =>
                        dispatcher.ProcessNext(
                            nestedTrigger => { })));

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(1));

            Assert.That(
                dispatcher.PeekNext(),
                Is.SameAs(currentTrigger));
        }

        [Test]
        public void ProcessNext_WhenCallbackEnqueuesEqualKeys_PreservesDeferredEnqueueOrder()
        {
            var dispatcher =
                new CombatTriggerDispatcher<TestTrigger>();

            var currentTrigger =
                new TestTrigger("Current");

            var firstDeferredTrigger =
                new TestTrigger("First Deferred");

            var secondDeferredTrigger =
                new TestTrigger("Second Deferred");

            dispatcher.Enqueue(
                CreateCandidate(
                    currentTrigger,
                    CombatTriggerSourceKind.Slot,
                    0,
                    0));

            dispatcher.ProcessNext(
                trigger =>
                {
                    dispatcher.Enqueue(
                        CreateCandidate(
                            firstDeferredTrigger,
                            CombatTriggerSourceKind.Card,
                            2,
                            1));

                    dispatcher.Enqueue(
                        CreateCandidate(
                            secondDeferredTrigger,
                            CombatTriggerSourceKind.Card,
                            2,
                            1));
                });

            Assert.That(
                dispatcher.Count,
                Is.EqualTo(2));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(firstDeferredTrigger));

            Assert.That(
                dispatcher.ProcessNext(
                    trigger => { }),
                Is.SameAs(secondDeferredTrigger));

            Assert.That(
                dispatcher.HasPending,
                Is.False);
        }

        private static CombatTriggerCandidate<TestTrigger>
            CreateCandidate(
                TestTrigger trigger,
                CombatTriggerSourceKind sourceKind,
                int horizontalOrder,
                int verticalOrder)
        {
            var orderKey =
                new CombatTriggerOrderKey(
                    sourceKind,
                    CombatSide.Player,
                    horizontalOrder,
                    verticalOrder);

            return new CombatTriggerCandidate<TestTrigger>(
                orderKey,
                trigger);
        }

        private sealed class TestTrigger
        {
            public TestTrigger(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}