


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Metrics;
public class NamedMetricNode(int lineNumber, string name) : MetricNode(lineNumber)
{
    public string Name { get; } = name;

    public override string ToString() => $"NamedMetricNode({Name})";

    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Name}";

}