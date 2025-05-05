


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class NotNode(ExpressionNode inner) : ExpressionNode
{
    public ExpressionNode Expression { get; } = inner;

    public override string ToString() => $"NotNode({Expression})";
}