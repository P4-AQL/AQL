

public class QueueMetrics
{
    public int TotalArrived { get; init; }
    public int TotalServed { get; init; }
    public double AvgWaitTime { get; init; }
    public int MaxQueueLength { get; init; }
    public double ServerUtilization { get; init; }
    public double Throughput { get; init; }
}