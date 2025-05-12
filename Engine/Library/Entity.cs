using System.Collections.Generic;

public class Entity
{
    public double ArrivalTime { get; set; }
    public readonly double CreationTime;
    public List<double> WaitingTimesInQueues { get; set; } = new();
    public List<double> ServiceTimesInQueues { get; set; } = new();

    public string CurrentNetworkName { get; set; } = "";

    public Entity(double creationTime)
    {
        CreationTime = creationTime;
        ArrivalTime = creationTime;
    }
}