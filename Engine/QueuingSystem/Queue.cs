using System.Collections.Generic;

public class Queue
{
    public string Name { get; }
    public int Capacity { get; }
    public int Servers { get; }
    public Func<double> ServiceTimeDistribution { get; }
    private readonly Simulation simulation;
    private readonly Network network;
    private readonly Queue<Entity> waitingEntities = new();
    private int busyServers = 0;

    public Queue(string name, int servers, int capacity, Func<double> serviceTimeDistribution, Simulation simulation, Network network)
    {
        Name = name;
        Servers = servers;
        Capacity = capacity;
        ServiceTimeDistribution = serviceTimeDistribution;
        this.simulation = simulation;
        this.network = network;
    }

    public void Enqueue(Entity entity)
    {
        if (waitingEntities.Count >= Capacity)
        {
            // Queue full, handle according (reject entity or log, etc.)
            return;
        }

        waitingEntities.Enqueue(entity);
        TryStartService();
    }

    private void TryStartService()
    {
        if (waitingEntities.Count > 0 && busyServers < Servers)
        {
            busyServers++;
            Entity entity = waitingEntities.Dequeue();
            double serviceTime = ServiceTimeDistribution();

            simulation.Schedule(serviceTime, () => CompleteService(entity));
        }
    }

    private void CompleteService(Entity entity)
    {
        busyServers--;

        // Route the entity to the next queue via the network routing logic
        network.RouteEntity(Name, entity);

        // Metrics tracking add here

        TryStartService(); // Check if the next entity can begin service
    }
}
