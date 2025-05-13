using System;
using System.Collections.Generic;

public class QueueNode : Node
{
    private readonly SimulationEngineAPI _engine;
    private Simulation Simulation => _engine._simulation;
    private readonly string _name;
    private readonly int _capacity;
    private readonly int _servers;
    private int _busyServers = 0;
    private Queue<Entity> _waitingQueue = new();
    private readonly Func<double> _serviceTimeDist;
    private string _network { get; set; } = string.Empty;

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
    public int MaxDroppedEntities { get; private set; } = 0;
    public int TotalDroppedEntities { get; private set; } = 0;
    public string Network => _network;

    public QueueNode(SimulationEngineAPI engine, string name, int servers, int capacity, Func<double> serviceTimeDist, Func<double>? arrivalDist = null)
        : base(name)
    {
        _engine = engine;
        _name = name;
        _servers = servers;
        _capacity = capacity <= 0 ? int.MaxValue : capacity;
        _serviceTimeDist = serviceTimeDist;
        _network = name.Split('.')[0];
    }

    public void ProcessArrival(Entity entity)
    {
        entity.ArrivalTime = Simulation.Now;
        _runArrived++;

        if (_busyServers + _waitingQueue.Count >= _capacity)
        {
            _runDroppedEntities++;
            _engine._entities.Remove(entity);
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
        if (entity.CurrentNetworkName != _network)
        {
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);
            entity.CurrentNetworkName = _network;
        }
    }

    private void StartService(Entity entity)
    {
        _busyServers++;
        double serviceTime = _serviceTimeDist();
        _runBusyTime += serviceTime;
        entity.ServiceTimesInQueues.Add(serviceTime);
        entity.WaitingTimesInQueues.Add(Simulation.Now - entity.ArrivalTime);
        Simulation.Schedule(serviceTime, () => ProcessDeparture(entity));
    }

    public void ProcessDeparture(Entity entity)
    {
        _runServed++;
        _runTotalWait += Simulation.Now - entity.ArrivalTime;
        _busyServers--;

        if (_waitingQueue.Count > 0)
            StartService(_waitingQueue.Dequeue());

        if (NextNode != null || NextNodeChoices != null)
        {
            QueueNode target = NextNode!;
            if (NextNodeChoices is not null)
            {
                double r = Random.Shared.NextDouble();
                double cumulative = 0;
                foreach ((QueueNode node, double prob) in NextNodeChoices)
                {
                    cumulative += prob;
                    if (r <= cumulative)
                    {
                        target = node;
                        break;
                    }
                }
            }

            if (entity.CurrentNetworkName != target.Network)
            {
                _engine.RecordNetworkExit(entity, entity.CurrentNetworkName, Simulation.Now);
                _engine.RecordNetworkEntry(entity, target.Network, Simulation.Now);
                entity.CurrentNetworkName = target.Network;
            }

            Simulation.Schedule(0, () => target.ProcessArrival(entity));
        }
        else if (NextNode == null && NextNodeChoices == null)
        {
            _engine.RecordNetworkExit(entity, entity.CurrentNetworkName, Simulation.Now);
            entity.DepartureTime = Simulation.Now;
        }
    }

    public void Reset(Simulation simulation)
    {
        _waitingQueue.Clear();
        _busyServers = 0;

        TotalArrived += _runArrived;
        TotalServed += _runServed;
        TotalWaitingTime += _runTotalWait;
        TotalBusyTime += _runBusyTime;
        TotalDroppedEntities += _runDroppedEntities;
        if (_runMaxQueue > MaxQueueLength)
            MaxQueueLength = _runMaxQueue;
        if (_runDroppedEntities > MaxDroppedEntities)
            MaxDroppedEntities = _runDroppedEntities;

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
        SimulationTimePerRun = Simulation.Now;

        double avgWait = TotalServed > 0 ? TotalWaitingTime / TotalServed : 0;
        double utilization = (SimulationTimePerRun > 0 && Runs > 0)
            ? TotalBusyTime / (SimulationTimePerRun * Runs * _servers)
            : 0;
        double avgThroughput = Runs > 0 ? (double)TotalServed / Runs : TotalServed;
        double entitiesDroppedRate = TotalArrived > 0 ? (double)TotalDroppedEntities / TotalArrived : 0;

        return new QueueMetrics
        {
            TotalArrived = TotalArrived,
            TotalServed = TotalServed,
            AvgWaitTime = avgWait,
            MaxQueueLength = MaxQueueLength,
            ServerUtilization = utilization,
            Throughput = avgThroughput,
            MaxDroppedEntities = MaxDroppedEntities,
            EntitiesDroppedRate = entitiesDroppedRate,
        };
    }
}