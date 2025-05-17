namespace SimEngine.Nodes;

using SimEngine.Core;

public class RouterNode : Node
{
    private readonly SimulationEngineAPI _engine;
    private Simulation Simulation => _engine._simulation;

    public RouterNode(SimulationEngineAPI engine, string name)
        : base(name)
    {
        _engine = engine;
    }

    public void Route(Entity entity)
    {
        if (NextNode == null && NextNodeChoices == null)
        {
            entity.DepartureTime = Simulation.Now;
            _engine.RecordNetworkExit(entity, entity.CurrentNetworkName, Simulation.Now);
            return;
        }

        Node target = NextNode!;

        if (NextNodeChoices is not null)
        {
            double r = _engine.RandomGenerator.NextDouble();
            double cumulative = 0;
            foreach (var (node, prob) in NextNodeChoices)
            {
                cumulative += prob;
                if (r <= cumulative)
                {
                    target = node;
                    break;
                }
            }
        }

        if (target is QueueNode queue)
        {
            if (entity.CurrentNetworkName != queue.Network)
            {
                _engine.RecordNetworkExit(entity, entity.CurrentNetworkName, Simulation.Now);
                _engine.RecordNetworkEntry(entity, queue.Network, Simulation.Now);
                entity.CurrentNetworkName = queue.Network;
            }

            Simulation.Schedule(0, () => queue.ProcessArrival(entity));
        }
        else if (target is RouterNode router)
        {
            Simulation.Schedule(0, () => router.Route(entity));
        }
    }
}
