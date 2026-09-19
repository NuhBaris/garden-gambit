using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeOtterTests
    {
        [Test]
        public void Catalogue_RegistersIdentityAcrossEveryRegistryLevel()
        {
            var environment =
                new Environment();

            Assert.That(
                CombatPetDefinitionIds.OtterValue,
                Is.EqualTo(
                    "pet.otter"));

            Assert.That(
                CombatPetDefinitionIds.Otter,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.otter")));

            Assert.That(
                environment.Runtime.FactoryRegistry.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.Runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds.Otter),
                Is.False);

            var factory =
                environment.CreateFullRegistry()
                    .GetFactory(
                        CombatPetDefinitionIds.Otter)
                    as OtterPetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.UsageCommitter));

            Assert.That(
                factory.AttackGainResolver,
                Is.SameAs(
                    environment.AttackGainResolver));

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
        public void Runtime_FullRegistryBuildsSharedOtterSource(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side);

            var sources =
                environment.BuildSources();

            Assert.That(
                sources.Count,
                Is.EqualTo(1));

            var source =
                sources.Sources[0]
                    as OtterPetTriggerSource;

            Assert.That(
                source,
                Is.Not.Null);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    side));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    environment.Pet.InstanceId));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.UsageCommitter));

            Assert.That(
                source.AttackGainResolver,
                Is.SameAs(
                    environment.AttackGainResolver));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void ResolutionRunner_AppliesBonusOnlyAfterFirstAttack(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side,
                    sourceSuit: CombatCardSuit.Drink,
                    sourceAttack: 2,
                    targetHp: 5);

            var completed =
                environment.ResolveCombat();

            var attacks =
                environment.GetSourceAttackEvents();

            var gains =
                environment.GetAttackGainEvents();

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
                attacks.Count,
                Is.EqualTo(2));

            Assert.That(
                attacks[0].BaseDamage,
                Is.EqualTo(2));

            Assert.That(
                attacks[1].BaseDamage,
                Is.EqualTo(3));

            Assert.That(
                gains.Count,
                Is.EqualTo(1));

            Assert.That(
                gains[0].Metadata.ParentEventId.Value,
                Is.EqualTo(
                    attacks[0].Metadata.EventId));

            Assert.That(
                gains[0].PreviousAttack,
                Is.EqualTo(2));

            Assert.That(
                gains[0].CurrentAttack,
                Is.EqualTo(3));

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                environment.State.GetSide(
                        environment.OpposingSide)
                    .Cards.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Queue.PendingCount,
                Is.Zero);
        }

        [TestCase(CombatCardSuit.Unspecified)]
        [TestCase(CombatCardSuit.Fruit)]
        [TestCase(CombatCardSuit.Vegetable)]
        [TestCase(CombatCardSuit.Nut)]
        public void ResolutionRunner_NonDrinkCardDoesNotTrigger(
            CombatCardSuit suit)
        {
            var environment =
                new Environment(
                    sourceSuit: suit,
                    sourceAttack: 2,
                    targetHp: 3);

            environment.ResolveCombat();

            var attacks =
                environment.GetSourceAttackEvents();

            Assert.That(
                attacks.Count,
                Is.EqualTo(2));

            Assert.That(
                attacks[0].BaseDamage,
                Is.EqualTo(2));

            Assert.That(
                attacks[1].BaseDamage,
                Is.EqualTo(2));

            Assert.That(
                environment.SourceCard.Attack,
                Is.EqualTo(2));

            Assert.That(
                environment.GetAttackGainEvents(),
                Is.Empty);

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void ResolutionRunner_RepeatedAttacksGainOnlyOnce()
        {
            var environment =
                new Environment(
                    sourceAttack: 2,
                    targetHp: 8);

            environment.ResolveCombat();

            var attacks =
                environment.GetSourceAttackEvents();

            Assert.That(
                attacks.Count,
                Is.EqualTo(3));

            Assert.That(
                attacks[0].BaseDamage,
                Is.EqualTo(2));

            Assert.That(
                attacks[1].BaseDamage,
                Is.EqualTo(3));

            Assert.That(
                attacks[2].BaseDamage,
                Is.EqualTo(3));

            Assert.That(
                environment.GetAttackGainEvents().Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FreshRuntime_UsesIndependentBattleScopedUsage()
        {
            var first =
                new Environment();

            var second =
                new Environment();

            first.ResolveCombat();

            Assert.That(
                first.Pet.InstanceId,
                Is.EqualTo(
                    second.Pet.InstanceId));

            Assert.That(
                first.SourceCard.InstanceId,
                Is.EqualTo(
                    second.SourceCard.InstanceId));

            Assert.That(
                first.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                second.Runtime.UsageRegistry.Count,
                Is.Zero);

            second.ResolveCombat();

            Assert.That(
                first.SourceCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                second.SourceCard.Attack,
                Is.EqualTo(3));

            Assert.That(
                second.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatCardState SourceCard;
            public readonly CombatCardState TargetCard;
            public readonly CombatPetState Pet;
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
                CombatCardSuit sourceSuit = CombatCardSuit.Drink,
                int sourceAttack = 2,
                int targetHp = 5)
            {
                Side = side;

                SourceCard =
                    CreateCard(
                        1,
                        sourceSuit,
                        hp: 10,
                        attack: sourceAttack);

                TargetCard =
                    CreateCard(
                        2,
                        CombatCardSuit.Fruit,
                        hp: targetHp,
                        attack: 0);

                var sourceState =
                    CreateSideState(
                        side,
                        SourceCard);

                var targetState =
                    CreateSideState(
                        OpposingSide,
                        TargetCard);

                Pet =
                    new CombatPetState(
                        CombatPetDefinitionIds.Otter,
                        new InstanceId(1001));

                var sourcePets =
                    new CombatSidePetState(
                        side,
                        new CombatPetRegistry(
                            new[] { Pet }));

                var targetPets =
                    new CombatSidePetState(
                        OpposingSide,
                        new CombatPetRegistry(
                            Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? sourceState
                        : targetState,
                    side == CombatSide.Player
                        ? targetState
                        : sourceState,
                    side == CombatSide.Player
                        ? sourcePets
                        : targetPets,
                    side == CombatSide.Player
                        ? targetPets
                        : sourcePets);

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

            public CombatSide OpposingSide =>
                Side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;

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

            public CombatCompletedCombatEvent ResolveCombat()
            {
                var runner =
                    Runtime.CreateResolutionRunner(
                        State,
                        MetadataFactory,
                        Log,
                        Queue);

                return runner.StartAndResolveCombat(
                    maximumExchangeCountPerColumn: 100,
                    maximumPassCountPerExchange: 100,
                    maximumEventCountPerPass: 100,
                    maximumTriggerCountPerEvent: 100);
            }

            public List<NormalAttackCombatEvent>
                GetSourceAttackEvents()
            {
                var attacks =
                    new List<NormalAttackCombatEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    var attackEvent =
                        combatEvent as NormalAttackCombatEvent;

                    if (attackEvent != null &&
                        attackEvent.AttackerSide == Side)
                    {
                        attacks.Add(
                            attackEvent);
                    }
                }

                return attacks;
            }

            public List<AttackGainCombatEvent>
                GetAttackGainEvents()
            {
                var gains =
                    new List<AttackGainCombatEvent>();

                foreach (var combatEvent in Log.Events)
                {
                    var gainEvent =
                        combatEvent as AttackGainCombatEvent;

                    if (gainEvent != null)
                    {
                        gains.Add(
                            gainEvent);
                    }
                }

                return gains;
            }

            private static CombatSideState CreateSideState(
                CombatSide side,
                CombatCardState card)
            {
                var frontPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Front,
                        new BoardColumn(1));

                var backPosition =
                    new BoardPosition(
                        side,
                        BoardRow.Back,
                        new BoardColumn(1));

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        new[]
                        {
                            new CombatSlotState(
                                new SlotId(1),
                                frontPosition,
                                card.InstanceId),
                            new CombatSlotState(
                                new SlotId(2),
                                backPosition)
                        }),
                    new CombatCardRegistry(
                        new[] { card }),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardState CreateCard(
                long instanceId,
                CombatCardSuit suit,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.otter_runtime_card"),
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
