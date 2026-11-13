using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontier.Core
{
    // Shared only by the authoritative simulation. Commands/replays retain their
    // existing format; each selection (including a queued order) gets its own group.
    internal sealed class AttackFormation
    {
        internal Point forward;
        internal readonly Dictionary<int, Point> offsets = new Dictionary<int, Point>();
        internal int contactTick = -1;
        internal bool inContact;
        internal long trafficPriority;
        internal float trafficMass;
    }

    public sealed partial class Simulation
    {
        private static float Projection(Point point, Point axis) => point.x * axis.x + point.y * axis.y;

        private int AttackRank(EntityState unit)
        {
            float range = Type(unit).range;
            return range < 2 ? 0 : range < 4 ? 1 : range < 8 ? 2 : 3;
        }

        private AttackFormation PlanAttackFormation(List<EntityState> selected, Point destination, bool queued, int replacing)
        {
            var army = selected.Where(e => Type(e).role != "worker" && Type(e).damage > 0).ToArray();
            if (army.Length < 2 || army.Select(AttackRank).Distinct().Count() < 2) return null;
            Point Origin(EntityState unit) => queued ? PlanningOrigin(unit, replacing) : unit.position;
            var center = new Point(army.Average(e => Origin(e).x), army.Average(e => Origin(e).y));
            float distance = center.Distance(destination);
            var forward = distance > .01f ? new Point((destination.x-center.x)/distance, (destination.y-center.y)/distance)
                : new Point(selected[0].owner == 0 ? 1 : -1, 0);
            var right = new Point(forward.y, -forward.x);
            var formation = new AttackFormation { forward = forward,
                trafficPriority = ((long)selected.Min(e => e.id) << 32) | ++nextMovementOrder,
                trafficMass = selected.Max(e => Type(e).collisionMass) };
            int width = Math.Max(2, (int)Math.Ceiling(Math.Sqrt(army.Length * 1.5f)));
            float depth = 0, previousRange = 0;
            foreach (var rank in army.GroupBy(AttackRank).OrderBy(g => g.Key))
            {
                float range = rank.Min(e => Type(e).range);
                if (formation.offsets.Count > 0) depth += Math.Max(2, (range-previousRange)*.75f);
                var members = rank.OrderByDescending(e => Projection(Origin(e), forward)).ThenBy(e => e.id).ToArray();
                for (int row = 0; row * width < members.Length; row++)
                {
                    var line = members.Skip(row * width).Take(width)
                        .OrderBy(e => Projection(Origin(e), right)).ThenBy(e => e.id).ToArray();
                    for (int column = 0; column < line.Length; column++)
                    {
                        float side = (column - (line.Length-1)*.5f)*2;
                        float back = depth + row*2;
                        formation.offsets.Add(line[column].id,
                            new Point(right.x*side-forward.x*back, right.y*side-forward.y*back));
                    }
                }
                depth += ((members.Length-1)/width)*2;
                previousRange = range;
            }
            // The clicked point is the army's center, so a deep selection does not
            // park its entire back half short of the requested battlefield.
            var average = new Point(formation.offsets.Values.Average(p => p.x), formation.offsets.Values.Average(p => p.y));
            foreach (int id in formation.offsets.Keys.ToArray())
            {
                var offset = formation.offsets[id];
                formation.offsets[id] = new Point(offset.x-average.x, offset.y-average.y);
            }
            return formation;
        }

        private void ActivateAttackFormation(EntityState unit, AttackFormation formation)
        {
            unit.attackFormation = formation;
            if (formation == null) return;
            unit.trafficPriority = formation.trafficPriority;
            unit.trafficMass = formation.trafficMass;
        }

        private bool FormationMemberMoving(EntityState unit, AttackFormation formation)
        {
            if (unit.attackFormation != formation || unit.health <= 0 || unit.clearingSite != 0
                || (unit.order != CommandKind.Attack && unit.order != CommandKind.AttackMove)
                || unit.yieldUntil > Tick || unit.blockedTicks >= TickRate * 2) return false;
            int target = unit.order == CommandKind.Attack ? unit.target : unit.combatTarget;
            // Contact releases marching cohesion: combatants spread into firing
            // positions at their own range, without waiting for a rear rank that
            // has stopped to shoot or trying to dress ranks under enemy fire.
            return !Entities.TryGetValue(target, out var enemy) || !ValidEnemy(unit, enemy)
                || Center(unit).Distance(Center(enemy)) > Type(unit).vision + (Type(enemy).building ? Type(enemy).footprint*.4f : 0);
        }

        private Point FormationVelocity(EntityState unit, Point desired)
        {
            var formation = unit.attackFormation;
            if (formation == null || !FormationMemberMoving(unit, formation)) return desired;
            if (formation.contactTick != Tick)
            {
                formation.contactTick = Tick;
                formation.inContact = mobiles.Any(ally => ally.attackFormation == formation && ally.health > 0
                    && (ally.order == CommandKind.Attack || ally.order == CommandKind.AttackMove)
                    && Entities.TryGetValue(ally.order == CommandKind.Attack ? ally.target : ally.combatTarget, out var enemy)
                    && ValidEnemy(ally, enemy) && Center(ally).Distance(Center(enemy)) <= Type(ally).vision);
            }
            if (formation.inContact) return desired;
            Point forward = formation.forward;
            float advance = Projection(desired, forward);
            if (advance <= 0) return desired; // Retreating/reforming never waits for the group.
            float progress = Projection(unit.stepOrigin, forward) - Projection(formation.offsets[unit.id], forward);
            float limit = advance;
            foreach (var other in mobiles)
            {
                if (other.id == unit.id || !FormationMemberMoving(other, formation)
                    || unit.stepOrigin.Distance(other.stepOrigin) > 12
                    || !Map.CanTraverse(unit.stepOrigin, other.stepOrigin, Type(unit).collisionRadius)) continue;
                // Follow nearby ranks at their pace. A detached or obstructed unit
                // cannot immobilize the army, and narrow passages can break ranks.
                float otherProgress = Projection(other.stepOrigin, forward) - Projection(formation.offsets[other.id], forward);
                limit = Math.Min(limit, Math.Max(0, Type(other).speed + (otherProgress-progress)*2));
            }
            return new Point(desired.x-forward.x*(advance-limit), desired.y-forward.y*(advance-limit));
        }
    }
}
