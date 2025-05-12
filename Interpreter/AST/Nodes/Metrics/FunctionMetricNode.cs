


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Metrics;
public class FunctionMetricNode(int lineNumber, IdentifierNode function) : MetricNode(lineNumber)
{
    IdentifierNode Function { get; } = function;

    public override string ToString() => $"FunctionMetricNode({Function})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Function,
            .. base.GetChildren(),
        ];
    }
}