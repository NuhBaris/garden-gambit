using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        CombatPetTriggerRuntimeIguanaTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var environment =
                new Environment();

            Assert.That(
                CombatPetDefinitionIds.IguanaValue,
                Is.EqualTo(
                    "pet.iguana"));

            Assert.That(
                CombatPetDefinitionIds.Iguana,
                Is.EqualTo(
                    new DefinitionId(
                        "pet.iguana")));

            var registry =
                environment.CreateEventLogAwareFactoryRegistry();

            Assert.That(
                registry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .EventLogAwareFactoryCount));

            var factory =
                registry.GetFactory(
                        CombatPetDefinitionIds.Iguana)
                    as IguanaPetTriggerSourceFactory;

            Assert.That(
                factory,
                Is.Not.Null);

            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.UsageCommitter));

            Assert.That(
                factory.HpGainResolver,
                Is.SameAs(
                    environment.HpGainResolver));

            Assert.That(
                factory.EventLog,
                Is.SameAs(
                    environment.Log));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsSourceWithSharedDependencies(
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
                    as IguanaPetTriggerSource;

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
                source.HpGainResolver,
                Is.SameAs(
                    environment.HpGainResolver));

            Assert.That(
                source.EventLog,
                Is.SameAs(
                    environment.Log));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void ResolutionRunner_AppliesIguanaAfterFirstNormalKill(
            CombatSide side)
        {
            var environment =
                new Environment(
                    side);

            var completed =
                environment.ResolveCombat();

            Assert.That(
                completed,
                Is.Not.Null);

            Assert.That(
                environment.SourceCard.CurrentHp,
                Is.EqualTo(10));

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(11));

            Assert.That(
                environment.Runtime.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        environment.SourceCard.InstanceId),
                Is.True);

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.Queue.PendingCount,
                Is.Zero);
        }

        [TestCase(2)]
        [TestCase(7)]
        [TestCase(10)]
        public void ResolutionRunner_RankBelowJackDoesNotTrigger(
            int rank)
        {
            var environment = new Environment(
                sourceRank: rank);

            environment.ResolveCombat();

            Assert.That(
                environment.SourceCard.HpCapacity,
                Is.EqualTo(10));

            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void LegacyCompleteOverload_RemainsBackwardCompatible()
        {
            var environment =
                new Environment();

            var legacyRegistry =
                environment.Runtime.FactoryCatalog
                    .CreateRegistry(
                        environment.ArmorGainResolver,
                        environment.AttackGainResolver,
                        environment.CardLookup,
                        environment.Runtime.PetUsageCommitter,
                        environment.RescueResolver,
                        environment.HpGainResolver);

            Assert.That(
                legacyRegistry.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .CompleteFactoryCount));

            Assert.That(
                legacyRegistry.Contains(
                    CombatPetDefinitionIds.Iguana),
                Is.False);
        }

        [Test]
        public void EventLogAwareOverloads_RejectNullEventLog()
        {
            var environment =
                new Environment();

            Assert.Throws<ArgumentNullException>(
                () => environment.Runtime.FactoryCatalog
                    .CreateRegistry(
                        environment.ArmorGainResolver,
                        environment.AttackGainResolver,
                        environment.CardLookup,
                        environment.Runtime.PetUsageCommitter,
                        environment.RescueResolver,
                        environment.HpGainResolver,
                        null));

            Assert.Throws<ArgumentNullException>(
                () => environment.Runtime.BuildSourceRegistry(
                    environment.State,
                    environment.ArmorGainResolver,
                    environment.AttackGainResolver,
                    environment.CardLookup,
                    environment.RescueResolver,
                    environment.HpGainResolver,
                    null));
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
                int sourceRank = 12)
            {
                Side = side;

                SourceCard =
                    CreateCard(
                        1,
                        sourceRank,
                        hp: 10,
                        attack: 100);

                TargetCard =
                    CreateCard(
                        2,
                        8,
                        hp: 5,
                        attack: 1);

                var sourceState =
                    CreateSideState(
                        side,
                        SourceCard);

                var targetSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;

                var targetState =
                    CreateSideState(
                        targetSide,
                        TargetCard);

                Pet =
                    new CombatPetState(
                        CombatPetDefinitionIds.Iguana,
                        new InstanceId(1001));

                var sourcePets =
                    new CombatSidePetState(
                        side,
                        new CombatPetRegistry(
                            new[] { Pet }));

                var targetPets =
                    new CombatSidePetState(
                        targetSide,
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

            public CombatPetTriggerSourceFactoryRegistry
                CreateEventLogAwareFactoryRegistry()
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

            public CombatTriggerSourceRegistry
                BuildSources()
            {
                return Runtime.BuildSourceRegistry(
                    State,
                    ArmorGainResolver,
                    AttackGainResolver,
                    CardLookup,
                    RescueResolver,
                    HpGainResolver,
                    Log);
            }

            public CombatCompletedCombatEvent
                ResolveCombat()
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
                int rank,
                int hp,
                int attack)
            {
                return new CombatCardState(
                    new DefinitionId(
                        "test.iguana_runtime_card"),
                    new InstanceId(
                        instanceId),
                    new CardRank(
                        rank),
                    CombatCardSuit.Unspecified,
                    CombatCardSeason.Spring,
                    hp,
                    hp,
                    0,
                    attack);
            }
        }
    }
}
