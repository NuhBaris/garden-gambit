using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeHazelDormouseTests
    {
        [Test]
        public void Catalogue_RegistersIdentityAcrossEveryRegistryLevel()
        {
            var environment =
                new Environment();

            Assert.That(
                CombatPetDefinitionIds.HazelDormouseValue,
                Is.EqualTo(
                    "pet.hazel_dormouse"));

            Assert.That(
                CombatPetDefinitionIds.HazelDormouse,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.hazel_dormouse")));

            Assert.That(
                environment.Runtime.FactoryRegistry.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.Runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds.HazelDormouse),
                Is.False);

            var factory =
                environment.CreateFullRegistry()
                    .GetFactory(
                        CombatPetDefinitionIds.HazelDormouse)
                    as HazelDormousePetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.UsageCommitter));

            Assert.That(
                factory.TargetDamageReductionRegistry,
                Is.SameAs(
                    environment.Runtime
                        .TargetDamageReductionRegistry));

            Assert.That(
                environment.CreateFullRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .FullFactoryCount));

            Assert.That(
                environment.CreateRescueAwareRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .RescueAwareFactoryCount));

            Assert.That(
                environment.CreateCompleteRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .CompleteFactoryCount));

            Assert.That(
                environment.CreateEventLogAwareRegistry().Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .EventLogAwareFactoryCount));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_FullRegistryBuildsBothSharedSources(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side);

            var sources =
                environment.BuildSources();

            Assert.That(
                sources.Count,
                Is.EqualTo(2));

            var owners =
                new HashSet<InstanceId>();

            foreach (var item in sources.Sources)
            {
                var source =
                    item as HazelDormousePetTriggerSource;

                Assert.That(
                    source,
                    Is.Not.Null);

                Assert.That(
                    source.Side,
                    Is.EqualTo(
                        side));

                Assert.That(
                    source.UsageCommitter,
                    Is.SameAs(
                        environment.Runtime.UsageCommitter));

                Assert.That(
                    source.TargetDamageReductionRegistry,
                    Is.SameAs(
                        environment.Runtime
                            .TargetDamageReductionRegistry));

                Assert.That(
                    owners.Add(
                        source.PetInstanceId),
                    Is.True);
            }

            Assert.That(
                owners.Contains(
                    environment.UpperPet.InstanceId),
                Is.True);

            Assert.That(
                owners.Contains(
                    environment.LowerPet.InstanceId),
                Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void ResolutionRunner_ReducesFirstNutDamage(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side,
                    targetSuit: CombatCardSuit.Nut,
                    opponentAttack: 3);

            var completed =
                Start(
                    environment.CreateRunner());

            Assert.That(
                completed,
                Is.Not.Null);

            Assert.That(
                completed.Outcome,
                Is.EqualTo(
                    side == CombatSide.Player
                        ? CombatOutcome.PlayerVictory
                        : CombatOutcome.EnemyVictory));

            Assert.That(
                environment.Target.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                environment.State.GetOpposingSide(
                        side)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Target.InstanceId),
                Is.True);

            Assert.That(
                environment.Runtime.UsageCommitter.HasTriggered(
                    environment.LowerPet.InstanceId,
                    environment.Target.InstanceId),
                Is.False);

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runtime
                    .TargetDamageReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.Queue.PendingCount,
                Is.Zero);

            var hit =
                environment.OpponentHit();

            Assert.That(
                hit.Result.IncomingDamage,
                Is.EqualTo(2));

            Assert.That(
                hit.Result.HpDamage,
                Is.EqualTo(2));
        }

        [TestCase(CombatCardSuit.Unspecified)]
        [TestCase(CombatCardSuit.Fruit)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Drink)]
        public void ResolutionRunner_NonNutCardDoesNotTrigger(
            CombatCardSuit suit)
        {
            var environment =
                new Environment(
                    targetSuit: suit,
                    opponentAttack: 3);

            Start(
                environment.CreateRunner());

            Assert.That(
                environment.Target.CurrentHp,
                Is.EqualTo(7));

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime
                    .TargetDamageReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.OpponentHit()
                    .Result.IncomingDamage,
                Is.EqualTo(3));
        }

        [Test]
        public void ResolutionRunner_ZeroDamageLeavesUsageAvailable()
        {
            var environment =
                new Environment(
                    targetSuit: CombatCardSuit.Nut,
                    opponentAttack: 0);

            Start(
                environment.CreateRunner());

            Assert.That(
                environment.Target.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageCommitter.HasTriggered(
                    environment.UpperPet.InstanceId,
                    environment.Target.InstanceId),
                Is.False);

            Assert.That(
                environment.Runtime
                    .TargetDamageReductionRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.OpponentHit()
                    .Result.IncomingDamage,
                Is.Zero);
        }

        [Test]
        public void FreshRuntime_UsesIndependentBattleScopedUsage()
        {
            var first =
                new Environment();

            var second =
                new Environment();

            Start(
                first.CreateRunner());

            Assert.That(
                first.UpperPet.InstanceId,
                Is.EqualTo(
                    second.UpperPet.InstanceId));

            Assert.That(
                first.Target.InstanceId,
                Is.EqualTo(
                    second.Target.InstanceId));

            Assert.That(
                first.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                second.Runtime.UsageRegistry.Count,
                Is.Zero);

            Start(
                second.CreateRunner());

            Assert.That(
                first.Target.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                second.Target.CurrentHp,
                Is.EqualTo(8));

            Assert.That(
                second.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        private static CombatCompletedCombatEvent Start(
            CombatResolutionRunner runner)
        {
            return runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 10,
                maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100,
                maximumTriggerCountPerEvent: 100);
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState Target;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatPetTriggerRuntime Runtime;
            public readonly CombatEventMetadataFactory MetadataFactory;
            public readonly CombatEventLog Log;
            public readonly CombatEventQueue Queue;
            public readonly CombatArmorGainResolver ArmorGainResolver;
            public readonly CombatAttackGainResolver AttackGainResolver;
            public readonly CombatCardLookup CardLookup;
            public readonly CombatRescueResolver RescueResolver;
            public readonly CombatHpGainResolver HpGainResolver;

            public Environment(
                CombatSide side = CombatSide.Player,
                CombatCardSuit targetSuit = CombatCardSuit.Nut,
                int opponentAttack = 3)
            {
                Side = side;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                TargetPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        new BoardColumn(1));

                OpponentPosition =
                    new BoardPosition(
                        opposingSide,
                        BoardRow.Front,
                        new BoardColumn(1));

                Target =
                    CreateCard(
                        "test.hazel_dormouse_target",
                        1,
                        targetSuit,
                        hp: 10,
                        attack: 1);

                var opponent =
                    CreateCard(
                        "test.hazel_dormouse_opponent",
                        101,
                        CombatCardSuit.Fruit,
                        hp: 1,
                        attack: opponentAttack);

                UpperPet =
                    new CombatPetState(
                        CombatPetDefinitionIds.HazelDormouse,
                        new InstanceId(1001));

                LowerPet =
                    new CombatPetState(
                        CombatPetDefinitionIds.HazelDormouse,
                        new InstanceId(1002));

                var ownState =
                    CreateSideState(
                        side,
                        Target,
                        TargetPosition,
                        slotId: 1);

                var opposingState =
                    CreateSideState(
                        opposingSide,
                        opponent,
                        OpponentPosition,
                        slotId: 101);

                var ownPets =
                    new CombatSidePetState(
                        side,
                        new CombatPetRegistry(
                            new[]
                            {
                                UpperPet,
                                LowerPet
                            }));

                var opposingPets =
                    new CombatSidePetState(
                        opposingSide,
                        new CombatPetRegistry(
                            Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? ownState
                        : opposingState,
                    side == CombatSide.Enemy
                        ? ownState
                        : opposingState,
                    side == CombatSide.Player
                        ? ownPets
                        : opposingPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : opposingPets);

                Runtime =
                    new CombatPetTriggerRuntime();

                MetadataFactory =
                    new CombatEventMetadataFactory(
                        new CombatEventIdAllocator(),
                        new CombatSequenceNumberAllocator());

                Log =
                    new CombatEventLog();

                Queue =
                    new CombatEventQueue(
                        Log);

                ArmorGainResolver =
                    new CombatArmorGainResolver(
                        MetadataFactory,
                        Log);

                AttackGainResolver =
                    new CombatAttackGainResolver(
                        MetadataFactory,
                        Log);

                CardLookup =
                    new CombatCardLookup(
                        Log);

                RescueResolver =
                    new CombatRescueResolver(
                        MetadataFactory,
                        Log);

                HpGainResolver =
                    new CombatHpGainResolver(
                        MetadataFactory,
                        Log);
            }

            public BoardPosition TargetPosition
            {
                get;
            }

            public BoardPosition OpponentPosition
            {
                get;
            }

            public CombatPetTriggerSourceFactoryRegistry
                CreateFullRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    ArmorGainResolver,
                    AttackGainResolver,
                    CardLookup,
                    Runtime.PetUsageCommitter);
            }

            public CombatPetTriggerSourceFactoryRegistry
                CreateRescueAwareRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    ArmorGainResolver,
                    AttackGainResolver,
                    CardLookup,
                    Runtime.PetUsageCommitter,
                    RescueResolver);
            }

            public CombatPetTriggerSourceFactoryRegistry
                CreateCompleteRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    ArmorGainResolver,
                    AttackGainResolver,
                    CardLookup,
                    Runtime.PetUsageCommitter,
                    RescueResolver,
                    HpGainResolver);
            }

            public CombatPetTriggerSourceFactoryRegistry
                CreateEventLogAwareRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    ArmorGainResolver,
                    AttackGainResolver,
                    CardLookup,
                    Runtime.PetUsageCommitter,
                    RescueResolver,
                    HpGainResolver,
                    Log);
            }

            public CombatTriggerSourceRegistry BuildSources()
            {
                return Runtime.BuildSourceRegistry(
                    State,
                    ArmorGainResolver,
                    AttackGainResolver,
                    CardLookup);
            }

            public CombatResolutionRunner CreateRunner()
            {
                return Runtime.CreateResolutionRunner(
                    State,
                    MetadataFactory,
                    Log,
                    Queue);
            }

            public DamageAppliedCombatEvent OpponentHit()
            {
                foreach (var combatEvent in Log.Events)
                {
                    var damageEvent =
                        combatEvent as DamageAppliedCombatEvent;

                    if (damageEvent != null &&
                        damageEvent.TargetInstanceId ==
                        Target.InstanceId)
                    {
                        return damageEvent;
                    }
                }

                throw new InvalidOperationException(
                    "Opponent damage event was not found.");
            }

            private static CombatSideState CreateSideState(
                CombatSide side,
                CombatCardState card,
                BoardPosition frontPosition,
                long slotId)
            {
                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        frontPosition.Column);

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(
                                    slotId),
                                frontPosition,
                                card.InstanceId),
                            new CombatSlotState(
                                new SlotId(
                                    slotId + 1),
                                backPosition)
                        }),
                    new CombatCardRegistry(
                        new[] { card }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardState CreateCard(
                string definitionId,
                long instanceId,
                CombatCardSuit suit,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId(
                        definitionId),
                    new InstanceId(
                        instanceId),
                    new CardRank(5),
                    suit,
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    attack);
            }
        }
    }
}
