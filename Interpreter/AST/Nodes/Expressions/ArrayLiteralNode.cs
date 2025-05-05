


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class ArrayLiteralNode(IEnumerable<ExpressionNode> elements) : ExpressionNode
{
    public IReadOnlyList<ExpressionNode> Elements { get; } = [.. elements];

    public override string ToString() => $"ArrayTypeNode(Elements: [{string.Join(',', Elements)}])";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            .. Elements
        ];
    }
}