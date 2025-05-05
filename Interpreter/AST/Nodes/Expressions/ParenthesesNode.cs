


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class ParenthesesNode(ExpressionNode inner) : ExpressionNode
{
    public ExpressionNode Inner { get; } = inner;

    public override string ToString() => $"ParenthesesNode({Inner})";
}