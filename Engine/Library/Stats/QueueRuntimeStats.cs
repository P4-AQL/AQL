public class QueueRuntimeStats
{
    public string Name { get; init; } = "";
    public int TotalArrived { get; private set; } = 0;
    public int TotalServed { get; private set; } = 0;
    public double TotalWaitingTime { get; private set; } = 0.0;
    public double TotalServiceTime { get; private set; } = 0.0;
    public double TotalBusyTime { get; private set; } = 0.0;
    public int ServerCount { get; init; } = 1;
    public double SimulationTimePerRun { get; set; } = 1.0;
    public int RunCount { get; private set; } = 0;

    public void AddArrival() => TotalArrived++;
    public void AddServed(double waitingTime, double serviceTime)
    {
        TotalServed++;
        TotalWaitingTime += waitingTime;
        TotalServiceTime += serviceTime;
    }

    public void AddBusyTime(double time) => TotalBusyTime += time;
    public void IncrementRun() => RunCount++;

    public double AvgWaitTime => TotalServed > 0 ? TotalWaitingTime / TotalServed : 0;
    public double AvgServiceTime => TotalServed > 0 ? TotalServiceTime / TotalServed : 0;
    public double Utilization => (RunCount * SimulationTimePerRun * ServerCount) > 0 ? TotalBusyTime / (RunCount * SimulationTimePerRun * ServerCount) : 0;
    public double Throughput => RunCount > 0 ? (double)TotalServed / RunCount : 0;
}
