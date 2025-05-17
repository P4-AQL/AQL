namespace SimEngine.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public class Entity
{
    public double ArrivalTime { get; set; }
    public readonly double CreationTime;
    public List<double> WaitingTimesInQueues { get; set; } = new();
    public List<double> ServiceTimesInQueues { get; set; } = new();
    public double DepartureTime { get; set; }
    public double TotalWaitingTime => WaitingTimesInQueues.Sum();
    public double TotalServiceTime => ServiceTimesInQueues.Sum();
    public double TotalTime => DepartureTime - CreationTime;
    public Stack<string> NetworkStack { get; } = new();
    public Dictionary<string, double> NetworkEntryTimes { get; } = new();


    public Entity(double creationTime)
    {
        CreationTime = creationTime;
        ArrivalTime = creationTime;
    }
}
