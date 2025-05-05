


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.NonNodes;

namespace Interpreter.AST.Nodes.Statements;
public class IfElseNode(ExpressionNode condition, StatementNode ifBody, StatementNode elseBody) : StatementNode
{
    public ExpressionNode Condition { get; } = condition;
    public StatementNode IfBody { get; } = ifBody;
    public StatementNode ElseBody { get; } = elseBody;

    public override string ToString() => $"IfElseNode({Condition}) {{ {IfBody} }} {{ {ElseBody} }}";

    public IfElseNode(ElseIfReturn elseIfReturn, StatementNode elseBody) : this(condition: elseIfReturn.Condition, ifBody: elseIfReturn.Body, elseBody: elseBody)
    {

    }

}