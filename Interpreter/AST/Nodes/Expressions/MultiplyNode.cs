


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class MultiplyNode(ExpressionNode left, ExpressionNode right) : ExpressionNode
{
    public ExpressionNode Left { get; } = left;
    public ExpressionNode Right { get; } = right;

    public override string ToString() => $"MultiplyNode({Left}, {Right})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Left,
            Right,
        ];
    }
}