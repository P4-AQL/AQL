


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class VariableDeclarationNode(TypeNode type, IdentifierNode identifier) : StatementNode
{
    TypeNode Type { get; } = type;
    IdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"VariableDeclarationNode({Type}, {Identifier})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Type,
            Identifier,
        ];
    }

}