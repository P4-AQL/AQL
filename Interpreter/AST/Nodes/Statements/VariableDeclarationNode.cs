


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class VariableDeclarationNode(int lineNumber, StatementNode? nextStatement, TypeNode type, SingleIdentifierNode identifier) : StatementCompositionNode(lineNumber, nextStatement)
{
    public TypeNode Type { get; } = type;
    public SingleIdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"VariableDeclarationNode({Type}, {Identifier}, {NextStatement})";

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