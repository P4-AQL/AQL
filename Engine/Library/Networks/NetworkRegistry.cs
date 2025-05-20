namespace SimEngine.Networks;

using System.Collections.Generic;

using SimEngine.Core;
public class NetworkStats
{
    public string Name { get; }
    private Dictionary<Entity, double> _entryTimes = new();

    public NetworkStats(string name)
    {
        Name = name;
    }

    private readonly List<SimEngine.Nodes.QueueNode> _queues = new();
    public void RegisterQueue(SimEngine.Nodes.QueueNode queue) => _queues.Add(queue);

    public void RecordEntry(Entity entity, double currentTime)
    {
        _entryTimes[entity] = currentTime;
    }

    public void RecordExit(Entity entity, double currentTime)
    {
        _entryTimes.Remove(entity);
    }
}