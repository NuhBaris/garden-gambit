using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeAlpacaTests
    {
        [Test]
        public void Catalogue_RegistersStableAlpacaIdentityAndSharedDependencies()
        {
            var e = new Environment();
            Assert.That(CombatPetDefinitionIds.AlpacaValue, Is.EqualTo("pet.alpaca"));
            Assert.That(CombatPetDefinitionIds.Alpaca, Is.EqualTo(new DefinitionId("pet.alpaca")));
            var registry = e.Runtime.FactoryCatalog.CreateRegistry(
                e.Armor, e.Attack, new CombatCardLookup(e.Log), e.Runtime.PetUsageCommitter);
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            var factory = registry.GetFactory(CombatPetDefinitionIds.Alpaca) as AlpacaPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
            Assert.That(factory.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(registry.Contains(CombatPetDefinitionIds.Hawk), Is.True);
            Assert.That(registry.Contains(CombatPetDefinitionIds.Macaque), Is.True);
            Assert.That(e.Runtime.FactoryRegistry.Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Alpaca), Is.False);
            var armorRegistry = e.Runtime.FactoryCatalog.CreateRegistry(e.Armor);
            Assert.That(armorRegistry.Count, Is.EqualTo(4));
            Assert.That(armorRegistry.Contains(CombatPetDefinitionIds.Alpaca), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void RuntimeSources_CreateTwoAlpacasWithSharedUsage(CombatSide side)
        {
            var e = new Environment(side);
            var sources = e.Sources();
            Assert.That(sources.Count, Is.EqualTo(2));
            var owners = new HashSet<InstanceId>();
            foreach (var item in sources.Sources)
            {
                var source = item as AlpacaPetTriggerSource;
                Assert.That(source, Is.Not.Null);
                Assert.That(source.Side, Is.EqualTo(side));
                Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.PetUsageCommitter));
                Assert.That(source.AttackGainResolver, Is.SameAs(e.Attack));
                Assert.That(owners.Add(source.PetInstanceId), Is.True);
            }
            Assert.That(owners.Contains(e.UpperPet.InstanceId), Is.True);
            Assert.That(owners.Contains(e.LowerPet.InstanceId), Is.True);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BonusesPrecedeNormalAttacksAndChangeTheirDamage(CombatSide side)
        {
            var e = new Environment(side);
            var runner = e.CreateRunner();
            Assert.That(runner.UsesStagedNormalAttackByDefault, Is.True);
            var completed = Start(runner);
            AssertCompleted(e, runner, completed);
            AssertAlpacaRow(e, BoardRow.Front);
            AssertAlpacaRow(e, BoardRow.Back);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.UpperPet.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.LowerPet.InstanceId), Is.True);
            var gains = e.Events<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(4));
            var petStage = e.PetStage();
            Assert.That(petStage.HasBattleStartSnapshot, Is.True);
            for (var i = 0; i < gains.Count; i++)
            {
                var row = i < 2 ? BoardRow.Front : BoardRow.Back;
                var column = i % 2 == 0 ? 2 : 4;
                Assert.That(gains[i].TargetInstanceId, Is.EqualTo(e.Card(row, column).InstanceId));
                Assert.That(gains[i].TargetPosition.Row, Is.EqualTo(row));
                Assert.That(gains[i].ActualGainedAmount, Is.EqualTo(1));
                Assert.That(gains[i].Metadata.ParentEventId.Value, Is.EqualTo(petStage.Metadata.EventId));
                Assert.That(gains[i].Metadata.TriggerRootId, Is.EqualTo(petStage.Metadata.TriggerRootId));
            }
            var ownAttacks = e.OwnAttacks();
            Assert.That(ownAttacks.Count, Is.EqualTo(2));
            foreach (var attack in ownAttacks)
            {
                var column = attack.AttackerPosition.Column.Value;
                Assert.That(column == 2 || column == 4, Is.True);
                Assert.That(attack.AttackerPosition.Row, Is.EqualTo(BoardRow.Front));
                Assert.That(attack.BaseDamage, Is.EqualTo(column == 2 ? 2 : 3));
                Assert.That(gains[3].Metadata.SequenceNo.Value, Is.LessThan(attack.Metadata.SequenceNo.Value));
            }
        }

        [TestCase(CombatSide.Player, false)]
        [TestCase(CombatSide.Player, true)]
        [TestCase(CombatSide.Enemy, false)]
        [TestCase(CombatSide.Enemy, true)]
        public void Runtime_InvalidUpperSnapshotDoesNotPreventLowerBonus(CombatSide side, bool duplicate)
        {
            var e = new Environment(side, invalidUpper: true, duplicateUpperRank: duplicate);
            var runner = e.CreateRunner();
            AssertCompleted(e, runner, Start(runner));
            var upperCount = duplicate ? 5 : 4;
            for (var column = 1; column <= upperCount; column++)
            {
                Assert.That(e.Card(BoardRow.Front, column).Attack, Is.EqualTo(Environment.InitialAttack(column)));
            }
            AssertAlpacaRow(e, BoardRow.Back);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.UpperPet.InstanceId), Is.False);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.LowerPet.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Events<AttackGainCombatEvent>().Count, Is.EqualTo(2));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_ResumeKeepsUpperBonusAndOriginalRankSnapshot(CombatSide side)
        {
            var e = new Environment(side);
            // The two lowest lower-row targets are MaxValue-1 and MaxValue.
            // The second target fails; neither lower-row bonus may be applied.
            for (var column = 1; column <= 5; column++)
            {
                var card = e.Card(BoardRow.Back, column);
                var targetAttack = column == 2 ? int.MaxValue - 1 : int.MaxValue;
                card.ApplyAttackGain(targetAttack - card.Attack);
            }
            var runner = e.CreateRunner();
            Assert.Throws<OverflowException>(() => Start(runner));
            Assert.That(runner.HasActiveCombat, Is.True);
            AssertAlpacaRow(e, BoardRow.Front);
            for (var column = 1; column <= 5; column++)
            {
                Assert.That(e.Card(BoardRow.Back, column).Attack,
                    Is.EqualTo(column == 2 ? int.MaxValue - 1 : int.MaxValue));
            }
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.UpperPet.InstanceId), Is.True);
            Assert.That(e.Runtime.PetUsageCommitter.HasTriggered(e.LowerPet.InstanceId), Is.False);
            var firstGains = e.Events<AttackGainCombatEvent>();
            Assert.That(firstGains.Count, Is.EqualTo(2));
            var petStage = e.PetStage();
            for (var column = 1; column <= 5; column++)
            {
                var card = e.Card(BoardRow.Back, column);
                card.ReduceAttack(card.Attack - Environment.InitialAttack(column));
            }
            // Current ranks become duplicated while paused. The captured
            // five-unique-Rank condition must still govern the pending trigger.
            e.Card(BoardRow.Back, 5).SetRank(new CardRank(2));
            var completed = runner.ResumeActiveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);
            AssertCompleted(e, runner, completed);
            AssertAlpacaRow(e, BoardRow.Front);
            AssertAlpacaRow(e, BoardRow.Back);
            Assert.That(e.Card(BoardRow.Back, 5).Rank.Value, Is.EqualTo(2));
            var gains = e.Events<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(4));
            Assert.That(gains[0], Is.SameAs(firstGains[0]));
            Assert.That(gains[1], Is.SameAs(firstGains[1]));
            Assert.That(e.PetStage(), Is.SameAs(petStage));
            Assert.That(e.Events<CombatStartedCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Events<CombatCompletedCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_HawkAndAlpacaApplyTheirOwnRowSelections(CombatSide side)
        {
            var e = new Environment(side, hawkUpper: true);
            var runner = e.CreateRunner();
            AssertCompleted(e, runner, Start(runner));
            for (var column = 1; column <= 5; column++)
            {
                Assert.That(e.Card(BoardRow.Front, column).Attack,
                    Is.EqualTo(Environment.InitialAttack(column) + (column == 1 || column == 3 ? 3 : 0)));
            }
            AssertAlpacaRow(e, BoardRow.Back);
            var gains = e.Events<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(4));
            Assert.That(gains[0].TargetInstanceId, Is.EqualTo(e.Card(BoardRow.Front, 1).InstanceId));
            Assert.That(gains[1].TargetInstanceId, Is.EqualTo(e.Card(BoardRow.Front, 3).InstanceId));
            Assert.That(gains[2].TargetInstanceId, Is.EqualTo(e.Card(BoardRow.Back, 2).InstanceId));
            Assert.That(gains[3].TargetInstanceId, Is.EqualTo(e.Card(BoardRow.Back, 4).InstanceId));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
        }

        [Test]
        public void RebuiltSources_PreserveCompletedUsage()
        {
            var e = new Environment();
            Start(e.CreateRunner());
            Assert.That(e.Sources().DiscoverTriggers(e.State, e.PetStage()), Is.Empty);
            Assert.That(e.Events<AttackGainCombatEvent>().Count, Is.EqualTo(4));
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void FreshRuntime_AllowsSameIdsInNewBattle()
        {
            var first = new Environment();
            Start(first.CreateRunner());
            var second = new Environment();
            Assert.That(second.UpperPet.InstanceId, Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(second.Runtime.PetUsageRegistry.Count, Is.Zero);
            Start(second.CreateRunner());
            AssertAlpacaRow(first, BoardRow.Front);
            AssertAlpacaRow(second, BoardRow.Front);
            Assert.That(second.Events<AttackGainCombatEvent>().Count, Is.EqualTo(4));
            Assert.That(second.Runtime.PetUsageRegistry.Count, Is.EqualTo(2));
        }

        private static void AssertAlpacaRow(Environment e, BoardRow row)
        {
            for (var column = 1; column <= 5; column++)
            {
                var card = e.Card(row, column);
                Assert.That(card.Attack, Is.EqualTo(Environment.InitialAttack(column) + (column == 2 || column == 4 ? 1 : 0)));
                Assert.That(card.CurrentHp, Is.EqualTo(5));
                Assert.That(card.HpCapacity, Is.EqualTo(10));
                Assert.That(card.Armor, Is.Zero);
            }
        }

        private static void AssertCompleted(Environment e, CombatResolutionRunner runner, CombatCompletedCombatEvent completed)
        {
            Assert.That(completed, Is.Not.Null);
            Assert.That(completed.Outcome, Is.EqualTo(e.Side == CombatSide.Player ? CombatOutcome.PlayerVictory : CombatOutcome.EnemyVictory));
            Assert.That(e.State.GetOpposingSide(e.Side).Cards.Count, Is.Zero);
            Assert.That(runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        private static CombatCompletedCombatEvent Start(CombatResolutionRunner runner) =>
            runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatState State;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventQueue Queue;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatArmorGainResolver Armor;
            private readonly CombatCardState[,] _ownCards = new CombatCardState[2, 5];

            public Environment(CombatSide side = CombatSide.Player, bool invalidUpper = false,
                bool duplicateUpperRank = false, bool hawkUpper = false)
            {
                Side = side;
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                UpperPet = new CombatPetState(hawkUpper ? CombatPetDefinitionIds.Hawk : CombatPetDefinitionIds.Alpaca, new InstanceId(1002));
                LowerPet = new CombatPetState(CombatPetDefinitionIds.Alpaca, new InstanceId(1001));
                var own = MakeSide(side, true, invalidUpper, duplicateUpperRank);
                var opposing = MakeSide(otherSide, false, false, false);
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(side == CombatSide.Player ? own : opposing, side == CombatSide.Enemy ? own : opposing,
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
            }

            public static int InitialAttack(int column) => new[] { 5, 1, 4, 2, 3 }[column - 1];
            public CombatCardState Card(BoardRow row, int column) => _ownCards[row == BoardRow.Front ? 0 : 1, column - 1];
            public CombatTriggerSourceRegistry Sources() => Runtime.BuildSourceRegistry(State, Armor, Attack, new CombatCardLookup(Log));
            public CombatResolutionRunner CreateRunner() => Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);

            public List<T> Events<T>() where T : CombatEvent
            {
                var result = new List<T>();
                foreach (var item in Log.Events) { if (item is T typed) { result.Add(typed); } }
                return result;
            }

            public List<NormalAttackCombatEvent> OwnAttacks()
            {
                var result = new List<NormalAttackCombatEvent>();
                foreach (var attack in Events<NormalAttackCombatEvent>())
                { if (attack.AttackerSide == Side) { result.Add(attack); } }
                return result;
            }

            public BattleStartStageStartedCombatEvent PetStage()
            {
                BattleStartStageStartedCombatEvent result = null;
                foreach (var stage in Events<BattleStartStageStartedCombatEvent>())
                {
                    if (!stage.IsPetStage) { continue; }
                    Assert.That(result, Is.Null, "Pet stage must occur once.");
                    result = stage;
                }
                Assert.That(result, Is.Not.Null);
                return result;
            }

            private CombatSideState MakeSide(CombatSide side, bool own, bool invalidUpper, bool duplicateUpperRank)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var prefix = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10);
                        CombatCardState card = null;
                        var omitOwn = own && invalidUpper && !duplicateUpperRank && row == BoardRow.Front && column == 5;
                        if (own && !omitOwn)
                        {
                            var rank = invalidUpper && duplicateUpperRank && row == BoardRow.Front && column == 5 ? 2 : column + 1;
                            card = new CombatCardState(new DefinitionId("test.runtime_alpaca_card"), new InstanceId(prefix + 6 - column),
                                new CardRank(rank), CombatCardSeason.Spring, 10, 5, 0, InitialAttack(column));
                            _ownCards[row == BoardRow.Front ? 0 : 1, column - 1] = card;
                        }
                        else if (!own && row == BoardRow.Front && (column == 2 || column == 4))
                        {
                            var hp = column == 2 ? 2 : 3;
                            card = new CombatCardState(new DefinitionId("test.runtime_alpaca_opponent"), new InstanceId(prefix + 6 - column),
                                new CardRank(2), CombatCardSeason.Winter, hp, hp, 0, 0);
                        }
                        if (card != null) { cards.Add(card); }
                        slots.Add(new CombatSlotState(new SlotId(prefix + column), new BoardPosition(side, row, new BoardColumn(column)),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(100), new AttackMultiplier(1));
            }
        }
    }
}
