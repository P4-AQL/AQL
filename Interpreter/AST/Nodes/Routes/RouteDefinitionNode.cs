


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Routes;
public class RouteDefinitionNode(int lineNumber, IdentifierNode from, IEnumerable<RouteValuePairNode> to) : RouteNode(lineNumber)
{
    public IdentifierNode From { get; } = from;
    public IReadOnlyList<RouteValuePairNode> To { get; } = [.. to];

    public override string ToString() => $"Route({From}, {To})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            From,
            .. To,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{To.Count} destinations";

}