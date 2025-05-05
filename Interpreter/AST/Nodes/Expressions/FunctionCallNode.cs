


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class FunctionCallNode(ExpressionNode identifier, IEnumerable<ExpressionNode> actualParameters) : ExpressionNode
{
    public ExpressionNode Identifier { get; } = identifier;
    public IReadOnlyList<ExpressionNode> ActualParameters { get; } = [.. actualParameters];
    public override string ToString() => $"FunctionCallNode({Identifier}, ({string.Join(", ", ActualParameters)}))";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Identifier,
            .. ActualParameters,
        ];
    }
}