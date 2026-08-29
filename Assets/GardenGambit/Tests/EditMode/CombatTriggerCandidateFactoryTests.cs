using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatTriggerCandidateFactoryTests
    {
        [Test]
        public void TryCreate_WhenHandlerCanTrigger_CreatesCandidate()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var state =
                CreateState();

            var sourceEvent =
                CreateSourceEvent();

            var orderKey =
                CreateOrderKey();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            var wasCreated =
                factory.TryCreate(
                    state,
                    sourceEvent,
                    orderKey,
                    handler,
                    out candidate);

            Assert.That(
                wasCreated,
                Is.True);

            Assert.That(
                candidate,
                Is.Not.Null);

            Assert.That(
                candidate.OrderKey,
                Is.EqualTo(orderKey));

            Assert.That(
                candidate.Trigger,
                Is.SameAs(handler));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedState,
                Is.SameAs(state));

            Assert.That(
                handler.ReceivedSourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WhenHandlerCannotTrigger_ReturnsFalseWithoutCandidate()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: false,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            var wasCreated =
                factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    CreateOrderKey(),
                    handler,
                    out candidate);

            Assert.That(
                wasCreated,
                Is.False);

            Assert.That(
                candidate,
                Is.Null);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WithNullState_ThrowsWithoutQueryingHandler()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            Assert.Throws<ArgumentNullException>(
                () => factory.TryCreate(
                    null,
                    CreateSourceEvent(),
                    CreateOrderKey(),
                    handler,
                    out candidate));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WithNullSourceEvent_ThrowsWithoutQueryingHandler()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            Assert.Throws<ArgumentNullException>(
                () => factory.TryCreate(
                    CreateState(),
                    null,
                    CreateOrderKey(),
                    handler,
                    out candidate));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WithInvalidOrderKey_ThrowsWithoutQueryingHandler()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            Assert.Throws<ArgumentException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    default(CombatTriggerOrderKey),
                    handler,
                    out candidate));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WithNullHandler_Throws()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            Assert.Throws<ArgumentNullException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    CreateOrderKey(),
                    null,
                    out candidate));
        }

        [Test]
        public void TryCreate_WhenCanTriggerThrows_PropagatesWithoutCandidate()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: true);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate = null;

            Assert.Throws<InvalidOperationException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    CreateOrderKey(),
                    handler,
                    out candidate));

            Assert.That(
                candidate,
                Is.Null);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WithProvider_WhenHandlerCanTrigger_CreatesCandidate()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var state =
                CreateState();

            var sourceEvent =
                CreateSourceEvent();

            var orderKey =
                CreateOrderKey();

            var provider =
                new TestOrderKeyProvider(
                    orderKey);

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            var wasCreated =
                factory.TryCreate(
                    state,
                    sourceEvent,
                    provider,
                    handler,
                    out candidate);

            Assert.That(
                wasCreated,
                Is.True);

            Assert.That(
                candidate,
                Is.Not.Null);

            Assert.That(
                candidate.OrderKey,
                Is.EqualTo(orderKey));

            Assert.That(
                candidate.Trigger,
                Is.SameAs(handler));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.ReceivedState,
                Is.SameAs(state));

            Assert.That(
                provider.ReceivedSourceEvent,
                Is.SameAs(sourceEvent));
        }

        [Test]
        public void TryCreate_WithProvider_WhenHandlerCannotTrigger_DoesNotQueryProvider()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey());

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: false,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            var wasCreated =
                factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    provider,
                    handler,
                    out candidate);

            Assert.That(
                wasCreated,
                Is.False);

            Assert.That(
                candidate,
                Is.Null);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WithNullProvider_ThrowsWithoutQueryingHandler()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate;

            Assert.Throws<ArgumentNullException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    null,
                    handler,
                    out candidate));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void TryCreate_WhenProviderReturnsInvalidKey_ThrowsWithoutCandidate()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var provider =
                new TestOrderKeyProvider(
                    default(CombatTriggerOrderKey));

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate = null;

            Assert.Throws<InvalidOperationException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    provider,
                    handler,
                    out candidate));

            Assert.That(
                candidate,
                Is.Null);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryCreate_WhenProviderThrows_PropagatesWithoutCandidate()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey())
                {
                    ThrowDuringGetOrderKey = true
                };

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate = null;

            Assert.Throws<InvalidOperationException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    provider,
                    handler,
                    out candidate));

            Assert.That(
                candidate,
                Is.Null);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryCreate_WithProvider_WhenCanTriggerThrows_DoesNotQueryProvider()
        {
            var factory =
                new CombatTriggerCandidateFactory();

            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey());

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: true);

            CombatTriggerCandidate<
                ICombatTriggerHandler> candidate = null;

            Assert.Throws<InvalidOperationException>(
                () => factory.TryCreate(
                    CreateState(),
                    CreateSourceEvent(),
                    provider,
                    handler,
                    out candidate));

            Assert.That(
                candidate,
                Is.Null);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.Zero);
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateSideState(
                    CombatSide.Player),
                CreateSideState(
                    CombatSide.Enemy));
        }

        private static CombatSideState CreateSideState(
            CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    new CombatSlotState[0]),
                new CombatCardRegistry(
                    new CombatCardState[0]),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
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

        private static CombatTriggerOrderKey
            CreateOrderKey()
        {
            return new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Card,
                CombatSide.Player,
                2,
                1);
        }

        private sealed class TestOrderKeyProvider :
    ICombatTriggerOrderKeyProvider
        {
            private readonly CombatTriggerOrderKey
                _orderKey;

            public TestOrderKeyProvider(
                CombatTriggerOrderKey orderKey)
            {
                _orderKey = orderKey;
            }

            public bool ThrowDuringGetOrderKey
            {
                get;
                set;
            }

            public int CallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public CombatEvent ReceivedSourceEvent
            {
                get;
                private set;
            }

            public CombatTriggerOrderKey GetOrderKey(
                CombatState state,
                CombatEvent sourceEvent)
            {
                CallCount++;

                ReceivedState = state;
                ReceivedSourceEvent = sourceEvent;

                if (ThrowDuringGetOrderKey)
                {
                    throw new InvalidOperationException(
                        "Test order-key failure.");
                }

                return _orderKey;
            }
        }

        private sealed class TestTriggerHandler :
            ICombatTriggerHandler
        {
            private readonly bool
                _canTriggerResult;

            private readonly bool
                _throwDuringCanTrigger;

            public TestTriggerHandler(
                bool canTriggerResult,
                bool throwDuringCanTrigger)
            {
                _canTriggerResult =
                    canTriggerResult;

                _throwDuringCanTrigger =
                    throwDuringCanTrigger;
            }

            public int CanTriggerCallCount
            {
                get;
                private set;
            }

            public int ResolveCallCount
            {
                get;
                private set;
            }

            public CombatState ReceivedState
            {
                get;
                private set;
            }

            public CombatEvent ReceivedSourceEvent
            {
                get;
                private set;
            }

            public bool CanTrigger(
                CombatState state,
                CombatEvent sourceEvent)
            {
                CanTriggerCallCount++;

                ReceivedState = state;
                ReceivedSourceEvent = sourceEvent;

                if (_throwDuringCanTrigger)
                {
                    throw new InvalidOperationException(
                        "Test discovery failure.");
                }

                return _canTriggerResult;
            }

            public void Resolve(
                CombatState state,
                CombatEvent sourceEvent)
            {
                ResolveCallCount++;
            }
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