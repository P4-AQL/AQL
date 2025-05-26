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
        string currentNetwork = entity.NetworkStack.TryPeek(out var top) ? top : string.Empty;
        _engine.TransitionNetwork(entity, currentNetwork, _network, Simulation.Now);

        // Routing to next node
        var totalProb = 0.0;
        if (NextNodeChoices is not null)
        {
            foreach ((Node node, double prob) in NextNodeChoices)
            {
                totalProb += prob;
            }
        }

        Node? target = NextNode;
        if (NextNodeChoices is not null)
        {
            double r = _engine.RandomGenerator.NextDouble() * totalProb;
            double cumulative = 0;
            foreach ((Node node, double prob) in NextNodeChoices)
            {
                cumulative += prob;
                if (r <= cumulative)
                {
                    target = node;
                    break;
                }
            }
        }
        if (target != null)
            Simulation.Schedule(0, () => RouteTo(target, entity));

    }

    private void RouteTo(Node target, Entity entity)
    {
        if (target == null) return;

        string nextNetwork = target.Network;
        string currentNetwork = entity.NetworkStack.TryPeek(out var top) ? top : string.Empty;
        _engine.TransitionNetwork(entity, currentNetwork, nextNetwork, Simulation.Now);

        if (target is QueueNode queue)
            queue.ProcessArrival(entity);
        else if (target is RouterNode router)
            router.Route(entity);
    }
}