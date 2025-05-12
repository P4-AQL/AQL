


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class AssignNode(int lineNumber, StatementNode? nextStatement, SingleIdentifierNode identifier, ExpressionNode expression) : StatementCompositionNode(lineNumber, nextStatement)
{
    public SingleIdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"AssignNode({Identifier}, {Expression}, {NextStatement})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            Expression,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier}";

}