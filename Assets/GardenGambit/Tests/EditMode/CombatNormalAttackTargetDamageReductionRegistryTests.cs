using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatNormalAttackTargetDamageReductionRegistryTests
    {
        [Test]
        public void Constructor_CreatesEmptyRegistry()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            Assert.That(
                registry.Count,
                Is.Zero);

            Assert.That(
                registry.EventCount,
                Is.Zero);
        }

        [Test]
        public void TryRegister_StoresRequest()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var request =
                CreateRequest(
                    eventId: 11,
                    petId: 101,
                    cardId: 201,
                    reductionAmount: 1);

            var wasRegistered =
                registry.TryRegister(
                    request);

            Assert.That(
                wasRegistered,
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.EventCount,
                Is.EqualTo(1));

            Assert.That(
                registry.HasRequests(
                    request.NormalAttackEventId),
                Is.True);

            Assert.That(
                registry.GetRequests(
                    request.NormalAttackEventId)[0],
                Is.SameAs(request));
        }

        [Test]
        public void TryRegister_PreservesRegistrationOrder()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var firstRequest =
                CreateRequest(
                    eventId: 11,
                    petId: 101,
                    cardId: 201,
                    reductionAmount: 1);

            var secondRequest =
                CreateRequest(
                    eventId: 11,
                    petId: 102,
                    cardId: 201,
                    reductionAmount: 2);

            registry.TryRegister(
                firstRequest);

            registry.TryRegister(
                secondRequest);

            var requests =
                registry.GetRequests(
                    new CombatEventId(11));

            Assert.That(
                requests.Count,
                Is.EqualTo(2));

            Assert.That(
                requests[0],
                Is.SameAs(firstRequest));

            Assert.That(
                requests[1],
                Is.SameAs(secondRequest));
        }

        [Test]
        public void
            TryRegister_WithDuplicateEventAndUsageKey_ReturnsFalse()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var firstRequest =
                CreateRequest(
                    eventId: 11,
                    petId: 101,
                    cardId: 201,
                    reductionAmount: 1);

            var duplicateRequest =
                CreateRequest(
                    eventId: 11,
                    petId: 101,
                    cardId: 201,
                    reductionAmount: 3);

            Assert.That(
                registry.TryRegister(
                    firstRequest),
                Is.True);

            Assert.That(
                registry.TryRegister(
                    duplicateRequest),
                Is.False);

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.GetRequests(
                    new CombatEventId(11))[0],
                Is.SameAs(firstRequest));
        }

        [Test]
        public void
            TryRegister_WithDifferentUsageKeyForSameEvent_ReturnsTrue()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            Assert.That(
                registry.TryRegister(
                    CreateRequest(
                        eventId: 11,
                        petId: 101,
                        cardId: 201,
                        reductionAmount: 1)),
                Is.True);

            Assert.That(
                registry.TryRegister(
                    CreateRequest(
                        eventId: 11,
                        petId: 102,
                        cardId: 201,
                        reductionAmount: 1)),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.EventCount,
                Is.EqualTo(1));
        }

        [Test]
        public void
            TryRegister_WithSameUsageKeyForDifferentEvent_ReturnsTrue()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            Assert.That(
                registry.TryRegister(
                    CreateRequest(
                        eventId: 11,
                        petId: 101,
                        cardId: 201,
                        reductionAmount: 1)),
                Is.True);

            Assert.That(
                registry.TryRegister(
                    CreateRequest(
                        eventId: 12,
                        petId: 101,
                        cardId: 201,
                        reductionAmount: 1)),
                Is.True);

            Assert.That(
                registry.Count,
                Is.EqualTo(2));

            Assert.That(
                registry.EventCount,
                Is.EqualTo(2));
        }

        [Test]
        public void GetRequests_WithoutRequests_ReturnsEmptyList()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var requests =
                registry.GetRequests(
                    new CombatEventId(11));

            Assert.That(
                requests,
                Is.Not.Null);

            Assert.That(
                requests.Count,
                Is.Zero);

            Assert.That(
                registry.HasRequests(
                    new CombatEventId(11)),
                Is.False);
        }

        [Test]
        public void RemoveRequests_RemovesOnlyRequestedEvent()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            registry.TryRegister(
                CreateRequest(
                    eventId: 11,
                    petId: 101,
                    cardId: 201,
                    reductionAmount: 1));

            registry.TryRegister(
                CreateRequest(
                    eventId: 11,
                    petId: 102,
                    cardId: 201,
                    reductionAmount: 1));

            registry.TryRegister(
                CreateRequest(
                    eventId: 12,
                    petId: 103,
                    cardId: 202,
                    reductionAmount: 1));

            var removedCount =
                registry.RemoveRequests(
                    new CombatEventId(11));

            Assert.That(
                removedCount,
                Is.EqualTo(2));

            Assert.That(
                registry.Count,
                Is.EqualTo(1));

            Assert.That(
                registry.EventCount,
                Is.EqualTo(1));

            Assert.That(
                registry.HasRequests(
                    new CombatEventId(11)),
                Is.False);

            Assert.That(
                registry.HasRequests(
                    new CombatEventId(12)),
                Is.True);
        }

        [Test]
        public void RemoveRequests_WithoutRequests_ReturnsZero()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            var removedCount =
                registry.RemoveRequests(
                    new CombatEventId(11));

            Assert.That(
                removedCount,
                Is.Zero);

            Assert.That(
                registry.Count,
                Is.Zero);
        }

        [Test]
        public void TryRegister_WithNullRequest_Throws()
        {
            var registry =
                new
                    CombatNormalAttackTargetDamageReductionRegistry();

            Assert.Throws<ArgumentNullException>(
                () =>
                    registry.TryRegister(
                        null));
        }

        private static
            CombatNormalAttackTargetDamageReductionRequest
            CreateRequest(
                long eventId,
                long petId,
                long cardId,
                int reductionAmount)
        {
            return new
                CombatNormalAttackTargetDamageReductionRequest(
                    new CombatEventId(
                        eventId),
                    new GardenGambit.Domain.Identity
                        .InstanceId(
                            petId),
                    new GardenGambit.Domain.Identity
                        .InstanceId(
                            cardId),
                    reductionAmount);
        }
    }
}