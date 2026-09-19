using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeChinchillaTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var e = new Battle();
            Assert.That(CombatPetDefinitionIds.ChinchillaValue, Is.EqualTo("pet.chinchilla"));
            Assert.That(CombatPetDefinitionIds.Chinchilla, Is.EqualTo(new DefinitionId("pet.chinchilla")));
            var ids = new HashSet<DefinitionId>
            {
                CombatPetDefinitionIds.SunBird, CombatPetDefinitionIds.PolarFerret,
                CombatPetDefinitionIds.MuskCat, CombatPetDefinitionIds.RainSparrow,
                CombatPetDefinitionIds.HarvestMouse, CombatPetDefinitionIds.Wombat,
                CombatPetDefinitionIds.Ladybug, CombatPetDefinitionIds.Pika,
                CombatPetDefinitionIds.Jackal, CombatPetDefinitionIds.Lynx,
                CombatPetDefinitionIds.Beaver, CombatPetDefinitionIds.Chinchilla
            };
            var registry = e.FullCatalogue();
            Assert.That(ids.Count, Is.EqualTo(12));
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            foreach (var id in ids) { Assert.That(registry.Contains(id), Is.True); }
            var factory = registry.GetFactory(CombatPetDefinitionIds.Chinchilla) as ChinchillaPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.PetDefinitionId, Is.EqualTo(CombatPetDefinitionIds.Chinchilla));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(factory.SourceDamageModifierRegistry, Is.SameAs(e.Runtime.SourceDamageModifierRegistry));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry().Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry(e.Armor).Count, Is.EqualTo(4));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Chinchilla), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneSourceWithCorrectOwnerAndSharedDependencies(CombatSide side)
        {
            var e = new Battle(side);
            var factory = (ChinchillaPetTriggerSourceFactory)e.FullCatalogue().GetFactory(CombatPetDefinitionIds.Chinchilla);
            var sources = new List<ICombatTriggerSource>(factory.CreateSources(side, e.UpperPet));
            Assert.That(sources.Count, Is.EqualTo(1));
            var source = sources[0] as ChinchillaPetTriggerSource;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.UpperPet.InstanceId));
            Assert.That(source.Handler, Is.TypeOf<ChinchillaPetNormalAttackTriggerHandler>());
            Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(source.SourceDamageModifierRegistry, Is.SameAs(e.Runtime.SourceDamageModifierRegistry));
            Assert.That(source.OrderKeyProvider, Is.Not.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Factory_RejectsMissingDependency(bool missingUsage)
        {
            var e = new Battle();
            Assert.Throws<ArgumentNullException>(() => new ChinchillaPetTriggerSourceFactory(
                CombatPetDefinitionIds.Chinchilla, missingUsage ? null : e.Runtime.UsageCommitter,
                missingUsage ? e.Runtime.SourceDamageModifierRegistry : null));
        }

        [Test]
        public void Factory_RejectsInvalidDefinitionId()
        {
            var e = new Battle();
            Assert.Throws<ArgumentException>(() => new ChinchillaPetTriggerSourceFactory(
                default(DefinitionId), e.Runtime.UsageCommitter, e.Runtime.SourceDamageModifierRegistry));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Factory_RejectsInvalidSourceRequest(int invalid)
        {
            var e = new Battle();
            var factory = (ChinchillaPetTriggerSourceFactory)e.FullCatalogue().GetFactory(CombatPetDefinitionIds.Chinchilla);
            if (invalid == 0)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new List<ICombatTriggerSource>(
                    factory.CreateSources((CombatSide)99, e.UpperPet)));
            }
            else if (invalid == 1)
            {
                Assert.Throws<ArgumentNullException>(() => new List<ICombatTriggerSource>(
                    factory.CreateSources(e.Side, null)));
            }
            else
            {
                var pet = new CombatPetState(CombatPetDefinitionIds.SunBird, new InstanceId(1003));
                Assert.Throws<ArgumentException>(() => new List<ICombatTriggerSource>(factory.CreateSources(e.Side, pet)));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Source_RejectsMissingDependency(bool missingUsage)
        {
            var e = new Battle();
            Assert.Throws<ArgumentNullException>(() => new ChinchillaPetTriggerSource(
                e.Side, e.UpperPet.InstanceId, missingUsage ? null : e.Runtime.UsageCommitter,
                missingUsage ? e.Runtime.SourceDamageModifierRegistry : null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void SourceDiscovery_UsesAffectedRowAndPetOrder(CombatSide side, BoardRow row)
        {
            var e = new Battle(side, roster: Roster.Both, twoRows: true);
            var attack = e.SnapshotAttack(row);
            var sources = e.Sources();
            Assert.That(sources.Count, Is.EqualTo(2));
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                sources.DiscoverTriggers(e.State, attack));
            Assert.That(candidates.Count, Is.EqualTo(1));
            var handler = candidates[0].Trigger as ChinchillaPetNormalAttackTriggerHandler;
            Assert.That(handler, Is.Not.Null);
            Assert.That(handler.PetInstanceId, Is.EqualTo(row == BoardRow.Front ? e.UpperPet.InstanceId : e.LowerPet.InstanceId));
            Assert.That(candidates[0].OrderKey, Is.EqualTo(new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Pet, side, row == BoardRow.Front ? 0 : 1, 0)));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.Zero);
            handler.Resolve(e.State, attack);
            Assert.That(sources.DiscoverTriggers(e.State, attack), Is.Empty);
            Assert.That(e.Runtime.SourceDamageModifierRegistry.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_FirstSeasonlessAttackKillsThreeHpTargetInOneExchange(CombatSide side)
        {
            var e = new Battle(side);
            AssertCompleted(e, e.Start());
            var attacks = e.OwnAttacks();
            Assert.That(attacks.Count, Is.EqualTo(1));
            Assert.That(e.Runner.ResolvedExchangeCount, Is.EqualTo(1));
            Assert.That(attacks[0].IsSeasonlessAttack, Is.True);
            Assert.That(attacks[0].BaseDamage, Is.EqualTo(2));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.ResolveDamage(attacks[0]), Is.EqualTo(3));
            var damage = e.OwnDamage();
            Assert.That(damage.Count, Is.EqualTo(1));
            Assert.That(damage[0].Metadata.ParentEventId, Is.EqualTo(attacks[0].Metadata.EventId));
            Assert.That(damage[0].Result.IncomingDamage, Is.EqualTo(3));
            Assert.That(damage[0].Result.HpDamage, Is.EqualTo(3));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.FrontCard.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.FrontCard.Attack, Is.EqualTo(2));
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Autumn)]
        [TestCase(CombatCardSeason.Winter)]
        public void Runtime_NonSeasonlessCardReceivesNoBonus(CombatCardSeason season)
        {
            var e = new Battle(season: season);
            AssertCompleted(e, e.Start());
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(2));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            foreach (var damage in e.OwnDamage()) { Assert.That(damage.Result.IncomingDamage, Is.EqualTo(2)); }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_RepeatedAttacksReceiveBonusOnlyOnce(CombatSide side)
        {
            var e = new Battle(side, ownAttack: 1, opponentHp: 5);
            AssertCompleted(e, e.Start());
            var attacks = e.OwnAttacks();
            var damage = e.OwnDamage();
            Assert.That(attacks.Count, Is.EqualTo(4));
            Assert.That(damage.Count, Is.EqualTo(4));
            for (var i = 0; i < attacks.Count; i++)
            {
                Assert.That(e.Runtime.SourceDamageModifierRegistry.GetTotalModifier(attacks[i].Metadata.EventId), Is.EqualTo(i == 0 ? 1 : 0));
                Assert.That(damage[i].Result.IncomingDamage, Is.EqualTo(i == 0 ? 2 : 1));
            }
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.FrontCard.Attack, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_UpperPetBuffsOnlyFrontRow(CombatSide side)
        {
            var e = new Battle(side, twoRows: true);
            AssertCompleted(e, e.Start());
            Assert.That(e.OwnAttacks(e.FrontCard.InstanceId).Count, Is.EqualTo(1));
            Assert.That(e.OwnAttacks(e.BackCard.InstanceId).Count, Is.Zero);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.FrontCard.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.BackCard.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_LowerPetDoesNotBuffFrontAttack(CombatSide side)
        {
            var e = new Battle(side, roster: Roster.Lower, twoRows: true);
            AssertCompleted(e, e.Start());
            Assert.That(e.OwnAttacks(e.FrontCard.InstanceId).Count, Is.EqualTo(2));
            Assert.That(e.OwnAttacks(e.BackCard.InstanceId).Count, Is.Zero);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, e.BackCard.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, e.FrontCard.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.Zero);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_TwoChinchillasDoNotDuplicateUpperBonus(CombatSide side)
        {
            var e = new Battle(side, roster: Roster.Both, twoRows: true);
            AssertCompleted(e, e.Start());
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.FrontCard.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, e.BackCard.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.EqualTo(1));
            foreach (var damage in e.OwnDamage()) { Assert.That(damage.Result.IncomingDamage, Is.EqualTo(3)); }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_SunBirdAndLowerChinchillaKeepSeparateUsage(CombatSide side)
        {
            var e = new Battle(side, roster: Roster.Lower, twoRows: true, season: CombatCardSeason.Summer);
            AssertCompleted(e, e.Start());
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.FrontCard.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, e.BackCard.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.EqualTo(1));
            foreach (var damage in e.OwnDamage()) { Assert.That(damage.Result.IncomingDamage, Is.EqualTo(3)); }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_BonusDamagePassesThroughArmor(CombatSide side)
        {
            var e = new Battle(side, opponentHp: 1, opponentArmor: 2);
            AssertCompleted(e, e.Start());
            var damage = e.OwnDamage();
            Assert.That(damage.Count, Is.EqualTo(1));
            Assert.That(damage[0].Result.IncomingDamage, Is.EqualTo(3));
            Assert.That(damage[0].Result.ArmorAbsorbed, Is.EqualTo(2));
            Assert.That(damage[0].Result.HpDamage, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_PolarFerretReducesBonusAttackBeforeArmor(CombatSide side)
        {
            var e = new Battle(side, opponentHp: 1, opponentArmor: 1, opposingFerret: true);
            AssertCompleted(e, e.Start());
            var attacks = e.OwnAttacks();
            var damage = e.OwnDamage();
            Assert.That(attacks.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.ResolveDamage(attacks[0]), Is.EqualTo(3));
            Assert.That(damage.Count, Is.EqualTo(1));
            Assert.That(damage[0].Result.IncomingDamage, Is.EqualTo(2));
            Assert.That(damage[0].Result.ArmorAbsorbed, Is.EqualTo(1));
            Assert.That(damage[0].Result.HpDamage, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.FrontCard.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.OpposingPet.InstanceId, attacks[0].TargetInstanceId), Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(2));
        }

        [Test]
        public void Runtime_EventBudgetThenResume_DoesNotRepeatAttackOrBonus()
        {
            var e = new Battle(ownAttack: 1, opponentHp: 2);
            Assert.Throws<InvalidOperationException>(() => e.Runner.StartAndResolveCombat(
                maximumExchangeCountPerColumn: 20, maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 2, maximumTriggerCountPerEvent: 100));
            Assert.That(e.Runner.HasActiveCombat, Is.True);
            Assert.That(e.Runner.ActiveCombatUsesStagedNormalAttack, Is.True);
            var before = e.OwnAttacks();
            Assert.That(before.Count, Is.EqualTo(1));
            Assert.That(e.Find<NormalAttackExchangeCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<DamageAppliedCombatEvent>().Count, Is.Zero);
            Assert.That(e.Runtime.SourceDamageModifierRegistry.GetTotalModifier(before[0].Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));

            AssertCompleted(e, e.Runner.ResumeActiveCombat(
                maximumExchangeCountPerColumn: 20, maximumPassCountPerExchange: 100,
                maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100));

            Assert.That(e.OwnAttacks().Count, Is.EqualTo(1));
            Assert.That(e.OwnAttacks()[0], Is.SameAs(before[0]));
            Assert.That(e.Find<NormalAttackExchangeCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<NormalAttackCombatEvent>().Count, Is.EqualTo(2));
            Assert.That(e.Find<DamageAppliedCombatEvent>().Count, Is.EqualTo(2));
            Assert.That(e.OwnDamage()[0].Result.IncomingDamage, Is.EqualTo(2));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void RebuildingSources_PreservesSharedUsageAndEventModifiers()
        {
            var e = new Battle();
            var first = e.Sources();
            var attack = e.SnapshotAttack(BoardRow.Front);
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                first.DiscoverTriggers(e.State, attack));
            Assert.That(candidates.Count, Is.EqualTo(1));
            candidates[0].Trigger.Resolve(e.State, attack);
            var rebuilt = e.Sources();
            Assert.That(rebuilt.Count, Is.EqualTo(1));
            Assert.That(rebuilt.DiscoverTriggers(e.State, attack), Is.Empty);
            var laterAttack = e.SnapshotAttack(BoardRow.Front, eventId: 3);
            Assert.That(rebuilt.DiscoverTriggers(e.State, laterAttack), Is.Empty);
            Assert.That(e.Runtime.SourceDamageModifierRegistry.GetTotalModifier(attack.Metadata.EventId), Is.EqualTo(1));
            Assert.That(e.Runtime.SourceDamageModifierRegistry.HasModifier(laterAttack.Metadata.EventId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void NewRuntime_WithSameInstanceIdsStartsWithIndependentUsage()
        {
            var first = new Battle();
            AssertCompleted(first, first.Start());
            var second = new Battle();
            Assert.That(second.FrontCard.InstanceId, Is.EqualTo(first.FrontCard.InstanceId));
            Assert.That(second.UpperPet.InstanceId, Is.EqualTo(first.UpperPet.InstanceId));
            Assert.That(second.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(second.Runtime.SourceDamageModifierRegistry.Count, Is.Zero);
            AssertCompleted(second, second.Start());
            Assert.That(second.OwnAttacks().Count, Is.EqualTo(1));
            Assert.That(second.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(first.Runtime.UsageRegistry.Count, Is.EqualTo(1));
        }

        private static void AssertCompleted(Battle e, CombatCompletedCombatEvent completed)
        {
            var resultDamage = e.BackCard == null ? 4 : 8;
            Assert.That(completed.Outcome, Is.EqualTo(e.Side == CombatSide.Player ? CombatOutcome.PlayerVictory : CombatOutcome.EnemyVictory));
            Assert.That(completed.PlayerBattleHealth.Value, Is.EqualTo(e.Side == CombatSide.Player ? 100 : 100 - resultDamage));
            Assert.That(completed.EnemyBattleHealth.Value, Is.EqualTo(e.Side == CombatSide.Enemy ? 100 : 100 - resultDamage));
            Assert.That(e.State.GetOpposingSide(e.Side).Cards.Count, Is.Zero);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(e.Find<CombatCompletedCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<DeathCombatEvent>().Count, Is.EqualTo(1));
        }

        private enum Roster { Upper, Lower, Both }

        private sealed class Battle
        {
            public readonly CombatSide Side;
            public readonly CombatPetTriggerRuntime Runtime = new CombatPetTriggerRuntime();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventQueue Queue;
            public readonly CombatState State;
            public readonly CombatResolutionRunner Runner;
            public readonly CombatPetState UpperPet;
            public readonly CombatPetState LowerPet;
            public readonly CombatPetState OpposingPet;
            public readonly CombatCardState FrontCard;
            public readonly CombatCardState BackCard;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatCardLookup Lookup;

            public Battle(CombatSide side = CombatSide.Player, Roster roster = Roster.Upper,
                bool twoRows = false, CombatCardSeason season = CombatCardSeason.Seasonless,
                int ownAttack = 2, int opponentHp = 3, int opponentArmor = 0, bool opposingFerret = false)
            {
                Side = side;
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                UpperPet = new CombatPetState(roster == Roster.Lower ? CombatPetDefinitionIds.SunBird : CombatPetDefinitionIds.Chinchilla,
                    new InstanceId(1001));
                if (roster != Roster.Upper)
                {
                    LowerPet = new CombatPetState(CombatPetDefinitionIds.Chinchilla, new InstanceId(1002));
                }
                if (opposingFerret)
                {
                    OpposingPet = new CombatPetState(CombatPetDefinitionIds.PolarFerret, new InstanceId(2001));
                }
                var own = CreateSide(side, false, twoRows, season, 10, 0, ownAttack);
                var other = CreateSide(otherSide, true, twoRows, CombatCardSeason.Winter, opponentHp, opponentArmor, 0);
                FrontCard = own.GetCardAt(new BoardPosition(side, BoardRow.Front, new BoardColumn(1)));
                if (twoRows) { BackCard = own.GetCardAt(new BoardPosition(side, BoardRow.Back, new BoardColumn(1))); }
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(
                    LowerPet == null ? new[] { UpperPet } : new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(
                    OpposingPet == null ? Array.Empty<CombatPetState>() : new[] { OpposingPet }));
                State = new CombatState(side == CombatSide.Player ? own : other, side == CombatSide.Enemy ? own : other,
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Queue = new CombatEventQueue(Log);
                Armor = new CombatArmorGainResolver(Metadata, Log);
                Attack = new CombatAttackGainResolver(Metadata, Log);
                Lookup = new CombatCardLookup(Log);
                Runner = Runtime.CreateResolutionRunner(State, Metadata, Log, Queue);
            }

            public CombatPetTriggerSourceFactoryRegistry FullCatalogue() => Runtime.FactoryCatalog.CreateRegistry(
                Armor, Attack, Lookup, Runtime.PetUsageCommitter);
            public CombatTriggerSourceRegistry Sources() => Runtime.BuildSourceRegistry(State, Armor, Attack, Lookup);
            public CombatCompletedCombatEvent Start() => Runner.StartAndResolveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100);

            public NormalAttackCombatEvent SnapshotAttack(BoardRow row, long eventId = 2)
            {
                var column = new BoardColumn(1);
                var attacker = row == BoardRow.Front ? FrontCard : BackCard;
                var otherSide = Side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var targetPosition = new BoardPosition(otherSide, BoardRow.Front, column);
                var target = State.GetSide(otherSide).GetCardAt(targetPosition);
                var rootId = new CombatEventId(1);
                return new NormalAttackCombatEvent(new CombatEventMetadata(
                    new CombatEventId(eventId), new CombatSequenceNumber(eventId), rootId, rootId),
                    attacker.InstanceId, new BoardPosition(Side, row, column), attacker.Season,
                    target.InstanceId, targetPosition, target.Season, attacker.Attack);
            }

            public List<T> Find<T>() where T : CombatEvent
            {
                var events = new List<T>();
                foreach (var item in Log.Events) { if (item is T typed) { events.Add(typed); } }
                return events;
            }

            public List<NormalAttackCombatEvent> OwnAttacks(InstanceId? cardId = null)
            {
                var events = new List<NormalAttackCombatEvent>();
                foreach (var attack in Find<NormalAttackCombatEvent>())
                {
                    if (attack.AttackerSide == Side && (!cardId.HasValue || attack.AttackerInstanceId == cardId.Value))
                    { events.Add(attack); }
                }
                return events;
            }

            public List<DamageAppliedCombatEvent> OwnDamage()
            {
                var events = new List<DamageAppliedCombatEvent>();
                foreach (var damage in Find<DamageAppliedCombatEvent>())
                {
                    if (damage.SourcePosition.Side == Side) { events.Add(damage); }
                }
                return events;
            }

            private static CombatSideState CreateSide(CombatSide side, bool opponent, bool twoRows,
                CombatCardSeason frontSeason, int hp, int armor, int attack)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();
                for (var column = 1; column <= 5; column++)
                {
                    foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                    {
                        var id = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 5) + column;
                        CombatCardState card = null;
                        var occupied = column == 1 &&
                            (row == BoardRow.Front || twoRows && !opponent && row == BoardRow.Back);
                        if (occupied)
                        {
                            card = new CombatCardState(new DefinitionId("test.runtime_chinchilla_card"), new InstanceId(id),
                                new CardRank(opponent ? 2 : 4), row == BoardRow.Front ? frontSeason : CombatCardSeason.Seasonless,
                                hp, hp, armor, attack);
                            cards.Add(card);
                        }
                        slots.Add(new CombatSlotState(new SlotId(id), new BoardPosition(side, row, new BoardColumn(column)),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(100), new AttackMultiplier(1));
            }
        }
    }
}
