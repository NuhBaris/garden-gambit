using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetHpGainTriggerSourceTests
    {
        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Enemy, false)]
        [TestCase(CombatSide.Enemy, true)]
        public void Drain_WithHpGain_PreservesEventAndPetContexts(
            CombatSide petSide,
            bool selfSource)
        {
            var environment = CreateEnvironment(petSide);

            var hpGainEvent = AppendHpGain(
                environment,
                selfSource: selfSource);

            var processedCount = environment.Engine.Drain(
                maximumEventCount: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(processedCount, Is.EqualTo(2));

            var handlers = new[]
            {
                environment.UpperHandler,
                environment.LowerHandler
            };

            var pets = new[]
            {
                environment.UpperPet,
                environment.LowerPet
            };

            for (var index = 0; index < handlers.Length; index++)
            {
                var handler = handlers[index];

                Assert.That(handler.ResolveCallCount, Is.EqualTo(1));
                Assert.That(handler.LastContext, Is.Not.Null);
                Assert.That(handler.LastPet, Is.SameAs(pets[index]));

                Assert.That(
                    handler.LastContext.SourceEvent,
                    Is.SameAs(hpGainEvent));

                Assert.That(
                    handler.LastContext.State,
                    Is.SameAs(environment.State));

                Assert.That(
                    handler.LastContext.Side,
                    Is.EqualTo(petSide));

                Assert.That(
                    handler.LastContext.SideState,
                    Is.SameAs(environment.State.GetSide(petSide)));

                Assert.That(
                    handler.LastContext.OpposingSideState,
                    Is.SameAs(
                        environment.State.GetOpposingSide(petSide)));

                Assert.That(
                    handler.LastContext.SidePetState,
                    Is.SameAs(environment.State.GetPets(petSide)));
            }

            var expectedSourceId = selfSource
                ? environment.TargetCard.InstanceId
                : environment.UpperPet.InstanceId;

            Assert.That(
                hpGainEvent.SourceInstanceId,
                Is.EqualTo(expectedSourceId));

            Assert.That(
                hpGainEvent.TargetInstanceId,
                Is.EqualTo(environment.TargetCard.InstanceId));

            Assert.That(
                hpGainEvent.IsSelfSource,
                Is.EqualTo(selfSource));

            Assert.That(
                hpGainEvent.IsFromAnotherSource,
                Is.EqualTo(!selfSource));
        }

        [Test]
        public void Drain_WithNonHpEvents_DoesNotInvokeHpHandlers()
        {
            var environment = CreateEnvironment();

            var armorResolver = new CombatArmorGainResolver(
                environment.MetadataFactory,
                environment.EventLog);

            armorResolver.TryApplyArmorGain(
                environment.State,
                               environment.RootEvent,
                environment.TargetPosition,
                requestedAmount: 1);

            var processedCount = environment.Engine.Drain(
                maximumEventCount: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(processedCount, Is.EqualTo(2));
            Assert.That(environment.ResolutionOrder.Count, Is.Zero);

            Assert.That(
                environment.UpperHandler.CanCallCount,
                Is.Zero);

            Assert.That(
                environment.LowerHandler.CanCallCount,
                Is.Zero);

            Assert.That(
                environment.UpperHandler.ResolveCallCount,
                Is.Zero);

            Assert.That(
                environment.LowerHandler.ResolveCallCount,
                Is.Zero);
        }

        [Test]
        public void Drain_WhenHandlerRejects_SkipsThatPet()
        {
            var environment = CreateEnvironment();

            environment.UpperHandler.AllowTrigger = false;

            AppendHpGain(environment);

            environment.Engine.Drain(
                maximumEventCount: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.UpperHandler.CanCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.UpperHandler.ResolveCallCount,
                Is.Zero);

            Assert.That(
                environment.LowerHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.ResolutionOrder,
                Is.EqualTo(new[]
                {
                    environment.LowerPet.InstanceId
                }));
        }

        [Test]
        public void Drain_WithReverseRegistration_UsesPetOrderWithoutReplay()
        {
            var environment = CreateEnvironment();

            var hpGainEvent = AppendHpGain(
                environment,
                heal: true);

            Assert.That(hpGainEvent.IsHeal, Is.True);

            environment.Engine.Drain(
                maximumEventCount: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(
                environment.ResolutionOrder,
                Is.EqualTo(new[]
                {
                    environment.UpperPet.InstanceId,
                    environment.LowerPet.InstanceId
                }));

            Assert.That(
                environment.UpperHandler.LastContext.GetAffectedRow(
                    environment.UpperHandler.LastPet),
                Is.EqualTo(BoardRow.Front));

            Assert.That(
                environment.LowerHandler.LastContext.GetAffectedRow(
                    environment.LowerHandler.LastPet),
                Is.EqualTo(BoardRow.Back));

            var processedAgain = environment.Engine.Drain(
                maximumEventCount: 100,
                maximumTriggerCountPerEvent: 100);

            Assert.That(processedAgain, Is.Zero);
            Assert.That(environment.ResolutionOrder.Count, Is.EqualTo(2));

            Assert.That(
                environment.UpperHandler.ResolveCallCount,
                Is.EqualTo(1));

            Assert.That(
                environment.LowerHandler.ResolveCallCount,
                Is.EqualTo(1));
        }

        private static HpGainCombatEvent AppendHpGain(
            TestEnvironment environment,
            bool selfSource = false,
            bool heal = false)
        {
            var resolver = new CombatHpGainResolver(
                environment.MetadataFactory,
                environment.EventLog);

            var sourceInstanceId = selfSource
                ? environment.TargetCard.InstanceId
                : environment.UpperPet.InstanceId;

            return heal
                ? resolver.TryApplyHeal(
                    environment.State,
                    environment.RootEvent,
                    sourceInstanceId,
                    environment.TargetPosition,
                    requestedAmount: 3)
                : resolver.TryApplyHpStatGain(
                    environment.State,
                    environment.RootEvent,
                    sourceInstanceId,
                    environment.TargetPosition,
                    requestedAmount: 3);
        }

        private static TestEnvironment CreateEnvironment(
            CombatSide petSide = CombatSide.Player)
        {
            var upperPet = new CombatPetState(
                new DefinitionId("test.hp_gain_pet"),
                new InstanceId(101));

            var lowerPet = new CombatPetState(
                new DefinitionId("test.hp_gain_pet"),
                new InstanceId(102));

            var pets = new[]
            {
                upperPet,
                lowerPet
            };

            var targetPosition = new BoardPosition(
                CombatSide.Player,
                BoardRow.Front,
                new BoardColumn(1));

            var targetCard = new CombatCardState(
                new DefinitionId("test.hp_gain_target"),
                new InstanceId(1),
                new CardRank(2),
                hpCapacity: 10,
                currentHp: 4,
                armor: 0,
                attack: 2);

            var playerSide = new CombatSideState(
                new CombatBoardState(
                    CombatSide.Player,
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            targetPosition,
                            targetCard.InstanceId)
                    }),
                new CombatCardRegistry(new[] { targetCard }),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));

            var enemySide = new CombatSideState(
                new CombatBoardState(
                    CombatSide.Enemy,
                    Array.Empty<CombatSlotState>()),
                new CombatCardRegistry(
                    Array.Empty<CombatCardState>()),
                new BattleHealth(BattleHealth.NormalBaselineValue),
                new AttackMultiplier(AttackMultiplier.BaseValue));

            var state = new CombatState(
                playerSide,
                enemySide,
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        petSide == CombatSide.Player
                            ? pets
                            : Array.Empty<CombatPetState>())),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        petSide == CombatSide.Enemy
                            ? pets
                            : Array.Empty<CombatPetState>())));

            var resolutionOrder = new List<InstanceId>();

            var upperHandler = new RecordingHandler(
                petSide,
                upperPet.InstanceId,
                resolutionOrder);

            var lowerHandler = new RecordingHandler(
                petSide,
                lowerPet.InstanceId,
                resolutionOrder);

            var metadataFactory = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(),
                new CombatSequenceNumberAllocator());

            var eventLog = new CombatEventLog();

            var rootEvent = new CombatStartedCombatEvent(
                metadataFactory.CreateRoot());

            eventLog.Append(rootEvent);

            var eventQueue = new CombatEventQueue(eventLog);

            var sourceRegistry = new CombatTriggerSourceRegistry(
                new ICombatTriggerSource[]
                {
                    new CombatPetHpGainTriggerSource(lowerHandler),
                    new CombatPetHpGainTriggerSource(upperHandler)
                });

            return new TestEnvironment
            {
                State = state,
                TargetCard = targetCard,
                TargetPosition = targetPosition,
                UpperPet = upperPet,
                LowerPet = lowerPet,
                UpperHandler = upperHandler,
                LowerHandler = lowerHandler,
                ResolutionOrder = resolutionOrder,
                MetadataFactory = metadataFactory,
                EventLog = eventLog,
                RootEvent = rootEvent,
                Engine = new CombatTriggerEngine(
                    state,
                    eventQueue,
                    sourceRegistry)
            };
        }

        private sealed class RecordingHandler :
            CombatPetHpGainTriggerHandler
        {
            private readonly List<InstanceId> _resolutionOrder;

            public RecordingHandler(
                CombatSide side,
                InstanceId petInstanceId,
                List<InstanceId> resolutionOrder)
                : base(side, petInstanceId)
            {
                _resolutionOrder = resolutionOrder;
            }

            public bool AllowTrigger { get; set; } = true;

            public int CanCallCount { get; private set; }

            public int ResolveCallCount { get; private set; }

            public CombatPetHpGainContext LastContext
            {
                get;
                private set;
            }

            public CombatPetState LastPet { get; private set; }

            protected override bool CanTriggerOnHpGain(
                CombatPetHpGainContext context,
                CombatPetState pet)
            {
                CanCallCount++;
                return AllowTrigger;
            }

            protected override void ResolveOnHpGain(
                CombatPetHpGainContext context,
                CombatPetState pet)
            {
                ResolveCallCount++;
                LastContext = context;
                LastPet = pet;

                _resolutionOrder.Add(pet.InstanceId);
            }
        }

        private sealed class TestEnvironment
        {
            public CombatState State { get; set; }
            public CombatCardState TargetCard { get; set; }
            public BoardPosition TargetPosition { get; set; }
            public CombatPetState UpperPet { get; set; }
            public CombatPetState LowerPet { get; set; }
            public RecordingHandler UpperHandler { get; set; }
            public RecordingHandler LowerHandler { get; set; }
            public List<InstanceId> ResolutionOrder { get; set; }
            public CombatEventMetadataFactory MetadataFactory { get; set; }
            public CombatEventLog EventLog { get; set; }
            public CombatStartedCombatEvent RootEvent { get; set; }
            public CombatTriggerEngine Engine { get; set; }
        }
    }
}