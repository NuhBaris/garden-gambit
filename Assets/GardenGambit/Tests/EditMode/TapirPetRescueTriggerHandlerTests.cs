using System;
using System.Collections.Generic;
using GardenGambit.Domain.Combat;
using GardenGambit.Domain.Identity;
using GardenGambit.Simulation.Combat;
using NUnit.Framework;

namespace GardenGambit.Tests.EditMode
{
    public sealed class TapirPetRescueTriggerHandlerTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void Constructors_ValidateDependenciesAndPreserveReferences(bool hpPath)
        {
            var e = new Environment();
            if (hpPath)
            {
                Assert.Throws<ArgumentNullException>(() => new TapirPetHpGainTriggerHandler(e.Side, e.Pets[0].InstanceId, null, e.Attack));
                Assert.Throws<ArgumentNullException>(() => new TapirPetHpGainTriggerHandler(e.Side, e.Pets[0].InstanceId, e.Usage, null));
                var handler = (TapirPetHpGainTriggerHandler)e.Handler(1);
                Assert.That(handler.UsageCommitter, Is.SameAs(e.Usage));
                Assert.That(handler.AttackGainResolver, Is.SameAs(e.Attack));
            }
            else
            {
                Assert.Throws<ArgumentNullException>(() => new TapirPetRescueTriggerHandler(e.Side, e.Pets[0].InstanceId, null, e.Attack));
                Assert.Throws<ArgumentNullException>(() => new TapirPetRescueTriggerHandler(e.Side, e.Pets[0].InstanceId, e.Usage, null));
                var handler = (TapirPetRescueTriggerHandler)e.Handler(0);
                Assert.That(handler.UsageCommitter, Is.SameAs(e.Usage));
                Assert.That(handler.AttackGainResolver, Is.SameAs(e.Attack));
            }
        }

        // 0 = explicit Rescue resolver; 1 = Heal crossing the threshold;
        // 2 = HP stat gain crossing the threshold.
        [TestCase(0, CombatSide.Player, BoardRow.Front)]
        [TestCase(0, CombatSide.Player, BoardRow.Back)]
        [TestCase(0, CombatSide.Enemy, BoardRow.Front)]
        [TestCase(0, CombatSide.Enemy, BoardRow.Back)]
        [TestCase(1, CombatSide.Player, BoardRow.Front)]
        [TestCase(1, CombatSide.Player, BoardRow.Back)]
        [TestCase(1, CombatSide.Enemy, BoardRow.Front)]
        [TestCase(1, CombatSide.Enemy, BoardRow.Back)]
        [TestCase(2, CombatSide.Player, BoardRow.Front)]
        [TestCase(2, CombatSide.Player, BoardRow.Back)]
        [TestCase(2, CombatSide.Enemy, BoardRow.Front)]
        [TestCase(2, CombatSide.Enemy, BoardRow.Back)]
        public void SuccessfulRescue_AddsTwoAttackAndLogsChild(int path, CombatSide side, BoardRow row)
        {
            var e = new Environment(side);
            var target = e.Card(side, row);
            var source = e.Restore(path, side, row);
            var handler = e.Handler(path, row);
            var count = e.Log.Count;
            var hp = target.CurrentHp;
            var capacity = target.HpCapacity;
            Assert.That(handler.CanTrigger(e.State, source), Is.True);
            Assert.That(handler.CanTrigger(e.State, source), Is.True);
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(target.Attack, Is.EqualTo(2));

            handler.Resolve(e.State, source);

            Assert.That(target.Attack, Is.EqualTo(4));
            Assert.That(target.CurrentHp, Is.EqualTo(hp));
            Assert.That(target.HpCapacity, Is.EqualTo(capacity));
            Assert.That(target.Armor, Is.Zero);
            Assert.That(target.Rank.Value, Is.EqualTo(4));
            Assert.That(e.Card(side, row, 2).Attack, Is.EqualTo(2));
            Assert.That(e.Card(e.OtherSide, row).Attack, Is.EqualTo(2));
            Assert.That(e.Usage.HasTriggered(e.Pet(row).InstanceId, target.InstanceId), Is.True);
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            var gains = e.Gains();
            Assert.That(gains.Count, Is.EqualTo(1));
            Assert.That(gains[0].TargetInstanceId, Is.EqualTo(target.InstanceId));
            Assert.That(gains[0].TargetPosition, Is.EqualTo(e.Position(side, row)));
            Assert.That(gains[0].PreviousAttack, Is.EqualTo(2));
            Assert.That(gains[0].CurrentAttack, Is.EqualTo(4));
            Assert.That(gains[0].ActualGainedAmount, Is.EqualTo(2));
            Assert.That(gains[0].Metadata.ParentEventId.Value, Is.EqualTo(source.Metadata.EventId));
            Assert.That(gains[0].Metadata.TriggerRootId, Is.EqualTo(e.Root.Metadata.TriggerRootId));
            Assert.That(gains[0].Metadata.SequenceNo.Value, Is.GreaterThan(source.Metadata.SequenceNo.Value));
            Assert.That(handler.CanTrigger(e.State, source), Is.False);
        }

