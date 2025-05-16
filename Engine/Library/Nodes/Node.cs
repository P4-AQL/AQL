namespace SimEngine.Nodes;

using System.Collections.Generic;

public abstract class Node
{
    public string Name { get; }
    public List<(QueueNode Node, double Prob)>? NextNodeChoices { get; set; } = null;
    public QueueNode? NextNode { get; set; } = null;

    protected Node(string name)
    {
        Name = name;
    }
}