


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class RouteNode(ExpressionNode from, ExpressionNode to) : Node
{
    public ExpressionNode From { get; } = from;
    public ExpressionNode To { get; } = to;

    public override string ToString() => $"Route({From}, {To})";

    public override IEnumerable<Node> Children()
    {
        return [
            From,
            To,
        ];
    }

}