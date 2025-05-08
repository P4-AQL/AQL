public class Entity
{
    public double ArrivalTime { get; set; }
    public readonly double CreationTime;

    public Entity(double creationTime)
    {
        CreationTime = creationTime;
        ArrivalTime = creationTime;
    }
}


public class QueueMetrics
{
    public int TotalArrived { get; init; }
    public int TotalServed { get; init; }
    public double AvgWaitTime { get; init; }
    public int MaxQueueLength { get; init; }
    public double ServerUtilization { get; init; } // Fraction between 0.0 and 1.0
    public double Throughput => TotalServed; // Serves as total number of completions
}
