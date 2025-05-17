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

            if (entity.NetworkStack.TryPeek(out var exitNetwork))
                _engine.RecordNetworkExit(entity, exitNetwork, Simulation.Now);
                
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
            if (!entity.NetworkStack.TryPeek(out var current) || current != queue.Network)
            {
                if (entity.NetworkStack.TryPeek(out var exitNetwork))
                    _engine.RecordNetworkExit(entity, exitNetwork, Simulation.Now);

                _engine.RecordNetworkEntry(entity, queue.Network, Simulation.Now);
            }

            Simulation.Schedule(0, () => queue.ProcessArrival(entity));
        }
        else if (target is RouterNode router)
        {
            Simulation.Schedule(0, () => router.Route(entity));
        }
    }
}
