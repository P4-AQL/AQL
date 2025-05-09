


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes.Statements;
public class TypeAndIdentifier(int lineNumber, TypeNode type, IdentifierNode identifier) : Node(lineNumber)
{
    public TypeNode Type { get; } = type;
    public IdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"TypedIdentifierNode({Type}, {Identifier})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Type,
            Identifier,
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";

}
