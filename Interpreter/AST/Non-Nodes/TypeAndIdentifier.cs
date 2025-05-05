


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.NonNodes;
public class TypeAndIdentifier(TypeNode type, IdentifierNode identifier)
{
    public TypeNode Type { get; } = type;
    public IdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"TypedIdentifierNode({Type}, {Identifier})";
}