        [TestCase(-2, -1, false)]
        [TestCase(-2, 0, false)]
        [TestCase(-2, 1, true)]
        [TestCase(-2, 3, true)]
        [TestCase(0, 1, true)]
        [TestCase(1, 2, false)]
        public void HpGain_RequiresActualThresholdCrossing(int previousHp, int currentHp, bool expected)
        {
            var e = new Environment();
            var target = e.Card(e.Side, BoardRow.Front);
            target.ApplyIncomingDamage(target.CurrentHp - previousHp);
            var hp = e.Hp.TryApplyHeal(e.State, e.Root, e.Position(e.Side, BoardRow.Front), currentHp - previousHp);
            Assert.That(target.CurrentHp, Is.EqualTo(currentHp));
            Assert.That(e.Handler(1).CanTrigger(e.State, hp), Is.EqualTo(expected));
            e.Handler(1).Resolve(e.State, hp);
            Assert.That(target.Attack, Is.EqualTo(expected ? 4 : 2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(expected ? 1 : 0));
            Assert.That(e.Gains().Count, Is.EqualTo(expected ? 1 : 0));
        }

        [TestCase(0, CombatCardSeason.Unspecified)]
        [TestCase(1, CombatCardSeason.Spring)]
        [TestCase(2, CombatCardSeason.Summer)]
        [TestCase(0, CombatCardSeason.Autumn)]
        [TestCase(1, CombatCardSeason.Winter)]
        [TestCase(2, CombatCardSeason.Seasonless)]
        public void RescueBonus_HasNoSeasonRequirement(int path, CombatCardSeason season)
        {
            var e = new Environment(season: season);
            e.Handler(path).Resolve(e.State, e.Restore(path, e.Side, BoardRow.Front));
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void OpposingSideAndOtherRow_AreIgnored(int path)
        {
            var e = new Environment();
            var handler = e.Handler(path);
            var opposite = e.Restore(path, e.OtherSide, BoardRow.Front);
            var lower = e.Restore(path, e.Side, BoardRow.Back);
            Assert.That(handler.CanTrigger(e.State, opposite), Is.False);
            Assert.That(handler.CanTrigger(e.State, lower), Is.False);
            handler.Resolve(e.State, opposite);
            handler.Resolve(e.State, lower);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void RepeatedEvent_DoesNotDuplicateBonus(int path)
        {
            var e = new Environment();
            var source = e.Restore(path, e.Side, BoardRow.Front);
            var handler = e.Handler(path);
            handler.Resolve(e.State, source);
            var count = e.Log.Count;
            handler.Resolve(e.State, source);
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BothNotificationTypes_ShareOneUsageEvenWhenBothWereEligible(bool hpFirst)
        {
            var e = new Environment();
            var rescue = (RescueCombatEvent)e.Restore(0, e.Side, BoardRow.Front);
            var target = e.Card(e.Side, BoardRow.Front);
            // A second semantic notification describing the same transition.
            // It does not apply a second HP operation to the card.
            var hp = new HpGainCombatEvent(e.Metadata.CreateChild(rescue.Metadata), target.InstanceId,
                rescue.Position, target.HpCapacity, target.HpCapacity, rescue.PreviousHp, rescue.CurrentHp);
            e.Log.Append(hp);
            Assert.That(e.Handler(0).CanTrigger(e.State, rescue), Is.True);
            Assert.That(e.Handler(1).CanTrigger(e.State, hp), Is.True);
            if (hpFirst)
            {
                e.Handler(1).Resolve(e.State, hp);
                e.Handler(0).Resolve(e.State, rescue);
            }
            else
            {
                e.Handler(0).Resolve(e.State, rescue);
                e.Handler(1).Resolve(e.State, hp);
            }
            Assert.That(target.Attack, Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            Assert.That(e.Gains()[0].Metadata.ParentEventId.Value,
                Is.EqualTo(hpFirst ? hp.Metadata.EventId : rescue.Metadata.EventId));
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 0)]
        public void LaterRescueOfSameCard_DoesNotResetUsage(int firstPath, int secondPath)
        {
            var e = new Environment();
            e.Handler(firstPath).Resolve(e.State, e.Restore(firstPath, e.Side, BoardRow.Front));
            var second = e.Restore(secondPath, e.Side, BoardRow.Front);
            Assert.That(e.Handler(secondPath).CanTrigger(e.State, second), Is.False);
            e.Handler(secondPath).Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void DifferentCards_ReceiveIndependentBonuses(int path)
        {
            var e = new Environment();
            e.Handler(path).Resolve(e.State, e.Restore(path, e.Side, BoardRow.Front));
            e.Handler(path).Resolve(e.State, e.Restore(path, e.Side, BoardRow.Front, column: 2));
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(e.Card(e.Side, BoardRow.Front, 2).Attack, Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void TargetMovedWithinRow_BonusFollowsInstance(int path)
        {
            var e = new Environment();
            var target = e.Card(e.Side, BoardRow.Front);
            var source = e.Restore(path, e.Side, BoardRow.Front);
            var from = e.Position(e.Side, BoardRow.Front);
            var to = e.Position(e.Side, BoardRow.Front, 5);
            e.State.GetSide(e.Side).MoveCard(from, to);
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Side, BoardRow.Front, 2), from);
            e.Handler(path).Resolve(e.State, source);
            Assert.That(target.Attack, Is.EqualTo(4));
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(2));
            Assert.That(e.Gains()[0].TargetPosition, Is.EqualTo(to));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void TargetMovedToOtherRow_IsReevaluatedAtResolve(int path)
        {
            var e = new Environment();
            var source = e.Restore(path, e.Side, BoardRow.Front);
            Assert.That(e.Handler(path).CanTrigger(e.State, source), Is.True);
            var to = e.Position(e.Side, BoardRow.Back, 5);
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Side, BoardRow.Front), to);
            e.Handler(path).Resolve(e.State, source);
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            e.Handler(path, BoardRow.Back).Resolve(e.State, source);
            Assert.That(e.Card(e.Side, BoardRow.Back, 5).Attack, Is.EqualTo(4));
            Assert.That(e.Gains()[0].TargetPosition, Is.EqualTo(to));
            Assert.That(e.Usage.HasTriggered(e.Pets[1].InstanceId, e.Card(e.Side, BoardRow.Back, 5).InstanceId), Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void RemovedTarget_DoesNotBuffReplacement(int path)
        {
            var e = new Environment();
            var source = e.Restore(path, e.Side, BoardRow.Front);
            var from = e.Position(e.Side, BoardRow.Front);
            e.State.GetSide(e.Side).RemoveCardFromCombat(from);
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Side, BoardRow.Front, 2), from);
            Assert.That(e.Handler(path).CanTrigger(e.State, source), Is.False);
            e.Handler(path).Resolve(e.State, source);
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(2));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            Assert.That(e.Gains(), Is.Empty);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void AttackOverflow_DoesNotConsumeUsageAndAllowsRetry(int path)
        {
            var e = new Environment();
            var target = e.Card(e.Side, BoardRow.Front);
            target.ApplyAttackGain(int.MaxValue - 1 - target.Attack);
            var source = e.Restore(path, e.Side, BoardRow.Front);
            var count = e.Log.Count;
            Assert.Throws<OverflowException>(() => e.Handler(path).Resolve(e.State, source));
            Assert.That(target.Attack, Is.EqualTo(int.MaxValue - 1));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            target.ReduceAttack(int.MaxValue - 3);
            e.Handler(path).Resolve(e.State, source);
            Assert.That(target.Attack, Is.EqualTo(4));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void BonusCanReachMaximumAttackWithoutOverflow(int path)
        {
            var e = new Environment();
            var target = e.Card(e.Side, BoardRow.Front);
            target.ApplyAttackGain(int.MaxValue - 2 - target.Attack);
            e.Handler(path).Resolve(e.State, e.Restore(path, e.Side, BoardRow.Front));
            Assert.That(target.Attack, Is.EqualTo(int.MaxValue));
            Assert.That(e.Gains()[0].ActualGainedAmount, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void DifferentParentObject_IsRejectedWithoutUsage(int path)
        {
            var e = new Environment();
            var source = e.Restore(path, e.Side, BoardRow.Front);
            CombatEvent copy;
            if (source is RescueCombatEvent rescue)
            {
                copy = new RescueCombatEvent(rescue.Metadata, rescue.InstanceId, rescue.Position, rescue.PreviousHp, rescue.CurrentHp);
            }
            else
            {
                var hp = (HpGainCombatEvent)source;
                copy = new HpGainCombatEvent(hp.Metadata, hp.SourceInstanceId, hp.TargetInstanceId, hp.TargetPosition,
                    hp.PreviousHpCapacity, hp.CurrentHpCapacity, hp.PreviousHp, hp.CurrentHp);
            }
            var count = e.Log.Count;
            Assert.Throws<ArgumentException>(() => e.Handler(path).Resolve(e.State, copy));
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(2));
            Assert.That(e.Log.Count, Is.EqualTo(count));
            Assert.That(e.Usage.UsageRegistry.Count, Is.Zero);
            e.Handler(path).Resolve(e.State, source);
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void RescuedCardFallsToThresholdAgain_QueuedBonusRemainsEligible(int path)
        {
            var e = new Environment();
            var source = e.Restore(path, e.Side, BoardRow.Front);
            var target = e.Card(e.Side, BoardRow.Front);
            target.SetCurrentHpToZero();
            e.Handler(path).Resolve(e.State, source);
            Assert.That(target.Attack, Is.EqualTo(4));
            Assert.That(target.CurrentHp, Is.Zero);
        }

        [Test]
        public void TwoPetInstances_HaveIndependentUsageForSameCard()
        {
            var e = new Environment();
            var source = e.Restore(0, e.Side, BoardRow.Front);
            e.Handler(0).Resolve(e.State, source);
            e.State.GetSide(e.Side).MoveCard(e.Position(e.Side, BoardRow.Front), e.Position(e.Side, BoardRow.Back, 5));
            var second = e.Restore(1, e.Side, BoardRow.Back, 5);
            e.Handler(1, BoardRow.Back).Resolve(e.State, second);
            Assert.That(e.Card(e.Side, BoardRow.Back, 5).Attack, Is.EqualTo(6));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void BothHandlersRegistered_EngineAppliesExactlyOneBonus(int path)
        {
            var e = new Environment();
            e.Restore(path, e.Side, BoardRow.Front);
            var queue = new CombatEventQueue(e.Log);
            var registry = new CombatTriggerSourceRegistry(new ICombatTriggerSource[]
            {
                new CombatPetTriggerSource(e.Side, e.Pets[0].InstanceId, e.Handler(0)),
                new CombatPetTriggerSource(e.Side, e.Pets[0].InstanceId, e.Handler(1))
            });
            var engine = new CombatTriggerEngine(e.State, queue, registry);
            engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 20);
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
            Assert.That(e.Usage.UsageRegistry.Count, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(engine.Drain(maximumEventCount: 20, maximumTriggerCountPerEvent: 20), Is.Zero);
        }

        [Test]
        public void ExternalSourceHpStatRescue_IsAlsoEligible()
        {
            var e = new Environment();
            var death = e.Kill(e.Side, BoardRow.Front, 1);
            var hp = e.Hp.TryApplyHpStatGain(e.State, death,
                e.Card(e.Side, BoardRow.Front, 2).InstanceId, e.Position(e.Side, BoardRow.Front), 3);
            Assert.That(hp.IsFromAnotherSource, Is.True);
            e.Handler(2).Resolve(e.State, hp);
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
        }

        [Test]
        public void RebuiltHandlers_KeepSharedUsage()
        {
            var e = new Environment();
            e.Handler(0).Resolve(e.State, e.Restore(0, e.Side, BoardRow.Front));
            var rebuilt = new TapirPetHpGainTriggerHandler(e.Side, e.Pets[0].InstanceId, e.Usage, e.Attack);
            var hp = e.Restore(1, e.Side, BoardRow.Front);
            Assert.That(rebuilt.CanTrigger(e.State, hp), Is.False);
            rebuilt.Resolve(e.State, hp);
            Assert.That(e.Card(e.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(e.Gains().Count, Is.EqualTo(1));
        }

        [Test]
        public void FreshBattleUsage_AllowsSameIdsAgain()
        {
            var first = new Environment();
            first.Handler(0).Resolve(first.State, first.Restore(0, first.Side, BoardRow.Front));
            var second = new Environment();
            second.Handler(1).Resolve(second.State, second.Restore(1, second.Side, BoardRow.Front));
            Assert.That(first.Card(first.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(second.Card(second.Side, BoardRow.Front).Attack, Is.EqualTo(4));
            Assert.That(second.Usage.UsageRegistry.Count, Is.EqualTo(1));
        }

        private sealed class Environment
        {
            public readonly CombatSide Side;
            public readonly CombatSide OtherSide;
            public readonly CombatState State;
            public readonly CombatPetState[] Pets;
            public readonly CombatEventLog Log = new CombatEventLog();
            public readonly CombatEventMetadataFactory Metadata = new CombatEventMetadataFactory(
                new CombatEventIdAllocator(), new CombatSequenceNumberAllocator());
            public readonly CombatStartedCombatEvent Root;
            public readonly CombatAttackGainResolver Attack;
            public readonly CombatHpGainResolver Hp;
            public readonly CombatPetCardTriggerUsageCommitter Usage =
                new CombatPetCardTriggerUsageCommitter(new CombatPetCardTriggerUsageRegistry());
            private readonly ICombatTriggerHandler[] _rescueHandlers;
            private readonly ICombatTriggerHandler[] _hpHandlers;

            public Environment(CombatSide side = CombatSide.Player, CombatCardSeason season = CombatCardSeason.Spring)
            {
                Side = side;
                OtherSide = side == CombatSide.Player ? CombatSide.Enemy : CombatSide.Player;
                Pets = new[]
                {
                    new CombatPetState(new DefinitionId("test.tapir"), new InstanceId(1002)),
                    new CombatPetState(new DefinitionId("test.tapir"), new InstanceId(1001))
                };
                var ownPets = new CombatSidePetState(side, new CombatPetRegistry(Pets));
                var otherPets = new CombatSidePetState(OtherSide, new CombatPetRegistry(Array.Empty<CombatPetState>()));
                State = new CombatState(MakeSide(CombatSide.Player, season), MakeSide(CombatSide.Enemy, season),
                    side == CombatSide.Player ? ownPets : otherPets, side == CombatSide.Enemy ? ownPets : otherPets);
                Root = new CombatStartedCombatEvent(Metadata.CreateRoot());
                Log.Append(Root);
                Attack = new CombatAttackGainResolver(Metadata, Log);
                Hp = new CombatHpGainResolver(Metadata, Log);
                _rescueHandlers = new ICombatTriggerHandler[2];
                _hpHandlers = new ICombatTriggerHandler[2];
                for (var i = 0; i < 2; i++)
                {
                    _rescueHandlers[i] = new TapirPetRescueTriggerHandler(side, Pets[i].InstanceId, Usage, Attack);
                    _hpHandlers[i] = new TapirPetHpGainTriggerHandler(side, Pets[i].InstanceId, Usage, Attack);
                }
            }

            public ICombatTriggerHandler Handler(int path, BoardRow row = BoardRow.Front) =>
                (path == 0 ? _rescueHandlers : _hpHandlers)[row == BoardRow.Front ? 0 : 1];
            public CombatPetState Pet(BoardRow row) => Pets[row == BoardRow.Front ? 0 : 1];
            public BoardPosition Position(CombatSide side, BoardRow row, int column = 1) =>
                new BoardPosition(side, row, new BoardColumn(column));
            public CombatCardState Card(CombatSide side, BoardRow row, int column = 1) => State.GetSide(side).GetCardAt(Position(side, row, column));

            public DeathCombatEvent Kill(CombatSide side, BoardRow row, int column)
            {
                var card = Card(side, row, column);
                var previousHp = card.CurrentHp;
                card.SetCurrentHpToZero();
                card.ApplyIncomingDamage(2);
                var death = new DeathCombatEvent(Metadata.CreateChild(Root.Metadata), card.InstanceId,
                    Position(side, row, column), previousHp, card.CurrentHp);
                Log.Append(death);
                return death;
            }

            public CombatEvent Restore(int path, CombatSide side, BoardRow row, int column = 1)
            {
                var death = Kill(side, row, column);
                if (path == 0) { return new CombatRescueResolver(Metadata, Log).ApplyRescue(State, death); }
                if (path == 1) { return Hp.TryApplyHeal(State, death, Position(side, row, column), 3); }
                return Hp.TryApplyHpStatGain(State, death, Position(side, row, column), 3);
            }

            public List<AttackGainCombatEvent> Gains()
            {
                var events = new List<AttackGainCombatEvent>();
                foreach (var item in Log.Events) { if (item is AttackGainCombatEvent gain) { events.Add(gain); } }
                return events;
            }

            private CombatSideState MakeSide(CombatSide side, CombatCardSeason season)
            {
                var slots = new List<CombatSlotState>();
                var cards = new List<CombatCardState>();
                foreach (var row in new[] { BoardRow.Back, BoardRow.Front })
                {
                    foreach (var column in new[] { 5, 2, 1, 4, 3 })
                    {
                        var id = (side == CombatSide.Player ? 0 : 100) + (row == BoardRow.Front ? 0 : 10) + column;
                        CombatCardState card = null;
                        if (column <= 2)
                        {
                            card = new CombatCardState(new DefinitionId("test.tapir_card"), new InstanceId(id),
                                new CardRank(4), season, hpCapacity: 10, currentHp: 5, armor: 0, attack: 2);
                            cards.Add(card);
                        }
                        slots.Add(new CombatSlotState(new SlotId(id), Position(side, row, column),
                            card == null ? (InstanceId?)null : card.InstanceId));
                    }
                }
                return new CombatSideState(new CombatBoardState(side, slots), new CombatCardRegistry(cards),
                    new BattleHealth(20), new AttackMultiplier(1));
            }
        }
    }
}
