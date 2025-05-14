


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class VariableDeclarationNode(int lineNumber, StatementNode? nextStatement, TypeNode type, SingleIdentifierNode identifier, ExpressionNode expression) : StatementCompositionNode(lineNumber, nextStatement)
{
    public TypeNode Type { get; } = type;
    public SingleIdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"VariableDeclarationNode({Type}, {Identifier}, {Expression}, {NextStatement})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Type,
            Identifier,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";

}