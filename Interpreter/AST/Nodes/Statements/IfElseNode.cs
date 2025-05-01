


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class IfElseNode(ExpressionNode condition, StatementNode trueStatement, StatementNode falseStatement) : StatementNode
{
    public ExpressionNode Condition { get; } = condition;
    public StatementNode TrueStatement { get; } = trueStatement;
    public StatementNode FalseStatement { get; } = falseStatement;

    public override string ToString() => $"IfElseNode({Condition}) {{ {TrueStatement} }} {{ {FalseStatement} }}";

}