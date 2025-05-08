


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class AssignNode(IdentifierNode identifier, ExpressionNode expression) : StatementNode
{
    public IdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"AssignNode({Identifier} = {Expression})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            Identifier,
            Expression,
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier}";

}