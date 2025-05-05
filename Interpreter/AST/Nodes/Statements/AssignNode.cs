


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class AssignNode(IdentifierNode identifier, ExpressionNode expression) : StatementNode
{
    public IdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"AssignNode({Identifier} = {Expression})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Identifier,
            Expression,
        ];
    }

}