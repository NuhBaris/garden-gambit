using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetDeathTriggerSourceTests
    {
        [Test]
        public void Constructor_WithNullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ = new CombatPetDeathTriggerSource(null));
        }

        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Enemy, true)]
        [TestCase(CombatSide.Enemy, false)]
        public void Drain_RespectsHandlerDecisionPetOrderAndQueueProgress(
            CombatSide deathSide,
            bool canTrigger)
        {
            var state = CreateState();
            var resolvedPetIds = new List<InstanceId>();

            var pets = new[]
            {
                state.PlayerPets.GetPetAt(0),
                state.EnemyPets.GetPetAt(0),
                state.PlayerPets.GetPetAt(1),
                state.EnemyPets.GetPetAt(1)
            };

            var sides = new[]
            {
                CombatSide.Player,
                CombatSide.Enemy,
                CombatSide.Player,
                CombatSide.Enemy
            };

            var handlers = new TestHandler[pets.Length];

            for (var index = 0; index < handlers.Length; index++)
            {
                handlers[index] = new TestHandler(
                    sides[index],
                    pets[index].InstanceId,
                    canTrigger,
                    resolvedPetIds);
            }

            // Register sources in reverse of the expected execution order.
            var sources = new ICombatTriggerSource[handlers.Length];

            for (var index = 0; index < sources.Length; index++)
            {
                sources[index] = new CombatPetDeathTriggerSource(
                    handlers[handlers.Length - 1 - index]);
            }

            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());

            var eventLog = new CombatEventLog();

            var rootEvent = new CombatStartedCombatEvent(
                metadataFactory.CreateRoot());

            eventLog.Append(rootEvent);

            var deathEvent = new DeathCombatEvent(
                metadataFactory.CreateChild(rootEvent.Metadata),
                new InstanceId(1),
                new BoardPosition(
                    deathSide,
                    BoardRow.Front,
                    new BoardColumn(1)),
                previousHp: 3,
                currentHp: 0);

            eventLog.Append(deathEvent);

            var eventQueue = new CombatEventQueue(eventLog);

            var engine = new CombatTriggerEngine(
                state,
                eventQueue,
                new CombatTriggerSourceRegistry(sources));

            var processedCount = engine.Drain(
                maximumEventCount: 10,
                maximumTriggerCountPerEvent: 10);

            Assert.That(processedCount, Is.EqualTo(2));
            Assert.That(eventQueue.PendingCount, Is.Zero);
            Assert.That(engine.HasActiveBatch, Is.False);

            var expectedOrder = canTrigger
                ? new[]
                {
                    new InstanceId(1002),
                    new InstanceId(2002),
                    new InstanceId(1001),
                    new InstanceId(2001)
                }
                : Array.Empty<InstanceId>();

            Assert.That(
                resolvedPetIds,
                Is.EqualTo(expectedOrder));

            for (var index = 0; index < handlers.Length; index++)
            {
                var handler = handlers[index];

                Assert.That(handler.CanCallCount, Is.EqualTo(1));
                Assert.That(
                    handler.ResolveCallCount,
                    Is.EqualTo(canTrigger ? 1 : 0));

                Assert.That(handler.LastContext, Is.Not.Null);
                Assert.That(
                    handler.LastContext.State,
                    Is.SameAs(state));
                Assert.That(
                    handler.LastContext.Side,
                    Is.EqualTo(sides[index]));
                Assert.That(
                    handler.LastContext.SourceEvent,
                    Is.SameAs(deathEvent));
                Assert.That(
                    handler.LastPet,
                    Is.SameAs(pets[index]));

                Assert.That(
                    handler.LastContext.GetAffectedRow(
                        handler.LastPet),
                    Is.EqualTo(
                        index < 2 ? BoardRow.Front : BoardRow.Back));
            }

            Assert.That(
                engine.Drain(
                    maximumEventCount: 10,
                    maximumTriggerCountPerEvent: 10),
                Is.Zero);

            Assert.That(
                resolvedPetIds,
                Is.EqualTo(expectedOrder));

            foreach (var handler in handlers)
            {
                Assert.That(handler.CanCallCount, Is.EqualTo(1));
                Assert.That(
                    handler.ResolveCallCount,
                    Is.EqualTo(canTrigger ? 1 : 0));
            }

            Assert.That(eventLog.Count, Is.EqualTo(2));
        }

        private static CombatState CreateState()
        {
            return new CombatState(
                CreateEmptySide(CombatSide.Player),
                CreateEmptySide(CombatSide.Enemy),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        new[]
                        {
                            CreatePet(1002),
                            CreatePet(1001)
                        })),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        new[]
                        {
                            CreatePet(2002),
                            CreatePet(2001)
                        })));
        }

        private static CombatSideState CreateEmptySide(
            CombatSide side)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(
                    BattleHealth.NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static CombatPetState CreatePet(long instanceId)
        {
            return new CombatPetState(
                new DefinitionId("test.death_source_pet"),
                new InstanceId(instanceId));
        }

        private sealed class TestHandler :
            CombatPetDeathTriggerHandler
        {
            private readonly bool _canTrigger;
            private readonly List<InstanceId> _resolvedPetIds;

            public TestHandler(
                CombatSide side,
                InstanceId petInstanceId,
                bool canTrigger,
                List<InstanceId> resolvedPetIds)
                : base(side, petInstanceId)
            {
                _canTrigger = canTrigger;
                _resolvedPetIds = resolvedPetIds;
            }

            public int CanCallCount { get; private set; }

            public int ResolveCallCount { get; private set; }

            public CombatPetDeathContext LastContext { get; private set; }

            public CombatPetState LastPet { get; private set; }

            protected override bool CanTriggerOnDeath(
                CombatPetDeathContext context,
                CombatPetState pet)
            {
                CanCallCount++;
                LastContext = context;
                LastPet = pet;

                return _canTrigger;
            }

            protected override void ResolveOnDeath(
                CombatPetDeathContext context,
                CombatPetState pet)
            {
                ResolveCallCount++;
                LastContext = context;
                LastPet = pet;

                _resolvedPetIds.Add(pet.InstanceId);
            }
        }
    }
}