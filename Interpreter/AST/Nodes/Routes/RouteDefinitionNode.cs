


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Routes;
public class RouteDefinitionNode(ExpressionNode from, IEnumerable<RouteValuePairNode> to) : RouteNode
{
    public ExpressionNode From { get; } = from;
    public IReadOnlyList<RouteValuePairNode> To { get; } = [.. to];

    public override string ToString() => $"Route({From}, {To})";

    public override IEnumerable<Node> Children()
    {
        return [
            From,
            .. To,
        ];
    }

}