using System;
using System.Collections.Generic;

public class NetworkStats
{
    public string Name { get; }
    private int _entered = 0;
    private int _exited = 0;
    private double _totalTimeInNetwork = 0.0;
    private Dictionary<Entity, double> _entryTimes = new();

    public NetworkStats(string name)
    {
        Name = name;
    }

    private readonly List<QueueNode> _queues = new();
    public void RegisterQueue(QueueNode queue) => _queues.Add(queue);

    public void RecordEntry(Entity entity, double currentTime)
    {
        _entryTimes[entity] = currentTime;
        _entered++;

    }

    public void RecordExit(Entity entity, double currentTime)
    {
        if (_entryTimes.TryGetValue(entity, out double entryTime))
        {
            _totalTimeInNetwork += (currentTime - entryTime);
            _entryTimes.Remove(entity);
        }
        _exited++;
    }

    public NetworkMetrics GetMetrics()
    {
        return new NetworkMetrics
        {
            Name = Name,
            Entered = _entered,
            Exited = _exited,
            AvgTimeInNetwork = _exited > 0 ? _totalTimeInNetwork / _exited : 0.0
        };
    }
}

public class NetworkMetrics
{
    public string Name { get; init; } = string.Empty;
    public int Entered { get; init; }
    public int Exited { get; init; }
    public double AvgTimeInNetwork { get; init; }
}