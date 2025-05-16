namespace SimEngine.Nodes;

using System;
using System.Collections.Generic;

public class QueueNode : Node
{
    private readonly Core.SimulationEngineAPI _engine;
    private Core.Simulation Simulation => _engine._simulation;
    private readonly int _capacity;
    private readonly int _servers;
    private int _busyServers = 0;
    private Queue<Core.Entity> _waitingQueue = new();
    private readonly Func<double> _serviceTimeDist;
    private string _network => Name.Split('.')[0];

    public int ServerCount => _servers;
    private Metrics.QueueRuntimeStats? _runtimeStats;

    public QueueNode(Core.SimulationEngineAPI engine, string name, int servers, int capacity, Func<double> serviceTimeDist, Func<double>? arrivalDist = null)
        : base(name)
    {
        _engine = engine;
        _servers = servers;
        _capacity = capacity <= 0 ? int.MaxValue : capacity;
        _serviceTimeDist = serviceTimeDist;
    }

    public void AttachRuntimeStats(Metrics.QueueRuntimeStats stats)
    {
        _runtimeStats = stats;
    }

    public void ProcessArrival(Core.Entity entity)
    {
        entity.ArrivalTime = Simulation.Now;
        _runtimeStats?.AddArrival();

        if (_busyServers + _waitingQueue.Count >= _capacity)
        {
            _engine.GetEntities().Remove(entity);
            _runtimeStats?.AddDropped();
            return;
        }

        if (_busyServers < _servers)
        {
            StartService(entity);
        }
        else
        {
            _waitingQueue.Enqueue(entity);
        }

        if (entity.CurrentNetworkName != _network)
        {
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);
            entity.CurrentNetworkName = _network;
        }
    }

    private void StartService(Core.Entity entity)
    {
        _busyServers++;
        double serviceTime = _serviceTimeDist();
        double waitTime = Simulation.Now - entity.ArrivalTime;
        Simulation.Schedule(serviceTime, () => ProcessDeparture(entity));

        entity.ServiceTimesInQueues.Add(serviceTime);
        entity.WaitingTimesInQueues.Add(waitTime);
        _runtimeStats?.AddBusyTime(serviceTime);
        _runtimeStats?.AddServed(waitTime, serviceTime);
    }

    public void ProcessDeparture(Core.Entity entity)
    {
        _busyServers--;

        if (_waitingQueue.Count > 0)
        {
            StartService(_waitingQueue.Dequeue());
        }

        QueueNode target = NextNode!;
        if (NextNodeChoices is not null)
        {
            double r = _engine.RandomGenerator.NextDouble();
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

        if (target != null)
        {
            if (entity.CurrentNetworkName != target._network)
            {
                _engine.RecordNetworkExit(entity, entity.CurrentNetworkName, Simulation.Now);
                _engine.RecordNetworkEntry(entity, target._network, Simulation.Now);
                entity.CurrentNetworkName = target._network;
            }
            Simulation.Schedule(0, () => target.ProcessArrival(entity));
        }
        else
        {
            _engine.RecordNetworkExit(entity, entity.CurrentNetworkName, Simulation.Now);
            entity.DepartureTime = Simulation.Now;
        }
    }

    public void Reset(Core.Simulation simulation)
    {
        _waitingQueue.Clear();
        _busyServers = 0;
        _runtimeStats?.IncrementRun();
    }
}