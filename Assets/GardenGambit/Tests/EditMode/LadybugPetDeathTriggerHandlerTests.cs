using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class LadybugPetDeathTriggerHandlerTests
    {
        [Test]
        public void Constructor_RequiresAndExposesSharedDependencies()
        {
            var e = new Environment();
            var id = e.State.PlayerPets.GetPetAt(0).InstanceId;
            Assert.Throws<ArgumentNullException>(() => new LadybugPetDeathTriggerHandler(CombatSide.Player, id, null, e.Resolver));
            Assert.Throws<ArgumentNullException>(() => new LadybugPetDeathTriggerHandler(CombatSide.Player, id, e.Usage, null));
            var handler = e.Handler(CombatSide.Player, BoardRow.Front);
            Assert.That(handler.UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(handler.ArmorGainResolver, Is.SameAs(e.Resolver));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FirstFriendlyDeath_GrantsArmorOnlyToLivingOwnRowInColumnOrder(CombatSide side, BoardRow row)
        {
            var e = new Environment();
            var death = e.Kill(side, row);
            var handler = e.Handler(side, row);
            Assert.That(handler.CanTrigger(e.State, death), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            handler.Resolve(e.State, death);
            Assert.That(e.Card(side, row, 2).Armor, Is.EqualTo(3));
            Assert.That(e.Card(side, row, 3).Armor, Is.EqualTo(1));
            Assert.That(e.Card(side, row, 5).Armor, Is.EqualTo(2));
            Assert.That(e.Card(side, row, 1).Armor, Is.Zero);
            Assert.That(e.Card(side, row, 4).Armor, Is.EqualTo(4));
            Assert.That(e.Card(side, row == BoardRow.Front ? BoardRow.Back : BoardRow.Front, 2).Armor, Is.EqualTo(2));
            Assert.That(e.Card(side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player, row, 2).Armor, Is.EqualTo(2));
            var columns = new[] { 2, 3, 5 };
            var previousArmor = new[] { 2, 0, 1 };
            for (var i = 0; i < columns.Length; i++)
            {
                var card = e.Card(side, row, columns[i]);
                var gain = (ArmorGainCombatEvent)e.Log.Events[i + 2];
                Assert.That(gain.TargetInstanceId, Is.EqualTo(card.InstanceId));
                Assert.That(gain.TargetPosition, Is.EqualTo(Position(side, row, columns[i])));
                Assert.That(gain.PreviousArmor, Is.EqualTo(previousArmor[i]));
                Assert.That(gain.CurrentArmor, Is.EqualTo(previousArmor[i] + 1));
                Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
                Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(death.Metadata.EventId));
                Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
                Assert.That(gain.Metadata.SequenceNo.Value, Is.EqualTo(i + 3));
                Assert.That(card.CurrentHp, Is.EqualTo(3));
                Assert.That(card.HpCapacity, Is.EqualTo(5));
                Assert.That(card.Attack, Is.EqualTo(2));
            }
            Assert.That(e.Log.Count, Is.EqualTo(5));
            Assert.That(e.Usage.HasTriggered(handler.PetInstanceId), Is.True);
        }

        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Seasonless)]
        public void DeathEligibility_HasNoSeasonRequirement(CombatCardSeason season)
        {
            var e = new Environment(season);
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            var handler = e.Handler(CombatSide.Player, BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, death), Is.True);
            handler.Resolve(e.State, death);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Armor, Is.EqualTo(3));
            Assert.That(e.Usage.HasTriggered(handler.PetInstanceId), Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WrongSideOrRowDeath_DoesNotConsumeUse(bool wrongSide)
        {
            var e = new Environment();
            var death = e.Kill(wrongSide ? CombatSide.Enemy : CombatSide.Player,
                wrongSide ? BoardRow.Front : BoardRow.Back);
            var handler = e.Handler(CombatSide.Player, BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, e.Root), Is.False);
            Assert.That(handler.CanTrigger(e.State, death), Is.False);
            handler.Resolve(e.State, death);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(handler.CanTrigger(e.State, e.Kill(CombatSide.Player, BoardRow.Front)), Is.True);
        }

        [Test]
        public void FirstDeathWithNoLivingTargets_ConsumesUseWithoutGainOrAllocation()
        {
            var e = new Environment();
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            foreach (var column in new[] { 2, 3, 5 }) { e.Card(CombatSide.Player, BoardRow.Front, column).SetCurrentHpToZero(); }
            var handler = e.Handler(CombatSide.Player, BoardRow.Front);
            handler.Resolve(e.State, death);
            Assert.That(e.Usage.HasTriggered(handler.PetInstanceId), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            e.Card(CombatSide.Player, BoardRow.Front, 2).Heal(1);
            handler.Resolve(e.State, death);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Armor, Is.EqualTo(2));
        }

        [Test]
        public void TargetsUseResolutionTimeState_AndIncludeRescuedDeathSource()
        {
            var e = new Environment();
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            var handler = e.Handler(CombatSide.Player, BoardRow.Front);
            Assert.That(handler.CanTrigger(e.State, death), Is.True);
            e.Card(CombatSide.Player, BoardRow.Front, 1).Heal(1);
            e.Card(CombatSide.Player, BoardRow.Front, 2).SetCurrentHpToZero();
            e.Card(CombatSide.Player, BoardRow.Front, 4).Heal(1);
            var deleted = e.Card(CombatSide.Player, BoardRow.Front, 5);
            new CombatDirectDeleteResolver(e.Metadata, e.Log).ApplyDirectDelete(e.State, death,
                Position(CombatSide.Player, BoardRow.Front, 5));
            handler.Resolve(e.State, death);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 1).Armor, Is.EqualTo(1));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Armor, Is.EqualTo(2));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Armor, Is.EqualTo(1));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 4).Armor, Is.EqualTo(5));
            Assert.That(deleted.Armor, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(6));
        }

        [Test]
        public void RecreatedHandlerAndLaterDeath_CannotGrantSecondBonus()
        {
            var e = new Environment();
            var death = e.Kill(CombatSide.Player, BoardRow.Front);
            e.Handler(CombatSide.Player, BoardRow.Front).Resolve(e.State, death);
            var recreated = e.Handler(CombatSide.Player, BoardRow.Front);
            Assert.That(recreated.CanTrigger(e.State, death), Is.False);
            recreated.Resolve(e.State, death);
            recreated.Resolve(e.State, e.Kill(CombatSide.Player, BoardRow.Front, 2));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Armor, Is.EqualTo(1));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 5).Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(6));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void QueuedLadybug_ContinuesAfterEarlierSlotDirectDelete(CombatSide side)
        {
            var e = new Environment();
            var otherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
            var damage = new CombatDamageResolver(e.Metadata, e.Log).ApplyResolvedCardDamage(e.State, e.Root,
                Position(otherSide, BoardRow.Front, 1), Position(side, BoardRow.Front, 1), 3);
            var death = new CombatDeathEventResolver(e.Metadata, e.Log).AppendFromDamage(damage);
            Assert.That(death, Is.Not.Null);
            var sources = e.Sources();
            sources.Add(new CombatTriggerHandlerSource(
                new FixedCombatTriggerOrderKeyProvider(new CombatTriggerOrderKey(CombatTriggerSourceKind.Slot, side, 0, 0)),
                new DeleteOnDeath(new CombatDirectDeleteResolver(e.Metadata, e.Log))));
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, new CombatTriggerSourceRegistry(sources));
            Assert.That(engine.Drain(20, 10), Is.EqualTo(7));
            Assert.That(e.Log.Events[3].Kind, Is.EqualTo(CombatEventKind.DirectDelete));
            Assert.That(new CombatCardLookup(e.Log).Get(e.State, death.InstanceId).IsRemoved, Is.True);
            Assert.That(e.Card(side, BoardRow.Front, 2).Armor, Is.EqualTo(3));
            Assert.That(e.Card(side, BoardRow.Front, 5).Armor, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.Drain(20, 10), Is.Zero);
            Assert.That(queue.PendingCount, Is.Zero);
        }

        [Test]
        public void DirectDeleteWithoutDeath_DoesNotTriggerLadybug()
        {
            var e = new Environment();
            new CombatDirectDeleteResolver(e.Metadata, e.Log).ApplyDirectDelete(e.State, e.Root,
                Position(CombatSide.Player, BoardRow.Front, 1));
            var engine = new CombatTriggerEngine(e.State, new CombatEventQueue(e.Log), new CombatTriggerSourceRegistry(e.Sources()));
            Assert.That(engine.Drain(20, 10), Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Armor, Is.EqualTo(2));
        }

        [Test]
        public void LastTargetOverflow_LeavesAllTargetsAndUsageIntact_EngineRetryAppliesOnce()
        {
            var e = new Environment();
            e.Kill(CombatSide.Player, BoardRow.Front);
            var last = e.Card(CombatSide.Player, BoardRow.Front, 5);
            last.ApplyArmorGain(int.MaxValue - last.Armor);
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, new CombatTriggerSourceRegistry(e.Sources()));
            Assert.Throws<OverflowException>(() => engine.Drain(20, 10));
            Assert.That(engine.HasActiveBatch, Is.True);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Armor, Is.EqualTo(2));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Armor, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            last.RemoveArmor(int.MaxValue - 1);
            engine.Drain(20, 10);
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 2).Armor, Is.EqualTo(3));
            Assert.That(e.Card(CombatSide.Player, BoardRow.Front, 3).Armor, Is.EqualTo(1));
            Assert.That(last.Armor, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(5));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.Drain(20, 10), Is.Zero);
        }

        [Test]
        public void FourSameDefinitionPets_KeepUsesAndRowsIndependent()
        {
            var e = new Environment();
            foreach (var side in new[] { CombatSide.Player, CombatSide.Enemy })
            {
                foreach (var row in new[] { BoardRow.Front, BoardRow.Back }) { e.Kill(side, row); }
            }
            var engine = new CombatTriggerEngine(e.State, new CombatEventQueue(e.Log), new CombatTriggerSourceRegistry(e.Sources()));
            Assert.That(engine.Drain(30, 10), Is.EqualTo(17));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(4));
            foreach (var side in new[] { CombatSide.Player, CombatSide.Enemy })
            {
                foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                {
                    Assert.That(e.Card(side, row, 2).Armor, Is.EqualTo(3));
                    Assert.That(e.Card(side, row, 3).Armor, Is.EqualTo(1));
                }
            }
        }

        private static BoardPosition Position(CombatSide side, BoardRow row, int column) => new BoardPosition(side, row, new BoardColumn(column));

        private sealed class DeleteOnDeath : CombatEventTriggerHandler<DeathCombatEvent>
        {
            private readonly CombatDirectDeleteResolver _resolver;
            public DeleteOnDeath(CombatDirectDeleteResolver resolver) { _resolver = resolver; }
            protected override bool CanTriggerTyped(CombatState state, DeathCombatEvent sourceEvent) => true;
            protected override void ResolveTyped(CombatState state, DeathCombatEvent sourceEvent) =>
                _resolver.ApplyDirectDelete(state, sourceEvent, sourceEvent.Position);
        }

        private sealed class Environment
        {
            public readonly CombatState State;
            public readonly CombatEventIdAllocator Ids = new CombatEventIdAllocator();
            public readonly CombatEventMetadataFactory Metadata;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatPetTriggerUsageCommitter Usage = new CombatPetTriggerUsageCommitter(new CombatPetTriggerUsageRegistry());
            public readonly CombatArmorGainResolver Resolver;

            public Environment(CombatCardSeason sourceSeason = CombatCardSeason.Autumn)
            {
                State = new CombatState(CreateSide(CombatSide.Player, sourceSeason), CreateSide(CombatSide.Enemy, sourceSeason),
                    CreatePets(CombatSide.Player), CreatePets(CombatSide.Enemy));
                Metadata = new CombatEventMetadataFactory(Ids, new CombatSequenceNumberAllocator());
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatArmorGainResolver(Metadata, Log);
            }

            public CombatCardState Card(CombatSide side, BoardRow row, int column) => State.GetSide(side).GetCardAt(Position(side, row, column));
            public LadybugPetDeathTriggerHandler Handler(CombatSide side, BoardRow row) => new LadybugPetDeathTriggerHandler(
                side, State.GetPets(side).GetPetAt(row == BoardRow.Front ? 0 : 1).InstanceId, Usage, Resolver);

            public List<ICombatTriggerSource> Sources()
            {
                var sources = new List<ICombatTriggerSource>();
                foreach (var side in new[] { CombatSide.Player, CombatSide.Enemy })
                {
                    foreach (var row in new[] { BoardRow.Front, BoardRow.Back })
                    {
                        sources.Add(new CombatPetDeathTriggerSource(Handler(side, row)));
                    }
                }
                return sources;
            }

            public DeathCombatEvent Kill(CombatSide side, BoardRow row, int column = 1)
            {
                var card = Card(side, row, column);
                var previousHp = card.CurrentHp;
                card.SetCurrentHpToZero();
                var death = new DeathCombatEvent(Metadata.CreateChild(Root.Metadata), card.InstanceId,
                    Position(side, row, column), previousHp, card.CurrentHp);
                Log.Append(death);
                return death;
            }

            private static CombatSidePetState CreatePets(CombatSide side)
            {
                var value = side == CombatSide.Player ? 1000 : 2000;
                var definition = new DefinitionId("test.ladybug");
                return new CombatSidePetState(side, new CombatPetRegistry(new[]
                {
                    new CombatPetState(definition, new InstanceId(value + 2)),
                    new CombatPetState(definition, new InstanceId(value + 1))
                }));
            }

            private static CombatSideState CreateSide(CombatSide side, CombatCardSeason sourceSeason)
            {
                var cards = new List<CombatCardState>();
                var slots = new List<CombatSlotState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 4, 1, 3, 2 })
                    {
                        var value = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        var season = column == 1 ? sourceSeason : column == 2 ? CombatCardSeason.Winter :
                            column == 3 ? CombatCardSeason.Seasonless : CombatCardSeason.Spring;
                        var armor = column == 2 ? 2 : column == 4 ? 4 : column == 5 ? 1 : 0;
                        var card = new CombatCardState(new DefinitionId("test.ladybug_card"), new InstanceId(value),
                            new CardRank(2), season, 5, column == 4 ? 0 : 3, armor, 2);
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
