namespace SimEngine.Nodes;

using System;

using SimEngine.Core;
public class DispatcherNode : Node
{
    private readonly SimulationEngineAPI _engine;
    private Simulation Simulation => _engine._simulation;
    private readonly Func<double> _arrivalDist;

    public DispatcherNode(SimulationEngineAPI engine, string name, Func<double> arrivalDist, string networkName)
        : base(name)
    {
        _engine = engine;
        _arrivalDist = arrivalDist;
        _network = networkName;
    }

    public void ScheduleInitialArrival()
    {
        double delay = _arrivalDist();
        //Console.WriteLine($"[{Simulation.Now:F2}] Dispatcher '{Name}' scheduling next arrival in {delay:F2} time units");

        Simulation.Schedule(_arrivalDist(), () =>
        {
            var entity = new Entity(Simulation.Now);
            //Console.WriteLine($"[{Simulation.Now:F2}] Dispatcher '{Name}' creating entity");

            _engine.RegisterEntity(entity);

            //Console.WriteLine($"[{Simulation.Now:F2}] Dispatcher '{Name}' recording network entry into '{_network}'");
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);

            Node target = NextNode!;
            if (NextNodeChoices != null)
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

            //Console.WriteLine($"[{Simulation.Now:F2}] Dispatcher '{Name}' sending entity to: {target.Name}");

            if (target is QueueNode queue)
            {
                Simulation.Schedule(0, () => queue.ProcessArrival(entity));
            }
            else if (target is RouterNode router)
            {
                Simulation.Schedule(0, () => router.Route(entity));
            }
            else
            {
                Console.WriteLine($"[{Simulation.Now:F2}] Dispatcher '{Name}' ERROR: Target node '{target.Name}' is of unhandled type: {target.GetType()}");
            }

            ScheduleInitialArrival();
        });
    }
}