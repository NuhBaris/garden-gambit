using System;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class
        MuskCatPetBattleEndTriggerHandlerTests
    {
        [Test]
        public void
            Constructor_WithNullUsageCommitter_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _ =
                    new
                        MuskCatPetBattleEndTriggerHandler(
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
                    new
                        MuskCatPetBattleEndTriggerHandler(
                            CombatSide.Player,
                            new InstanceId(1001),
                            CreateUsageCommitter(),
                            null));
        }

        [Test]
        public void
            CanTrigger_WithExactlyOneLivingCardInAffectedRow_ReturnsTrue()
        {
            var pet =
                CreatePet(
                    1001);

            var card =
                CreateCard(
                    1);

            var environment =
                CreateEnvironment(
                    pet,
                    new[]
                    {
                        card
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            card.InstanceId)
                    });

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                result,
                Is.True);

            Assert.That(
                environment.FinalRankRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            CanTrigger_WithNoLivingCardInAffectedRow_ReturnsFalse()
        {
            var pet =
                CreatePet(
                    1001);

            var environment =
                CreateEnvironment(
                    pet,
                    Array.Empty<
                        CombatCardState>(),
                    Array.Empty<
                        CombatSlotState>());

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.FinalRankRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            CanTrigger_WithTwoLivingCardsInAffectedRow_ReturnsFalse()
        {
            var firstCard =
                CreateCard(
                    1);

            var secondCard =
                CreateCard(
                    2);

            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001),
                    new[]
                    {
                        firstCard,
                        secondCard
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            firstCard.InstanceId),

                        CreateOccupiedSlot(
                            slotId: 2,
                            row: BoardRow.Front,
                            column: 2,
                            secondCard.InstanceId)
                    });

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                result,
                Is.False);

            Assert.That(
                environment.FinalRankRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void
            CanTrigger_IgnoresLivingCardsOutsideAffectedRow()
        {
            var affectedRowCard =
                CreateCard(
                    1);

            var otherRowCard =
                CreateCard(
                    2);

            var environment =
                CreateEnvironment(
                    CreatePet(
                        1001),
                    new[]
                    {
                        affectedRowCard,
                        otherRowCard
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            affectedRowCard.InstanceId),

                        CreateOccupiedSlot(
                            slotId: 2,
                            row: BoardRow.Back,
                            column: 1,
                            otherRowCard.InstanceId)
                    });

            var result =
                environment.Handler.CanTrigger(
                    environment.State,
                    CreateBattleEndEvent());

            Assert.That(
                result,
                Is.True);
        }

        [Test]
        public void
            Resolve_WithExactlyOneLivingCard_AddsFourFinalRankAndCommitsUsage()
        {
            var pet =
                CreatePet(
                    1001);

            var card =
                CreateCard(
                    1);

            var environment =
                CreateEnvironment(
                    pet,
                    new[]
                    {
                        card
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            card.InstanceId)
                    });

            environment.Handler.Resolve(
                environment.State,
                CreateBattleEndEvent());

            Assert.That(
                environment.FinalRankRegistry
                    .GetTotalModifier(
                        card.InstanceId),
                Is.EqualTo(
                    MuskCatPetBattleEndTriggerHandler
                        .FinalRankBonus));

            Assert.That(
                environment.UsageCommitter
                    .HasTriggered(
                        pet.InstanceId,
                        card.InstanceId),
                Is.True);

            Assert.That(
                environment.FinalRankRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            Resolve_CalledTwice_DoesNotRepeatFinalRankBonus()
        {
            var pet =
                CreatePet(
                    1001);

            var card =
                CreateCard(
                    1);

            var environment =
                CreateEnvironment(
                    pet,
                    new[]
                    {
                        card
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            card.InstanceId)
                    });

            var battleEndEvent =
                CreateBattleEndEvent();

            environment.Handler.Resolve(
                environment.State,
                battleEndEvent);

            var canTriggerAgain =
                environment.Handler.CanTrigger(
                    environment.State,
                    battleEndEvent);

            environment.Handler.Resolve(
                environment.State,
                battleEndEvent);

            Assert.That(
                canTriggerAgain,
                Is.False);

            Assert.That(
                environment.FinalRankRegistry
                    .GetTotalModifier(
                        card.InstanceId),
                Is.EqualTo(
                    MuskCatPetBattleEndTriggerHandler
                        .FinalRankBonus));

            Assert.That(
                environment.FinalRankRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void
            Resolve_WithTwoMuskCats_AllowsOneBonusPerPetForItsRow()
        {
            var upperPet =
                CreatePet(
                    1001);

            var lowerPet =
                CreatePet(
                    1002);

            var upperCard =
                CreateCard(
                    1);

            var lowerCard =
                CreateCard(
                    2);

            var state =
                CreateState(
                    new[]
                    {
                        upperPet,
                        lowerPet
                    },
                    new[]
                    {
                        upperCard,
                        lowerCard
                    },
                    new[]
                    {
                        CreateOccupiedSlot(
                            slotId: 1,
                            row: BoardRow.Front,
                            column: 1,
                            upperCard.InstanceId),

                        CreateOccupiedSlot(
                            slotId: 2,
                            row: BoardRow.Back,
                            column: 1,
                            lowerCard.InstanceId)
                    });

            var usageCommitter =
                CreateUsageCommitter();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            var upperHandler =
                new
                    MuskCatPetBattleEndTriggerHandler(
                        CombatSide.Player,
                        upperPet.InstanceId,
                        usageCommitter,
                        finalRankRegistry);

            var lowerHandler =
                new
                    MuskCatPetBattleEndTriggerHandler(
                        CombatSide.Player,
                        lowerPet.InstanceId,
                        usageCommitter,
                        finalRankRegistry);

            var battleEndEvent =
                CreateBattleEndEvent();

            Assert.That(
                upperHandler.CanTrigger(
                    state,
                    battleEndEvent),
                Is.True);

            Assert.That(
                lowerHandler.CanTrigger(
                    state,
                    battleEndEvent),
                Is.True);

            upperHandler.Resolve(
                state,
                battleEndEvent);

            lowerHandler.Resolve(
                state,
                battleEndEvent);

            Assert.That(
                finalRankRegistry.GetTotalModifier(
                    upperCard.InstanceId),
                Is.EqualTo(4));

            Assert.That(
                finalRankRegistry.GetTotalModifier(
                    lowerCard.InstanceId),
                Is.EqualTo(4));

            Assert.That(
                usageCommitter.HasTriggered(
                    upperPet.InstanceId,
                    upperCard.InstanceId),
                Is.True);

            Assert.That(
                usageCommitter.HasTriggered(
                    lowerPet.InstanceId,
                    lowerCard.InstanceId),
                Is.True);

            Assert.That(
                finalRankRegistry.Count,
                Is.EqualTo(2));
        }

        private static TestEnvironment
            CreateEnvironment(
                CombatPetState pet,
                CombatCardState[] cards,
                CombatSlotState[] slots)
        {
            var usageCommitter =
                CreateUsageCommitter();

            var finalRankRegistry =
                new
                    CombatFinalRankModifierRegistry();

            return new TestEnvironment
            {
                State =
                    CreateState(
                        new[]
                        {
                            pet
                        },
                        cards,
                        slots),

                UsageCommitter =
                    usageCommitter,

                FinalRankRegistry =
                    finalRankRegistry,

                Handler =
                    new
                        MuskCatPetBattleEndTriggerHandler(
                            CombatSide.Player,
                            pet.InstanceId,
                            usageCommitter,
                            finalRankRegistry)
            };
        }

        private static CombatState CreateState(
            CombatPetState[] playerPets,
            CombatCardState[] playerCards,
            CombatSlotState[] playerSlots)
        {
            return new CombatState(
                CreateSide(
                    CombatSide.Player,
                    playerCards,
                    playerSlots),
                CreateSide(
                    CombatSide.Enemy,
                    Array.Empty<
                        CombatCardState>(),
                    Array.Empty<
                        CombatSlotState>()),
                new CombatSidePetState(
                    CombatSide.Player,
                    new CombatPetRegistry(
                        playerPets)),
                new CombatSidePetState(
                    CombatSide.Enemy,
                    new CombatPetRegistry(
                        Array.Empty<
                            CombatPetState>())));
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

        private static CombatCardState CreateCard(
            long instanceId)
        {
            return new CombatCardState(
                new DefinitionId(
                    $"test.card-{instanceId}"),
                new InstanceId(
                    instanceId),
                new CardRank(2),
                hpCapacity: 5,
                currentHp: 5,
                armor: 0,
                attack: 0);
        }

        private static CombatPetState CreatePet(
            long instanceId)
        {
            return new CombatPetState(
                new DefinitionId(
                    "pet.musk_cat"),
                new InstanceId(
                    instanceId));
        }

        private static CombatSlotState
            CreateOccupiedSlot(
                long slotId,
                BoardRow row,
                int column,
                InstanceId occupantInstanceId)
        {
            return new CombatSlotState(
                new SlotId(
                    slotId),
                new BoardPosition(
                    CombatSide.Player,
                    row,
                    new BoardColumn(
                        column)),
                occupantInstanceId);
        }

        private static
            BattleEndStartedCombatEvent
            CreateBattleEndEvent()
        {
            var triggerRootId =
                new CombatEventId(1);

            var metadata =
                new CombatEventMetadata(
                    new CombatEventId(2),
                    new CombatSequenceNumber(2),
                    triggerRootId,
                    triggerRootId);

            return new
                BattleEndStartedCombatEvent(
                    metadata);
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

            public
                MuskCatPetBattleEndTriggerHandler
                Handler
            {
                get;
                set;
            }
        }
    }
}