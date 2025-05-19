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

        if (!entity.NetworkStack.TryPeek(out var current) || current != _network)
        {
            _engine.RecordNetworkEntry(entity, _network, Simulation.Now);
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

        Node? target = NextNode;
        if (NextNodeChoices is not null)
        {
            double r = _engine.RandomGenerator.NextDouble();
            double cumulative = 0;
            foreach ((Node node, double prob) in NextNodeChoices)
            {
                cumulative += prob;
                if (r <= cumulative)
                {
                    target = node;
                    break;
                }
            }
        }

        if (target is QueueNode queue)
        {
            if (!entity.NetworkStack.TryPeek(out var current) || current != queue.Network)
            {
                if (entity.NetworkStack.TryPeek(out var exitNetwork))
                    _engine.RecordNetworkExit(entity, exitNetwork, Simulation.Now);

                _engine.RecordNetworkEntry(entity, queue.Network, Simulation.Now);
            }

            Simulation.Schedule(0, () => queue.ProcessArrival(entity));
        }
        else if (target is RouterNode router)
        {
            Simulation.Schedule(0, () => router.Route(entity));
        }
        else
        {
            // No next node or unsupported target type — end of line
            if (entity.NetworkStack.TryPeek(out var exitNetwork))
                _engine.RecordNetworkExit(entity, exitNetwork, Simulation.Now);

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