


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class IdentifierExpressionNode(int lineNumber, IdentifierNode identifier) : ExpressionNode(lineNumber)
{
    public IdentifierNode Identifier { get; } = identifier;

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            .. base.GetChildren(),
        ];
    }
}