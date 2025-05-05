


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Metrics;
public class FunctionMetricNode(FunctionCallNode functionCall) : MetricNode
{
    FunctionCallNode FunctionCall { get; } = functionCall;

    public override string ToString() => $"FunctionMetricNode({FunctionCall})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            FunctionCall,
        ];
    }
}