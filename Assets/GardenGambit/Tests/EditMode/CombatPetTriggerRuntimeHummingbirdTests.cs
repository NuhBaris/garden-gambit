using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class CombatPetTriggerRuntimeHummingbirdTests
    {
        [Test]
        public void Catalogue_RegistersStableIdentityAndSharedDependencies()
        {
            var e = new Battle();
            Assert.That(CombatPetDefinitionIds.HummingbirdValue, Is.EqualTo("pet.hummingbird"));
            Assert.That(CombatPetDefinitionIds.Hummingbird, Is.EqualTo(new DefinitionId("pet.hummingbird")));
            var ids = new HashSet<DefinitionId>
            {
                CombatPetDefinitionIds.SunBird, CombatPetDefinitionIds.PolarFerret,
                CombatPetDefinitionIds.MuskCat, CombatPetDefinitionIds.RainSparrow,
                CombatPetDefinitionIds.HarvestMouse, CombatPetDefinitionIds.Wombat,
                CombatPetDefinitionIds.Ladybug, CombatPetDefinitionIds.Pika,
                CombatPetDefinitionIds.Jackal, CombatPetDefinitionIds.Lynx,
                CombatPetDefinitionIds.Beaver, CombatPetDefinitionIds.Chinchilla, CombatPetDefinitionIds.Hummingbird
            };
            var registry = e.FullCatalogue();
            Assert.That(ids.Count, Is.EqualTo(13));
            Assert.That(registry.Count, Is.EqualTo(CombatPetCatalogTestExpectations.FullFactoryCount));
            foreach (var id in ids) { Assert.That(registry.Contains(id), Is.True); }
            var factory = registry.GetFactory(CombatPetDefinitionIds.Hummingbird) as HummingbirdPetTriggerSourceFactory;
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.PetDefinitionId, Is.EqualTo(CombatPetDefinitionIds.Hummingbird));
            Assert.That(factory.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(factory.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry().Count, Is.EqualTo(3));
            Assert.That(e.Runtime.FactoryCatalog.CreateRegistry(e.Armor).Count, Is.EqualTo(4));
            Assert.That(e.Runtime.FactoryRegistry.Contains(CombatPetDefinitionIds.Hummingbird), Is.False);
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Factory_CreatesOneSourceWithCorrectOwnerAndSharedDependencies(CombatSide side)
        {
            var e = new Battle(side);
            var factory = (HummingbirdPetTriggerSourceFactory)e.FullCatalogue().GetFactory(CombatPetDefinitionIds.Hummingbird);
            var sources = new List<ICombatTriggerSource>(factory.CreateSources(side, e.UpperPet));
            Assert.That(sources.Count, Is.EqualTo(1));
            var source = sources[0] as HummingbirdPetTriggerSource;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.UpperPet.InstanceId));
            Assert.That(source.Handler, Is.TypeOf<HummingbirdPetHpGainTriggerHandler>());
            Assert.That(source.UsageCommitter, Is.SameAs(e.Runtime.UsageCommitter));
            Assert.That(source.AttackGainResolver, Is.SameAs(e.Attack));
            Assert.That(source.OrderKeyProvider, Is.Not.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Factory_RejectsMissingDependency(bool missingUsage)
        {
            var e = new Battle();
            Assert.Throws<ArgumentNullException>(() => new HummingbirdPetTriggerSourceFactory(
                CombatPetDefinitionIds.Hummingbird, missingUsage ? null : e.Runtime.UsageCommitter,
                missingUsage ? e.Attack : null));
        }

        [Test]
        public void Factory_RejectsInvalidDefinitionId()
        {
            var e = new Battle();
            Assert.Throws<ArgumentException>(() => new HummingbirdPetTriggerSourceFactory(
                default(DefinitionId), e.Runtime.UsageCommitter, e.Attack));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Factory_RejectsInvalidSourceRequest(int invalid)
        {
            var e = new Battle();
            var factory = (HummingbirdPetTriggerSourceFactory)e.FullCatalogue().GetFactory(CombatPetDefinitionIds.Hummingbird);
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
            Assert.Throws<ArgumentNullException>(() => new HummingbirdPetTriggerSource(
                e.Side, e.UpperPet.InstanceId, missingUsage ? null : e.Runtime.UsageCommitter,
                missingUsage ? e.Attack : null));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void SourceDiscovery_UsesCorrectOwnerRowAndOrder(CombatSide side, BoardRow row)
        {
            var e = new Battle(side, row == BoardRow.Front ? Roster.Upper : Roster.Lower);
            var hpGain = e.LoggedGain(statGain: true);
            var sources = e.Sources();
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(
                sources.DiscoverTriggers(e.State, hpGain));
            Assert.That(candidates.Count, Is.EqualTo(1));
            var handler = candidates[0].Trigger as HummingbirdPetHpGainTriggerHandler;
            Assert.That(handler, Is.Not.Null);
            Assert.That(handler.PetInstanceId, Is.EqualTo(e.PrimaryPet.InstanceId));
            Assert.That(candidates[0].OrderKey, Is.EqualTo(new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Pet, side, row == BoardRow.Front ? 0 : 1, 0)));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Primary.Attack, Is.EqualTo(2));
            handler.Resolve(e.State, hpGain);
            Assert.That(sources.DiscoverTriggers(e.State, hpGain), Is.Empty);
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(1));
        }

        [Test]
        public void SourceDiscovery_IgnoresNonHpEvents()
        {
            var e = new Battle();
            Assert.That(e.Sources().DiscoverTriggers(e.State, e.EnsureRoot()), Is.Empty);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Primary.Attack, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TriggerEngine_ExternalHealOrStatGainProducesOneAttackGain(bool statGain)
        {
            var e = new Battle();
            var hpGain = e.LoggedGain(statGain);
            var engine = new CombatTriggerEngine(e.State, e.Queue, e.Sources());
            engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            var gains = e.Find<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(gains[0].Metadata.ParentEventId.Value, Is.EqualTo(hpGain.Metadata.EventId));
            Assert.That(gains[0].Metadata.TriggerRootId, Is.EqualTo(hpGain.Metadata.TriggerRootId));
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 20), Is.Zero);
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
        }

        [Test]
        public void TriggerEngine_TwoQueuedHpGainsConsumeOnlyOneUse()
        {
            var e = new Battle();
            var first = e.LoggedGain(statGain: false);
            e.LoggedGain(statGain: true);
            var engine = new CombatTriggerEngine(e.State, e.Queue, e.Sources());
            engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Find<HpGainCombatEvent>().Count, Is.EqualTo(2));
            var gains = e.Find<AttackGainCombatEvent>();
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(gains[0].Metadata.ParentEventId.Value, Is.EqualTo(first.Metadata.EventId));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void HpResolver_LegacySelfSourceDoesNotTriggerHummingbird()
        {
            var e = new Battle();
            var gain = new CombatHpGainResolver(e.Metadata, e.Log).TryApplyHpStatGain(
                e.State, e.EnsureRoot(), e.PrimaryPosition, 1);
            Assert.That(gain.IsSelfSource, Is.True);
            new CombatTriggerEngine(e.State, e.Queue, e.Sources()).Drain(
                maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            Assert.That(e.Primary.Attack, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Find<AttackGainCombatEvent>(), Is.Empty);
        }

        [TestCase(0)]
        [TestCase(2)]
        public void HpResolver_NoActualHealDoesNotTriggerOrConsumeUsage(int amount)
        {
            var e = new Battle();
            var gain = new CombatHpGainResolver(e.Metadata, e.Log).TryApplyHeal(
                e.State, e.EnsureRoot(), e.PrimaryPosition, amount);
            Assert.That(gain, Is.Null);
            new CombatTriggerEngine(e.State, e.Queue, e.Sources()).Drain(
                maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            Assert.That(e.Primary.Attack, Is.EqualTo(2));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Find<HpGainCombatEvent>(), Is.Empty);
            Assert.That(e.Find<AttackGainCombatEvent>(), Is.Empty);
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void Runtime_AltarHpGainBuffsRecipientBeforeItsNormalAttack(CombatSide side, BoardRow row)
        {
            var e = new Battle(side, row == BoardRow.Front ? Roster.Upper : Roster.Lower);
            AssertCompleted(e, e.Start());
            AssertPrimaryBonus(e);
            var attacks = e.OwnAttacks();
            Assert.That(attacks.Count, Is.EqualTo(1));
            Assert.That(attacks[0].AttackerInstanceId, Is.EqualTo(e.Primary.InstanceId));
            Assert.That(attacks[0].AttackerPosition.Row, Is.EqualTo(BoardRow.Front));
            Assert.That(attacks[0].BaseDamage, Is.EqualTo(3));
            var hp = e.Find<HpGainCombatEvent>()[0];
            Assert.That(hp.TargetPosition.Row, Is.EqualTo(row));
            var attackGain = e.Find<AttackGainCombatEvent>()[0];
            Assert.That(attackGain.TargetPosition.Row, Is.EqualTo(row));
            Assert.That(attackGain.Metadata.SequenceNo.Value, Is.LessThan(attacks[0].Metadata.SequenceNo.Value));
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Autumn)]
        [TestCase(CombatCardSeason.Winter)]
        [TestCase(CombatCardSeason.Seasonless)]
        public void Runtime_AltarBonusDoesNotRequireASeason(CombatCardSeason season)
        {
            var e = new Battle(season: season);
            AssertCompleted(e, e.Start());
            AssertPrimaryBonus(e);
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_TwoHummingbirdsUseIndependentPetCardKeys(CombatSide side)
        {
            var e = new Battle(side, Roster.Both);
            AssertCompleted(e, e.Start());
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Secondary.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.Primary.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, e.Secondary.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.Secondary.InstanceId), Is.False);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Find<HpGainCombatEvent>().Count, Is.EqualTo(2));
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(2));
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(2));
            foreach (var attack in e.OwnAttacks()) { Assert.That(attack.BaseDamage, Is.EqualTo(3)); }
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_HummingbirdAndRainSparrowResolveOwnRowEffects(CombatSide side)
        {
            var e = new Battle(side, Roster.Rain);
            AssertCompleted(e, e.Start());
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Primary.Armor, Is.Zero);
            Assert.That(e.Secondary.Attack, Is.EqualTo(2));
            Assert.That(e.Secondary.Armor, Is.EqualTo(1));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.UpperPet.InstanceId, e.Primary.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.LowerPet.InstanceId, e.Secondary.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<ArmorGainCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Find<HpGainCombatEvent>().Count, Is.EqualTo(2));
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(3));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void Runtime_HpTriggerOverflowThenResumeDoesNotRepeatAltarOrTransfer(CombatSide side)
        {
            var e = new Battle(side);
            e.Primary.ApplyAttackGain(int.MaxValue - e.Primary.Attack);
            Assert.Throws<OverflowException>(() => e.Start());
            Assert.That(e.Runner.HasActiveCombat, Is.True);
            Assert.That(e.Primary.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(e.Primary.CurrentHp, Is.EqualTo(11));
            Assert.That(e.Primary.HpCapacity, Is.EqualTo(11));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Find<AttackGainCombatEvent>(), Is.Empty);
            var hp = e.Find<HpGainCombatEvent>();
            var altars = e.Find<SacrificialAltarActivatedCombatEvent>();
            Assert.That(hp.Count, Is.EqualTo(1));
            Assert.That(altars.Count, Is.EqualTo(1));
            e.Primary.ReduceAttack(int.MaxValue - 2);

            AssertCompleted(e, e.Runner.ResumeActiveCombat(maximumExchangeCountPerColumn: 20,
                maximumPassCountPerExchange: 100, maximumEventCountPerPass: 100, maximumTriggerCountPerEvent: 100));

            AssertPrimaryBonus(e);
            Assert.That(e.Find<HpGainCombatEvent>()[0], Is.SameAs(hp[0]));
            Assert.That(e.Find<SacrificialAltarActivatedCombatEvent>()[0], Is.SameAs(altars[0]));
            Assert.That(e.Find<SacrificialAltarActivatedCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.OwnAttacks().Count, Is.EqualTo(1));
        }

        [Test]
        public void RebuildingSources_PreservesUsageAcrossLaterHpEvents()
        {
            var e = new Battle();
            e.LoggedGain(statGain: true);
            new CombatTriggerEngine(e.State, e.Queue, e.Sources()).Drain(
                maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            var later = e.LoggedGain(statGain: false);
            var rebuilt = e.Sources();
            Assert.That(rebuilt.DiscoverTriggers(e.State, later), Is.Empty);
            new CombatTriggerEngine(e.State, e.Queue, rebuilt).Drain(
                maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Find<AttackGainCombatEvent>().Count, Is.EqualTo(1));
            Assert.That(e.Queue.PendingCount, Is.Zero);
        }

        [Test]
        public void NewRuntime_WithSameIdsStartsWithUnusedHummingbird()
        {
            var first = new Battle();
            AssertCompleted(first, first.Start());
            var second = new Battle();
            Assert.That(second.Primary.InstanceId, Is.EqualTo(first.Primary.InstanceId));
            Assert.That(second.PrimaryPet.InstanceId, Is.EqualTo(first.PrimaryPet.InstanceId));
            Assert.That(second.Runtime.UsageRegistry.Count, Is.Zero);
            AssertCompleted(second, second.Start());
            AssertPrimaryBonus(first);
            AssertPrimaryBonus(second);
        }

        private static void AssertPrimaryBonus(Battle e)
        {
            Assert.That(e.Primary.Attack, Is.EqualTo(3));
            Assert.That(e.Primary.CurrentHp, Is.EqualTo(11));
            Assert.That(e.Primary.HpCapacity, Is.EqualTo(11));
            Assert.That(e.Runtime.UsageCommitter.HasTriggered(e.PrimaryPet.InstanceId, e.Primary.InstanceId), Is.True);
            Assert.That(e.Runtime.UsageRegistry.Count, Is.EqualTo(1));
            var hp = e.Find<HpGainCombatEvent>();
            var gains = e.Find<AttackGainCombatEvent>();
            Assert.That(hp.Count, Is.EqualTo(1));
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(hp[0].SourceInstanceId, Is.EqualTo(e.Donor.InstanceId));
            Assert.That(hp[0].TargetInstanceId, Is.EqualTo(e.Primary.InstanceId));
            Assert.That(hp[0].IsFromAnotherSource, Is.True);
            Assert.That(gains[0].TargetInstanceId, Is.EqualTo(e.Primary.InstanceId));
            Assert.That(gains[0].ActualGainedAmount, Is.EqualTo(1));
            Assert.That(gains[0].Metadata.ParentEventId.Value, Is.EqualTo(hp[0].Metadata.EventId));
            Assert.That(gains[0].Metadata.TriggerRootId, Is.EqualTo(hp[0].Metadata.TriggerRootId));
        }

        private static void AssertCompleted(Battle e, CombatCompletedCombatEvent completed)
        {
            var damage = e.Secondary == null ? 4 : 8;
            Assert.That(completed.Outcome, Is.EqualTo(e.Side == CombatSide.Player ? CombatOutcome.PlayerVictory : CombatOutcome.EnemyVictory));
            Assert.That(completed.PlayerBattleHealth.Value, Is.EqualTo(e.Side == CombatSide.Player ? 100 : 100 - damage));
            Assert.That(completed.EnemyBattleHealth.Value, Is.EqualTo(e.Side == CombatSide.Enemy ? 100 : 100 - damage));
            Assert.That(e.State.GetOpposingSide(e.Side).Cards.Count, Is.Zero);
            Assert.That(e.Runner.HasActiveCombat, Is.False);
            Assert.That(e.Queue.PendingCount, Is.Zero);
            Assert.That(e.Runtime.PetUsageRegistry.Count, Is.Zero);
            Assert.That(e.Find<DeathCombatEvent>().Count, Is.EqualTo(e.Secondary == null ? 2 : 4));
            Assert.That(e.Find<CombatCompletedCombatEvent>().Count, Is.EqualTo(1));
        }

        private enum Roster { Upper, Lower, Both, Rain }

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
            public readonly CombatPetState PrimaryPet;
            public readonly CombatCardState Primary;
            public readonly CombatCardState Donor;
            public readonly CombatCardState Secondary;
            public readonly BoardPosition PrimaryPosition;
            public readonly CombatArmorGainResolver Armor;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatCardLookup Lookup;
            private CombatStartedCombatEvent _root;

            public Battle(CombatSide side = CombatSide.Player, Roster roster = Roster.Upper,
                CombatCardSeason season = CombatCardSeason.Winter)
            {
                Side = side;
                var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                var primaryRow = roster == Roster.Lower ? BoardRow.Back : BoardRow.Front;
                PrimaryPosition = new BoardPosition(side, primaryRow, new BoardColumn(1));
                Primary = Card(1, season, 10, 2);
                Donor = Card(2, CombatCardSeason.Winter, 1, 0);
                var ownCards = new List<CombatCardState> { Primary, Donor };
                var ownSlots = new List<CombatSlotState>();
                var opponentCards = new List<CombatCardState>();
                var opponentSlots = new List<CombatSlotState>();
                CombatCardState secondDonor = null;
                if (roster == Roster.Both || roster == Roster.Rain)
                {
                    Secondary = Card(3, CombatCardSeason.Spring, 10, 2);
                    secondDonor = Card(4, CombatCardSeason.Winter, 1, 0);
                    ownCards.Add(Secondary);
                    ownCards.Add(secondDonor);
                }
                for (var column = 1; column <= 5; column++)
                {
                    CombatCardState opponent = null;
                    if (column == 1 || column == 2 && Secondary != null)
                    {
                        opponent = Card(100 + column, CombatCardSeason.Winter, 3, 0);
                        opponentCards.Add(opponent);
                    }
                    foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                    {
                        var occupant = column == 1 ? (row == primaryRow ? Primary : Donor) :
                            column == 2 && Secondary != null ? (row == BoardRow.Back ? Secondary : secondDonor) : null;
                        var altar = occupant != null && (ReferenceEquals(occupant, Donor) || ReferenceEquals(occupant, secondDonor));
                        ownSlots.Add(new CombatSlotState(new SlotId(column * 2 + (row == BoardRow.Front ? 0 : 1)),
                            new BoardPosition(side, row, new BoardColumn(column)), occupant == null ? (InstanceId?)null : occupant.InstanceId,
                            altar ? CombatSlotEnhanceKind.SacrificialAltar : CombatSlotEnhanceKind.None));
                        opponentSlots.Add(new CombatSlotState(new SlotId(100 + column * 2 + (row == BoardRow.Front ? 0 : 1)),
                            new BoardPosition(otherSide, row, new BoardColumn(column)),
                            row == BoardRow.Front && opponent != null ? (InstanceId?)opponent.InstanceId : null));
                    }
                }
                var own = new CombatSideState(new CombatBoardState(side, ownSlots), new CombatCardRegistry(ownCards),
                    new BattleHealth(100), new AttackMultiplier(1));
                var other = new CombatSideState(new CombatBoardState(otherSide, opponentSlots), new CombatCardRegistry(opponentCards),
                    new BattleHealth(100), new AttackMultiplier(1));
                UpperPet = new CombatPetState(roster == Roster.Lower ? CombatPetDefinitionIds.SunBird : CombatPetDefinitionIds.Hummingbird,
                    new InstanceId(1001));
                if (roster != Roster.Upper)
                {
                    LowerPet = new CombatPetState(roster == Roster.Rain ? CombatPetDefinitionIds.RainSparrow : CombatPetDefinitionIds.Hummingbird,
                        new InstanceId(1002));
                }
                PrimaryPet = roster == Roster.Lower ? LowerPet : UpperPet;
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(
                    LowerPet == null ? new[] { UpperPet } : new[] { UpperPet, LowerPet }));
                var otherPets = new CombatSidePetState(otherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
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

            public CombatStartedCombatEvent EnsureRoot()
            {
                if (_root == null) { _root = new CombatStartedCombatEvent(Metadata.CreateRoot()); Log.Append(_root); }
                return _root;
            }

            public HpGainCombatEvent LoggedGain(bool statGain)
            {
                var root = EnsureRoot();
                if (!statGain) { Primary.ApplyIncomingDamage(2); }
                var previousHp = Primary.CurrentHp;
                var previousCapacity = Primary.HpCapacity;
                if (statGain) { Primary.ApplyHpStatGain(1); }
                else { Primary.Heal(1); }
                // Semantic event fixture for source/engine tests. Full runner tests
                // obtain the external HP_GAIN from the actual Altar pipeline.
                var gain = new HpGainCombatEvent(Metadata.CreateChild(root.Metadata), Donor.InstanceId,
                    Primary.InstanceId, PrimaryPosition, previousCapacity, Primary.HpCapacity, previousHp, Primary.CurrentHp);
                Log.Append(gain);
                return gain;
            }

            public List<T> Find<T>() where T : CombatEvent
            {
                var events = new List<T>();
                foreach (var item in Log.Events) { if (item is T typed) { events.Add(typed); } }
                return events;
            }

            public List<NormalAttackCombatEvent> OwnAttacks()
            {
                var events = new List<NormalAttackCombatEvent>();
                foreach (var attack in Find<NormalAttackCombatEvent>())
                { if (attack.AttackerSide == Side) { events.Add(attack); } }
                return events;
            }

            private static CombatCardState Card(long id, CombatCardSeason season, int hp, int attack)
            {
                return new CombatCardState(new DefinitionId("test.runtime_hummingbird_card"), new InstanceId(id),
                    new CardRank(4), season, hp, hp, 0, attack);
            }
        }
    }
}
