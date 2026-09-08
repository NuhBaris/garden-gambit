using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        MuskCatPetTriggerSourceTests
    {
        [Test]
        public void
            Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new MuskCatPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(1001),
                        null,
                        new
                            CombatFinalRankModifierRegistry()));
        }

        [Test]
        public void
            Constructor_WithNullFinalRankRegistry_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new MuskCatPetTriggerSource(
                        CombatSide.Player,
                        new InstanceId(1001),
                        CreateUsageCommitter(),
                        null));
        }

        [Test]
        public void
            Constructor_ExposesExactDependenciesAndIdentity()
        {
            var usageCommitter =
                CreateUsageCommitter();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var source =
                new MuskCatPetTriggerSource(
                    CombatSide.Player,
                    new InstanceId(1001),
                    usageCommitter,
                    finalRankRegistry);

            Assert.That(
                source.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(
                    new InstanceId(1001)));

            Assert.That(
                source.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                source.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankRegistry));

            Assert.That(
                source.Handler,
                Is.Not.Null);

            Assert.That(
                source.Handler.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.Handler.PetInstanceId,
                Is.EqualTo(
                    new InstanceId(1001)));

            Assert.That(
                source.Handler.UsageCommitter,
                Is.SameAs(
                    usageCommitter));

            Assert.That(
                source.Handler.FinalRankModifierRegistry,
                Is.SameAs(
                    finalRankRegistry));

            Assert.That(
                source.OrderKeyProvider.Side,
                Is.EqualTo(
                    CombatSide.Player));

            Assert.That(
                source.OrderKeyProvider
                    .PetInstanceId,
                Is.EqualTo(
                    new InstanceId(1001)));
        }

        [Test]
        public void
            DiscoverTriggers_WithExactlyOneLivingAffectedRowCard_ReturnsOneCandidate()
        {
            var environment =
                CreateEnvironment(
                    includeLivingCard: true);

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            Assert.That(
                environment.FinalRankRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            DiscoverTriggers_WithNoLivingCardOrWrongEvent_ReturnsNoCandidates()
        {
            var emptyEnvironment =
                CreateEnvironment(
                    includeLivingCard: false);

            var noLivingCandidates =
                Discover(
                    emptyEnvironment.Source,
                    emptyEnvironment.State,
                    CreateBattleEndEvent());

            var populatedEnvironment =
                CreateEnvironment(
                    includeLivingCard: true);

            var wrongEventCandidates =
                Discover(
                    populatedEnvironment.Source,
                    populatedEnvironment.State,
                    CreateNormalAttackEvent());

            Assert.That(
                noLivingCandidates.Count,
                Is.Zero);

            Assert.That(
                wrongEventCandidates.Count,
                Is.Zero);
        }

        [Test]
        public void
            HandlerResolution_AddsFinalRankAndRemovesCandidate()
        {
            var environment =
                CreateEnvironment(
                    includeLivingCard: true);

            var battleEndEvent =
                CreateBattleEndEvent();

            var candidates =
                Discover(
                    environment.Source,
                    environment.State,
                    battleEndEvent);

            Assert.That(
                candidates.Count,
                Is.EqualTo(1));

            environment.Source.Handler.Resolve(
                environment.State,
                battleEndEvent);

            Assert.That(
                environment.FinalRankRegistry
                    .GetTotalModifier(
                        environment.Card.InstanceId),
                Is.EqualTo(
                    MuskCatPetBattleEndTriggerHandler
                        .FinalRankBonus));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        environment.Pet.InstanceId,
                        environment.Card.InstanceId),
                Is.True);

            Assert.That(
                Discover(
                    environment.Source,
                    environment.State,
                    battleEndEvent),
                Is.Empty);
        }

        private static TestEnvironment
            CreateEnvironment(
                bool includeLivingCard)
        {
            var pet =
                new CombatPetState(
                    new DefinitionId(
                        "pet.musk_cat"),
                    new InstanceId(1001));

            CombatCardState card;
            CombatCardState[] cards;
            CombatSlotState[] slots;

            if (includeLivingCard)
            {
                card =
                    new CombatCardState(
                        new DefinitionId(
                            "test.card"),
                        new InstanceId(1),
                        new CardRank(2),
                        hpCapacity: 5,
                        currentHp: 5,
                        armor: 0,
                        attack: 0);

                cards =
                    new[]
                    {
                        card
                    };

                slots =
                    new[]
                    {
                        new CombatSlotState(
                            new SlotId(1),
                            new BoardPosition(
                                CombatSide.Player,
                                BoardRow.Front,
                                new BoardColumn(1)),
                            card.InstanceId)
                    };
            }
            else
            {
                card =
                    null;

                cards =
                    Array.Empty<
                        CombatCardState>();

                slots =
                    Array.Empty<
                        CombatSlotState>();
            }

            var state =
                new CombatState(
                    CreateSide(
                        CombatSide.Player,
                        cards,
                        slots),
                    CreateSide(
                        CombatSide.Enemy,
                        Array.Empty<
                            CombatCardState>(),
                        Array.Empty<
                            CombatSlotState>()),
                    new CombatSidePetState(
                        CombatSide.Player,
                        new CombatPetRegistry(
                            new[]
                            {
                                pet
                            })),
                    new CombatSidePetState(
                        CombatSide.Enemy,
                        new CombatPetRegistry(
                            Array.Empty<
                                CombatPetState>())));

            var usageCommitter =
                CreateUsageCommitter();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            return new TestEnvironment
            {
                State =
                    state,

                Pet =
                    pet,

                Card =
                    card,

                UsageCommitter =
                    usageCommitter,

                FinalRankRegistry =
                    finalRankRegistry,

                Source =
                    new MuskCatPetTriggerSource(
                        CombatSide.Player,
                        pet.InstanceId,
                        usageCommitter,
                        finalRankRegistry)
            };
        }

        private static CombatSideState CreateSide(
            CombatSide side,
            CombatCardState[] cards,
            CombatSlotState[] slots)
        {
            return new CombatSideState(
                new CombatBoardState(
                    side,
                    slots),
                new CombatCardRegistry(
                    cards),
                new BattleHealth(
                    BattleHealth
                        .NormalBaselineValue),
                new AttackMultiplier(
                    AttackMultiplier.BaseValue));
        }

        private static List<
            CombatTriggerCandidate<
                ICombatTriggerHandler>>
            Discover(
                MuskCatPetTriggerSource source,
                CombatState state,
                CombatEvent sourceEvent)
        {
            return new List<
                CombatTriggerCandidate<
                    ICombatTriggerHandler>>(
                        source.DiscoverTriggers(
                            state,
                            sourceEvent));
        }

        private static
            BattleEndStartedCombatEvent
            CreateBattleEndEvent()
        {
            return new
                BattleEndStartedCombatEvent(
                    CreateChildMetadata());
        }

        private static NormalAttackCombatEvent
            CreateNormalAttackEvent()
        {
            return new NormalAttackCombatEvent(
                CreateChildMetadata(),
                new InstanceId(1),
                new BoardPosition(
                    CombatSide.Player,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardSeason.Summer,
                new InstanceId(101),
                new BoardPosition(
                    CombatSide.Enemy,
                    BoardRow.Front,
                    new BoardColumn(1)),
                CombatCardSeason.Winter,
                baseDamage: 1);
        }

        private static CombatEventMetadata
            CreateChildMetadata()
        {
            var triggerRootId =
                new CombatEventId(1);

            return new CombatEventMetadata(
                new CombatEventId(2),
                new CombatSequenceNumber(2),
                triggerRootId,
                triggerRootId);
        }

        private static
            CombatPetCardTriggerUsageCommitter
            CreateUsageCommitter()
        {
            return new
                CombatPetCardTriggerUsageCommitter(
                    new
                        CombatPetCardTriggerUsageRegistry());
        }

        private sealed class TestEnvironment
        {
            public CombatState State
            {
                get;
                set;
            }

            public CombatPetState Pet
            {
                get;
                set;
            }

            public CombatCardState Card
            {
                get;
                set;
            }

            public CombatPetCardTriggerUsageCommitter
                UsageCommitter
            {
                get;
                set;
            }

            public CombatFinalRankModifierRegistry
                FinalRankRegistry
            {
                get;
                set;
            }

            public MuskCatPetTriggerSource Source
            {
                get;
                set;
            }
        }
    }
}