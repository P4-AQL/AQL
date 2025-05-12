


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;

namespace Interpreter.AST.Nodes.Statements;
public class TypeAndIdentifier(int lineNumber, TypeNode type, SingleIdentifierNode identifier) : Node(lineNumber)
{
    public TypeNode Type { get; } = type;
    public SingleIdentifierNode Identifier { get; } = identifier;

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
