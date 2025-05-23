namespace SimEngine.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using SimEngine.Nodes;
using SimEngine.Networks;
using SimEngine.Metrics;
using SimEngine.Utils;


public class SimulationEngineAPI
{
    public Simulation _simulation = new();
    public Random RandomGenerator { get; private set; } = new();
    private Dictionary<string, QueueNode> _queues = new();
    private Dictionary<string, DispatcherNode> _dispatchers = new();
    public Dictionary<string, NetworkStats> _networks = new();
    public List<Entity> _entities = new();
    private List<QueueNode> _allNodes = new();
    private Dictionary<string, Node> _nodes = new();
    private double _untilTime = 1000;
    private int _runCount = 1;
    private readonly HashSet<string> _validNetworkNames = new();
    public SimulationStats Stats { get; private set; } = new();

    public QueueNode GetQueueNode(string name) => _queues[name];

    public void PrintMetric(SimulationStats stats) => MetricsPrinter.Print(stats);

    public void SetSimulationParameters(double untilTime, int runCount)
    {
        _untilTime = untilTime;
        _runCount = runCount;
    }

    public void SetSeed(int seed)
    {
        RandomGenerator = new Random(seed);
    }

    public void CreateNetwork(NetworkDefinition network, string prefix = "")
    {
        _validNetworkNames.Add(network.FullName);

        // Register queues
        foreach (var (name, servers, capacity, serviceTime) in network.Queues)
        {
            CreateQueueNode($"{network.FullName}.{name}", servers, capacity, serviceTime);
        }

        // Register router entry/exit points
        foreach (var entry in network.RouterEntries)
        {
            var nodeName = $"{network.FullName}.{entry}";
            var node = new RouterNode(this, nodeName);
            _nodes[nodeName] = node;
        }


        foreach (var exit in network.RouterExits)
        {
            var node = new RouterNode(this, $"{network.FullName}.{exit}");
            _nodes[$"{network.FullName}.{exit}"] = node;
        }

        // Recursively register sub-networks
        foreach (var sub in network.SubNetworks)
        {
            CreateNetwork(sub, string.IsNullOrEmpty(prefix) ? network.Name : $"{prefix}.{network.Name}");
        }

        // Register internal routes
        foreach (var (from, to, prob) in network.Routes)
        {
            var qualifiedFrom = Qualify(network.FullName, from);
            var qualifiedTo = Qualify(network.FullName, to);
            ConnectNode(qualifiedFrom, qualifiedTo, prob);
        }

    }

    private string Qualify(string prefix, string name)
    {
        // Already fully qualified with current prefix
        if (name.StartsWith(prefix + ".")) return name;

        // Already fully qualified with a deeper prefix (e.g., Mall.PizzaShop.X)
        if (name.Split('.').Length > 1 && name.StartsWith(prefix)) return name;

        return string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
    }

    public void CreateDispatcherNode(string name, Func<double> arrivalDist)
    {
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

        if (!_nodes.TryGetValue(to, out var toNode))
            throw new ArgumentException($"Target node '{to}' not found.");

        if (fromNode.NextNodeChoices == null && probability < 1.0)
        {
            fromNode.NextNodeChoices = new List<(Node, double)> { (toNode, probability) };
        }
        else if (fromNode.NextNodeChoices != null)
        {
            fromNode.NextNodeChoices.Add((toNode, probability));
        }
        else
        {
            fromNode.NextNode = toNode;
        }
    }

    public void RecordNetworkEntry(Entity entity, string networkName, double time)
    {
        if (!_validNetworkNames.Contains(networkName))
            return;

        if (entity.NetworkStack.Contains(networkName))
            return;

        entity.NetworkStack.Push(networkName);

        //Console.WriteLine($"[ENTRY] {entity.CreationTime:F2} → {networkName} at {time:F2}");
        //Console.WriteLine($"[STACK] {string.Join(" → ", entity.NetworkStack)}");

        if (!_networks.TryGetValue(networkName, out var stats))
        {
            stats = new NetworkStats(networkName);
            _networks[networkName] = stats;
        }

        stats.RecordEntry(entity, time);
        entity.NetworkEntryTimes[networkName] = time;
    }

    public void RecordNetworkExit(Entity entity, string networkName, double time)
    {
        if (!_validNetworkNames.Contains(networkName))
            return;

        if (!entity.NetworkStack.TryPeek(out var top) || top != networkName)
            return;

        //Console.WriteLine($"[EXIT] {entity.CreationTime:F2} ← {networkName} at {time:F2}");

        _networks.TryAdd(networkName, new NetworkStats(networkName));
        _networks[networkName].RecordExit(entity, time);
        entity.NetworkStack.Pop();

        if (entity.NetworkEntryTimes.TryGetValue(networkName, out var entryTime))
        {
            var runtimeStats = Stats.NetworkStats.FirstOrDefault(n => n.Name == networkName);
            if (runtimeStats == null)
            {
                runtimeStats = new NetworkRuntimeStats { Name = networkName };
                Stats.NetworkStats.Add(runtimeStats);
            }

            runtimeStats.AddRespondTime(time - entryTime);
            entity.NetworkEntryTimes.Remove(networkName);
        }
    }

    public void TransitionNetwork(Entity entity, string parentNetwork, string childNetwork, double time)
    {
        if (!_validNetworkNames.Contains(childNetwork))
        {
            while (entity.NetworkStack.TryPeek(out var current))
            {
                RecordNetworkExit(entity, current, time);
            }
            return;
        }

        while (entity.NetworkStack.TryPeek(out var current) &&
               !NetworkUtils.IsSubnetwork(current, childNetwork))
        {
            RecordNetworkExit(entity, current, time);
        }

        RecordNetworkEntry(entity, childNetwork, time);
    }

    public void RunSimulation()
    {
        Stats = new SimulationStats();

        foreach (var queue in _allNodes)
        {
            string networkName = queue.Name.Split('.')[0];
            if (!_networks.ContainsKey(networkName))
                _networks[networkName] = new NetworkStats(networkName);

            _networks[networkName].RegisterQueue(queue);

            var runtimeStat = new QueueRuntimeStats
            {
                Name = queue.Name,
                ServerCount = queue.ServerCount,
                SimulationTimePerRun = _untilTime
            };
            Stats.QueueStats.Add(runtimeStat);
            queue.AttachRuntimeStats(runtimeStat);
        }

        for (int i = 0; i < _runCount; i++)
        {
            _simulation = new Simulation();

            foreach (var dispatcher in _dispatchers)
                dispatcher.Value.ScheduleInitialArrival();

            _simulation.Run(_untilTime);
            Stats.AddSimulationRunTime(_simulation.Now);

            foreach (var q in _allNodes)
            {
                q.Reset(_simulation);
            }
        }
    }

    public SimulationStats GetSimulationStats() => Stats;

    public List<Entity> GetEntities() => _entities;

    public void RegisterEntity(Entity entity)
    {
        _entities.Add(entity);
        Stats.AddEntityCreated();
    }

    public void SafePopAndRestore(Entity entity, double time)
    {
        if (entity.NetworkStack.Count == 0)
            return;

        var exited = entity.NetworkStack.Pop();

        if (entity.NetworkStack.TryPeek(out var maybeParent) && _validNetworkNames.Contains(maybeParent))
        {
            RecordNetworkEntry(entity, maybeParent, time);
        }
    }
}