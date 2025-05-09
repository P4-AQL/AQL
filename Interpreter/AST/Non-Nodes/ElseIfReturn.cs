


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.NonNodes;
public class ElseIfReturn(ExpressionNode condition, StatementNode body)
{
    public ExpressionNode Condition { get; } = condition;
    public StatementNode Body { get; } = body;
}