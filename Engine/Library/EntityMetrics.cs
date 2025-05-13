public class EntityMetrics
{
    public int Entered { get; init; } = 0;
    public int Exited { get; init; } = 0;
    public double AvgTimeInNetwork { get; init; } = 0.0;
    public double AvgWaitTime { get; init; } = 0.0;
    public double AvgServiceTime { get; init; } = 0.0;
}