using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        FixedCombatTriggerOrderKeyProviderTests
    {
        [Test]
        public void Constructor_WithInvalidOrderKey_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new FixedCombatTriggerOrderKeyProvider(
                        default(CombatTriggerOrderKey)));
        }

        [Test]
        public void Constructor_WithValidOrderKey_SetsSnapshot()
        {
            var orderKey =
                CreateOrderKey();

            var provider =
                new FixedCombatTriggerOrderKeyProvider(
                    orderKey);

            Assert.That(
                provider.OrderKey,
                Is.EqualTo(orderKey));
        }

        [Test]
        public void GetOrderKey_WithValidInputs_ReturnsFixedSnapshot()
        {
            var orderKey =
                CreateOrderKey();

            var provider =
                new FixedCombatTriggerOrderKeyProvider(
                    orderKey);

            var returnedOrderKey =
                provider.GetOrderKey(
                    CreateState(),
                    CreateSourceEvent());

            Assert.That(
                returnedOrderKey,
                Is.EqualTo(orderKey));
        }

        [Test]
        public void GetOrderKey_WithNullState_Throws()
        {
            var provider =
                new FixedCombatTriggerOrderKeyProvider(
                    CreateOrderKey());

            Assert.Throws<ArgumentNullException>(
                () => provider.GetOrderKey(
                    null,
                    CreateSourceEvent()));
        }

        [Test]
        public void GetOrderKey_WithNullSourceEvent_Throws()
        {
            var provider =
                new FixedCombatTriggerOrderKeyProvider(
                    CreateOrderKey());

            Assert.Throws<ArgumentNullException>(
                () => provider.GetOrderKey(
                    CreateState(),
                    null));
        }

        private static CombatTriggerOrderKey
            CreateOrderKey()
        {
            return new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Slot,
                CombatSide.Player,
                2,
                1);
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