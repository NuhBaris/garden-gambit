using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class PandaPetTriggerTests
    {
        [Test]
        public void ConstructorsValidateAndPreserveDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetBattleStartTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null));
            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.LimitedUsage,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.ActivationUsage,
                    null,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.ActivationUsage,
                    environment.LimitedUsage,
                    null));

            Assert.That(
                environment.BattleStartHandler
                    .ActivationUsageCommitter,
                Is.SameAs(environment.ActivationUsage));
            Assert.That(
                environment.NormalAttackHandler
                    .ActivationUsageCommitter,
                Is.SameAs(environment.ActivationUsage));
            Assert.That(
                environment.NormalAttackHandler
                    .LimitedUsageCommitter,
                Is.SameAs(environment.LimitedUsage));
            Assert.That(
                environment.NormalAttackHandler
                    .TargetDamageReductionRegistry,
                Is.SameAs(environment.Reductions));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void ThreeSnapshotSeasonTypesActivateCorrectRow(
            CombatSide side,
            BoardRow row)
        {
            var environment = new Environment(side, row);

            Assert.That(
                environment.BattleStartHandler.CanTrigger(
                    environment.State,
                    environment.PetStage),
                Is.True);

            environment.Activate();

            Assert.That(
                environment.ActivationUsage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
            Assert.That(
                environment.ActivationRegistry.Count,
                Is.EqualTo(1));
            Assert.That(
                environment.LimitedRegistry.Count,
                Is.Zero);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void FewerThanThreeSnapshotSeasonTypesDoNotActivate(
            int distinctSeasonCount)
        {
            var environment = new Environment(
                distinctSeasonCount: distinctSeasonCount);

            Assert.That(
                environment.BattleStartHandler.CanTrigger(
                    environment.State,
                    environment.PetStage),
                Is.False);

            environment.Activate();

            Assert.That(
                environment.ActivationRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void SeasonlessCountsAsASeasonType()
        {
            var environment = new Environment(
                distinctSeasonCount: 3);

            Assert.That(
                environment.Card(
                    environment.Side,
                    environment.Row,
                    3).Season,
                Is.EqualTo(CombatCardSeason.Seasonless));

            environment.Activate();

            Assert.That(
                environment.ActivationUsage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
        }

        [Test]
        public void ActivationUsesCapturedSnapshotAfterCardsAreRemoved()
        {
            var environment = new Environment();
            var sideState = environment.State.GetSide(
                environment.Side);

            sideState.RemoveCardFromCombat(
                environment.Position(environment.Row, 2));
            sideState.RemoveCardFromCombat(
                environment.Position(environment.Row, 3));

            environment.Activate();

            Assert.That(
                environment.ActivationUsage.HasTriggered(
                    environment.Pet.InstanceId),
                Is.True);
        }

        [Test]
        public void MissingSnapshotUsesLegacyNoOp()
        {
            var environment = new Environment();
            var source = environment.MakePetStage(
                withSnapshot: false);

            Assert.That(
                environment.BattleStartHandler.CanTrigger(
                    environment.State,
                    source),
                Is.False);

            environment.BattleStartHandler.Resolve(
                environment.State,
                source);

            Assert.That(
                environment.ActivationRegistry.Count,
                Is.Zero);
        }

        [TestCase(CombatBattleStartStage.Slot)]
        [TestCase(CombatBattleStartStage.Card)]
        public void OtherBattleStartStagesDoNotActivate(
            CombatBattleStartStage stage)
        {
            var environment = new Environment();
            var source = environment.MakePetStage(
                stage: stage);

            Assert.That(
                environment.BattleStartHandler.CanTrigger(
                    environment.State,
                    source),
                Is.False);
            Assert.Throws<InvalidOperationException>(() =>
                environment.BattleStartHandler.Resolve(
                    environment.State,
                    source));
            Assert.That(
                environment.ActivationRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void InactivePandaDoesNotRegisterReduction()
        {
            var environment = new Environment();
            var attack = environment.MakeEligibleAttack(1);

            Assert.That(
                environment.NormalAttackHandler.CanTrigger(
                    environment.State,
                    attack),
                Is.False);

            environment.NormalAttackHandler.Resolve(
                environment.State,
                attack);

            Assert.That(environment.Reductions.Count, Is.Zero);
            Assert.That(
                environment.ResolveDamage(attack, 5),
                Is.EqualTo(5));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void ActivePandaRegistersLimitedRowGlobalReduction(
            CombatSide side,
            BoardRow row)
        {
            var environment = new Environment(side, row);
            environment.Activate();
            var attack = environment.MakeEligibleAttack(1);

            Assert.That(
                environment.NormalAttackHandler.CanTrigger(
                    environment.State,
                    attack),
                Is.True);

            environment.NormalAttackHandler.Resolve(
                environment.State,
                attack);

            var requests = environment.Reductions.GetRequests(
                attack.Metadata.EventId);

            Assert.That(requests.Count, Is.EqualTo(1));
            Assert.That(
                requests[0].PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
            Assert.That(
                requests[0].TargetCardInstanceId,
                Is.EqualTo(attack.TargetInstanceId));
            Assert.That(
                requests[0].ReductionAmount,
                Is.EqualTo(1));
            Assert.That(
                requests[0].MaximumPetUsageCount,
                Is.EqualTo(2));
            Assert.That(
                requests[0].UsesLimitedPetUsage,
                Is.True);
            Assert.That(
                environment.ResolveDamage(attack, 5),
                Is.EqualTo(4));
        }

        [Test]
        public void OtherRowAndOtherSideAttacksAreIgnored()
        {
            var environment = new Environment();
            environment.Activate();
            var otherRow = environment.Row == BoardRow.Front
                ? BoardRow.Back
                : BoardRow.Front;
            var otherSide = environment.Side == CombatSide.Player
                ? CombatSide.Enemy
                : CombatSide.Player;
            var wrongRow = environment.MakeAttack(
                environment.Side,
                otherRow,
                1);
            var wrongSide = environment.MakeAttack(
                otherSide,
                environment.Row,
                1);

            Assert.That(
                environment.NormalAttackHandler.CanTrigger(
                    environment.State,
                    wrongRow),
                Is.False);
            Assert.That(
                environment.NormalAttackHandler.CanTrigger(
                    environment.State,
                    wrongSide),
                Is.False);

            environment.NormalAttackHandler.Resolve(
                environment.State,
                wrongRow);
            environment.NormalAttackHandler.Resolve(
                environment.State,
                wrongSide);

            Assert.That(environment.Reductions.Count, Is.Zero);
        }

        [Test]
        public void FirstTwoPositiveEventsReduceSameCardThirdDoesNot()
        {
            var environment = new Environment();
            environment.Activate();
            var first = environment.MakeEligibleAttack(1, 1);
            var second = environment.MakeEligibleAttack(2, 1);
            var third = environment.MakeEligibleAttack(3, 1);

            Assert.That(
                environment.RegisterAndResolve(first, 5),
                Is.EqualTo(4));
            Assert.That(
                environment.RegisterAndResolve(second, 5),
                Is.EqualTo(4));
            Assert.That(
                environment.RegisterAndResolve(third, 5),
                Is.EqualTo(5));
            Assert.That(
                environment.LimitedRegistry.GetUsageCount(
                    environment.Pet.InstanceId),
                Is.EqualTo(2));
        }

        [Test]
        public void FirstTwoPositiveEventsAreSharedAcrossCards()
        {
            var environment = new Environment();
            environment.Activate();
            var first = environment.MakeEligibleAttack(1, 1);
            var second = environment.MakeEligibleAttack(2, 2);
            var third = environment.MakeEligibleAttack(3, 3);

            Assert.That(
                environment.RegisterAndResolve(first, 3),
                Is.EqualTo(2));
            Assert.That(
                environment.RegisterAndResolve(second, 3),
                Is.EqualTo(2));
            Assert.That(
                environment.RegisterAndResolve(third, 3),
                Is.EqualTo(3));
            Assert.That(
                environment.LimitedRegistry.TotalUsageCount,
                Is.EqualTo(2));
        }

        [Test]
        public void ZeroDamageDoesNotConsumeCharge()
        {
            var environment = new Environment();
            environment.Activate();
            var zero = environment.MakeEligibleAttack(1, 1);
            var positive = environment.MakeEligibleAttack(2, 1);

            Assert.That(
                environment.RegisterAndResolve(zero, 0),
                Is.Zero);
            Assert.That(
                environment.LimitedRegistry.TotalUsageCount,
                Is.Zero);
            Assert.That(
                environment.RegisterAndResolve(positive, 3),
                Is.EqualTo(2));
            Assert.That(
                environment.LimitedRegistry.TotalUsageCount,
                Is.EqualTo(1));
        }

        [Test]
        public void CompositeSourceDiscoversActivationThenReduction()
        {
            var environment = new Environment();
            var source = environment.CreateSource();
            var activationCandidates = new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        environment.State,
                        environment.PetStage));

            Assert.That(
                activationCandidates.Count,
                Is.EqualTo(1));
            Assert.That(
                activationCandidates[0].Trigger,
                Is.SameAs(source.BattleStartHandler));

            source.BattleStartHandler.Resolve(
                environment.State,
                environment.PetStage);

            var attack = environment.MakeEligibleAttack(1);
            var damageCandidates = new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        environment.State,
                        attack));

            Assert.That(damageCandidates.Count, Is.EqualTo(1));
            Assert.That(
                damageCandidates[0].Trigger,
                Is.SameAs(source.NormalAttackHandler));

            source.NormalAttackHandler.Resolve(
                environment.State,
                attack);

            Assert.That(
                environment.ResolveDamage(attack, 5),
                Is.EqualTo(4));
        }

        [Test]
        public void RecreatedSourcePreservesActivationAndConsumedCharges()
        {
            var environment = new Environment();
            var first = environment.CreateSource();
            var rebuilt = environment.CreateSource();

            first.BattleStartHandler.Resolve(
                environment.State,
                environment.PetStage);
            environment.RegisterAndResolve(
                environment.MakeEligibleAttack(1),
                5,
                first.NormalAttackHandler);
            environment.RegisterAndResolve(
                environment.MakeEligibleAttack(2),
                5,
                rebuilt.NormalAttackHandler);

            var third = environment.MakeEligibleAttack(3);

            Assert.That(
                rebuilt.NormalAttackHandler.CanTrigger(
                    environment.State,
                    third),
                Is.False);
            Assert.That(
                environment.LimitedRegistry.TotalUsageCount,
                Is.EqualTo(2));
        }

        [Test]
        public void FactoryValidatesAndPreservesDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new PandaPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.ActivationUsage,
                    environment.LimitedUsage,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.LimitedUsage,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.ActivationUsage,
                    null,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new PandaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.ActivationUsage,
                    environment.LimitedUsage,
                    null));

            Assert.That(
                environment.Factory.ActivationUsageCommitter,
                Is.SameAs(environment.ActivationUsage));
            Assert.That(
                environment.Factory.LimitedUsageCommitter,
                Is.SameAs(environment.LimitedUsage));
            Assert.That(
                environment.Factory
                    .TargetDamageReductionRegistry,
                Is.SameAs(environment.Reductions));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FactoryCreatesOneCompositeSource(
            CombatSide side)
        {
            var environment = new Environment(side);
            var sources = new List<ICombatTriggerSource>(
                environment.Factory.CreateSources(
                    side,
                    environment.Pet));

            Assert.That(sources.Count, Is.EqualTo(1));
            Assert.That(
                sources[0],
                Is.TypeOf<PandaPetTriggerSource>());

            var source = (PandaPetTriggerSource)sources[0];

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
            Assert.That(
                source.ActivationUsageCommitter,
                Is.SameAs(environment.ActivationUsage));
            Assert.That(
                source.LimitedUsageCommitter,
                Is.SameAs(environment.LimitedUsage));
            Assert.That(
                source.TargetDamageReductionRegistry,
                Is.SameAs(environment.Reductions));
        }

        [Test]
        public void FactoryRejectsInvalidOwner()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                environment.Factory.CreateSources(
                    (CombatSide)99,
                    environment.Pet));
            Assert.Throws<ArgumentNullException>(() =>
                environment.Factory.CreateSources(
                    environment.Side,
                    null));

            var otherPet = new CombatPetState(
                new DefinitionId("test.other_pet"),
                new InstanceId(2000));

            Assert.Throws<ArgumentException>(() =>
                environment.Factory.CreateSources(
                    environment.Side,
                    otherPet));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow Row;
            public readonly CombatState State;
            public readonly CombatPetState Pet;
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly BattleStartStageStartedCombatEvent
                PetStage;
            public readonly CombatPetTriggerUsageRegistry
                ActivationRegistry =
                    new CombatPetTriggerUsageRegistry();
            public readonly CombatPetTriggerUsageCommitter
                ActivationUsage;
            public readonly CombatPetLimitedTriggerUsageRegistry
                LimitedRegistry =
                    new CombatPetLimitedTriggerUsageRegistry();
            public readonly CombatPetLimitedTriggerUsageCommitter
                LimitedUsage;
            public readonly
                CombatNormalAttackTargetDamageReductionRegistry
                Reductions = new
                    CombatNormalAttackTargetDamageReductionRegistry();
            public readonly
                CombatNormalAttackTargetDamageReductionResolver
                ReductionResolver;
            public readonly PandaPetBattleStartTriggerHandler
                BattleStartHandler;
            public readonly PandaPetNormalAttackTriggerHandler
                NormalAttackHandler;
            public readonly PandaPetTriggerSourceFactory Factory;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int distinctSeasonCount = 3)
            {
                Side = side;
                Row = row;

                var definitionId = new DefinitionId(
                    "test.panda");
                var upper = new CombatPetState(
                    definitionId,
                    new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId,
                    new InstanceId(1001));

                Pet = row == BoardRow.Front
                    ? upper
                    : lower;

                var otherSide = side == CombatSide.Player
                    ? CombatSide.Enemy
                    : CombatSide.Player;
                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { upper, lower }));
                var otherPets = new CombatSidePetState(
                    otherSide,
                    new CombatPetRegistry(
                        Array.Empty<CombatPetState>()));

                State = new CombatState(
                    MakeSide(
                        CombatSide.Player,
                        distinctSeasonCount),
                    MakeSide(
                        CombatSide.Enemy,
                        distinctSeasonCount),
                    side == CombatSide.Player
                        ? ownPets
                        : otherPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : otherPets);

                Root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
                Log.Append(Root);
                PetStage = MakePetStage();

                ActivationUsage =
                    new CombatPetTriggerUsageCommitter(
                        ActivationRegistry);
                LimitedUsage =
                    new CombatPetLimitedTriggerUsageCommitter(
                        LimitedRegistry);
                var cardUsage =
                    new CombatPetCardTriggerUsageCommitter(
                        new CombatPetCardTriggerUsageRegistry());
                ReductionResolver = new
                    CombatNormalAttackTargetDamageReductionResolver(
                        Reductions,
                        cardUsage,
                        LimitedUsage);
                BattleStartHandler =
                    new PandaPetBattleStartTriggerHandler(
                        side,
                        Pet.InstanceId,
                        ActivationUsage);
                NormalAttackHandler =
                    new PandaPetNormalAttackTriggerHandler(
                        side,
                        Pet.InstanceId,
                        ActivationUsage,
                        LimitedUsage,
                        Reductions);
                Factory = new PandaPetTriggerSourceFactory(
                    definitionId,
                    ActivationUsage,
                    LimitedUsage,
                    Reductions);
            }

            public BoardPosition Position(
                BoardRow row,
                int column)
            {
                return new BoardPosition(
                    Side,
                    row,
                    new BoardColumn(column));
            }

            public CombatCardState Card(
                CombatSide side,
                BoardRow row,
                int column)
            {
                return State.GetSide(side).GetCardAt(
                    new BoardPosition(
                        side,
                        row,
                        new BoardColumn(column)));
            }

            public void Activate()
            {
                BattleStartHandler.Resolve(
                    State,
                    PetStage);
            }

            public PandaPetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory.CreateSources(Side, Pet));

                return (PandaPetTriggerSource)sources[0];
            }

            public BattleStartStageStartedCombatEvent MakePetStage(
                CombatBattleStartStage stage =
                    CombatBattleStartStage.Pet,
                bool withSnapshot = true)
            {
                var metadata = Metadata.CreateChild(
                    Root.Metadata);
                var source = withSnapshot
                    ? new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage,
                        new CombatBattleStartSnapshotResolver()
                            .Resolve(State))
                    : new BattleStartStageStartedCombatEvent(
                        metadata,
                        stage);

                Log.Append(source);

                return source;
            }

            public NormalAttackCombatEvent MakeEligibleAttack(
                long eventOffset,
                int column = 1)
            {
                return MakeAttack(
                    Side,
                    Row,
                    column,
                    eventOffset);
            }

            public NormalAttackCombatEvent MakeAttack(
                CombatSide targetSide,
                BoardRow targetRow,
                int column,
                long eventOffset = 1)
            {
                var attackerSide =
                    targetSide == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;
                var attacker = Card(
                    attackerSide,
                    BoardRow.Front,
                    column);
                var target = Card(
                    targetSide,
                    targetRow,
                    column);

                return new NormalAttackCombatEvent(
                    Metadata.CreateChild(Root.Metadata),
                    attacker.InstanceId,
                    new BoardPosition(
                        attackerSide,
                        BoardRow.Front,
                        new BoardColumn(column)),
                    attacker.Season,
                    target.InstanceId,
                    new BoardPosition(
                        targetSide,
                        targetRow,
                        new BoardColumn(column)),
                    target.Season,
                    5);
            }

            public int RegisterAndResolve(
                NormalAttackCombatEvent attack,
                int incomingDamage,
                PandaPetNormalAttackTriggerHandler handler = null)
            {
                var selectedHandler =
                    handler ?? NormalAttackHandler;

                if (selectedHandler.CanTrigger(State, attack))
                {
                    selectedHandler.Resolve(State, attack);
                }

                return ResolveDamage(
                    attack,
                    incomingDamage);
            }

            public int ResolveDamage(
                NormalAttackCombatEvent attack,
                int incomingDamage)
            {
                return ReductionResolver.ResolveDamage(
                    attack,
                    incomingDamage);
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                int distinctSeasonCount)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();

                foreach (var row in new[]
                         {
                             BoardRow.Back,
                             BoardRow.Front
                         })
                {
                    foreach (var column in new[]
                             {
                                 5, 2, 1, 4, 3
                             })
                    {
                        var prefix =
                            (side == CombatSide.Player
                                ? 0
                                : 100) +
                            (row == BoardRow.Front
                                ? 0
                                : 10);
                        var card = new CombatCardState(
                            new DefinitionId(
                                "test.panda_card"),
                            new InstanceId(
                                prefix + 6 - column),
                            new CardRank(column + 1),
                            CombatCardSuit.Fruit,
                            SeasonForColumn(
                                column,
                                distinctSeasonCount),
                            10,
                            10,
                            0,
                            5);

                        cards.Add(card);
                        slots.Add(new CombatSlotState(
                            new SlotId(prefix + column),
                            new BoardPosition(
                                side,
                                row,
                                new BoardColumn(column)),
                            card.InstanceId));
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private static CombatCardSeason SeasonForColumn(
                int column,
                int distinctSeasonCount)
            {
                if (distinctSeasonCount <= 0)
                {
                    return CombatCardSeason.Unspecified;
                }

                if (distinctSeasonCount == 1)
                {
                    return CombatCardSeason.Spring;
                }

                if (distinctSeasonCount == 2)
                {
                    return column % 2 == 0
                        ? CombatCardSeason.Summer
                        : CombatCardSeason.Spring;
                }

                if (column == 2)
                {
                    return CombatCardSeason.Summer;
                }

                if (column == 3)
                {
                    return CombatCardSeason.Seasonless;
                }

                return CombatCardSeason.Spring;
            }
        }
    }
}
