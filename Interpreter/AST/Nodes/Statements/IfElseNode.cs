


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.NonNodes;

namespace Interpreter.AST.Nodes.Statements;
public class IfElseNode(int lineNumber, StatementNode? nextStatement, ExpressionNode condition, StatementNode ifBody, StatementNode elseBody) : StatementCompositionNode(lineNumber, nextStatement)
{
    public ExpressionNode Condition { get; } = condition;
    public StatementNode IfBody { get; } = ifBody;
    public StatementNode ElseBody { get; } = elseBody;

    public override string ToString() => $"IfElseNode({Condition}, {IfBody}, {ElseBody}, {NextStatement})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Condition,
            IfBody,
            ElseBody,
            .. base.GetChildren(),
        ];
    }

    public IfElseNode(int lineNumber, StatementNode? nextStatement, ElseIfReturn elseIfReturn, StatementNode elseBody) : this(lineNumber: lineNumber, nextStatement: nextStatement, condition: elseIfReturn.Condition, ifBody: elseIfReturn.Body, elseBody: elseBody)
    {

    }

}