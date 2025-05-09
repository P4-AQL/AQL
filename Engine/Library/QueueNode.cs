using System;
using System.Collections.Generic;

public class QueueNode
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

    private double _runBusyTime = 0.0;
    private int _runArrived = 0;
    private int _runServed = 0;
    private double _runTotalWait = 0.0;
    private int _runMaxQueue = 0;
    private int _runDroppedEntities = 0;

    public int TotalArrived { get; private set; } = 0;
    public int TotalServed { get; private set; } = 0;
    public double TotalWaitingTime { get; private set; } = 0.0;
    public int MaxQueueLength { get; private set; } = 0;
    private double TotalBusyTime { get; set; } = 0.0;
    private int Runs { get; set; } = 0;
    private double SimulationTimePerRun { get; set; } = 0.0;

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
        _runArrived++;

        if (_busyServers + _waitingQueue.Count >= _capacity)
        {
            _runDroppedEntities++;
            return;
        }

        if (_busyServers < _servers)
        {
            StartService(entity);
        }
        else
        {
            _waitingQueue.Enqueue(entity);
            if (_waitingQueue.Count > _runMaxQueue)
                _runMaxQueue = _waitingQueue.Count;
        }
    }
    
    private void StartService(Entity entity)
    {
        _busyServers++;
        double serviceTime = _serviceTimeDist();
        _runBusyTime += serviceTime;
        _sim.Schedule(serviceTime, () => ProcessDeparture(entity));
    }

    public void ProcessDeparture(Entity entity)
    {
        _runServed++;
        _runTotalWait += _sim.Now - entity.ArrivalTime;
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

        TotalArrived += _runArrived;
        TotalServed += _runServed;
        TotalWaitingTime += _runTotalWait;
        TotalBusyTime += _runBusyTime;
        if (_runMaxQueue > MaxQueueLength)
            MaxQueueLength = _runMaxQueue;
        if (_runDroppedEntities > MaxDroppedEntities)
            DroppedEntities = _runDroppedEntities;

        Runs++;

        _runArrived = 0;
        _runServed = 0;
        _runTotalWait = 0.0;
        _runMaxQueue = 0;
        _runBusyTime = 0.0;
        _runDroppedEntities = 0;
    }

    public QueueMetrics GetMetrics()
    {
        SimulationTimePerRun = _sim.Now;
        
        double avgWait = TotalServed > 0 ? TotalWaitingTime / TotalServed : 0;
        double utilization = (SimulationTimePerRun > 0 && Runs > 0)
            ? TotalBusyTime / (SimulationTimePerRun * Runs * _servers)
            : 0;
        double avgThroughput = Runs > 0 ? (double)TotalServed / Runs : TotalServed;

        return new QueueMetrics
        {
            TotalArrived = TotalArrived,
            TotalServed = TotalServed,
            AvgWaitTime = avgWait,
            MaxQueueLength = MaxQueueLength,
            ServerUtilization = utilization,
            Throughput = avgThroughput,
            DroppedEntities = DroppedEntities
        };
    }
}