using System;
using System.Collections.Generic;
using System.Linq;

public class Network
{
    public string Name { get; }
    private readonly Simulation simulation;
    private readonly Dictionary<string, Queue> queues = new();
    private readonly Dictionary<string, List<Route>> routingTable = new();

    public Network(string name, Simulation simulation)
    {
        Name = name;
        this.simulation = simulation;
    }

    public void AddQueue(Queue queue)
    {
        queues[queue.Name] = queue;
    }

    public void AddRoute(string fromQueue, string toQueue, double probability = 1.0)
    {
        if (!routingTable.ContainsKey(fromQueue))
            routingTable[fromQueue] = new List<Route>();

        routingTable[fromQueue].Add(new Route(toQueue, probability));
    }

    public void RouteEntity(string fromQueueName, Entity entity)
    {
        if (!routingTable.ContainsKey(fromQueueName))
            return; // End of routing or entity leaves network.

        var routes = routingTable[fromQueueName];
        double randValue = new Random().NextDouble();
        double cumulativeProbability = 0.0;

        foreach (var route in routes)
        {
            cumulativeProbability += route.Probability;
            if (randValue <= cumulativeProbability)
            {
                queues[route.TargetQueueName].Enqueue(entity);
                return;
            }
        }
    }

    private class Route
    {
        public string TargetQueueName { get; }
        public double Probability { get; }

        public Route(string targetQueueName, double probability)
        {
            TargetQueueName = targetQueueName;
            Probability = probability;
        }
    }
}