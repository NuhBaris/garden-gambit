using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class LlamaPetTriggerTests
    {
        [Test]
        public void ConstructorsValidateAndPreserveDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentNullException>(() =>
                new LlamaPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new LlamaPetNormalAttackTriggerHandler(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    null));
            Assert.Throws<ArgumentNullException>(() =>
                new LlamaPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    null,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new LlamaPetTriggerSource(
                    environment.Side,
                    environment.Pet.InstanceId,
                    environment.Usage,
                    null));

            Assert.That(
                environment.Handler.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                environment.Handler
                    .TargetDamageReductionRegistry,
                Is.SameAs(environment.Reductions));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void RepeatedRankTargetRegistersFirstDamageReduction(
            CombatSide side,
            BoardRow row)
        {
            var environment = new Environment(side, row);
            var attack = environment.MakeTargetAttack();

            Assert.That(
                environment.Handler.CanTrigger(
                    environment.State,
                    attack),
                Is.True);

            environment.Handler.Resolve(
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
                Is.EqualTo(environment.Target.InstanceId));
            Assert.That(
                requests[0].ReductionAmount,
                Is.EqualTo(1));
            Assert.That(
                environment.ResolveDamage(attack, 5),
                Is.EqualTo(4));
        }

        [Test]
        public void UniqueRankTargetDoesNotTrigger()
        {
            var environment = new Environment(
                duplicateRank: 6);

            environment.AssertDoesNotTrigger(
                environment.MakeTargetAttack());
        }

        [Test]
        public void DuplicateInOtherRowDoesNotTrigger()
        {
            var environment = new Environment(
                duplicateInAffectedRow: false);

            environment.AssertDoesNotTrigger(
                environment.MakeTargetAttack());
        }

        [Test]
        public void DeathThresholdDuplicateDoesNotTrigger()
        {
            var environment = new Environment(
                duplicateCurrentHp: 0);

            environment.AssertDoesNotTrigger(
                environment.MakeTargetAttack());
        }

        [Test]
        public void RemovedDuplicateImmediatelyChangesDynamicEligibility()
        {
            var environment = new Environment();

            environment.State.GetSide(environment.Side)
                .RemoveCardFromCombat(
                    environment.DuplicatePosition);

            environment.AssertDoesNotTrigger(
                environment.MakeTargetAttack());
        }

        [Test]
        public void OtherSideTargetDoesNotTrigger()
        {
            var environment = new Environment();

            environment.AssertDoesNotTrigger(
                environment.MakeOpponentTargetAttack());
        }

        [Test]
        public void OnlyFirstPositiveDamageForCardIsReduced()
        {
            var environment = new Environment();
            var first = environment.MakeTargetAttack();
            var second = environment.MakeTargetAttack();

            Assert.That(
                environment.RegisterAndResolve(first, 5),
                Is.EqualTo(4));
            Assert.That(
                environment.RegisterAndResolve(second, 5),
                Is.EqualTo(5));
            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ZeroDamageDoesNotConsumeCardUsage()
        {
            var environment = new Environment();
            var zero = environment.MakeTargetAttack();
            var positive = environment.MakeTargetAttack();

            Assert.That(
                environment.RegisterAndResolve(zero, 0),
                Is.Zero);
            Assert.That(
                environment.UsageRegistry.Count,
                Is.Zero);
            Assert.That(
                environment.RegisterAndResolve(positive, 5),
                Is.EqualTo(4));
            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void EachRepeatedRankCardHasIndependentFirstUse()
        {
            var environment = new Environment();
            var targetAttack = environment.MakeTargetAttack();
            var duplicateAttack = environment.MakeDuplicateAttack();

            Assert.That(
                environment.RegisterAndResolve(
                    targetAttack,
                    3),
                Is.EqualTo(2));
            Assert.That(
                environment.RegisterAndResolve(
                    duplicateAttack,
                    3),
                Is.EqualTo(2));
            Assert.That(
                environment.UsageRegistry.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void SourceDiscoversOnlyEligibleNormalAttack()
        {
            var environment = new Environment();
            var source = environment.CreateSource();
            var attack = environment.MakeTargetAttack();
            var candidates = new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    source.DiscoverTriggers(
                        environment.State,
                        attack));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(source.Handler));

            source.Handler.Resolve(
                environment.State,
                attack);

            Assert.That(
                environment.ResolveDamage(attack, 3),
                Is.EqualTo(2));
            Assert.That(
                source.DiscoverTriggers(
                    environment.State,
                    environment.MakeTargetAttack()),
                Is.Empty);
        }

        [Test]
        public void RecreatedSourcePreservesConsumedCardUsage()
        {
            var environment = new Environment();
            var first = environment.CreateSource();
            var rebuilt = environment.CreateSource();
            var attack = environment.MakeTargetAttack();

            first.Handler.Resolve(
                environment.State,
                attack);
            environment.ResolveDamage(attack, 3);

            Assert.That(
                rebuilt.DiscoverTriggers(
                    environment.State,
                    environment.MakeTargetAttack()),
                Is.Empty);
        }

        [Test]
        public void FactoryValidatesAndPreservesDependencies()
        {
            var environment = new Environment();

            Assert.Throws<ArgumentException>(() =>
                new LlamaPetTriggerSourceFactory(
                    default(DefinitionId),
                    environment.Usage,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new LlamaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    null,
                    environment.Reductions));
            Assert.Throws<ArgumentNullException>(() =>
                new LlamaPetTriggerSourceFactory(
                    environment.Pet.DefinitionId,
                    environment.Usage,
                    null));

            Assert.That(
                environment.Factory.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                environment.Factory
                    .TargetDamageReductionRegistry,
                Is.SameAs(environment.Reductions));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void FactoryCreatesOneSourceForValidOwner(
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
                Is.TypeOf<LlamaPetTriggerSource>());

            var source = (LlamaPetTriggerSource)sources[0];

            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(
                source.PetInstanceId,
                Is.EqualTo(environment.Pet.InstanceId));
            Assert.That(
                source.UsageCommitter,
                Is.SameAs(environment.Usage));
            Assert.That(
                source.TargetDamageReductionRegistry,
                Is.SameAs(environment.Reductions));
        }

        [Test]
        public void FactoryRejectsInvalidOwnerAndPet()
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
            public readonly CombatCardState Target;
            public readonly CombatCardState Duplicate;
            public readonly CombatCardState Opponent;
            public readonly CombatPetState Pet;
            public readonly BoardPosition TargetPosition;
            public readonly BoardPosition DuplicatePosition;
            public readonly BoardPosition OpponentPosition;
            public readonly CombatPetCardTriggerUsageRegistry
                UsageRegistry =
                    new CombatPetCardTriggerUsageRegistry();
            public readonly CombatPetCardTriggerUsageCommitter
                Usage;
            public readonly
                CombatNormalAttackTargetDamageReductionRegistry
                Reductions = new
                    CombatNormalAttackTargetDamageReductionRegistry();
            public readonly
                CombatNormalAttackTargetDamageReductionResolver
                ReductionResolver;
            public readonly LlamaPetNormalAttackTriggerHandler
                Handler;
            public readonly LlamaPetTriggerSourceFactory Factory;
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatStartedCombatEvent Root;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow row = BoardRow.Front,
                int targetRank = 5,
                int duplicateRank = 5,
                bool duplicateInAffectedRow = true,
                int duplicateCurrentHp = 10)
            {
                Side = side;
                Row = row;

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;
                var duplicateRow = duplicateInAffectedRow
                    ? row
                    : OtherRow(row);

                TargetPosition = new BoardPosition(
                    side,
                    row,
                    new BoardColumn(1));
                DuplicatePosition = new BoardPosition(
                    side,
                    duplicateRow,
                    new BoardColumn(2));
                OpponentPosition = new BoardPosition(
                    opposingSide,
                    BoardRow.Front,
                    new BoardColumn(1));

                Target = Card(
                    "test.llama_target",
                    1,
                    targetRank,
                    currentHp: 10);
                Duplicate = Card(
                    "test.llama_duplicate",
                    2,
                    duplicateRank,
                    currentHp: duplicateCurrentHp);
                Opponent = Card(
                    "test.llama_opponent",
                    101,
                    8,
                    currentHp: 10);

                var own = MakeSide(
                    side,
                    new[]
                    {
                        new CardPlacement(
                            TargetPosition,
                            Target),
                        new CardPlacement(
                            DuplicatePosition,
                            Duplicate)
                    },
                    slotPrefix: 0);
                var opposing = MakeSide(
                    opposingSide,
                    new[]
                    {
                        new CardPlacement(
                            OpponentPosition,
                            Opponent)
                    },
                    slotPrefix: 100);

                var definitionId = new DefinitionId(
                    "test.llama");
                var upper = new CombatPetState(
                    definitionId,
                    new InstanceId(1002));
                var lower = new CombatPetState(
                    definitionId,
                    new InstanceId(1001));

                Pet = row == BoardRow.Front
                    ? upper
                    : lower;

                var ownPets = new CombatSidePetState(
                    side,
                    new CombatPetRegistry(
                        new[] { upper, lower }));
                var opposingPets = new CombatSidePetState(
                    opposingSide,
                    new CombatPetRegistry(
                        Array.Empty<CombatPetState>()));

                State = new CombatState(
                    side == CombatSide.Player
                        ? own
                        : opposing,
                    side == CombatSide.Enemy
                        ? own
                        : opposing,
                    side == CombatSide.Player
                        ? ownPets
                        : opposingPets,
                    side == CombatSide.Enemy
                        ? ownPets
                        : opposingPets);

                Usage =
                    new CombatPetCardTriggerUsageCommitter(
                        UsageRegistry);
                ReductionResolver = new
                    CombatNormalAttackTargetDamageReductionResolver(
                        Reductions,
                        Usage);
                Handler =
                    new LlamaPetNormalAttackTriggerHandler(
                        side,
                        Pet.InstanceId,
                        Usage,
                        Reductions);
                Factory = new LlamaPetTriggerSourceFactory(
                    definitionId,
                    Usage,
                    Reductions);
                Root = new CombatStartedCombatEvent(
                    Metadata.CreateRoot());
            }

            public LlamaPetTriggerSource CreateSource()
            {
                var sources = new List<ICombatTriggerSource>(
                    Factory.CreateSources(Side, Pet));

                return (LlamaPetTriggerSource)sources[0];
            }

            public NormalAttackCombatEvent MakeTargetAttack()
            {
                return MakeAttack(
                    Target,
                    TargetPosition);
            }

            public NormalAttackCombatEvent MakeDuplicateAttack()
            {
                return MakeAttack(
                    Duplicate,
                    DuplicatePosition);
            }

            public NormalAttackCombatEvent
                MakeOpponentTargetAttack()
            {
                return new NormalAttackCombatEvent(
                    Metadata.CreateChild(Root.Metadata),
                    Target.InstanceId,
                    TargetPosition,
                    Opponent.InstanceId,
                    OpponentPosition,
                    5);
            }

            public void AssertDoesNotTrigger(
                NormalAttackCombatEvent attack)
            {
                Assert.That(
                    Handler.CanTrigger(State, attack),
                    Is.False);

                Handler.Resolve(State, attack);

                Assert.That(Reductions.Count, Is.Zero);
            }

            public int RegisterAndResolve(
                NormalAttackCombatEvent attack,
                int incomingDamage)
            {
                if (Handler.CanTrigger(State, attack))
                {
                    Handler.Resolve(State, attack);
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

            private NormalAttackCombatEvent MakeAttack(
                CombatCardState target,
                BoardPosition targetPosition)
            {
                return new NormalAttackCombatEvent(
                    Metadata.CreateChild(Root.Metadata),
                    Opponent.InstanceId,
                    OpponentPosition,
                    target.InstanceId,
                    targetPosition,
                    5);
            }

            private static BoardRow OtherRow(
                BoardRow row)
            {
                return row == BoardRow.Front
                    ? BoardRow.Back
                    : BoardRow.Front;
            }

            private static CombatCardState Card(
                string definition,
                long instanceId,
                int rank,
                int currentHp)
            {
                return new CombatCardState(
                    new DefinitionId(definition),
                    new InstanceId(instanceId),
                    new CardRank(rank),
                    CombatCardSeason.Spring,
                    10,
                    currentHp,
                    0,
                    5);
            }

            private static CombatSideState MakeSide(
                CombatSide side,
                IReadOnlyList<CardPlacement> placements,
                long slotPrefix)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                var placementByPosition =
                    new Dictionary<
                        BoardPosition,
                        CombatCardState>();

                foreach (var placement in placements)
                {
                    placementByPosition.Add(
                        placement.Position,
                        placement.Card);
                    cards.Add(placement.Card);
                }

                foreach (var row in new[]
                         {
                             BoardRow.Front,
                             BoardRow.Back
                         })
                {
                    for (var column = 1;
                         column <= 2;
                         column++)
                    {
                        var position = new BoardPosition(
                            side,
                            row,
                            new BoardColumn(column));
                        CombatCardState occupant;
                        var slotId = slotPrefix +
                                     (row == BoardRow.Front
                                         ? 0
                                         : 10) +
                                     column;

                        if (placementByPosition.TryGetValue(
                                position,
                                out occupant))
                        {
                            slots.Add(new CombatSlotState(
                                new SlotId(slotId),
                                position,
                                occupant.InstanceId));
                        }
                        else
                        {
                            slots.Add(new CombatSlotState(
                                new SlotId(slotId),
                                position));
                        }
                    }
                }

                return new CombatSideState(
                    new CombatBoardState(side, slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(20),
                    new AttackMultiplier(1));
            }

            private sealed class CardPlacement
            {
                public CardPlacement(
                    BoardPosition position,
                    CombatCardState card)
                {
                    Position = position;
                    Card = card;
                }

                public BoardPosition Position
                {
                    get;
                }

                public CombatCardState Card
                {
                    get;
                }
            }
        }
    }
}
