using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Routes;

public class RouteValuePairNode(int lineNumber, ExpressionNode probability, ExpressionNode routeTo) : Node(lineNumber)
{
    public ExpressionNode Probability { get; } = probability;
    public ExpressionNode RouteTo { get; } = routeTo;

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Probability, RouteTo
        ];
    }
}