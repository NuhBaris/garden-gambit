using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackTargetDamageReductionRequestTests
    {
        [Test]
        public void Constructor_StoresValues()
        {
            var normalAttackEventId =
                new CombatEventId(11);

            var petInstanceId =
                new InstanceId(101);

            var targetCardInstanceId =
                new InstanceId(201);

            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        normalAttackEventId,
                        petInstanceId,
                        targetCardInstanceId,
                        reductionAmount: 1);

            Assert.That(
                request.NormalAttackEventId,
                Is.EqualTo(
                    normalAttackEventId));

            Assert.That(
                request.PetInstanceId,
                Is.EqualTo(
                    petInstanceId));

            Assert.That(
                request.TargetCardInstanceId,
                Is.EqualTo(
                    targetCardInstanceId));

            Assert.That(
                request.ReductionAmount,
                Is.EqualTo(1));
        }

        [Test]
        public void Constructor_CreatesUsageKey()
        {
            var petInstanceId =
                new InstanceId(101);

            var targetCardInstanceId =
                new InstanceId(201);

            var request =
                new
                    CombatNormalAttackTargetDamageReductionRequest(
                        new CombatEventId(11),
                        petInstanceId,
                        targetCardInstanceId,
                        reductionAmount: 1);

            var expectedKey =
                new CombatPetCardTriggerKey(
                    petInstanceId,
                    targetCardInstanceId);

            Assert.That(
                request.UsageKey,
                Is.EqualTo(
                    expectedKey));
        }

        [Test]
        public void Constructor_WithInvalidEventId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatNormalAttackTargetDamageReductionRequest(
                            default(CombatEventId),
                            new InstanceId(101),
                            new InstanceId(201),
                            reductionAmount: 1));
        }

        [Test]
        public void Constructor_WithInvalidPetId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatNormalAttackTargetDamageReductionRequest(
                            new CombatEventId(11),
                            default(InstanceId),
                            new InstanceId(201),
                            reductionAmount: 1));
        }

        [Test]
        public void Constructor_WithInvalidTargetCardId_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatNormalAttackTargetDamageReductionRequest(
                            new CombatEventId(11),
                            new InstanceId(101),
                            default(InstanceId),
                            reductionAmount: 1));
        }

        [Test]
        public void Constructor_WithMatchingInstanceIds_Throws()
        {
            var sharedInstanceId =
                new InstanceId(101);

            Assert.Throws<ArgumentException>(
                () => _ =
                    new
                        CombatNormalAttackTargetDamageReductionRequest(
                            new CombatEventId(11),
                            sharedInstanceId,
                            sharedInstanceId,
                            reductionAmount: 1));
        }

        [Test]
        public void Constructor_WithZeroReduction_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                    () => _ =
                        new
                            CombatNormalAttackTargetDamageReductionRequest(
                                new CombatEventId(11),
                                new InstanceId(101),
                                new InstanceId(201),
                                reductionAmount: 0));
        }

        [Test]
        public void Constructor_WithNegativeReduction_Throws()
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                    () => _ =
                        new
                            CombatNormalAttackTargetDamageReductionRequest(
                                new CombatEventId(11),
                                new InstanceId(101),
                                new InstanceId(201),
                                reductionAmount: -1));
        }
    }
}