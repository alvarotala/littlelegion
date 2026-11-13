using Frontier.Core;

static class AttackFormationChecks
{
    public static void Run(Catalog catalog, Action<bool, string> check)
    {
        foreach (string faction in new[] { "industrial", "biological" })
        {
            foreach (var forward in new[] { new Point(1,0), new Point(-1,0), new Point(0,1), new Point(.6f,.8f) })
            {
                var sim = Fixture(catalog, faction);
                var army = Army(sim, new Point(40.5f,40.5f), forward);
                var goal = Add(new Point(40.5f,40.5f), forward, 25);
                Submit(sim, army, CommandKind.AttackMove, goal);
                var melee = army.Where(e => sim.Type(e).role == "melee").ToArray();
                var ranged = army.Where(e => sim.Type(e).role == "ranged").ToArray();
                var siege = army.Where(e => sim.Type(e).role == "siege").ToArray();
                check(melee.Min(e => Dot(e.destination,forward)) > ranged.Max(e => Dot(e.destination,forward))
                    && ranged.Min(e => Dot(e.destination,forward)) > siege.Max(e => Dot(e.destination,forward)),
                    faction + ": attack destinations put melee, shooters and artillery in facing-relative ranks " + forward.x + "," + forward.y);
                Steps(sim, 200);
                check(melee.Average(e => Dot(e.position,forward)) > ranged.Average(e => Dot(e.position,forward)) + 1
                    && ranged.Average(e => Dot(e.position,forward)) > siege.Average(e => Dot(e.position,forward)) + 1,
                    faction + ": initially reversed ranks reform during the march, before arrival (" + string.Join("; ",army.Select(e=>$"{sim.Type(e).role}: {e.position.x:F1},{e.position.y:F1} blocked={e.blockedTicks}")) + ")");
                bool physical = true;
                for (int tick = 0; tick < 650; tick++)
                {
                    var before = army.Select(e => e.position).ToArray();
                    sim.Step();
                    for (int i = 0; i < army.Length; i++)
                    {
                        physical &= before[i].Distance(army[i].position) <= sim.Type(army[i]).speed/Simulation.TickRate+.002f;
                        physical &= sim.Map.CanStand(army[i].position,sim.Type(army[i]).collisionRadius);
                        for (int j = i+1; j < army.Length; j++)
                            physical &= army[i].position.Distance(army[j].position)+.001f >= sim.Type(army[i]).collisionRadius+sim.Type(army[j]).collisionRadius;
                    }
                }
                check(physical && army.All(e => e.order == CommandKind.Stop && e.position.Distance(e.destination) < .15f),
                    faction + ": mixed army arrives without overlaps, teleports or stranded ranks");
            }

            foreach (bool queued in new[] { false, true })
            {
                var sim = Fixture(catalog, faction);
                var army = Army(sim,new Point(20.5f,40.5f),new Point(1,0));
                var target = Unit(sim,"supply",new Point(54,39),1);
                target.health = 100000;
                Unit(sim,"worker",new Point(54.5f,44.5f)); // Own vision for an explicit attack.
                Steps(sim,4);
                if (queued) Submit(sim,army,CommandKind.Move,new Point(28.5f,40.5f));
                Submit(sim,army,CommandKind.Attack,target:target.id,append:queued);
                Steps(sim,600);
                check(army.All(e => e.lastShotTick > 0) && army.All(e => e.target == target.id),
                    faction + ": every rank reaches weapon range and preserves explicit focus, queued=" + queued + " ("+string.Join("; ",army.Select(e=>$"{sim.Type(e).role}: {e.position.x:F1},{e.position.y:F1} shot={e.lastShotTick} order={e.order} target={e.target}"))+")");
                check(army.Where(e=>sim.Type(e).role=="melee").Average(e=>e.position.x)
                    > army.Where(e=>sim.Type(e).role=="ranged").Average(e=>e.position.x)
                    && army.Where(e=>sim.Type(e).role=="ranged").Average(e=>e.position.x)
                    > army.Where(e=>sim.Type(e).role=="siege").Average(e=>e.position.x),
                    faction + ": direct combat keeps short range in front and artillery behind");
                Submit(sim,army,CommandKind.Move,new Point(10.5f,40.5f));
                check(army.All(e => e.attackFormation == null),faction + ": manual movement immediately releases formation pacing");
            }

            {
                var sim = Fixture(catalog,faction);
                var army = Army(sim,new Point(30.5f,30.5f),new Point(1,0));
                Submit(sim,army,CommandKind.Move,new Point(45.5f,30.5f),sequence:1);
                Submit(sim,army,CommandKind.AttackMove,new Point(45.5f,55.5f),append:true,sequence:2);
                check(army.All(e => e.pendingOrders.Count==1 && e.pendingOrders[0].attackFormation!=null)
                    && army.Where(e=>sim.Type(e).role=="melee").Min(e=>e.pendingOrders[0].position.y)
                    > army.Where(e=>sim.Type(e).role=="siege").Max(e=>e.pendingOrders[0].position.y),
                    faction + ": queued formation faces the next leg from the previous waypoint");
                Steps(sim,1000);
                check(army.All(e=>e.pendingOrders.Count==0 && e.order==CommandKind.Stop && e.position.Distance(e.destination)<.15f),
                    faction + ": queued formation finishes after turning");
                Submit(sim,army,CommandKind.Move,new Point(70.5f,55.5f),sequence:3);
                Submit(sim,army,CommandKind.AttackMove,new Point(70.5f,55.5f),sequence:4,replace:3);
                check(army.All(e=>e.order==CommandKind.AttackMove && e.attackFormation!=null && e.pendingOrders.Count==0),
                    faction + ": double-tap replacement creates one shared attack formation");
            }

            {
                var sim = Fixture(catalog,faction);
                var army = Army(sim,new Point(30.5f,40.5f),new Point(1,0));
                Submit(sim,army,CommandKind.AttackMove,new Point(65.5f,40.5f));
                foreach (var unit in army.Where(e=>sim.Type(e).role=="melee")) unit.health=0;
                Steps(sim,700);
                check(army.Where(e=>e.health>0).All(e=>e.order==CommandKind.Stop && e.position.Distance(e.destination)<.15f),
                    faction + ": losing the front rank does not immobilize surviving shooters");
            }

            foreach (bool corner in new[] { false, true })
            {
                var sim = Fixture(catalog,faction);
                var army = Enumerable.Range(0,60).Select(i=>Unit(sim,new[]{"melee","scout","ranged","caster","siege"}[i%5],
                    new Point(20.5f+(i/10)*2,30.5f+(i%10)*2))).ToArray();
                if (!corner)
                    for(int y=0;y<sim.Map.Height;y++) if(y<37||y>42) sim.Map.Occupant[sim.Map.Index(48,y)]=9999;
                Submit(sim,army,CommandKind.AttackMove,corner?new Point(1.5f,1.5f):new Point(72.5f,40.5f));
                Steps(sim,2400);
                check(army.All(e=>e.order==CommandKind.Stop && e.position.Distance(e.destination)<.15f),
                    faction + ": full mixed army navigates " + (corner?"a map corner":"a narrow passage") + " without waiting forever for its formation ("
                    + army.Count(e=>e.order==CommandKind.Stop) + "/60, max distance=" + army.Max(e=>e.position.Distance(e.destination)) + "; "
                    + string.Join("; ",army.Where(e=>e.position.Distance(e.destination)>.15f).Select(e=>$"{e.id}: {e.position.x:F1},{e.position.y:F1} goal={e.destination.x},{e.destination.y}")) + ")");
            }
        }
    }

