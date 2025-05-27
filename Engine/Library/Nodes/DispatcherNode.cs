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
        double lamda = 1 / _arrivalDist();
        double arrivalTime = GetArrivalExponetialDistribution(lamda, _engine.RandomGenerator);

        Simulation.Schedule(arrivalTime, () =>
        {
            var entity = new Entity(Simulation.Now);
            _engine.RegisterEntity(entity);
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);

            Node? target = ChooseProbabilisticNode(NextNodeChoices, _engine, NextNode);

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

    public static double GetArrivalExponetialDistribution(double lamda, Random random)
    {
        if (lamda <= 0)
        {
            throw new ArgumentException("Lambda must be greater than zero for exponential distribution.");
        }
        return -Math.Log(1.0 - random.NextDouble()) / lamda;
    }
}
