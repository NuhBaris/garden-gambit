using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatEventTriggerHandlerTests
    {
        [Test]
        public void CanTrigger_WithMatchingEvent_PassesTypedEventAndReturnsCoreResult()
        {
            var state =
                CreateState();

            var sourceEvent =
                CreateEventA();

            var handler =
                new TestTypedHandler
                {
                    CanTriggerResult = true
                };

            var result =
                handler.CanTrigger(
                    state,
                    sourceEvent);

            Assert.That(
                result,
                Is.True);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedCanTriggerState,
                Is.SameAs(state));

            Assert.That(
                handler.ReceivedCanTriggerEvent,
                Is.SameAs(sourceEvent));
        }

        [Test]
        public void CanTrigger_WhenTypedCoreReturnsFalse_ReturnsFalse()
        {
            var handler =
                new TestTypedHandler
                {
                    CanTriggerResult = false
                };

            var result =
                handler.CanTrigger(
                    CreateState(),
                    CreateEventA());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void CanTrigger_WithDifferentEventType_ReturnsFalseWithoutCallingCore()
        {
            var handler =
                new TestTypedHandler
                {
                    CanTriggerResult = true
                };

            var result =
                handler.CanTrigger(
                    CreateState(),
                    CreateEventB());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void CanTrigger_WithNullInputs_ThrowsWithoutCallingCore()
        {
            var handler =
                new TestTypedHandler();

            Assert.Throws<ArgumentNullException>(
                () => handler.CanTrigger(
                    null,
                    CreateEventA()));

            Assert.Throws<ArgumentNullException>(
                () => handler.CanTrigger(
                    CreateState(),
                    null));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void CanTrigger_WhenTypedCoreThrows_Propagates()
        {
            var handler =
                new TestTypedHandler
                {
                    ThrowDuringCanTrigger = true
                };

            Assert.Throws<InvalidOperationException>(
                () => handler.CanTrigger(
                    CreateState(),
                    CreateEventA()));

            Assert.That(
                handler.CanTriggerCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithMatchingEvent_PassesTypedEventWithoutRecheckingCondition()
        {
            var state =
                CreateState();

            var sourceEvent =
                CreateEventA();

            var handler =
                new TestTypedHandler
                {
                    CanTriggerResult = false
                };

            handler.Resolve(
                state,
                sourceEvent);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                handler.ReceivedResolveState,
                Is.SameAs(state));

            Assert.That(
                handler.ReceivedResolveEvent,
                Is.SameAs(sourceEvent));
        }

        [Test]
        public void Resolve_WithDifferentEventType_ThrowsWithoutCallingCore()
        {
            var handler =
                new TestTypedHandler();

            Assert.Throws<ArgumentException>(
                () => handler.Resolve(
                    CreateState(),
                    CreateEventB()));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);

            Assert.That(
                handler.CanTriggerCallCount,
                Is.Zero);
        }

        [Test]
        public void Resolve_WithNullInputs_ThrowsWithoutCallingCore()
        {
            var handler =
                new TestTypedHandler();

            Assert.Throws<ArgumentNullException>(
                () => handler.Resolve(
                    null,
                    CreateEventA()));

            Assert.Throws<ArgumentNullException>(
                () => handler.Resolve(
                    CreateState(),
                    null));

            Assert.That(
                handler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void Resolve_WhenTypedCoreThrows_Propagates()
        {
            var handler =
                new TestTypedHandler
                {
                    ThrowDuringResolve = true
                };

            Assert.Throws<InvalidOperationException>(
                () => handler.Resolve(
                    CreateState(),
                    CreateEventA()));

            Assert.That(
                handler.ResolveCallCount,
                Is.EqualTo(1));
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

        private static TestCombatEventA CreateEventA()
        {
            var metadataFactory =
                CreateMetadataFactory();

            return new TestCombatEventA(
                metadataFactory.CreateRoot());
        }

        private static TestCombatEventB CreateEventB()
        {
            var metadataFactory =
                CreateMetadataFactory();

            return new TestCombatEventB(
                metadataFactory.CreateRoot());
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }

        private sealed class TestTypedHandler :
            CombatEventTriggerHandler<
                TestCombatEventA>
        {
            public bool CanTriggerResult
            {
                get;
                set;
            }

            public bool ThrowDuringCanTrigger
            {
                get;
                set;
            }

            public bool ThrowDuringResolve
            {
                get;
                set;
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

            public CombatState ReceivedCanTriggerState
            {
                get;
                private set;
            }

            public TestCombatEventA
                ReceivedCanTriggerEvent
            {
                get;
                private set;
            }

            public CombatState ReceivedResolveState
            {
                get;
                private set;
            }

            public TestCombatEventA ReceivedResolveEvent
            {
                get;
                private set;
            }

            protected override bool CanTriggerTyped(
                CombatState state,
                TestCombatEventA sourceEvent)
            {
                CanTriggerCallCount++;

                ReceivedCanTriggerState = state;
                ReceivedCanTriggerEvent = sourceEvent;

                if (ThrowDuringCanTrigger)
                {
                    throw new InvalidOperationException(
                        "Test typed discovery failure.");
                }

                return CanTriggerResult;
            }

            protected override void ResolveTyped(
                CombatState state,
                TestCombatEventA sourceEvent)
            {
                ResolveCallCount++;

                ReceivedResolveState = state;
                ReceivedResolveEvent = sourceEvent;

                if (ThrowDuringResolve)
                {
                    throw new InvalidOperationException(
                        "Test typed resolution failure.");
                }
            }
        }

        private sealed class TestCombatEventA :
            CombatEvent
        {
            public TestCombatEventA(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.NormalAttack)
            {
            }
        }

        private sealed class TestCombatEventB :
            CombatEvent
        {
            public TestCombatEventB(
                CombatEventMetadata metadata)
                : base(
                    metadata,
                    CombatEventKind.HpGain)
            {
            }
        }
    }
}