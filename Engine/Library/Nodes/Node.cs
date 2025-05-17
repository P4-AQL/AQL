namespace SimEngine.Nodes;

using System.Collections.Generic;

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
        _network = name.Split('.')[0];
    }
}