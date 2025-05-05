


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Metrics;
public class NamedMetricNode(string name) : MetricNode
{
    public string Name { get; } = name;

    public override string ToString() => $"NamedMetricNode({Name})";

    public override IEnumerable<Node> Children()
    {
        return base.Children();
    }
}