using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        ComputedCombatTriggerOrderKeyProviderTests
    {
        [Test]
        public void Constructor_WithNullComputeCallback_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new ComputedCombatTriggerOrderKeyProvider(
                        null));
        }

        [Test]
        public void GetOrderKey_WithValidInputs_PassesExactStateAndEvent()
        {
            var state =
                CreateState();

            var sourceEvent =
                CreateSourceEvent();

            var expectedOrderKey =
                CreateOrderKey(
                    horizontalOrder: 2,
                    verticalOrder: 1);

            CombatState receivedState = null;
            CombatEvent receivedEvent = null;

            var callCount = 0;

            var provider =
                new ComputedCombatTriggerOrderKeyProvider(
                    (currentState, currentEvent) =>
                    {
                        callCount++;

                        receivedState =
                            currentState;

                        receivedEvent =
                            currentEvent;

                        return expectedOrderKey;
                    });

            var returnedOrderKey =
                provider.GetOrderKey(
                    state,
                    sourceEvent);

            Assert.That(
                returnedOrderKey,
                Is.EqualTo(expectedOrderKey));

            Assert.That(
                callCount,
                Is.EqualTo(1));

            Assert.That(
                receivedState,
                Is.SameAs(state));

            Assert.That(
                receivedEvent,
                Is.SameAs(sourceEvent));
        }

        [Test]
        public void GetOrderKey_OnEveryCall_RecomputesOrderKey()
        {
            var firstOrderKey =
                CreateOrderKey(
                    horizontalOrder: 1,
                    verticalOrder: 0);

            var secondOrderKey =
                CreateOrderKey(
                    horizontalOrder: 4,
                    verticalOrder: 1);

            var currentOrderKey =
                firstOrderKey;

            var callCount = 0;

            var provider =
                new ComputedCombatTriggerOrderKeyProvider(
                    (state, sourceEvent) =>
                    {
                        callCount++;

                        return currentOrderKey;
                    });

            var state =
                CreateState();

            var sourceEvent =
                CreateSourceEvent();

            var firstResult =
                provider.GetOrderKey(
                    state,
                    sourceEvent);

            currentOrderKey =
                secondOrderKey;

            var secondResult =
                provider.GetOrderKey(
                    state,
                    sourceEvent);

            Assert.That(
                firstResult,
                Is.EqualTo(firstOrderKey));

            Assert.That(
                secondResult,
                Is.EqualTo(secondOrderKey));

            Assert.That(
                callCount,
                Is.EqualTo(2));
        }

        [Test]
        public void GetOrderKey_WithNullState_ThrowsWithoutCallingCallback()
        {
            var callCount = 0;

            var provider =
                new ComputedCombatTriggerOrderKeyProvider(
                    (state, sourceEvent) =>
                    {
                        callCount++;

                        return CreateOrderKey(
                            horizontalOrder: 0,
                            verticalOrder: 0);
                    });

            Assert.Throws<ArgumentNullException>(
                () => provider.GetOrderKey(
                    null,
                    CreateSourceEvent()));

            Assert.That(
                callCount,
                Is.Zero);
        }

        [Test]
        public void GetOrderKey_WithNullSourceEvent_ThrowsWithoutCallingCallback()
        {
            var callCount = 0;

            var provider =
                new ComputedCombatTriggerOrderKeyProvider(
                    (state, sourceEvent) =>
                    {
                        callCount++;

                        return CreateOrderKey(
                            horizontalOrder: 0,
                            verticalOrder: 0);
                    });

            Assert.Throws<ArgumentNullException>(
                () => provider.GetOrderKey(
                    CreateState(),
                    null));

            Assert.That(
                callCount,
                Is.Zero);
        }

        [Test]
        public void GetOrderKey_WhenCallbackReturnsInvalidKey_Throws()
        {
            var provider =
                new ComputedCombatTriggerOrderKeyProvider(
                    (state, sourceEvent) =>
                        default(CombatTriggerOrderKey));

            Assert.Throws<InvalidOperationException>(
                () => provider.GetOrderKey(
                    CreateState(),
                    CreateSourceEvent()));
        }

        [Test]
        public void GetOrderKey_WhenCallbackThrows_Propagates()
        {
            var provider =
                new ComputedCombatTriggerOrderKeyProvider(
                    (state, sourceEvent) =>
                        throw new InvalidOperationException(
                            "Test computation failure."));

            Assert.Throws<InvalidOperationException>(
                () => provider.GetOrderKey(
                    CreateState(),
                    CreateSourceEvent()));
        }

        private static CombatTriggerOrderKey
            CreateOrderKey(
                int horizontalOrder,
                int verticalOrder)
        {
            return new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Card,
                CombatSide.Player,
                horizontalOrder,
                verticalOrder);
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