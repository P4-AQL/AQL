using System;

public class DispatcherNode : Node
{
    private readonly SimulationEngineAPI _engine;
    private Simulation Simulation => _engine._simulation;
    private readonly Func<double> _arrivalDist;
    private readonly string _network;

    public DispatcherNode(SimulationEngineAPI engine, string name, Func<double> arrivalDist)
        : base(name)
    {
        _engine = engine;
        _arrivalDist = arrivalDist;
        _network = name.Split('.')[0];
    }

    public void ScheduleInitialArrival()
    {
        Simulation.Schedule(_arrivalDist(), () =>
        {
            var entity = new Entity(Simulation.Now)
            {
                CurrentNetworkName = _network
            };

            _engine._entities.Add(entity);
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);

            // Use base's routing logic
            QueueNode target = NextNode!;
            if (NextNodeChoices != null)
            {
                double r = Random.Shared.NextDouble();
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

            Simulation.Schedule(0, () => target.ProcessArrival(entity));

            // Recurse
            ScheduleInitialArrival();
        });
    }
}