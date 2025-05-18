namespace SimEngine.Nodes;

using System;
using System.Collections.Generic;
using System.Linq;

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
}