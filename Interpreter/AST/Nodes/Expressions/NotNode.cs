


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class NotNode(ExpressionNode expression) : ExpressionNode
{
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"NotNode({Expression})";
}