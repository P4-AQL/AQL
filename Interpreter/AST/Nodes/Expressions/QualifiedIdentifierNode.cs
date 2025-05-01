


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class QualifiedIdentifierNode(IdentifierNode identifier, ExpressionNode expression) : ExpressionNode
{
    public IdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"QualifiedIdentifierNode({Identifier}, {Expression})";
}