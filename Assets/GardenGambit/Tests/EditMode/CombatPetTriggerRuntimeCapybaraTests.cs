using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeCapybaraTests
    {
        [Test]
        public void Catalogue_RegistersIdentityAcrossEveryRegistryLevel()
        {
            var environment =
                new Environment();

            Assert.That(
                CombatPetDefinitionIds.CapybaraValue,
                Is.EqualTo(
                    "pet.capybara"));

            Assert.That(
                CombatPetDefinitionIds.Capybara,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.capybara")));

            Assert.That(
                environment.Runtime.FactoryRegistry.Count,
                Is.EqualTo(3));

            Assert.That(
                environment.Runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds.Capybara),
                Is.False);

            var factory =
                environment.CreateFullRegistry()
                    .GetFactory(
                        CombatPetDefinitionIds.Capybara)
                    as CapybaraPetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.UsageCommitter));

            Assert.That(
                factory.SourceDamageModifierRegistry,
                Is.SameAs(
                    environment.Runtime
                        .SourceDamageModifierRegistry));

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
        public void Runtime_FullRegistryBuildsSharedCapybaraSource(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side);

            var sources =
                environment.Runtime.BuildSourceRegistry(
                    environment.State,
                    environment.ArmorGainResolver,
                    environment.AttackGainResolver,
                    environment.CardLookup);

            Assert.That(
                sources.Count,
                Is.EqualTo(1));

            var source =
                sources.Sources[0]
                    as CapybaraPetTriggerSource;

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
                source.SourceDamageModifierRegistry,
                Is.SameAs(
                    environment.Runtime
                        .SourceDamageModifierRegistry));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void ResolutionRunner_AppliesCapybaraToFirstVegetableAttack(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side,
                    sourceAttack: 2,
                    targetHp: 3);

            var completed =
                environment.ResolveCombat();

            var attacks =
                environment.GetSourceAttackEvents();

            Assert.That(
                completed,
                Is.Not.Null);

            Assert.That(
                attacks.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Runtime
                    .SourceDamageModifierRegistry
                    .GetTotalModifier(
                        attacks[0].Metadata.EventId),
                Is.EqualTo(1));

            Assert.That(
                environment.Runtime
                    .SourceDamageModifierRegistry
                    .ResolveDamage(
                        attacks[0]),
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
        [TestCase(CombatCardSuit.Nut)]
        [TestCase(CombatCardSuit.Drink)]
        public void ResolutionRunner_NonVegetableCardDoesNotTrigger(
            CombatCardSuit suit)
        {
            var environment =
                new Environment(
                    sourceSuit: suit,
                    sourceAttack: 2,
                    targetHp: 3);

            environment.ResolveCombat();

            Assert.That(
                environment.Runtime
                    .SourceDamageModifierRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);

            Assert.That(
                environment.GetSourceAttackEvents().Count,
                Is.EqualTo(2));
        }

        [Test]
        public void ResolutionRunner_RepeatedAttacksModifyOnlyFirstAttack()
        {
            var environment =
                new Environment(
                    sourceAttack: 1,
                    targetHp: 5);

            environment.ResolveCombat();

            var attacks =
                environment.GetSourceAttackEvents();

            Assert.That(
                attacks.Count,
                Is.EqualTo(4));

            Assert.That(
                environment.Runtime
                    .SourceDamageModifierRegistry
                    .GetTotalModifier(
                        attacks[0].Metadata.EventId),
                Is.EqualTo(1));

            for (var index = 1;
                 index < attacks.Count;
                 index++)
            {
                Assert.That(
                    environment.Runtime
                        .SourceDamageModifierRegistry
                        .GetTotalModifier(
                            attacks[index]
                                .Metadata.EventId),
                    Is.Zero);
            }

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
                CombatCardSuit sourceSuit =
                    CombatCardSuit.Vegetable,
                int sourceAttack = 2,
                int targetHp = 3)
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
                        CombatPetDefinitionIds.Capybara,
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
                            new CombatPetState[0]));

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

            public CombatCompletedCombatEvent ResolveCombat()
            {
                var runner =
                    Runtime.CreateResolutionRunner(
                        State,
                        MetadataFactory,
                        Log,
                        Queue);

                return runner.StartAndResolveCombat(
                    100,
                    100,
                    100,
                    100,
                    CombatPokerHand.FlushFive,
                    CombatPokerHand.FlushFive,
                    CombatPokerHand.FlushFive,
                    CombatPokerHand.FlushFive);
            }

            public List<NormalAttackCombatEvent>
                GetSourceAttackEvents()
            {
                var attacks =
                    new List<NormalAttackCombatEvent>();

                foreach (var combatEvent in
                         Log.Events)
                {
                    var attackEvent =
                        combatEvent as
                            NormalAttackCombatEvent;

                    if (attackEvent != null &&
                        attackEvent.AttackerSide == Side)
                    {
                        attacks.Add(
                            attackEvent);
                    }
                }

                return attacks;
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
                        "test.capybara_runtime_card"),
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
