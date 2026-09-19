using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class WombatPetTriggerTests
    {
        [Test]
        public void Constructors_RequireSharedDependencies()
        {
            var e = new Environment();
            var pet = e.State.PlayerPets.GetPetAt(0);
            Assert.Throws<ArgumentNullException>(() => new WombatPetDeathTriggerHandler(CombatSide.Player, pet.InstanceId, null, e.Resolver));
            Assert.Throws<ArgumentNullException>(() => new WombatPetDeathTriggerHandler(CombatSide.Player, pet.InstanceId, e.Usage, null));
            Assert.Throws<ArgumentNullException>(() => new WombatPetTriggerSource(CombatSide.Player, pet.InstanceId, null, e.Resolver));
            Assert.Throws<ArgumentNullException>(() => new WombatPetTriggerSource(CombatSide.Player, pet.InstanceId, e.Usage, null));
            Assert.Throws<ArgumentNullException>(() => new WombatPetTriggerSourceFactory(e.DefinitionId, null, e.Resolver));
            Assert.Throws<ArgumentNullException>(() => new WombatPetTriggerSourceFactory(e.DefinitionId, e.Usage, null));
        }

        [Test]
        public void IdentityAndFactoryGuards_RejectInvalidRequests()
        {
            var e = new Environment();
            var pet = e.State.PlayerPets.GetPetAt(0);
            Assert.Throws<ArgumentException>(() => new WombatPetTriggerSourceFactory(default(DefinitionId), e.Usage, e.Resolver));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WombatPetTriggerSource(default(CombatSide), pet.InstanceId, e.Usage, e.Resolver));
            Assert.Throws<ArgumentException>(() => new WombatPetTriggerSource(CombatSide.Player, default(InstanceId), e.Usage, e.Resolver));
            Assert.Throws<ArgumentOutOfRangeException>(() => e.Factory.CreateSources(default(CombatSide), pet));
            Assert.Throws<ArgumentNullException>(() => e.Factory.CreateSources(CombatSide.Player, null));
            Assert.Throws<ArgumentException>(() => e.Factory.CreateSources(CombatSide.Player,
                new CombatPetState(new DefinitionId("test.other_pet"), new InstanceId(3001))));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FactorySource_FirstFriendlyDeath_BuffsOnlyLivingOwnRowInColumnOrder(CombatSide side, BoardRow row)
        {
            var e = new Environment();
            var source = e.Source(side, row);
            var death = e.Kill(side, row);
            Assert.That(source.Side, Is.EqualTo(side));
            Assert.That(source.PetInstanceId, Is.EqualTo(e.State.GetPets(side).GetPetAt(row == BoardRow.Front ? 0 : 1).InstanceId));
            Assert.That(source.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(source.AttackGainResolver, Is.SameAs(e.Resolver));
            Assert.That(e.Factory.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Factory.AttackGainResolver, Is.SameAs(e.Resolver));
            Assert.That(e.Factory.PetDefinitionId, Is.EqualTo(e.DefinitionId));
            var candidates = new List<CombatTriggerCandidate<ICombatTriggerHandler>>(source.DiscoverTriggers(e.State, death));
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Trigger, Is.SameAs(source.Handler));
            Assert.That(candidates[0].OrderKey, Is.EqualTo(new CombatTriggerOrderKey(
                CombatTriggerSourceKind.Pet, side, row == BoardRow.Front ? 0 : 1, 0)));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            candidates[0].Trigger.Resolve(e.State, death);

            Assert.That(e.Log.Count, Is.EqualTo(5));
            var columns = new[] { 2, 3, 5 };
            for (var i = 0; i < columns.Length; i++)
            {
                var target = e.Card(side, row, columns[i]);
                Assert.That(target.Attack, Is.EqualTo(3));
                Assert.That(target.CurrentHp, Is.EqualTo(3));
                Assert.That(target.Armor, Is.Zero);
                var gain = (AttackGainCombatEvent)e.Log.Events[i + 2];
                Assert.That(gain.TargetInstanceId, Is.EqualTo(target.InstanceId));
                Assert.That(gain.TargetPosition, Is.EqualTo(Position(side, row, columns[i])));
                Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
                Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(death.Metadata.EventId));
                Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
                Assert.That(gain.Metadata.SequenceNo.Value, Is.EqualTo(i + 3));
            }
            Assert.That(e.Card(side, row, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Card(side, row, 4).Attack, Is.EqualTo(2));
            Assert.That(e.Card(side, OtherRow(row), 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(OtherSide(side), row, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Usage.HasTriggered(source.PetInstanceId), Is.True);
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Autumn)]
        [TestCase(CombatCardSeason.Winter)]
        [TestCase(CombatCardSeason.Seasonless)]
        public void FirstDeathAndLivingTargets_HaveNoSeasonFilter(CombatCardSeason season)
        {
            var e = new Environment(season);
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            var source = e.Source(CombatSide.Player, BoardRow.Front);
            Assert.That(source.Handler.CanTrigger(e.State, death), Is.True);
            source.Handler.Resolve(e.State, death);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Attack, Is.EqualTo(3));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 5).Attack, Is.EqualTo(3));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WrongSideOrRowDeath_DoesNotConsumeFirstUse(bool wrongSide)
        {
            var e = new Environment();
            var source = e.Source(CombatSide.Player, BoardRow.Front);
            var wrong = e.Kill(wrongSide ? CombatSide.Enemy : CombatSide.Player,
                wrongSide ? BoardRow.Front : BoardRow.Back);
            Assert.That(source.DiscoverTriggers(e.State, wrong), Is.Empty);
            source.Handler.Resolve(e.State, wrong);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            var eligible = e.Kill(CombatSide.Player, BoardRow.Front);
            Assert.That(source.Handler.CanTrigger(e.State, eligible), Is.True);
            source.Handler.Resolve(e.State, eligible);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(3));
        }

        [Test]
        public void RebuiltFactorySources_AndLaterDeath_DoNotGrantSecondBonus()
        {
            var e = new Environment();
            var source = e.Source(CombatSide.Player, BoardRow.Front);
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            source.Handler.Resolve(e.State, death);
            var rebuilt = e.Source(CombatSide.Player, BoardRow.Front);
            Assert.That(rebuilt.Handler, Is.Not.SameAs(source.Handler));
            Assert.That(rebuilt.UsageCommitter, Is.SameAs(source.UsageCommitter));
            Assert.That(rebuilt.DiscoverTriggers(e.State, death), Is.Empty);
            rebuilt.Handler.Resolve(e.State, death);
            var laterDeath = e.Kill(CombatSide.Player, BoardRow.Front, 2);
            Assert.That(rebuilt.DiscoverTriggers(e.State, laterDeath), Is.Empty);
            rebuilt.Handler.Resolve(e.State, laterDeath);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(6));
        }

        [Test]
        public void NoLivingTargets_FirstEligibleDeathConsumesUseWithoutGainOrAllocation()
        {
            var e = new Environment();
            var source = e.Source(CombatSide.Player, BoardRow.Front);
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            foreach (var column in new[] { 2, 3, 5 }) { e.Card(CombatSide.Player, BoardRow.Front, column).SetCurrentHpToZero(); }
            source.Handler.Resolve(e.State, death);
            Assert.That(e.Usage.HasTriggered(source.PetInstanceId), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            e.Card(CombatSide.Player, BoardRow.Front, 2).Heal(1);
            source.Handler.Resolve(e.State, death);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_ReevaluatesCurrentLivingTargetsAndIncludesRescuedSource()
        {
            var e = new Environment();
            var source = e.Source(CombatSide.Player, BoardRow.Front);
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            Assert.That(source.Handler.CanTrigger(e.State, death), Is.True);
            e.Card(CombatSide.Player, BoardRow.Front, 1).Heal(1);
            e.Card(CombatSide.Player, BoardRow.Front, 2).SetCurrentHpToZero();
            e.Card(CombatSide.Player, BoardRow.Front, 4).Heal(1);
            var removedTarget = e.Card(CombatSide.Player, BoardRow.Front, 5);
            new CombatDirectDeleteResolver(e.Metadata, e.Log).ApplyDirectDelete(
                e.State, death, Position(CombatSide.Player, BoardRow.Front, 5));
            source.Handler.Resolve(e.State, death);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 1).Attack, Is.EqualTo(3));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Attack, Is.EqualTo(3));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 4).Attack, Is.EqualTo(3));
            Assert.That(removedTarget.Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(6));
        }

        [Test]
        public void DirectDeleteWithoutDeath_DoesNotTriggerWombat()
        {
            var e = new Environment();
            new CombatDirectDeleteResolver(e.Metadata, e.Log).ApplyDirectDelete(
                e.State, e.Root, Position(CombatSide.Player, BoardRow.Front, 1));
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, e.Builder.BuildRegistry(e.State));
            Assert.That(engine.Drain(20, 10), Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(2));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void QueuedPetTrigger_SurvivesSlotDirectDeleteAfterRealDamageDeath(CombatSide side)
        {
            var e = new Environment();
            var damage = new CombatDamageResolver(e.Metadata, e.Log).ApplyResolvedCardDamage(
                e.State, e.Root, Position(OtherSide(side), BoardRow.Front, 1), Position(side, BoardRow.Front, 1), 3);
            var death = new CombatDeathEventResolver(e.Metadata, e.Log).AppendFromDamage(damage);
            Assert.That(death, Is.Not.Null);
            var sources = new List<ICombatTriggerSource>(e.Builder.BuildSources(e.State));
            sources.Add(new CombatTriggerHandlerSource(
                new FixedCombatTriggerOrderKeyProvider(new CombatTriggerOrderKey(CombatTriggerSourceKind.Slot, side, 0, 0)),
                new DeleteOnDeath(new CombatDirectDeleteResolver(e.Metadata, e.Log))));
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, new CombatTriggerSourceRegistry(sources));
            Assert.That(engine.Drain(20, 10), Is.EqualTo(7));
            Assert.That(e.Log.Events[3].Kind, Is.EqualTo(CombatEventKind.DirectDelete));
            Assert.That(new CombatCardLookup(e.Log).Get(e.State, death.InstanceId).IsRemoved, Is.True);
            Assert.That(e.Card(side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(side, BoardRow.Front, 5).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.Drain(20, 10), Is.Zero);
            Assert.That(queue.PendingCount, Is.Zero);
        }

        [Test]
        public void LastTargetOverflow_LeavesWholeEffectUnused_AndEngineRetryAppliesOnce()
        {
            var e = new Environment();
            e.Kill(CombatSide.Player, BoardRow.Front);
            var last = e.Card(CombatSide.Player, BoardRow.Front, 5);
            last.ApplyAttackGain(int.MaxValue - last.Attack);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, e.Builder.BuildRegistry(e.State));
            Assert.Throws<OverflowException>(() => engine.Drain(20, 10));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            last.ReduceAttack(int.MaxValue - 2);
            engine.Drain(20, 10);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Attack, Is.EqualTo(3));
            Assert.That(last.Attack, Is.EqualTo(3));
            Assert.That(e.Log.Count, Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.Drain(20, 10), Is.Zero);
            Assert.That(queue.PendingCount, Is.Zero);
        }

        [Test]
        public void Builder_FourSameDefinitionPets_HaveIndependentUses()
        {
            var e = new Environment();
            Assert.That(e.Builder.BuildSources(e.State).Count, Is.EqualTo(4));
            foreach (var side in new[] { CombatSide.Player, CombatSide.Enemy })
            {
                foreach (var row in new[] { BoardRow.Front, BoardRow.Back }) { e.Kill(side, row); }
            }
            var engine = new CombatTriggerEngine(e.State, new CombatEventQueue(e.Log), e.Builder.BuildRegistry(e.State));
            Assert.That(engine.Drain(30, 10), Is.EqualTo(17));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            foreach (var side in new[] { CombatSide.Player, CombatSide.Enemy })
            {
                foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                {
                    Assert.That(e.Card(side, row, 2).Attack, Is.EqualTo(3));
                    Assert.That(e.Card(side, row, 5).Attack, Is.EqualTo(3));
                }
            }
        }

        [Test]
        public void UnloggedEligibleDeath_IsRejectedWithoutConsumingUse()
        {
            var e = new Environment();
            var source = e.Source(CombatSide.Player, BoardRow.Front);
            var death = e.Kill(CombatSide.Player, BoardRow.Front, append: false);
            Assert.Throws<ArgumentException>(() => source.Handler.Resolve(e.State, death));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(1));
        }

        private static CombatSide OtherSide(CombatSide side) => side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
        private static BoardRow OtherRow(BoardRow row) => row == BoardRow.Front ? BoardRow.Back : BoardRow.Front;
        private static BoardPosition Position(CombatSide side, BoardRow row, int column) => new BoardPosition(side, row, new BoardColumn(column));

        private sealed class DeleteOnDeath : CombatEventTriggerHandler<DeathCombatEvent>
        {
            private readonly CombatDirectDeleteResolver _resolver;
            public DeleteOnDeath(CombatDirectDeleteResolver resolver) { _resolver = resolver; }
            protected override bool CanTriggerTyped(CombatState state, DeathCombatEvent sourceEvent) => true;
            protected override void ResolveTyped(CombatState state, DeathCombatEvent sourceEvent)
            {
                _resolver.ApplyDirectDelete(state, sourceEvent, sourceEvent.Position);
            }
        }

        private sealed class Environment
        {
            public readonly DefinitionId DefinitionId = new DefinitionId("test.wombat");
            public readonly CombatState State;
            public readonly CombatEventIdAllocator Ids = new CombatEventIdAllocator();
            public readonly CombatEventMetadataFactory Metadata;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatPetTriggerUsageCommitter Usage = new CombatPetTriggerUsageCommitter(new CombatPetTriggerUsageRegistry());
            public readonly CombatAttackGainResolver Resolver;
            public readonly WombatPetTriggerSourceFactory Factory;
            public readonly CombatPetTriggerSourceBuilder Builder;

            public Environment(CombatCardSeason season = CombatCardSeason.Winter)
            {
                State = new CombatState(CreateSide(CombatSide.Player, season), CreateSide(CombatSide.Enemy, season),
                    CreatePets(CombatSide.Player), CreatePets(CombatSide.Enemy));
                Metadata = new CombatEventMetadataFactory(Ids, new CombatSequenceNumberAllocator());
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAttackGainResolver(Metadata, Log);
                Factory = new WombatPetTriggerSourceFactory(DefinitionId, Usage, Resolver);
                Builder = new CombatPetTriggerSourceBuilder(new CombatPetTriggerSourceFactoryRegistry(
                    new ICombatPetTriggerSourceFactory[] { Factory }));
            }

            public WombatPetTriggerSource Source(CombatSide side, BoardRow row)
            {
                var sources = new List<ICombatTriggerSource>(Factory.CreateSources(side,
                    State.GetPets(side).GetPetAt(row == BoardRow.Front ? 0 : 1)));
                Assert.That(sources.Count, Is.EqualTo(1));
                return (WombatPetTriggerSource)sources[0];
            }

            public CombatCardState Card(CombatSide side, BoardRow row, int column) => State.GetSide(side).GetCardAt(Position(side, row, column));

            public DeathCombatEvent Kill(CombatSide side, BoardRow row, int column = 1, bool append = true)
            {
                var card = Card(side, row, column);
                var previousHp = card.CurrentHp;
                card.SetCurrentHpToZero();
                var death = new DeathCombatEvent(Metadata.CreateChild(Root.Metadata), card.InstanceId,
                    Position(side, row, column), previousHp, card.CurrentHp);
                if (append) { Log.Append(death); }
                return death;
            }

            private CombatSidePetState CreatePets(CombatSide side)
            {
                var value = side == CombatSide.Player ? 1000 : 2000;
                return new CombatSidePetState(side, new CombatPetRegistry(new[]
                {
                    new CombatPetState(DefinitionId, new InstanceId(value + 2)),
                    new CombatPetState(DefinitionId, new InstanceId(value + 1))
                }));
            }

            private static CombatSideState CreateSide(CombatSide side, CombatCardSeason season)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 4, 1, 3, 2 })
                    {
                        var value = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        var cardSeason = column <= 2 ? season : column == 3 ? CombatCardSeason.Seasonless : CombatCardSeason.Spring;
                        var card = new CombatCardState(new DefinitionId("test.wombat_card"), new InstanceId(value),
                            new CardRank(2), cardSeason, 5, column == 4 ? 0 : 3, 0, 2);
                        cards.Add(card);
                        slots.Add(new CombatSlotState(new SlotId(value), Position(side, row, column), card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
