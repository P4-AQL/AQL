using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.VisualBasic;

public class SimulationEngineAPI
{
    public Simulation _simulation = new();
    private Dictionary<string, QueueNode> _queues = new();
    private Dictionary<string, DispatcherNode> _dispatchers = new();
    public Dictionary<string, NetworkStats> _networks = new();
    public List<Entity> _entities = new();
    private List<QueueNode> _allNodes = new();
    private Dictionary<string, Engine.Node> _nodes = new();
    private double _untilTime = 1000;
    private int _runCount = 1;

    public void SetSimulationParameters(double untilTime, int runCount)
    {
        _untilTime = untilTime;
        _runCount = runCount;
    }

    public void CreateDispatcherNode(string name, Func<double> arrivalDist) {
        var dispatcher = new DispatcherNode(this, name, arrivalDist);
        _nodes.Add(name, dispatcher);
        _dispatchers[name] = dispatcher;
    }
    public void CreateQueueNode(string name, int servers, int capacity, Func<double> serviceTime, Func<double>? arrivalTime = null)
    {
        var queue = new QueueNode(this, name, servers, capacity, serviceTime, arrivalTime);
        _nodes.Add(name, queue);
        _queues[name] = queue;
        _allNodes.Add(queue);
    }

    public void ConnectNode(string from, string to, double probability = 1.0)
    {
        if (!_nodes.TryGetValue(from, out var fromNode))
            throw new ArgumentException($"Node '{from}' not found.");

        if (!_queues.TryGetValue(to, out var toQueue))
            throw new ArgumentException($"Queue '{to}' not found.");

        if (fromNode.NextNodeChoices == null && probability < 1.0)
        {
            fromNode.NextNodeChoices = new List<(QueueNode, double)> { (toQueue, probability) };
        }
        else if (fromNode.NextNodeChoices != null)
        {
            fromNode.NextNodeChoices.Add((toQueue, probability));
        }
        else
        {
            fromNode.NextNode = toQueue;
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

            foreach (var dispatcher in _dispatchers)
                dispatcher.Value.ScheduleInitialArrival();

            _simulation.Run(_untilTime);

            foreach (var q in _allNodes)
            {
                q.Reset(_simulation);
            }
        }
    }

    public EntityMetrics GetEntityMetrics()  
    {   
        var exited = _entities.Where(e => e.DepartureTime > 0).ToList();    

        return new EntityMetrics    
        {   
            Entered = _entities.Count,  
            Exited = exited.Count,  
            AvgTimeInNetwork = exited.Count > 0 ? exited.Average(e => e.TotalTime) : 0.0,   
            AvgWaitTime = exited.Count > 0 ? exited.Average(e => e.TotalWaitingTime) : 0.0, 
            AvgServiceTime = exited.Count > 0 ? exited.Average(e => e.TotalServiceTime) : 0.0   
        };  
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

    public List<Entity> GetEntities()
    {
        return _entities;
    }

}