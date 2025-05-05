


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class AndNode(ExpressionNode left, ExpressionNode right) : ExpressionNode
{
    public ExpressionNode Left { get; } = left;
    public ExpressionNode Right { get; } = right;

    public override string ToString() => $"AndNode({Left}, {Right})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Left,
            Right,
        ];
    }
}