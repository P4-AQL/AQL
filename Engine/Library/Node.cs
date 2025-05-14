using System.Collections.Generic;

namespace Engine;
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