    private static float Dot(Point p,Point axis) => p.x*axis.x+p.y*axis.y;
    private static Point Add(Point p,Point axis,float distance) => new Point(p.x+axis.x*distance,p.y+axis.y*distance);
    private static void Steps(Simulation sim,int ticks) { for(int i=0;i<ticks;i++)sim.Step(); }
    private static void Submit(Simulation sim,EntityState[] units,CommandKind kind,Point point=default,int target=0,bool append=false,int sequence=0,int replace=0)
    {
        var result=sim.Submit(new GameCommand{player=0,entities=units.Select(e=>e.id).ToArray(),kind=kind,position=point,
            target=target,append=append,sequence=sequence,replacePendingSequence=replace});
        if(!result.accepted)throw new Exception("Formation fixture: "+result.message);
    }
    private static EntityState[] Army(Simulation sim,Point center,Point forward)
    {
        var side=new Point(-forward.y,forward.x);
        // Start artillery ahead of shooters ahead of melee: arrival-only sorting
        // would leave the army in the wrong order through most of this journey.
        return Enumerable.Range(0,9).Select(i=>Unit(sim,new[]{"melee","ranged","siege"}[i/3],
            Add(Add(center,forward,(i/3-1)*2),side,(i%3-1)*3))).ToArray();
    }
    private static EntityState Unit(Simulation sim,string role,Point position,int owner=0)
    {
        var type=sim.Catalog.Role(sim.Players[owner].faction,role);
        var unit=new EntityState{id=sim.Entities.Keys.Max()+1,owner=owner,definition=type.id,position=position,health=type.health,order=CommandKind.Hold};
        sim.Entities.Add(unit.id,unit);
        if(type.building)sim.Map.Occupy(position,type.footprint,unit.id);
        return unit;
    }
    private static Simulation Fixture(Catalog catalog,string faction)
    {
        var sim=new Simulation(new MapDefinition{id="formations",title="Formations",width=96,height=96,
            spawns=new[]{new SpawnDefinition{player=0,x=6,y=6},new SpawnDefinition{player=1,x=86,y=86}},
            resources=new[]{new ResourceDefinition{id="a",x=12,y=12},new ResourceDefinition{id="b",x=82,y=82},
                new ResourceDefinition{id="c",kind="catalyst",x=14,y=12},new ResourceDefinition{id="d",kind="catalyst",x=80,y=82}}},catalog,faction,faction);
        foreach(var unit in sim.Entities.Values.Where(e=>!sim.Type(e).building).ToArray())sim.Entities.Remove(unit.id);
        return sim;
    }
}
