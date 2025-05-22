namespace SimEngine.Metrics;

using System.Collections.Generic;


public class SimulationStats
{
    public double TotalSimulationTime { get; private set; } = 0;
    public int TotalEntitiesCreated { get; private set; } = 0;
    public List<NetworkRuntimeStats> NetworkStats { get; } = new();
    public List<QueueRuntimeStats> QueueStats { get; } = new();
    private readonly List<double> _simulationRunTimes = new();

    public void AddEntityCreated() => TotalEntitiesCreated++;

    public void AddSimulationRunTime(double time)
    {
        TotalSimulationTime += time;
        _simulationRunTimes.Add(time);
    }

    public IReadOnlyList<double> SimulationRunTimes => _simulationRunTimes;
    public int RunCount => _simulationRunTimes.Count;

    public double ArrivalRate => TotalSimulationTime > 0 ? TotalEntitiesCreated / TotalSimulationTime : 0.0;
    public double MeanInterarrivalTime => ArrivalRate > 0 ? 1.0 / ArrivalRate : 0.0;
}
