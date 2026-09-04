using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackSourceDamageModifierRegistryTests
    {
        [Test]
        public void
            Constructor_StartsWithoutModifiers()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    3);

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.HasModifier(
                    attackEvent.Metadata.EventId),
                Is.False);

            Assert.That(
                registry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.Zero);

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.EqualTo(3));
        }

        [Test]
        public void
            AddModifier_WithInvalidEventId_Throws()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.AddModifier(
                    default(CombatEventId),
                    1));

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void AddModifier_WithZeroDelta_Throws()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    3);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => registry.AddModifier(
                    attackEvent.Metadata.EventId,
                    0));

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void
            HasModifier_WithInvalidEventId_Throws()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.HasModifier(
                    default(CombatEventId)));
        }

        [Test]
        public void
            GetTotalModifier_WithInvalidEventId_Throws()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            Assert.Throws<ArgumentException>(
                () => registry.GetTotalModifier(
                    default(CombatEventId)));
        }

        [Test]
        public void
            AddModifier_WithPositiveDelta_IncreasesResolvedDamage()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    3);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                2);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.HasModifier(
                    attackEvent.Metadata.EventId),
                Is.True);

            Assert.That(
                registry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.EqualTo(5));
        }

        [Test]
        public void
            AddModifier_WithNegativeDelta_DecreasesResolvedDamage()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    5);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                -2);

            Assert.That(
                registry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.EqualTo(-2));

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.EqualTo(3));
        }

        [Test]
        public void
            ResolveDamage_WhenModifierExceedsBaseDamage_ClampsToZero()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    3);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                -5);

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.Zero);
        }

        [Test]
        public void
            AddModifier_MultipleTimesForSameEvent_CombinesModifiers()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    4);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                1);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                2);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                -1);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.EqualTo(2));

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.EqualTo(6));
        }

        [Test]
        public void
            AddModifier_ForDifferentEvents_KeepsIndependentTotals()
        {
            var metadataFactory =
                CreateMetadataFactory();

            var firstAttackEvent =
                CreateAttackEvent(
                    metadataFactory,
                    3);

            var secondAttackEvent =
                CreateAttackEvent(
                    metadataFactory,
                    5);

            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            registry.AddModifier(
                firstAttackEvent.Metadata.EventId,
                1);

            registry.AddModifier(
                secondAttackEvent.Metadata.EventId,
                4);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.ResolveDamage(
                    firstAttackEvent),
                Is.EqualTo(4));

            Assert.That(
                registry.ResolveDamage(
                    secondAttackEvent),
                Is.EqualTo(9));
        }

        [Test]
        public void
            ResolveDamage_WithNullAttackEvent_Throws()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            Assert.Throws<ArgumentNullException>(
                () => registry.ResolveDamage(
                    null));
        }

        [Test]
        public void
            ResolveDamage_WhenResultExceedsIntMaximum_Throws()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    int.MaxValue);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                1);

            Assert.Throws<OverflowException>(
                () => registry.ResolveDamage(
                    attackEvent));
        }

        [Test]
        public void
            AddModifier_WhenCombinedModifierOverflows_ThrowsWithoutReplacingPreviousTotal()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    0);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                int.MaxValue);

            Assert.Throws<OverflowException>(
                () => registry.AddModifier(
                    attackEvent.Metadata.EventId,
                    1));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.EqualTo(int.MaxValue));

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void
            AddModifier_WhenModifiersCancel_KeepsRegisteredZeroTotal()
        {
            var registry =
                new
                    CombatNormalAttackSourceDamageModifierRegistry();

            var attackEvent =
                CreateAttackEvent(
                    4);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                3);

            registry.AddModifier(
                attackEvent.Metadata.EventId,
                -3);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.HasModifier(
                    attackEvent.Metadata.EventId),
                Is.True);

            Assert.That(
                registry.GetTotalModifier(
                    attackEvent.Metadata.EventId),
                Is.Zero);

            Assert.That(
                registry.ResolveDamage(
                    attackEvent),
                Is.EqualTo(4));
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                int baseDamage)
        {
            return CreateAttackEvent(
                CreateMetadataFactory(),
                baseDamage);
        }

        private static NormalAttackCombatEvent
            CreateAttackEvent(
                CombatEventMetadataFactory
                    metadataFactory,
                int baseDamage)
        {
            var rootMetadata =
                metadataFactory.CreateRoot();

            var attackMetadata =
                metadataFactory.CreateChild(
                    rootMetadata);

            return new NormalAttackCombatEvent(
                attackMetadata,
                new InstanceId(100),
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                new InstanceId(200),
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1)),
                baseDamage);
        }

        private static CombatEventMetadataFactory
            CreateMetadataFactory()
        {
            return new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());
        }
    }
}