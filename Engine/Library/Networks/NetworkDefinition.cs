namespace SimEngine.Networks;

using System;
using System.Collections.Generic;

public class NetworkDefinition
{
    public string Name { get; init; } = string.Empty;

    public List<(string Name, int Servers, int Capacity, Func<double> ServiceTime)> Queues { get; } = new();
    public List<(string From, string To, double Probability)> Routes { get; } = new();
    public List<string> RouterEntries { get; } = new();
    public List<string> RouterExits { get; } = new();
    public List<NetworkDefinition> SubNetworks { get; } = new();

    public void AddQueue(string name, int servers, int capacity, Func<double> serviceTime)
        => Queues.Add((name, servers, capacity, serviceTime));

    public void Connect(string from, string to, double probability = 1.0)
        => Routes.Add((from, to, probability));

    public void AddEntryPoint(string name)
        => RouterEntries.Add(name);

    public void AddExitPoint(string name)
        => RouterExits.Add(name);

    public void AddSubNetwork(NetworkDefinition sub)
        => SubNetworks.Add(sub);
}
