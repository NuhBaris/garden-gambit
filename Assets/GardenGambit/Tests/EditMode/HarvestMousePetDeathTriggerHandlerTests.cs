using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class HarvestMousePetDeathTriggerHandlerTests
    {
        [Test]
        public void Constructor_RequiresDependenciesAndPreservesSharedReferences()
        {
            var e = new Environment();
            var id = e.Pets[0].InstanceId;
            Assert.Throws<ArgumentNullException>(() => new HarvestMousePetDeathTriggerHandler(
                e.Side, id, null, e.Resolver, e.Lookup));
            Assert.Throws<ArgumentNullException>(() => new HarvestMousePetDeathTriggerHandler(
                e.Side, id, e.Usage, null, e.Lookup));
            Assert.Throws<ArgumentNullException>(() => new HarvestMousePetDeathTriggerHandler(
                e.Side, id, e.Usage, e.Resolver, null));
            Assert.That(e.Handlers[0].UsageCommitter, Is.SameAs(e.Usage));
            Assert.That(e.Handlers[0].AttackGainResolver, Is.SameAs(e.Resolver));
            Assert.That(e.Handlers[0].CardLookup, Is.SameAs(e.Lookup));
        }

        [TestCase(CombatSide.Player, BoardRow.Front)]
        [TestCase(CombatSide.Player, BoardRow.Back)]
        [TestCase(CombatSide.Enemy, BoardRow.Front)]
        [TestCase(CombatSide.Enemy, BoardRow.Back)]
        public void FirstAutumnDeath_BuffsOnlyLivingAutumnInPetRowInColumnOrder(CombatSide side, BoardRow row)
        {
            var e = new Environment(side);
            var death = e.Kill(side, row, 1);
            var handler = e.Handler(row);
            var otherRow = row == BoardRow.Front ? BoardRow.Back : BoardRow.Front;

            Assert.That(handler.CanTrigger(e.State, death), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            handler.Resolve(e.State, death);

            Assert.That(e.Card(side, row, 1).Attack, Is.EqualTo(2));
            Assert.That(e.Card(side, row, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(side, row, 3).Attack, Is.EqualTo(2));
            Assert.That(e.Card(side, row, 4).Attack, Is.EqualTo(3));
            Assert.That(e.Card(side, otherRow, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(e.OtherSide, row, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(side, row, 2).CurrentHp, Is.EqualTo(3));
            Assert.That(e.Card(side, row, 2).Armor, Is.Zero);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Usage.HasTriggered(handler.PetInstanceId), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(4));

            var first = (AttackGainCombatEvent)e.Log.Events[2];
            var second = (AttackGainCombatEvent)e.Log.Events[3];
            Assert.That(first.TargetPosition, Is.EqualTo(e.Position(side, row, 2)));
            Assert.That(second.TargetPosition, Is.EqualTo(e.Position(side, row, 4)));
            foreach (var gain in new[] { first, second })
            {
                Assert.That(gain.ActualGainedAmount, Is.EqualTo(1));
                Assert.That(gain.Metadata.ParentEventId.Value, Is.EqualTo(death.Metadata.EventId));
                Assert.That(gain.Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.EventId));
            }
            Assert.That(first.Metadata.SequenceNo.Value, Is.LessThan(second.Metadata.SequenceNo.Value));
        }

        [TestCase(CombatCardSeason.Unspecified)]
        [TestCase(CombatCardSeason.Spring)]
        [TestCase(CombatCardSeason.Summer)]
        [TestCase(CombatCardSeason.Winter)]
        [TestCase(CombatCardSeason.Seasonless)]
        public void NonAutumnDeath_DoesNotConsumeUse(CombatCardSeason season)
        {
            var e = new Environment(sourceSeason: season);
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.False);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(2));
            var eligibleDeath = e.Kill(e.Side, BoardRow.Front, 2);
            Assert.That(e.Handlers[0].CanTrigger(e.State, eligibleDeath), Is.True);
            e.Handlers[0].Resolve(e.State, eligibleDeath);
            Assert.That(e.Card(e.Side, BoardRow.Front, 4).Attack, Is.EqualTo(3));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WrongSideOrRow_DoesNotTriggerOrConsumeUse(bool wrongSide)
        {
            var e = new Environment();
            var death = e.Kill(wrongSide ? e.OtherSide : e.Side,
                wrongSide ? BoardRow.Front : BoardRow.Back, 1);
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.False);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void RepeatedResolveAndLaterEligibleDeath_DoNotGrantSecondBonus()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.False);
            e.Handlers[0].Resolve(e.State, death);
            var later = e.Kill(e.Side, BoardRow.Front, 2);
            Assert.That(e.Handlers[0].CanTrigger(e.State, later), Is.False);
            e.Handlers[0].Resolve(e.State, later);
            Assert.That(e.Card(e.Side, BoardRow.Front, 4).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(5));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(5));
        }

        [Test]
        public void TwoDiscoveredDeaths_BeforeFirstResolve_StillConsumeOnlyOneUse()
        {
            var e = new Environment();
            var first = e.Kill(e.Side, BoardRow.Front, 1);
            var second = e.Kill(e.Side, BoardRow.Front, 2);
            Assert.That(e.Handlers[0].CanTrigger(e.State, first), Is.True);
            Assert.That(e.Handlers[0].CanTrigger(e.State, second), Is.True);
            e.Handlers[0].Resolve(e.State, first);
            e.Handlers[0].Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Front, 4).Attack, Is.EqualTo(3));
            Assert.That(e.Log.Count, Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [Test]
        public void NoLivingTargets_FirstEligibleDeathStillConsumesUseWithoutGainOrAllocation()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            e.Card(e.Side, BoardRow.Front, 2).SetCurrentHpToZero();
            e.Card(e.Side, BoardRow.Front, 4).SetCurrentHpToZero();
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.True);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Usage.HasTriggered(e.Pets[0].InstanceId), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            e.Card(e.Side, BoardRow.Front, 4).Heal(1);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Card(e.Side, BoardRow.Front, 4).Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(2));
        }

        [Test]
        public void TargetsAreReevaluatedAtResolution_AfterDeathAndRowMovement()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.True);
            var dyingTarget = e.Card(e.Side, BoardRow.Front, 2);
            var movingOut = e.Card(e.Side, BoardRow.Front, 4);
            var movingIn = e.Card(e.Side, BoardRow.Back, 2);
            dyingTarget.SetCurrentHpToZero();
            var owner = e.State.GetSide(e.Side);
            owner.MoveCard(e.Position(e.Side, BoardRow.Front, 4), e.Position(e.Side, BoardRow.Back, 5));
            owner.MoveCard(e.Position(e.Side, BoardRow.Back, 2), e.Position(e.Side, BoardRow.Front, 5));

            e.Handlers[0].Resolve(e.State, death);

            Assert.That(dyingTarget.Attack, Is.EqualTo(2));
            Assert.That(movingOut.Attack, Is.EqualTo(2));
            Assert.That(movingIn.Attack, Is.EqualTo(3));
            Assert.That(e.Log.Count, Is.EqualTo(3));
            Assert.That(((AttackGainCombatEvent)e.Log.Events[2]).TargetPosition,
                Is.EqualTo(e.Position(e.Side, BoardRow.Front, 5)));
        }

        [Test]
        public void RescuedDeathSource_RemainsEligibleAndCanReceiveLivingTargetBonus()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.True);
            var source = e.Card(e.Side, BoardRow.Front, 1);
            source.Heal(1);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(source.CurrentHp, Is.EqualTo(1));
            Assert.That(source.Attack, Is.EqualTo(3));
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Log.Count, Is.EqualTo(5));
            Assert.That(((AttackGainCombatEvent)e.Log.Events[2]).TargetInstanceId, Is.EqualTo(source.InstanceId));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RemovedDeathSource_UsesTombstoneAndStillResolves(bool directDelete)
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.True);
            if (directDelete)
            {
                new CombatDirectDeleteResolver(e.Factory, e.Log).ApplyDirectDelete(e.State, death, death.Position);
            }
            else
            {
                new CombatDeathRemovalResolver(e.Factory, e.Log).TryApplyRemoval(e.State, death);
            }
            Assert.That(e.Lookup.Get(e.State, death.InstanceId).IsRemoved, Is.True);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(e.Side, BoardRow.Front, 4).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.HasTriggered(e.Pets[0].InstanceId), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(5));
        }

        [Test]
        public void TwoSameDefinitionPets_KeepUsesIndependentAndAffectTheirOwnRows()
        {
            var e = new Environment();
            Assert.That(e.Pets[0].DefinitionId, Is.EqualTo(e.Pets[1].DefinitionId));
            var upperDeath = e.Kill(e.Side, BoardRow.Front, 1);
            e.Handlers[0].Resolve(e.State, upperDeath);
            Assert.That(e.Usage.HasTriggered(e.Pets[1].InstanceId), Is.False);
            var lowerDeath = e.Kill(e.Side, BoardRow.Back, 1);
            Assert.That(e.Handlers[1].CanTrigger(e.State, lowerDeath), Is.True);
            e.Handlers[1].Resolve(e.State, lowerDeath);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(e.Side, BoardRow.Back, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Log.Count, Is.EqualTo(7));
        }

        [Test]
        public void OverflowOnLastTarget_DoesNotChangeEarlierTargetsOrConsumeUse_RetryAppliesOnce()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            var last = e.Card(e.Side, BoardRow.Front, 4);
            last.ApplyAttackGain(int.MaxValue - last.Attack);
            Assert.Throws<OverflowException>(() => e.Handlers[0].Resolve(e.State, death));
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(2));
            Assert.That(last.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(2));
            Assert.That(e.Ids.LastIssuedValue, Is.EqualTo(2));
            Assert.That(e.Handlers[0].CanTrigger(e.State, death), Is.True);
            last.ReduceAttack(1);
            e.Handlers[0].Resolve(e.State, death);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(last.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Log.Count, Is.EqualTo(4));
        }

        [Test]
        public void UnloggedEligibleDeath_WithNoTargets_RejectsWithoutConsumingUse()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1, append: false);
            e.Card(e.Side, BoardRow.Front, 2).SetCurrentHpToZero();
            e.Card(e.Side, BoardRow.Front, 4).SetCurrentHpToZero();
            Assert.Throws<ArgumentException>(() => e.Handlers[0].Resolve(e.State, death));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(1));
            e.Log.Append(death);
            e.Handlers[0].Resolve(e.State, death);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(CombatSide.Player)]
        [TestCase(CombatSide.Enemy)]
        public void TriggerEngine_QueuedMouseSurvivesEarlierSlotDelete_AndDrainDoesNotRepeat(CombatSide side)
        {
            var e = new Environment(side);
            var damage = new CombatDamageResolver(e.Factory, e.Log).ApplyResolvedCardDamage(
                e.State, e.Root, e.Position(e.OtherSide, BoardRow.Front, 1),
                e.Position(side, BoardRow.Front, 1), incomingDamage: 3);
            var death = new CombatDeathEventResolver(e.Factory, e.Log).AppendFromDamage(damage);
            Assert.That(death, Is.Not.Null);
            var sources = new CombatTriggerSourceRegistry(new ICombatTriggerSource[]
            {
                new CombatPetDeathTriggerSource(e.Handlers[0]),
                new CombatTriggerHandlerSource(
                    new FixedCombatTriggerOrderKeyProvider(new CombatTriggerOrderKey(
                        CombatTriggerSourceKind.Slot, side, horizontalOrder: 0, verticalOrder: 0)),
                    new DeleteHandler(new CombatDirectDeleteResolver(e.Factory, e.Log)))
            });
            var queue = new CombatEventQueue(e.Log);
            var engine = new CombatTriggerEngine(e.State, queue, sources);
            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.EqualTo(6));
            Assert.That(e.Log.Events[3].Kind, Is.EqualTo(CombatEventKind.DirectDelete));
            Assert.That(e.Log.Events[4].Kind, Is.EqualTo(CombatEventKind.AttackGain));
            Assert.That(e.Log.Events[5].Kind, Is.EqualTo(CombatEventKind.AttackGain));
            Assert.That(e.Lookup.Get(e.State, death.InstanceId).IsRemoved, Is.True);
            Assert.That(e.Card(side, BoardRow.Front, 2).Attack, Is.EqualTo(3));
            Assert.That(e.Card(side, BoardRow.Front, 4).Attack, Is.EqualTo(3));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 10), Is.Zero);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(e.Log.Count, Is.EqualTo(6));
        }

        private sealed class DeleteHandler : CombatEventTriggerHandler<DeathCombatEvent>
        {
            private readonly CombatDirectDeleteResolver _resolver;
            public DeleteHandler(CombatDirectDeleteResolver resolver) { _resolver = resolver; }
            protected override bool CanTriggerTyped(CombatState state, DeathCombatEvent sourceEvent) { return true; }
            protected override void ResolveTyped(CombatState state, DeathCombatEvent sourceEvent)
            {
                _resolver.ApplyDirectDelete(state, sourceEvent, sourceEvent.Position);
            }
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatSide OtherSide;
            public readonly CombatState State;
            public readonly CombatPetState[] Pets;
            public readonly HarvestMousePetDeathTriggerHandler[] Handlers;
            public readonly CombatEventIdAllocator Ids = new CombatEventIdAllocator();
            public readonly CombatEventMetadataFactory Factory;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatAttackGainResolver Resolver;
            public readonly CombatCardLookup Lookup;
            public readonly CombatPetTriggerUsageCommitter Usage =
                new CombatPetTriggerUsageCommitter(new CombatPetTriggerUsageRegistry());

            public Environment(CombatSide side = CombatSide.Player, CombatCardSeason sourceSeason = CombatCardSeason.Autumn)
            {
                Side = side;
                OtherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Pets = new[]
                {
                    new CombatPetState(new DefinitionId("test.harvest_mouse"), new InstanceId(1002)),
                    new CombatPetState(new DefinitionId("test.harvest_mouse"), new InstanceId(1001))
                };
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(Pets));
                var otherPets = new CombatSidePetState(OtherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(MakeSide(CombatSide.Player, sourceSeason), MakeSide(CombatSide.Enemy, sourceSeason),
                    side == CombatSide.Player ? ownPets : otherPets,
                    side == CombatSide.Enemy ? ownPets : otherPets);
                Factory = new CombatEventMetadataFactory(Ids, new CombatSequenceNumberAllocator());
                Root = new CombatStartedCombatEvent(Factory.CreateRoot());
                Log.Append(Root);
                Resolver = new CombatAttackGainResolver(Factory, Log);
                Lookup = new CombatCardLookup(Log);
                Handlers = new[]
                {
                    new HarvestMousePetDeathTriggerHandler(side, Pets[0].InstanceId, Usage, Resolver, Lookup),
                    new HarvestMousePetDeathTriggerHandler(side, Pets[1].InstanceId, Usage, Resolver, Lookup)
                };
            }

            public BoardPosition Position(CombatSide side, BoardRow row, int column)
            {
                return new BoardPosition(side, row, new BoardColumn(column));
            }

            public CombatCardState Card(CombatSide side, BoardRow row, int column)
            {
                return State.GetSide(side).GetCardAt(Position(side, row, column));
            }

            public HarvestMousePetDeathTriggerHandler Handler(BoardRow row)
            {
                return Handlers[row == BoardRow.Front ? 0 : 1];
            }

            public DeathCombatEvent Kill(CombatSide side, BoardRow row, int column, bool append = true)
            {
                var card = Card(side, row, column);
                var previousHp = card.CurrentHp;
                card.SetCurrentHpToZero();
                var death = new DeathCombatEvent(Factory.CreateChild(Root.Metadata), card.InstanceId,
                    Position(side, row, column), previousHp, card.CurrentHp);
                if (append) { Log.Append(death); }
                return death;
            }

            private CombatSideState MakeSide(CombatSide side, CombatCardSeason sourceSeason)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                // Deliberately unsorted rows and columns, and Pet IDs in reverse
                // numeric order: effect order must come from board/Pet positions.
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 4, 3, 5, 2, 1 })
                    {
                        var value = (side == CombatSide.Player ? 0 : 100) +
                            (row == BoardRow.Front ? 0 : 10) + column;
                        var position = Position(side, row, column);
                        if (column == 5)
                        {
                            slots.Add(new CombatSlotState(new SlotId(value), position));
                            continue;
                        }
                        var season = column == 1 ? sourceSeason :
                            (row == BoardRow.Front && column == 3 ? CombatCardSeason.Spring : CombatCardSeason.Autumn);
                        var card = new CombatCardState(new DefinitionId("test.mouse_card"), new InstanceId(value),
                            new CardRank(2), season, hpCapacity: 5,
                            currentHp: row == BoardRow.Back && column == 3 ? 0 : 3, armor: 0, attack: 2);
                        cards.Add(card);
                        slots.Add(new CombatSlotState(new SlotId(value), position, card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(BattleHealth.NormalBaselineValue), new AttackMultiplier(AttackMultiplier.BaseValue));
            }
        }
    }
}
