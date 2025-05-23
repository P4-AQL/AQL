namespace SimEngine.Nodes;

using System;

using SimEngine.Core;
public class DispatcherNode : Node
{
    private readonly SimulationEngineAPI _engine;
    private Simulation Simulation => _engine._simulation;
    private readonly Func<double> _arrivalDist;

    public DispatcherNode(SimulationEngineAPI engine, string name, Func<double> arrivalDist)
        : base(name)
    {
        _engine = engine;
        _arrivalDist = arrivalDist;
    }

    public void ScheduleInitialArrival()
    {
        Simulation.Schedule(_arrivalDist(), () =>
        {
            var entity = new Entity(Simulation.Now);
            _engine.RegisterEntity(entity);
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);

            Node target = NextNode!;
            if (NextNodeChoices != null)
            {
                double r = _engine.RandomGenerator.NextDouble();
                double cumulative = 0;
                Console.WriteLine($"DispatcherNode {Name} - Random number: {r}");
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
                Simulation.Schedule(0, () => queue.ProcessArrival(entity));
            }
            else if (target is RouterNode router)
            {
                Simulation.Schedule(0, () => router.Route(entity));
            }


            ScheduleInitialArrival();
        });
    }
}