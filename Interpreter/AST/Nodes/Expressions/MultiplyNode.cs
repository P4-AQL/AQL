


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class MultiplyNode(int lineNumber, ExpressionNode left, ExpressionNode right) : ExpressionNode(lineNumber)
{
    public ExpressionNode Left { get; } = left;
    public ExpressionNode Right { get; } = right;

    public override string ToString() => $"MultiplyNode({Left}, {Right})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Left,
            Right,
            .. base.GetChildren(),
        ];
    }
}