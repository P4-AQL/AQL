using System;
using System.Collections.Generic;
using System.Linq;

public class SimulationEngineAPI
{
    private Simulation _simulation = new();
    private Dictionary<string, QueueNode> _queues = new();
    private Dictionary<string, QueueNode> _entryPoints = new();
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
        var queue = new QueueNode(_simulation, name, servers, capacity, serviceTime, arrivalTime);
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

    public void RunSimulation()
    {
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
}

public class QueueMetrics
{
    public int TotalArrived { get; init; }
    public int TotalServed { get; init; }
    public double AvgWaitTime { get; init; }
    public int MaxQueueLength { get; init; }
}

public class Simulation
{
    private double _currentTime = 0;
    private readonly PriorityQueue<Event, double> _eventQueue = new();
    public double Now => _currentTime;

    public void Schedule(double delay, Action action)
    {
        var ev = new Event(_currentTime + delay, action);
        _eventQueue.Enqueue(ev, ev.Time);
    }

    public void Run(double untilTime)
    {
        while (_eventQueue.Count > 0)
        {
            var ev = _eventQueue.Peek();
            if (ev.Time > untilTime) break;
            _eventQueue.Dequeue();
            _currentTime = ev.Time;
            ev.Action();
        }
        _currentTime = untilTime;
    }
}

public class Event
{
    public double Time { get; }
    public Action Action { get; }

    public Event(double time, Action action)
    {
        Time = time;
        Action = action;
    }
}

public partial class QueueNode
{
    private Simulation _sim;
    private readonly string _name;
    private readonly int _capacity;
    private readonly int _servers;
    private int _busyServers = 0;
    private Queue<Entity> _waitingQueue = new();
    private readonly Func<double> _serviceTimeDist;
    private readonly Func<double>? _arrivalDist;
    public QueueNode? NextNode { get; set; } = null;
    public List<(QueueNode Node, double Prob)>? NextNodeChoices { get; set; } = null;

    public int TotalArrived { get; private set; } = 0;
    public int TotalServed { get; private set; } = 0;
    public double TotalWaitingTime { get; private set; } = 0.0;
    public int MaxQueueLength { get; private set; } = 0;

    public QueueNode(Simulation sim, string name, int servers, int capacity, Func<double> serviceTimeDist, Func<double>? arrivalDist = null)
    {
        _sim = sim;
        _name = name;
        _servers = servers;
        _capacity = capacity <= 0 ? int.MaxValue : capacity;
        _serviceTimeDist = serviceTimeDist;
        _arrivalDist = arrivalDist;
    }

    public void ScheduleInitialArrival()
    {
        if (_arrivalDist != null)
        {
            _sim.Schedule(_arrivalDist(), () =>
            {
                var entity = new Entity(_sim.Now);
                ProcessArrival(entity);
                ScheduleInitialArrival();
            });
        }
    }

    public void ProcessArrival(Entity entity)
    {
        entity.ArrivalTime = _sim.Now;
        TotalArrived++;

        if (_busyServers + _waitingQueue.Count >= _capacity)
            return; // Queue full

        if (_busyServers < _servers)
            StartService(entity);
        else
        {
            _waitingQueue.Enqueue(entity);
            if (_waitingQueue.Count > MaxQueueLength)
                MaxQueueLength = _waitingQueue.Count;
        }
    }

    private void StartService(Entity entity)
    {
        _busyServers++;
        double serviceTime = _serviceTimeDist();
        _sim.Schedule(serviceTime, () => ProcessDeparture(entity));
    }

    public void ProcessDeparture(Entity entity)
    {
        TotalServed++;
        TotalWaitingTime += _sim.Now - entity.ArrivalTime;
        _busyServers--;

        if (_waitingQueue.Count > 0)
            StartService(_waitingQueue.Dequeue());

        if (NextNode != null || NextNodeChoices != null)
        {
            QueueNode target = NextNode!;
            if (NextNodeChoices != null)
            {
                double r = Random.Shared.NextDouble();
                double cumulative = 0;
                foreach (var (node, prob) in NextNodeChoices)
                {
                    cumulative += prob;
                    if (r <= cumulative)
                    {
                        target = node;
                        break;
                    }
                }
            }
            _sim.Schedule(0, () => target.ProcessArrival(entity));
        }
    }

    public void Reset(Simulation simulation)
    {
        _sim = simulation;
        _waitingQueue.Clear();
        _busyServers = 0;
        TotalArrived = 0;
        TotalServed = 0;
        TotalWaitingTime = 0;
        MaxQueueLength = 0;
    }

    public QueueMetrics GetMetrics()
    {
        return new QueueMetrics
        {
            TotalArrived = TotalArrived,
            TotalServed = TotalServed,
            AvgWaitTime = TotalServed > 0 ? TotalWaitingTime / TotalServed : 0,
            MaxQueueLength = MaxQueueLength
        };
    }
}

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
