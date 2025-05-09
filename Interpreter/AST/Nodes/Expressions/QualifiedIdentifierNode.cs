


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class QualifiedIdentifierNode(int lineNumber, IdentifierNode identifier, ExpressionNode expression) : ExpressionNode(lineNumber)
{
    public IdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"QualifiedIdentifierNode({Identifier}, {Expression})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            Expression,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";
}