using Frontier.Core;

static class BalanceChecks
{
    public static void Run(Catalog catalog,Action<bool,string> check)
    {
        foreach(string faction in new[]{"industrial","biological"})
        {
            foreach(string role in new[]{"siege","caster"})
            {
                var sim=Fixture(catalog,faction);sim.Players[0].minerals=10000;sim.Players[0].catalyst=10000;
                var type=catalog.Role(faction,role);var producer=Add(sim,0,type.producer,new Point(20,20));
                for(int i=0;i<6;i++)Add(sim,0,"supply",new Point(30+i*3,4));
                for(int i=0;i<97;i++)Add(sim,0,"worker",new Point(30.5f+i%12,15.5f+i/12));
                bool Train()=>sim.Submit(Order(producer,CommandKind.Train,type.id)).accepted;
                check(sim.Capacity(0)==100&&Train()&&sim.Reserved(0)==2&&sim.View(0).supply==99,
                    faction+" balance: "+role+" reserves two of 100 population points");
                int minerals=sim.Players[0].minerals,gas=sim.Players[0].catalyst;
                check(!Train()&&sim.Players[0].minerals==minerals&&sim.Players[0].catalyst==gas,
                    faction+" balance: one free point cannot buy a two-point "+role);
                check(sim.Submit(Order(producer,CommandKind.Cancel)).accepted&&sim.Reserved(0)==0&&Train(),
                    faction+" balance: cancellation releases both points");
                var supply=sim.Entities.Values.First(e=>e.owner==0&&sim.Type(e).role=="supply");supply.complete=false;
                var lostWorkers=sim.Entities.Values.Where(e=>e.owner==0&&sim.Type(e).role=="worker").Take(13).ToArray();
                foreach(var worker in lostWorkers)sim.Entities.Remove(worker.id);
                producer.queue[0].remaining=0;sim.Step();
                check(sim.Population(0)==84&&sim.Capacity(0)==85&&producer.queue.Count==1,
                    faction+" balance: one free point after supply loss cannot deploy a finished "+role);
                foreach(var worker in lostWorkers)sim.Entities.Add(worker.id,worker);
                supply.complete=true;sim.Step();
                check(sim.Population(0)==99&&sim.Units(0)==98&&sim.Reserved(0)==0&&sim.View(0).supply==99,
                    faction+" balance: deployment replaces the reservation with two live points");
                check(sim.View(1).supply==0,faction+" balance: population accounting is owner-specific");
            }

            foreach(string role in new[]{"scout","ranged","siege"})foreach(int attack in new[]{0,1,3})foreach(int armor in new[]{0,1,3})
            {
                var sim=Fixture(catalog,faction);
                var attacker=Add(sim,0,role,new Point(20.5f,20.5f));var target=Add(sim,1,"melee",new Point(22.5f,20.5f));
                sim.Players[0].attackLevel=attack;sim.Players[1].armorLevel=armor;
                Steps(sim,4);float health=target.health;attacker.attackReadyTick=0;sim.Step();
                float expected=sim.Type(attacker).damage*(1+.12f*attack)*(1-.08f*armor);
                check(Math.Abs(health-target.health-expected)<.01f,$"{faction} balance: {role} actual weapon hit at upgrades {attack}/{armor}");
            }
            {
                var sim=Fixture(catalog,faction);var gun=Add(sim,0,"siege",new Point(20.5f,20.5f));
                var building=Add(sim,1,"supply",new Point(25,20));var nearby=Add(sim,1,"melee",new Point(26.5f,22.5f));
                sim.Players[0].attackLevel=3;sim.Players[1].armorLevel=3;Steps(sim,4);
                check(sim.Submit(new GameCommand{player=0,kind=CommandKind.Attack,entities=new[]{gun.id},target=building.id}).accepted,
                    faction+" balance: artillery can target an enemy building");
                gun.attackReadyTick=0;float buildingHealth=building.health,unitHealth=nearby.health;sim.Step();
                check(Math.Abs(buildingHealth-building.health-93.024f)<.02f&&Math.Abs(unitHealth-nearby.health-15.504f)<.02f,
                    faction+" balance: building bonus affects the direct hit, not nearby units; armor protects both");
            }
        }
        foreach(string kind in new[]{"mineral","catalyst"})
        {
            int optimal=Harvesting.OptimalWorkers(kind);
            float normal=Harvest(kind,optimal),extra=Harvest(kind,optimal*2),crowded=Harvest(kind,optimal*4);
            check(normal>0&&extra>normal&&extra<=normal*1.3f&&Math.Abs(extra-crowded)<=2,
                $"Balance: real {kind} extraction has diminishing returns ({normal}, {extra}, {crowded})");
        }

        int Harvest(string kind,int count)
        {
            var sim=Fixture(catalog,"industrial");var resource=sim.Resources.First(r=>r.kind==kind);
            if(kind=="catalyst")Add(sim,0,"extractor",new Point(resource.position.x-1.5f,resource.position.y-1.5f));
            var workers=Enumerable.Range(0,count).Select(i=>Add(sim,0,"worker",new Point(resource.position.x+1.2f,resource.position.y))).ToArray();
            Steps(sim,4);int before=resource.remaining;
            foreach(var worker in workers){worker.order=CommandKind.Gather;worker.resourceId=worker.target=resource.id;}
            // Hold the geometry/cargo constant to isolate extraction from travel and traffic.
            for(int tick=0;tick<640;tick++)
            {
                foreach(var worker in workers){worker.cargo=0;worker.depositing=false;worker.velocity=default;worker.yieldUntil=0;worker.position=new Point(resource.position.x+1.2f,resource.position.y);}
                sim.Step();
            }
            return before-resource.remaining;
        }
    }
    private static GameCommand Order(EntityState entity,CommandKind kind,string definition=null)
        =>new GameCommand{player=entity.owner,kind=kind,entities=new[]{entity.id},definition=definition};
    private static void Steps(Simulation sim,int count){for(int i=0;i<count;i++)sim.Step();}
    private static EntityState Add(Simulation sim,int owner,string role,Point point)
    {
        var type=sim.Catalog.Role(sim.Players[owner].faction,role);int id=sim.Entities.Keys.Max()+100;
        var entity=new EntityState{id=id,owner=owner,definition=type.id,position=point,health=type.health,energy=type.energyMax,order=CommandKind.Hold,attackReadyTick=int.MaxValue};
        sim.Entities.Add(id,entity);if(type.building)sim.Map.Occupy(point,type.footprint,id);return entity;
    }
    private static Simulation Fixture(Catalog catalog,string faction)
    {
        var map=new MapDefinition{id="balance-checks",title="Balance checks",width=80,height=48,
            spawns=new[]{new SpawnDefinition{player=0,x=2,y=2},new SpawnDefinition{player=1,x=72,y=40}},
            resources=new[]{new ResourceDefinition{id="a",x=10,y=12},new ResourceDefinition{id="b",x=69,y=35},
                new ResourceDefinition{id="c",kind="catalyst",x=8,y=8},new ResourceDefinition{id="d",kind="catalyst",x=74,y=35}}};
        var sim=new Simulation(map,catalog,faction,faction=="industrial"?"biological":"industrial");
        foreach(var entity in sim.Entities.Values.Where(e=>!sim.Type(e).building).ToArray())sim.Entities.Remove(entity.id);
        return sim;
    }
}
