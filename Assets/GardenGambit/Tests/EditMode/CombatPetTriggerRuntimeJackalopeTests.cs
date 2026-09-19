using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeJackalopeTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var environment = new Environment();

            Assert.That(
                CombatPetDefinitionIds.JackalopeValue,
                Is.EqualTo("pet.jackalope"));
            Assert.That(
                CombatPetDefinitionIds.Jackalope,
                Is.EqualTo(
                    new DefinitionId("pet.jackalope")));

            var full = environment.FullFactoryRegistry();
            var rescueAware =
                environment.RescueAwareFactoryRegistry();
            var complete =
                environment.CompleteFactoryRegistry();

            Assert.That(
                full.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .FullFactoryCount));
            Assert.That(
                rescueAware.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .RescueAwareFactoryCount));
            Assert.That(
                complete.Count,
                Is.EqualTo(
                    CombatPetCatalogTestExpectations
                        .CompleteFactoryCount));
            Assert.That(
                environment.Runtime.FactoryRegistry.Count,
                Is.EqualTo(3));
            Assert.That(
                environment.Runtime.FactoryRegistry.Contains(
                    CombatPetDefinitionIds.Jackalope),
                Is.False);

            var factory =
                full.GetFactory(
                        CombatPetDefinitionIds.Jackalope)
                    as JackalopePetTriggerSourceFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(
                factory.PetDefinitionId,
                Is.EqualTo(
                    CombatPetDefinitionIds.Jackalope));
            Assert.That(
                factory.UsageCommitter,
                Is.SameAs(
                    environment.Runtime.UsageCommitter));
            Assert.That(
                factory.FinalRankModifierRegistry,
                Is.SameAs(
                    environment.Runtime
                        .FinalRankModifierRegistry));
            Assert.That(
                rescueAware.Contains(
                    CombatPetDefinitionIds.Jackalope),
                Is.True);
            Assert.That(
                complete.Contains(
                    CombatPetDefinitionIds.Jackalope),
                Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BuildsBothSourcesWithSharedDependencies(
            CombatSide side)
        {
            var environment = new Environment(side);
            var sources = environment.Sources();

            Assert.That(sources.Count, Is.EqualTo(2));

            var owners = new HashSet<InstanceId>();

            foreach (var item in sources.Sources)
            {
                var source =
                    item as JackalopePetTriggerSource;

                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(
                    source.UsageCommitter,
                    Is.SameAs(
                        environment.Runtime.UsageCommitter));
                Assert.That(
                    source.FinalRankModifierRegistry,
                    Is.SameAs(
                        environment.Runtime
                            .FinalRankModifierRegistry));
                Assert.That(
                    owners.Add(source.PetInstanceId),
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

        [TestCase(CombatSide.Player, BoardRow.Front, 3)]
        [TestCase(CombatSide.Player, BoardRow.Back, 2)]
        [TestCase(CombatSide.Enemy, BoardRow.Front, 3)]
        [TestCase(CombatSide.Enemy, BoardRow.Back, 2)]
        public void Runtime_DiscoveryUsesLockedPokerForCorrectPetRow(
            CombatSide side,
            BoardRow row,
            int expectedBonus)
        {
            var environment = new Environment(
                side,
                row,
                row == BoardRow.Front
                    ? CombatPokerHand.HighCard
                    : CombatPokerHand.Pair);
            var sources = environment.Sources();
            var battleEnd = environment.BattleEnd();
            var candidates = Discover(
                sources,
                environment.State,
                battleEnd);

            Assert.That(candidates.Count, Is.EqualTo(1));

            var source =
                sources.Sources[
                    row == BoardRow.Front
                        ? 0
                        : 1]
                as JackalopePetTriggerSource;

            Assert.That(source, Is.Not.Null);
            Assert.That(
                candidates[0].Trigger,
                Is.SameAs(source.Handler));

            candidates[0].Trigger.Resolve(
                environment.State,
                battleEnd);

            var target = environment.Card(row);
            var otherTarget =
                environment.Card(OtherRow(row));

            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(target.InstanceId),
                Is.EqualTo(expectedBonus));
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(otherTarget.InstanceId),
                Is.Zero);
            Assert.That(
                environment.Runtime.UsageCommitter.HasTriggered(
                    source.PetInstanceId,
                    target.InstanceId),
                Is.True);
            Assert.That(
                Discover(
                    sources,
                    environment.State,
                    battleEnd),
                Is.Empty);
        }

        [TestCase(CombatPokerHand.ThreeOfAKind)]
        [TestCase(CombatPokerHand.Straight)]
        [TestCase(CombatPokerHand.FlushFive)]
        public void Runtime_NonEligibleLockedPokerCreatesNoCandidate(
            CombatPokerHand pokerHand)
        {
            var environment = new Environment(
                lockedPokerHand: pokerHand);
            var sources = environment.Sources();

            Assert.That(
                Discover(
                    sources,
                    environment.State,
                    environment.BattleEnd()),
                Is.Empty);
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .Count,
                Is.Zero);
        }

        [Test]
        public void Runtime_BattleEndWithoutSnapshotCreatesNoCandidate()
        {
            var environment = new Environment();

            Assert.That(
                Discover(
                    environment.Sources(),
                    environment.State,
                    environment.LegacyBattleEnd()),
                Is.Empty);
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.Zero);
        }

        [Test]
        public void RebuiltRuntimeSourcesPreserveCompletedCardUsage()
        {
            var environment = new Environment();
            var battleEnd = environment.BattleEnd();
            var first = environment.Sources();
            var candidates = Discover(
                first,
                environment.State,
                battleEnd);

            Assert.That(candidates.Count, Is.EqualTo(1));

            candidates[0].Trigger.Resolve(
                environment.State,
                battleEnd);

            var rebuilt = environment.Sources();

            Assert.That(
                rebuilt.Sources[0],
                Is.Not.SameAs(first.Sources[0]));
            Assert.That(
                Discover(
                    rebuilt,
                    environment.State,
                    battleEnd),
                Is.Empty);
            Assert.That(
                environment.Runtime.FinalRankModifierRegistry
                    .GetTotalModifier(
                        environment.Card(BoardRow.Front)
                            .InstanceId),
                Is.EqualTo(3));
            Assert.That(
                environment.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void FreshRuntimeUsesIndependentBattleScopedState()
        {
            var first = new Environment();
            var second = new Environment();
            var firstEvent = first.BattleEnd();
            var firstCandidate = Discover(
                first.Sources(),
                first.State,
                firstEvent)[0];

            firstCandidate.Trigger.Resolve(
                first.State,
                firstEvent);

            Assert.That(
                first.UpperPet.InstanceId,
                Is.EqualTo(second.UpperPet.InstanceId));
            Assert.That(
                first.Card(BoardRow.Front).InstanceId,
                Is.EqualTo(
                    second.Card(BoardRow.Front).InstanceId));
            Assert.That(
                first.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
            Assert.That(
                second.Runtime.UsageRegistry.Count,
                Is.Zero);
            Assert.That(
                second.Runtime.FinalRankModifierRegistry.Count,
                Is.Zero);

            var secondEvent = second.BattleEnd();
            var secondCandidate = Discover(
                second.Sources(),
                second.State,
                secondEvent)[0];

            secondCandidate.Trigger.Resolve(
                second.State,
                secondEvent);

            Assert.That(
                second.Runtime.UsageRegistry.Count,
                Is.EqualTo(1));
            Assert.That(
                second.Runtime.UsageRegistry,
                Is.Not.SameAs(
                    first.Runtime.UsageRegistry));
        }

        private static List<
            CombatTriggerCandidate<ICombatTriggerHandler>>
            Discover(
                CombatTriggerSourceRegistry sources,
                CombatState state,
                CombatEvent sourceEvent)
        {
            return new List<
                CombatTriggerCandidate<ICombatTriggerHandler>>(
                    sources.DiscoverTriggers(
                        state,
                        sourceEvent));
        }

        private static BoardRow OtherRow(
            BoardRow row)
        {
            return row == BoardRow.Front
                ? BoardRow.Back
                : BoardRow.Front;
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly BoardRow QualifiedRow;
            public readonly CombatPokerHand LockedPokerHand;
            public readonly CombatState State;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatPetTriggerRuntime Runtime =
                new CombatPetTriggerRuntime();
            public readonly CombatEventMetadataFactory Metadata =
                new CombatEventMetadataFactory(
                    new CombatEventIdAllocator(),
                    new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log =
                new CombatEventLog();
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatRescueResolver Rescue;
            public readonly CombatHpGainResolver Hp;

            public Environment(
                CombatSide side = CombatSide.Player,
                BoardRow qualifiedRow = BoardRow.Front,
                CombatPokerHand lockedPokerHand =
                    CombatPokerHand.HighCard)
            {
                Side = side;
                QualifiedRow = qualifiedRow;
                LockedPokerHand = lockedPokerHand;

                UpperPet = new CombatPetState(
                    CombatPetDefinitionIds.Jackalope,
                    new InstanceId(1001));
                LowerPet = new CombatPetState(
                    CombatPetDefinitionIds.Jackalope,
                    new InstanceId(1002));

                var opposingSide =
                    side == CombatSide.Player
                        ? CombatSide.Enemy
                        : CombatSide.Player;
                var own = MakeSide(side);
                var opposing = MakeSide(opposingSide);
                var ownPets = new CombatSidePetState(
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

                Armor = new CombatArmorGainResolver(
                    Metadata,
                    Log);
                Attack = new CombatAttackGainResolver(
                    Metadata,
                    Log);
                Rescue = new CombatRescueResolver(
                    Metadata,
                    Log);
                Hp = new CombatHpGainResolver(
                    Metadata,
                    Log);
            }

            public CombatPetTriggerSourceFactoryRegistry
                FullFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter);
            }

            public CombatPetTriggerSourceFactoryRegistry
                RescueAwareFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue);
            }

            public CombatPetTriggerSourceFactoryRegistry
                CompleteFactoryRegistry()
            {
                return Runtime.FactoryCatalog.CreateRegistry(
                    Armor,
                    Attack,
                    new CombatCardLookup(Log),
                    Runtime.PetUsageCommitter,
                    Rescue,
                    Hp);
            }

            public CombatTriggerSourceRegistry Sources()
            {
                return Runtime.BuildSourceRegistry(
                    State,
                    Armor,
                    Attack,
                    new CombatCardLookup(Log));
            }

            public BattleEndStartedCombatEvent BattleEnd()
            {
                return new BattleEndStartedCombatEvent(
                    CreateBattleEndMetadata(),
                    CreateSnapshot());
            }

            public BattleEndStartedCombatEvent LegacyBattleEnd()
            {
                return new BattleEndStartedCombatEvent(
                    CreateBattleEndMetadata());
            }

            public CombatCardState Card(
                BoardRow row)
            {
                return State.GetSide(Side).GetCardAt(
                    Position(
                        Side,
                        row));
            }

            private CombatBattleStartSnapshot CreateSnapshot()
            {
                var ownFront =
                    QualifiedRow == BoardRow.Front
                        ? LockedPokerHand
                        : CombatPokerHand.FlushFive;
                var ownBack =
                    QualifiedRow == BoardRow.Back
                        ? LockedPokerHand
                        : CombatPokerHand.FlushFive;

                return new CombatBattleStartSnapshotResolver()
                    .Resolve(
                        State,
                        Side == CombatSide.Player
                            ? ownFront
                            : CombatPokerHand.FlushFive,
                        Side == CombatSide.Player
                            ? ownBack
                            : CombatPokerHand.FlushFive,
                        Side == CombatSide.Enemy
                            ? ownFront
                            : CombatPokerHand.FlushFive,
                        Side == CombatSide.Enemy
                            ? ownBack
                            : CombatPokerHand.FlushFive);
            }

            private CombatEventMetadata CreateBattleEndMetadata()
            {
                var root = Metadata.CreateRoot();
                return Metadata.CreateChild(root);
            }

            private static CombatSideState MakeSide(
                CombatSide side)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();

                foreach (var row in
                         new[]
                         {
                             BoardRow.Front,
                             BoardRow.Back
                         })
                {
                    var value =
                        (side == CombatSide.Player
                            ? 0
                            : 100) +
                        (row == BoardRow.Front
                            ? 1
                            : 11);
                    var card = new CombatCardState(
                        new DefinitionId(
                            "test.jackalope.runtime.card"),
                        new InstanceId(value),
                        new CardRank(2),
                        5,
                        5,
                        0,
                        2);

                    cards.Add(card);
                    slots.Add(
                        new CombatSlotState(
                            new SlotId(value),
                            Position(side, row),
                            card.InstanceId));
                }

                return new CombatSideState(
                    new CombatBoardState(
                        side,
                        slots),
                    new CombatCardRegistry(cards),
                    new BattleHealth(
                        BattleHealth.NormalBaselineValue),
                    new AttackMultiplier(
                        AttackMultiplier.BaseValue));
            }

            private static BoardPosition Position(
                CombatSide side,
                BoardRow row)
            {
                return new BoardPosition(
                    side,
                    row,
                    new BoardColumn(1));
            }
        }
    }
}
