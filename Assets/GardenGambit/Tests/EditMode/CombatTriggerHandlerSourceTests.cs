using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatTriggerHandlerSourceTests
    {
        [Test]
        public void Constructor_WithNullProvider_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerHandlerSource(
                        null,
                        new TestTriggerHandler(
                            canTriggerResult: true,
                            throwDuringCanTrigger: false)));
        }

        [Test]
        public void Constructor_WithNullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new CombatTriggerHandlerSource(
                        new TestOrderKeyProvider(
                            CreateOrderKey()),
                        null));
        }

        [Test]
        public void Constructor_WithValidValues_SetsDependencies()
        {
            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey());

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            var source =
                new CombatTriggerHandlerSource(
                    provider,
                    handler);

            Assert.That(
                source.OrderKeyProvider,
                Is.SameAs(provider));

            Assert.That(
                source.Handler,
                Is.SameAs(handler));
        }

        [Test]
        public void DiscoverTriggers_WhenHandlerCanTrigger_ReturnsSingleCandidate()
        {
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

            var source =
                new CombatTriggerHandlerSource(
                    provider,
                    handler);

            var candidates =
                new List<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        state,
                        sourceEvent));

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                candidates[0].OrderKey,
                Is.EqualTo(orderKey));

            Assert.That(
                candidates[0].Trigger,
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
                provider.CallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.ReceivedState,
                Is.SameAs(state));

            Assert.That(
                provider.ReceivedSourceEvent,
                Is.SameAs(sourceEvent));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void DiscoverTriggers_WhenHandlerCannotTrigger_ReturnsEmptyWithoutQueryingProvider()
        {
            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey());

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: false,
                    throwDuringCanTrigger: false);

            var source =
                new CombatTriggerHandlerSource(
                    provider,
                    handler);

            var candidates =
                new List<
                    CombatTriggerCandidate<
                        ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        CreateState(),
                        CreateSourceEvent()));

            Assert.That(
                candidates,
                Is.Empty);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.Zero);

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void DiscoverTriggers_WithInvalidInputs_ThrowsWithoutQueryingDependencies()
        {
            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey());

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            var source =
                new CombatTriggerHandlerSource(
                    provider,
                    handler);

            Assert.Throws<ArgumentNullException>(
                () => source.DiscoverTriggers(
                    null,
                    CreateSourceEvent()));

            Assert.Throws<ArgumentNullException>(
                () => source.DiscoverTriggers(
                    CreateState(),
                    null));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);

            Assert.That(
                provider.CallCount,
                Is.Zero);
        }

        [Test]
        public void DiscoverTriggers_WhenHandlerThrows_DoesNotQueryProvider()
        {
            var provider =
                new TestOrderKeyProvider(
                    CreateOrderKey());

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: true);

            var source =
                new CombatTriggerHandlerSource(
                    provider,
                    handler);

            Assert.Throws<InvalidOperationException>(
                () => source.DiscoverTriggers(
                    CreateState(),
                    CreateSourceEvent()));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.Zero);

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void DiscoverTriggers_WhenProviderReturnsInvalidKey_ThrowsWithoutCandidate()
        {
            var provider =
                new TestOrderKeyProvider(
                    default(CombatTriggerOrderKey));

            var handler =
                new TestTriggerHandler(
                    canTriggerResult: true,
                    throwDuringCanTrigger: false);

            var source =
                new CombatTriggerHandlerSource(
                    provider,
                    handler);

            Assert.Throws<InvalidOperationException>(
                () => source.DiscoverTriggers(
                    CreateState(),
                    CreateSourceEvent()));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                provider.CallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
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
                        "Test handler discovery failure.");
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