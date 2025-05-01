


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class AddNode(ExpressionNode left, ExpressionNode right) : ExpressionNode
{
    public ExpressionNode Left { get; } = left;
    public ExpressionNode Right { get; } = right;

    public override string ToString() => $"AddNode({Left}, {Right})";
}