using System;
using System.Collections.Generic;
using System.Linq;

public class SimulationEngineAPI
{
    public Simulation _simulation = new();
    private Dictionary<string, QueueNode> _queues = new();
    private Dictionary<string, QueueNode> _entryPoints = new();
    public Dictionary<string, NetworkStats> _networks = new();
    private List<QueueNode> _allNodes = new();
    private double _untilTime = 1000;
    private int _runCount = 1;

    public void SetSimulationParameters(double untilTime, int runCount)
    {
        _untilTime = untilTime;
        _runCount = runCount;
    }

    public void CreateQueue(string name, int servers, int capacity, Func<double> serviceTime, Func<double>? arrivalTime = null)
    {
        var queue = new QueueNode(this, name, servers, capacity, serviceTime, arrivalTime);
        _queues[name] = queue;
        _allNodes.Add(queue);
        if (arrivalTime != null)
            _entryPoints[name] = queue;
    }

    public void ConnectQueues(string from, string to, double probability = 1.0)
    {
        var fromQueue = _queues[from];
        var toQueue = _queues[to];

        if (fromQueue.NextNodeChoices == null && probability < 1.0)
        {
            fromQueue.NextNodeChoices = new List<(QueueNode, double)> { (toQueue, probability) };
        }
        else if (fromQueue.NextNodeChoices != null)
        {
            fromQueue.NextNodeChoices.Add((toQueue, probability));
        }
        else
        {
            fromQueue.NextNode = toQueue;
        }
    }

    public void RecordNetworkEntry(Entity entity, string networkName, double time)
    {
        if (_networks.TryGetValue(networkName, out var stats))
        {
            stats.RecordEntry(entity, time);
        }
    }

    public void RecordNetworkExit(Entity entity, string networkName, double time)
    {
        if (_networks.TryGetValue(networkName, out var stats))
        {
            stats.RecordExit(entity, time);
        }
    }

    public void RunSimulation()
    {
        foreach (var queue in _allNodes)
        {
            string networkName = queue.Name.Split('.')[0];
            if (!_networks.ContainsKey(networkName))
                _networks[networkName] = new NetworkStats(networkName);

            _networks[networkName].RegisterQueue(queue);
        }

        for (int i = 0; i < _runCount; i++)
        {
            _simulation = new Simulation();
            foreach (var q in _allNodes)
            {
                q.Reset(_simulation);
            }

            foreach (var entry in _entryPoints.Values)
                entry.ScheduleInitialArrival();

            _simulation.Run(_untilTime);
        }
    }

    public Dictionary<string, QueueMetrics> GetMetrics()
    {
        return _queues.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.GetMetrics()
        );
    }

    public Dictionary<string, NetworkMetrics> GetNetworkMetrics()
{
    return _networks.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.GetMetrics()
    );
}

}
