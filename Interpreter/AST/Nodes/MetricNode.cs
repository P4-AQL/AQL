


namespace Interpreter.AST.Nodes;
public class MetricNode(string metricName) : Node
{
    public string MetricName { get; } = metricName;
}