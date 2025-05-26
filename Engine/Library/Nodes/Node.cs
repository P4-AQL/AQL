namespace SimEngine.Nodes;

using System;
using System.Collections.Generic;
using System.Linq;

using SimEngine.Core;

public abstract class Node
{
    public string Name { get; }
    public List<(Node Node, double Prob)>? NextNodeChoices { get; set; } = null;
    public Node? NextNode { get; set; } = null;

    protected string _network;
    public string Network => _network;

    protected Node(string name)
    {
        Name = name;

        var parts = name.Split('.');
        _network = parts.Length > 1
            ? string.Join('.', parts.Take(parts.Length - 1))
            : name;
    }

    public Node? ChooseProbabilisticNode(
        List<(Node node, double probability)>? nodeChoices,
        SimulationEngineAPI engine,
        Node? defaultNode)
    {
        if (nodeChoices is null || nodeChoices.Count == 0)
            return defaultNode;

        double totalProb = 0.0;
        foreach (var (_, prob) in nodeChoices)
        {
            totalProb += prob;
        }

        double r = engine.RandomGenerator.NextDouble() * totalProb;
        double cumulative = 0.0;

        foreach (var (node, prob) in nodeChoices)
        {
            cumulative += prob;
            if (r <= cumulative)
                return node;
        }

        return defaultNode;
    }

}