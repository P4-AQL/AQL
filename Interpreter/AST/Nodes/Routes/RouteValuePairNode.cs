using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Routes;

public class RouteValuePairNode(ExpressionNode probability, ExpressionNode routeTo) : Node
{
    public ExpressionNode Probability { get; } = probability;
    public ExpressionNode RouteTo { get; } = routeTo;

    public override IEnumerable<Node> Children()
    {
        return [
            Probability, RouteTo
        ];
    }
}